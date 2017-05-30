//
// LoadModuleApi.cs
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
using System.Net;
using System.Reflection;
using Com.Latipium.Daemon.Model;
using Com.Latipium.Daemon.Platform;

namespace Com.Latipium.Daemon.Apis {
    public class LoadModuleApi : AbstractApi<LoadModuleRequest, ResponseObject> {
        private const string NuGetDownload = "https://api.nuget.org/downloads/nuget.exe";

        private IEnumerable<Version> GetInstalledVersions(string moduleName, ApiClient client) {
            string prefix = string.Concat(moduleName, ".");
            return Directory.GetDirectories(client.LatipiumDir).Where(d => d.StartsWith(prefix)).Select(s => {
                Version version = null;
                Version.TryParse(s.Substring(prefix.Length), out version);
                return version;
            }).Where(v => v != null);
        }

        public override ResponseObject Handle(LoadModuleRequest req, ApiClient client) {
            if (client == null || client.Display == null || !Directory.Exists(client.LatipiumDir)) {
                throw new ClientException("Client is not authenticated");
            }
            if (req.MinimumVersion == null) {
                req.MinimumVersion = new Version(0, 0, 0, 0);
            }
            if (req.MaximumVersion == null) {
                req.MaximumVersion = new Version(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue);
            }
            IEnumerable<Version> matchingVersions = GetInstalledVersions(req.ModuleName, client).Where(v => v >= req.MinimumVersion && v <= req.MaximumVersion);
            string moduleBin;
            if (matchingVersions.Any()) {
                moduleBin = Path.Combine(client.LatipiumDir, string.Concat(req.ModuleName, ".", matchingVersions.OrderByDescending(v => v).First().ToString()), "bin");
            } else {
                string nuget = Path.Combine(client.LatipiumDir, "nuget.exe");
                if (!File.Exists(nuget)) {
                    using (WebClient wc = new WebClient()) {
                        wc.DownloadFile(NuGetDownload, nuget);
                    }
                }
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.Arguments = string.Concat("install \"", req.ModuleName, "\" -Version \"", req.Version.ToString(), "\"");
                psi.FileName = nuget;
                psi.WorkingDirectory = client.LatipiumDir;
                Process proc = PlatformFactory.Proxy.Start(psi, client.Display);
                proc.WaitForExit();
                if (proc.ExitCode != 0) {
                    throw new ClientException("Module does not exist");
                }
                moduleBin = Path.Combine(client.LatipiumDir, string.Concat(req.ModuleName, ".", req.Version.ToString()), "bin");
            }
            if (!Directory.Exists(moduleBin)) {
                throw new ModuleException("Module does not have a bin folder");
            }
            IEnumerable<string> exes = Directory.GetFiles(moduleBin).Where(f => f.EndsWith(".exe"));
            if (exes.Count() != 1) {
                throw new ModuleException("Ambiguous module executable");
            }
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                ApiClient moduleClient = Server.RegisterClient();
                moduleClient.Display = client.Display;
                moduleClient.LoadedModules = client.LoadedModules;
                psi.Arguments = string.Concat("\"", Server.BaseUrl, "\" ", moduleClient.Id.ToString());
                psi.FileName = exes.First();
                UriBuilder uri = new UriBuilder(Assembly.GetExecutingAssembly().CodeBase);
                psi.WorkingDirectory = Path.GetDirectoryName(Uri.UnescapeDataString(uri.Path));
                Process proc = PlatformFactory.Proxy.Start(psi, client.Display);
                proc.Exited += (sender, e) => {
                    moduleClient.TimeOut();
                    client.LoadedModules.Remove(req.ModuleName);
                };
                moduleClient.Deleted += () => proc.Kill();
                client.LoadedModules[req.ModuleName] = moduleClient.Id;
                return new ResponseObject();
            }
        }

        public LoadModuleApi() : base("/module/load") {
        }
    }
}

