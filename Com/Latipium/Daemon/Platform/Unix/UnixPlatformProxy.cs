//
// UnixPlatformProxy.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Com.Latipium.Daemon.Api.Model;

namespace Com.Latipium.Daemon.Platform.Unix {
    internal class UnixPlatformProxy : Native, IPlatformProxy {
        private static Dictionary<string, List<VerificationState>> VerificationStates = new Dictionary<string, List<VerificationState>>();

        public Process Start(ProcessStartInfo psi, DisplayDetectData display) {
            string oldArgs = psi.Arguments;
            string oldFile = psi.FileName;
            string oldDisplay = psi.EnvironmentVariables.ContainsKey("DISPLAY") ? psi.EnvironmentVariables["DISPLAY"] : null;
            string oldAuth = psi.EnvironmentVariables.ContainsKey("XAUTHORITY") ? psi.EnvironmentVariables["XAUTHORITY"] : null;
            psi.Arguments = string.Concat("-c \"mono '", psi.FileName, "' ", psi.Arguments.Replace("\\", "\\\\").Replace("\"", "\\\""), "\"");
            psi.FileName = "/bin/su";
            psi.EnvironmentVariables["DISPLAY"] = display.Display;
            psi.EnvironmentVariables["XAUTHORITY"] = display.Authority;
            psi.UseShellExecute = false;
            Process proc = Process.Start(psi);
            psi.Arguments = oldArgs;
            psi.FileName = oldFile;
            if (oldDisplay == null) {
                psi.EnvironmentVariables.Remove("DISPLAY");
            } else {
                psi.EnvironmentVariables["DISPLAY"] = oldDisplay;
            }
            if (oldAuth == null) {
                psi.EnvironmentVariables.Remove("XAUTHORITY");
            } else {
                psi.EnvironmentVariables["XAUTHORITY"] = oldAuth;
            }
            string pidStr;
            do {
                pidStr = File.ReadAllText(string.Format("/proc/{0}/task/{0}/children", proc.Id)).Trim();
            } while (string.IsNullOrEmpty(pidStr));
            int pid;
            if (int.TryParse(pidStr, out pid)) {
                proc.EnableRaisingEvents = true;
                Process realProc = Process.GetProcessById(pid);
                proc.Exited += (sender, e) => {
                    if (!realProc.HasExited) {
                        realProc.Kill();
                    }
                };
            } else {
                Console.Error.WriteLine("Unable to find real process");
            }
            return proc;
        }

        private string GetAuthority(object pid) {
            string path = string.Format("/proc/{0}/environ", pid);
            if (File.Exists(path)) {
                string[] environ = File.ReadAllText(path).Split('\0');
                string env = environ.FirstOrDefault(s => s.StartsWith("XAUTHORITY="));
                if (env == null) {
                    foreach (string task in Directory.GetDirectories(string.Format("/proc/{0}/task", pid))) {
                        string[] children = File.ReadAllText(Path.Combine(task, "children")).Split(' ');
                        foreach (string child in children) {
                            if (!string.IsNullOrWhiteSpace(child)) {
                                string authority = GetAuthority(child);
                                if (authority != null) {
                                    return authority;
                                }
                            }
                        }
                    }
                    return null;
                } else {
                    return env.Split(new [] { '=' }, 2)[1];
                }
            } else {
                return null;
            }
        }

        public DisplayDetectData DetectDisplay(string id) {
            setutxent();
            IntPtr utxp;
            List<DisplayDetectData> displays = new List<DisplayDetectData>();
            while ((utxp = getutxent()) != IntPtr.Zero) {
                utmpx utx = (utmpx) Marshal.PtrToStructure(utxp, typeof(utmpx));
                if (utx.ut_type == USER_PROCESS && utx.ut_host[0] != 0) {
                    string authority = GetAuthority(utx.ut_pid);
                    if (authority != null) {
                        displays.Add(new DisplayDetectData() {
                            User = new string(utx.ut_user, 0, Array.IndexOf(utx.ut_user, '\0')),
                            Display = new string(utx.ut_host, 0, Array.IndexOf(utx.ut_host, '\0')),
                            Authority = authority
                        });
                    }
                }
            }
            endutxent();
            switch (displays.Count) {
                case 0:
                    Console.Error.WriteLine("No displays found");
                    return new DisplayDetectData();
                case 1:
                    DisplayDetectData data = displays[0];
                    data.Detected = true;
                    return data;
                default:
                    List<VerificationState> verification;
                    if (VerificationStates.ContainsKey(id)) {
                        verification = VerificationStates[id];
                    } else {
                        verification = VerificationStates[id] = new List<VerificationState>();
                    }
                    if (verification.Any(s => !s.HasDenied)) {
                        VerificationState confirmed = verification.FirstOrDefault(s => s.HasConfirmed);
                        if (confirmed == null) {
                            return new DisplayDetectData() {
                                Token = verification.First().Display.Token
                            };
                        } else {
                            VerificationStates.Remove(id);
                            return new DisplayDetectData() {
                                Detected = true,
                                User = confirmed.Display.User,
                                Display = confirmed.Display.Display,
                                Authority = confirmed.Display.Authority
                            };
                        }
                    } else {
                        verification.Clear();
                        string token = new Random().Next(1000000).ToString().PadLeft(6, '0');
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.FileName = Assembly.GetEntryAssembly().CodeBase;
                        psi.Arguments = string.Concat("confirm ", token);
                        foreach (DisplayDetectData display in displays) {
                            VerificationState state = new VerificationState() {
                                Display = display,
                                SessionId = id,
                                HasConfirmed = false,
                                HasDenied = false
                            };
                            display.Token = token;
                            Process proc = Start(psi, display);
                            proc.EnableRaisingEvents = true;
                            proc.Exited += (sender, e) => {
                                if (proc.ExitCode == 0) {
                                    state.HasConfirmed = true;
                                    foreach (VerificationState s in verification) {
                                        if (!s.Process.HasExited) {
                                            s.Process.Kill();
                                        }
                                    }
                                } else {
                                    state.HasDenied = true;
                                }
                            };
                            state.Process = proc;
                            verification.Add(state);
                        }
                        return new DisplayDetectData() {
                            Token = token
                        };
                    }
            }
        }

        public string FindLatipiumDir(string user) {
            setpwent();
            IntPtr passwdPtr;
            while ((passwdPtr = getpwent()) != IntPtr.Zero) {
                passwd passwd = (passwd) Marshal.PtrToStructure(passwdPtr, typeof(passwd));
                if (Marshal.PtrToStringAuto(passwd.pw_name) == user) {
                    string dir = Path.Combine(Marshal.PtrToStringAuto(passwd.pw_dir), ".latipium");
                    Directory.CreateDirectory(dir);
                    endpwent();
                    return dir;
                }
            }
            endpwent();
            return null;
        }

        public static bool IsActuallyMac() {
            IntPtr buf = Marshal.AllocHGlobal(8192);
            try {
                uname(buf);
                if (Marshal.PtrToStringAuto(buf) == "Darwin") {
                    return true;
                }
            } finally {
                Marshal.FreeHGlobal(buf);
            }
            return false;
        }

        private static void KillAll(object sender, EventArgs e) {
            foreach (List<VerificationState> states in VerificationStates.Values) {
                foreach (VerificationState state in states) {
                    if (!state.Process.HasExited) {
                        state.Process.Kill();
                    }
                }
            }
        }

        public UnixPlatformProxy() {
            AppDomain.CurrentDomain.DomainUnload += KillAll;
        }
    }
}

