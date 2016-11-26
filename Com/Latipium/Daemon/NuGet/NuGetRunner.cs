//
// NuGetRunner.cs
//
// Author:
//       Zach Deibert <zachdeibert@gmail.com>
//
// Copyright (c) 2016 Zach Deibert
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
using System.Net;

namespace Com.Latipium.Daemon.NuGet {
    public class NuGetRunner {
        private static readonly string[] Packages = new [] {
            "Microsoft.AspNet.WebApi.Client:5.2.3",
            "Microsoft.AspNet.WebApi.Core:5.2.3",
            "Microsoft.AspNet.WebApi.Owin:5.2.3",
            "Microsoft.AspNet.WebApi.OwinSelfHost:5.2.3",
            "Microsoft.Bcl:1.1.9",
            "Microsoft.Bcl.Build:1.0.14",
            "Microsoft.Net.Http:2.2.22",
            "Microsoft.Owin:2.0.2",
            "Microsoft.Owin.Host.HttpListener:2.0.2",
            "Microsoft.Owin.Hosting:2.0.2",
            "Newtonsoft.Json:6.0.4",
            "Owin:1.0"
        };
        private string Dir;
        private string Executable;

        public bool IsDownloaded {
            get {
                return File.Exists(Executable);
            }
        }

        public bool IsInstalled {
            get {
                foreach (string pkg in Packages) {
                    if (!PackageIsInstalled(pkg)) {
                        return false;
                    }
                }
                return true;
            }
        }

        public IEnumerable<string> InstalledPackages {
            get {
                return Packages.Where(
                    p => PackageIsInstalled(p))
                        .Select(
                            s => Path.Combine(Dir, s.Replace(':', '.')));
            }
        }

        public void Download() {
            HttpWebRequest req = WebRequest.CreateHttp("https://nuget.org/nuget.exe");
            req.AllowAutoRedirect = true;
            req.UserAgent = "Latipium Launcher Daemon (https://github.com/latipium/daemon)";
            using (WebResponse res = req.GetResponse()) {
                using (Stream net = res.GetResponseStream()) {
                    using (FileStream file = new FileStream(Executable, FileMode.Create, FileAccess.Write)) {
                        net.CopyTo(file);
                    }
                }
            }
        }

        public bool Install(string pkg, string version) {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.Arguments = string.Format("install {0} -version {1}", pkg, version);
            psi.CreateNoWindow = true;
            psi.FileName = Executable;
            psi.UseShellExecute = true;
            psi.WorkingDirectory = Dir;
            using (Process proc = Process.Start(psi)) {
                proc.WaitForExit();
                return proc.ExitCode == 0;
            }
        }

        public bool Install(string pkg) {
            string[] parts = pkg.Split(new [] { ':' }, 2);
            return Install(parts[0], parts[1]);
        }

        public bool PackageIsInstalled(string pkg) {
            return Directory.Exists(Path.Combine(Dir, pkg.Replace(':', '.')));
        }

        public bool Install() {
            bool success = true;
            foreach (string pkg in Packages) {
                if (!PackageIsInstalled(pkg)) {
                    success = Install(pkg) && success;
                }
            }
            return success;
        }

        public NuGetRunner(string path) {
            Dir = path;
            Executable = Path.Combine(path, "nuget.exe");
        }
    }
}

