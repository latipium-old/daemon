//
// Entry.cs
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
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using Com.Latipium.Daemon.Platform.Unix;

namespace Com.Latipium.Daemon {
    public static class Entry {
        private static bool ServiceSwitch(string[] args) {
            switch (args.FirstOrDefault()) {
                case "confirm":
                    if (args.Length != 2) {
                        Console.Error.WriteLine("Usage: Com.Latipium.Daemon.exe confirm [token]");
                        Environment.Exit(1);
                    } else {
                        ConfirmDialog dialog = new ConfirmDialog();
                        dialog.Token = args[1];
                        Application.Run(dialog);
                    }
                    break;
                default:
                    return false;
            }
            return true;
        }

        private static bool DebugSwitch(string[] args) {
            switch (args.FirstOrDefault()) {
                case "run":
                    using (DaemonWebServer server = new DaemonWebServer()) {
                        server.Start();
                        Console.WriteLine("Application started.");
                        Console.WriteLine("Press any key to stop the server");
                        try {
                            ConsoleKeyInfo key;
                            do {
                                key = Console.ReadKey(true);
                            } while (key.Key == 0 && key.KeyChar == '\0');
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
                    break;
                default:
                    return false;
            }
            return true;
        }

        private static void PrintHelp() {
            Console.WriteLine("Usage: Com.Latipium.Daemon.exe [command]");
            Console.WriteLine("Available commands:");
            Console.WriteLine("  confirm  Opens a confirmation prompt");
            Console.WriteLine("  run      Runs the daemon in the foreground");
        }

        public static void Main(string[] args) {
            if (Environment.GetEnvironmentVariable("LATIPIUM_DEBUG") == "true") {
                if (!DebugSwitch(args) && !ServiceSwitch(args)) {
                    PrintHelp();
                }
            } else {
                Console.WriteLine("Try running with the LATIPIUM_DEBUG=true");
                if (args.Length > 0) {
                    if (!ServiceSwitch(args)) {
                        Console.Error.WriteLine("Unknown command");
                        Environment.Exit(1);
                    }
                } else {
                    ServiceBase.Run(new WindowsService());
                }
            }
        }
    }
}

