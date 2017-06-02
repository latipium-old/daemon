//
// WindowsPlatformProxy.cs
//
// Author:
//       Zach Deibert <zachdeibert@gmail.com>
//
// Copyright (c) 2017 Zach Deibert
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using Com.Latipium.Daemon.Api.Model;

namespace Com.Latipium.Daemon.Platform.Windows {
    internal class WindowsPlatformProxy : Native, IPlatformProxy {
        private bool IsService {
            get {
                uint session;
                if (ProcessIdToSessionId((uint) Process.GetCurrentProcess().Id, out session)) {
                    return session == 0;
                } else {
                    Error("ProcessIdToSessionId");
                    return false;
                }
            }
        }

        private IntPtr AccessToken {
            get {
                uint session = WTSGetActiveConsoleSessionId();
                if (session == 0xFFFFFFFF) {
                    Error("WTSGetActiveConsoleSessionId");
                } else {
                    IntPtr accessToken;
                    if (WTSQueryUserToken(session, out accessToken)) {
                        return accessToken;
                    } else {
                        Error("WTSQueryUserToken");
                    }
                }
                return IntPtr.Zero;
            }
        }

        private void Error(string function) {
            int error = Marshal.GetLastWin32Error();
            string message;
            FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS, null, (uint) error, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), out message, 0, 0);
            WindowsService.WriteLog(string.Format("Error in {0}: {1} ({2})", function, message, error));
        }

        private bool GetUser(IntPtr accessToken, out string username, out string domain) {
            uint tokenInfoSize = 0;
            if (GetTokenInformation(accessToken, TOKEN_INFORMATION_CLASS.TokenUser, IntPtr.Zero, tokenInfoSize, out tokenInfoSize) || Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER) {
                IntPtr tokenInfo = Marshal.AllocHGlobal((int)tokenInfoSize);
                try {
                    if (GetTokenInformation(accessToken, TOKEN_INFORMATION_CLASS.TokenUser, tokenInfo, tokenInfoSize, out tokenInfoSize)) {
                        TOKEN_USER user = (TOKEN_USER)Marshal.PtrToStructure(tokenInfo, typeof(TOKEN_USER));
                        uint usernameSize = UNLEN + 1;
                        StringBuilder usernameBuffer = new StringBuilder((int)usernameSize);
                        uint domainSize = DNLEN + 1;
                        StringBuilder domainBuffer = new StringBuilder((int)domainSize);
                        int peUse;
                        if (LookupAccountSid(null, user.User.Sid, usernameBuffer, ref usernameSize, domainBuffer, ref domainSize, out peUse)) {
                            username = usernameBuffer.ToString();
                            domain = domainBuffer.ToString();
                            return true;
                        } else {
                            Error("LookupAccountSid");
                        }
                    } else {
                        Error("GetTokenInformation");
                    }
                } finally {
                    Marshal.FreeHGlobal(tokenInfo);
                }
            } else {
                Error("GetTokenInformation");
            }
            username = null;
            domain = null;
            return false;
        }

        public DisplayDetectData DetectDisplay(string id) {
            if (IsService) {
                IntPtr accessToken = AccessToken;
                if (accessToken != IntPtr.Zero) {
                    try {
                        string username;
                        string domain;
                        if (GetUser(accessToken, out username, out domain)) {
                            return new DisplayDetectData() {
                                Detected = true,
                                User = string.Concat(domain, "\\", username)
                            };
                        }
                    } finally {
                        CloseHandle(accessToken);
                    }
                }
                return new DisplayDetectData();
            } else {
                return new DisplayDetectData() {
                    Detected = true,
                    User = Environment.UserName
                };
            }
        }

        private string EscapeProcessParameter(string str) {
            return str.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }

        public Process Start(ProcessStartInfo psi, DisplayDetectData display) {
            /*
             * Doesn't support:
             * psi.Domain
             * psi.ErrorDialog
             * psi.ErrorDialogParentHandle
             * psi.LoadUserProfile
             * psi.Password
             * psi.RedirectStandardError
             * psi.RedirectStandardInput
             * psi.RedirectStandardOutput
             * psi.StandardErrorEncoding
             * psi.StandardInputEncoding
             * psi.StandardOutputEncoding
             * psi.UserName
             * psi.UseShellExecute
             * psi.Verb
             * psi.Verbs
             */
            IntPtr accessToken = AccessToken;
            if (accessToken != IntPtr.Zero) {
                try {
                    STARTUPINFO startInfo = new STARTUPINFO() {
                        cb = (uint) Marshal.SizeOf(typeof(STARTUPINFO)),
                        lpDesktop = "winsta0\\default",
                        dwFlags = STARTF_USESHOWWINDOW
                    };
                    switch (psi.WindowStyle) {
                        case ProcessWindowStyle.Hidden:
                            startInfo.wShowWindow = SW_HIDE;
                            break;
                        case ProcessWindowStyle.Maximized:
                            startInfo.wShowWindow = SW_MAXIMIZE;
                            break;
                        case ProcessWindowStyle.Minimized:
                            startInfo.wShowWindow = SW_MINIMIZE;
                            break;
                        case ProcessWindowStyle.Normal:
                            startInfo.wShowWindow = SW_SHOWNORMAL;
                            break;
                        default:
                            startInfo.dwFlags ^= STARTF_USESHOWWINDOW;
                            break;
                    }
                    PROCESS_INFORMATION procInfo;
                    string monoExe = Type.GetType("Mono.Runtime") == null ? null : Environment.GetEnvironmentVariable("_");
                    IEnumerable<byte[]> envVars = psi.EnvironmentVariables.Cast<DictionaryEntry>().Select(e => Encoding.Unicode.GetBytes(string.Concat(e.Key.ToString(), "=", e.Value.ToString())));
                    int len = envVars.Select(e => e.Length + 2).Sum() + 2;
                    IntPtr environ = Marshal.AllocHGlobal(len);
                    try {
                        IntPtr it = environ;
                        foreach (byte[] var in envVars) {
                            foreach (byte b in var) {
                                Marshal.WriteByte(it, b);
                                it = IntPtr.Add(it, 1);
                            }
                            Marshal.WriteInt16(it, 0);
                            it = IntPtr.Add(it, 2);
                        }
                        Marshal.WriteInt16(it, 0);
                        SECURITY_ATTRIBUTES procSec = new SECURITY_ATTRIBUTES();
                        SECURITY_ATTRIBUTES threadSec = new SECURITY_ATTRIBUTES();
                        procSec.nLength = Marshal.SizeOf(procSec);
                        threadSec.nLength = Marshal.SizeOf(threadSec);
                        if (CreateProcessAsUser(accessToken, null, string.Concat(monoExe == null ? string.Concat("\"", EscapeProcessParameter(psi.FileName), "\" ") :
                            string.Concat("\"", EscapeProcessParameter(monoExe), "\" \"", EscapeProcessParameter(psi.FileName), "\" "), psi.Arguments), ref procSec, ref threadSec,
                            false, CREATE_UNICODE_ENVIRONMENT | (psi.CreateNoWindow ? CREATE_NO_WINDOW : 0), environ, psi.WorkingDirectory, ref startInfo, out procInfo)) {
                            CloseHandle(procInfo.hThread);
                            try {
                                Process proc = new Process();
                                proc.StartInfo = psi;
                                Type Process = typeof(Process);
                                MethodInfo SetProcessHandle = Process.GetMethod("SetProcessHandle", BindingFlags.NonPublic | BindingFlags.Instance);
                                if (SetProcessHandle == null) {
                                    WindowsService.WriteLog("Unable to find System.Diagnostics.Process.SetProcessHandle(Microsoft.Win32.SafeHandles.SafeProcessHandle)");
                                } else {
                                    ConstructorInfo ctor = SetProcessHandle.GetParameters()[0].ParameterType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(IntPtr) }, null);
                                    if (ctor == null) {
                                        WindowsService.WriteLog("Unable to find Microsoft.Win32.SafeHandles.SafeProcessHandle.SafeProcessHandle(IntPtr)");
                                    } else {
                                        object procHandle = ctor.Invoke(new object[] { procInfo.hProcess });
                                        SetProcessHandle.Invoke(proc, new[] { procHandle });
                                        MethodInfo SetProcessId = Process.GetMethod("SetProcessId", BindingFlags.NonPublic | BindingFlags.Instance);
                                        if (SetProcessId == null) {
                                            WindowsService.WriteLog("Unable to find System.Diagnostics.Process.SetProcessId(int)");
                                        } else {
                                            SetProcessId.Invoke(proc, new object[] { (int) procInfo.dwProcessId });
                                            return proc;
                                        }
                                    }
                                }
                            } catch (Exception ex) {
                                WindowsService.WriteLog(ex);
                            }
                            CloseHandle(procInfo.hProcess);
                            return Process.GetProcessById((int) procInfo.dwProcessId);
                        } else {
                            Error("CreateProcessAsUser");
                            return null;
                        }
                    } finally {
                        Marshal.FreeHGlobal(environ);
                    }
                } finally {
                    CloseHandle(accessToken);
                }
            }
            return null;
        }

        public string FindLatipiumDir(string user) {
            IntPtr accessToken = AccessToken;
            try {
                IntPtr path;
                uint error = SHGetKnownFolderPath(KNOWNFOLDERID.RoamingAppData, 0, accessToken, out path);
                switch (error) {
                    case S_OK:
                        try {
                            string dir = Path.Combine(Marshal.PtrToStringAuto(path), "latipium");
                            Directory.CreateDirectory(dir);
                            string username;
                            string domain;
                            if (GetUser(accessToken, out username, out domain)) {
                                NTAccount account = new NTAccount(domain, username);
                                DirectorySecurity acl = Directory.GetAccessControl(dir);
                                if (!acl.GetAccessRules(true, true, typeof(NTAccount)).OfType<FileSystemAccessRule>()
                                    .Any(r => r.IdentityReference == account && r.FileSystemRights == FileSystemRights.FullControl && r.AccessControlType == AccessControlType.Allow)) {
                                    acl.AddAccessRule(new FileSystemAccessRule(account, FileSystemRights.FullControl, AccessControlType.Allow));
                                }
                                Directory.SetAccessControl(dir, acl);
                            }
                            return dir;
                        } finally {
                            Marshal.FreeCoTaskMem(path);
                        }
                    case E_FAIL:
                        WindowsService.WriteLog("Error in SHGetKnownFolderPath: Unspecified failure (2147500037)");
                        break;
                    case E_INVALIDARG:
                        WindowsService.WriteLog("Error in SHGetKnownFolderPath: One or more arguments are not valid (2147942487)");
                        break;
                    default:
                        WindowsService.WriteLog(string.Format("Error in SHGetKnownFolderPath: ({0})", error));
                        break;
                }
            } finally {
                CloseHandle(accessToken);
            }
            return null;
        }
    }
}

