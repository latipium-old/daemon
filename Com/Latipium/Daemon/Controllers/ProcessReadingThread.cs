//
// ProcessReadingThread.cs
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Com.Latipium.Daemon.Controllers {
    /// <summary>
    /// The thread that reads the process stdout and stderr.
    /// </summary>
    public static class ProcessReadingThread {
        private static Thread Thread;

        private static void Run() {
            while (true) {
                try {
                    LaunchedProcess[] procs;
                    lock (ProcessController.Processes) {
                        procs = ProcessController.Processes.Values.ToArray();
                    }
                    foreach (LaunchedProcess proc in procs) {
                        if (proc.StdOutTask.IsCompleted) {
                            if (proc.StdOutTask.Result != null) {
                                Console.WriteLine(proc.StdOutTask.Result);
                                lock (proc.StdOutLines) {
                                    proc.StdOutLines.Add(proc.StdOutTask.Result);
                                }
                            }
                            proc.StdOutTask = proc.StdOut.ReadLineAsync();
                        }
                        if (proc.StdErrTask.IsCompleted) {
                            if (proc.StdErrTask.Result != null) {
                                Console.Error.WriteLine(proc.StdErrTask.Result);
                                lock (proc.StdErrLines) {
                                    proc.StdErrLines.Add(proc.StdErrTask.Result);
                                }
                            }
                            proc.StdErrTask = proc.StdErr.ReadLineAsync();
                        }
                    }
                    Thread.Sleep(100);
                } catch (ThreadInterruptedException) {
                    break;
                } catch (Exception ex) {
                    Console.Error.WriteLine(ex);
                }
            }
        }

        internal static void Init() {
            Thread = new Thread(Run);
            Thread.Start();
        }

        internal static void Stop() {
            Thread.Interrupt();
        }
    }
}

