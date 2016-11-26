//
// LatipiumLoader.cs
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
using System.Reflection;

namespace Com.Latipium.Daemon.NuGet {
    public class LatipiumLoader {
        public const string EnvKey = "LATIPIUM_ENV_READY";
        public const string ExpectedValue = "true";

        public bool IsEnvironmentReady {
            get {
                return Environment.GetEnvironmentVariable(EnvKey) == ExpectedValue;
            }
        }

        public void FixEnvironmentAndLaunch(NuGetRunner runner, string[] args) {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.Arguments = args.Select(
                s => string.Concat("\"", s, "\""))
                .Aggregate(
                    (a, b) => string.Concat(a, b));
            psi.CreateNoWindow = true;
            psi.EnvironmentVariables.Add(EnvKey, ExpectedValue);
            string PATH = Environment.GetEnvironmentVariable("PATH");
            string MONO_PATH = Environment.GetEnvironmentVariable("MONO_PATH");
            IEnumerable<string> NewPath = runner.InstalledPackages
                .Select(
                    d => Path.Combine(d, "lib"))
                .Where(
                    d => Directory.Exists(d))
                .SelectMany(
                    d => new [] { d }
                    .Concat(Directory.GetDirectories(d)))
                .Concat(string.IsNullOrWhiteSpace(PATH) ? new string[0] : PATH.Split(':', ';'))
                .Concat(string.IsNullOrWhiteSpace(MONO_PATH) ? new string[0] : MONO_PATH.Split(':', ';'))
                .Distinct();
            if (psi.EnvironmentVariables.ContainsKey("PATH")) {
                psi.EnvironmentVariables.Remove("PATH");
            }
            if (psi.EnvironmentVariables.ContainsKey("MONO_PATH")) {
                psi.EnvironmentVariables.Remove("MONO_PATH");
            }
            psi.EnvironmentVariables.Add("PATH", NewPath.Aggregate(
                (a, b) => string.Concat(a, ";", b)));
            psi.EnvironmentVariables.Add("MONO_PATH", NewPath.Aggregate(
                (a, b) => string.Concat(a, ":", b)));
            psi.FileName = Assembly.GetEntryAssembly().Location;
            psi.RedirectStandardError = false;
            psi.RedirectStandardInput = false;
            psi.RedirectStandardOutput = false;
            psi.UseShellExecute = false;
            psi.WorkingDirectory = Environment.CurrentDirectory;
            using (Process proc = Process.Start(psi)) {
                proc.WaitForExit();
                Environment.Exit(proc.ExitCode);
            }
        }
    }
}

