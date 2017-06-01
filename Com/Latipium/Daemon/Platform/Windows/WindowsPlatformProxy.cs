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
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public Process Start(ProcessStartInfo psi, DisplayDetectData display) {
            return Process.Start(psi);
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
                        break;
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

