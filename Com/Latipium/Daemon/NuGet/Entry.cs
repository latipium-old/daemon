﻿//
// Entry.cs
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
using System.IO;
using System.Threading;
using Microsoft.Owin.Hosting;

namespace Com.Latipium.Daemon.NuGet {
    /// <summary>
    /// Entry.
    /// </summary>
    public static class Entry {
        private static string Dir {
            get {
                string system = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                if (CheckDirectoryPermissions(system)) {
                    return Path.Combine(system, "latipium");
                } else {
                    Console.WriteLine("Unable to write to {0}; trying next option.", system);
                    string user = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    if (CheckDirectoryPermissions(user)) {
                        return Path.Combine(user, "latipium");
                    } else {
                        Console.WriteLine("Unable to write to {0}; trying next option.", user);
                        string local = ".";
                        if (CheckDirectoryPermissions(local)) {
                            return Path.Combine(local, ".latipium");
                        } else {
                            Console.WriteLine("Unable to write to .; no options left.");
                            Environment.Exit(1);
                            return null;
                        }
                    }
                }
            }
        }

        private static bool CheckDirectoryPermissions(string dir) {
            string file = Path.Combine(dir, ".latipium-test");
            try {
                if (!File.Exists(file)) {
                    using (File.Create(file)) {
                    }
                }
                File.Delete(file);
            } catch ( Exception ) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// The entry point of the program, where the program control starts and ends.
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        public static void Main(string[] args) {
            LatipiumLoader loader = new LatipiumLoader();
            if (loader.IsEnvironmentReady) {
                using (WebApp.Start<WebApiConfig>(args.Length == 0 ? "http://localhost:43475" : args[0])) {
                    Console.WriteLine("Application started.");
                    Console.WriteLine("Press any key to stop the server");
                    try {
                        Console.ReadKey(true);
                    } catch (Exception) {
                        try {
                            while (true) {
                                Thread.Sleep(int.MaxValue);
                            }
                        } catch (Exception ex) {
                            Console.Error.WriteLine(ex);
                        }
                    }
                    Console.WriteLine("Shutting down server...");
                }
            } else {
                string dir = Dir;
                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
                Console.WriteLine("Initialized in {0}", dir);
                NuGetRunner nuget = new NuGetRunner(dir);
                if (!nuget.IsDownloaded) {
                    nuget.Download();
                }
                if (!nuget.IsInstalled) {
                    nuget.Install();
                }
                loader.FixEnvironmentAndLaunch(nuget, args);
            }
        }
    }
}
