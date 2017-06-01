//
// MacPlatformProxy.cs
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
using System.Runtime.InteropServices;
using System.Threading;
using Com.Latipium.Daemon.Api.Model;

namespace Com.Latipium.Daemon.Platform.Mac {
    internal class MacPlatformProxy : Native, IPlatformProxy {
        public DisplayDetectData DetectDisplay(string id) {
            setutxent();
            IntPtr utxp;
            string user = null;
            while ((utxp = getutxent()) != IntPtr.Zero) {
                utmpx utx = (utmpx) Marshal.PtrToStructure(utxp, typeof(utmpx));
                if (utx.ut_type == USER_PROCESS && utx.ut_host[0] == '\0') {
                    user = new string(utx.ut_user, 0, Array.IndexOf(utx.ut_user, '\0'));
                }
            }
            endutxent();
            if (user == null) {
                return new DisplayDetectData();
            } else {
                return new DisplayDetectData() {
                    Detected = true,
                    User = user
                };
            }
        }

        public Process Start(ProcessStartInfo psi, DisplayDetectData display) {
            string oldArgs = psi.Arguments;
            string oldFile = psi.FileName;
            string oldPath = psi.EnvironmentVariables["PATH"];
            psi.Arguments = string.Concat(display.User, " -c \"mono '", psi.FileName, "' ", psi.Arguments.Replace("\\", "\\\\").Replace("\"", "\\\""), "\"");
            psi.FileName = "/usr/bin/su";
            string id = new Random().Next().ToString();
            psi.EnvironmentVariables["LATIPIUM_DAEMON_SPAWN_ID"] = id;
            psi.EnvironmentVariables["MONO_PATH"] = psi.EnvironmentVariables["PATH"] = oldPath.Replace(';', ':');
            psi.UseShellExecute = false;
            Process proc = Process.Start(psi);
            psi.Arguments = oldArgs;
            psi.FileName = oldFile;
            psi.EnvironmentVariables.Remove("LATIPIUM_DAEMON_SPAWN_ID");
            psi.EnvironmentVariables["PATH"] = oldPath;
            psi.EnvironmentVariables.Remove("MONO_PATH");
            proc.EnableRaisingEvents = true;
            proc.Exited += (sender, e) => {
                foreach (Process p in Process.GetProcesses()) {
                    if (p.StartInfo.EnvironmentVariables["LATIPIUM_DAEMON_SPAWN_ID"] == id) {
                        p.Kill();
                    }
                }
            };
            return proc;
        }

        public string FindLatipiumDir(string user) {
            IntPtr username = IntPtr.Zero;
            IntPtr homeDir = IntPtr.Zero;
            string home;
            try {
                username = objc_msgSend(NSString, stringWithUTF8String, user);
                homeDir = NSHomeDirectoryForUser(username);
                home = Marshal.PtrToStringAuto(objc_msgSend(homeDir, cStringUsingEncoding, NSUTF8StringEncoding));
            } finally {
                if (username != IntPtr.Zero) {
                    objc_msgSend(username, release);
                }
                if (homeDir != IntPtr.Zero) {
                    objc_msgSend(homeDir, release);
                }
            }
            IntPtr passwdPtr = getpwnam(user);
            passwd passwd = (passwd) Marshal.PtrToStructure(passwdPtr, typeof(passwd));
            string dir = Path.Combine(home, "Library", "Application Support", "latipium");
            Directory.CreateDirectory(dir);
            chown(dir, passwd.pw_uid, passwd.pw_gid);
            return dir;
        }
    }
}

