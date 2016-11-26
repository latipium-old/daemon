//
// LaunchedProcess.cs
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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Com.Latipium.Daemon.Model;

namespace Com.Latipium.Daemon.Controllers {
    public class LaunchedProcess {
        private Process Proc;
        private Stream StdIn;
        private StreamReader StdOut;
        private StreamReader StdErr;
        private Task<string> StdOutLine;
        private Task<string> StdErrLine;

        public ProcessData Data {
            get {
                return GetData(true);
            }
        }

        private ProcessData GetData(bool canWait) {
            string stdOut;
            if (StdOutLine.IsCompleted) {
                stdOut = StdOutLine.Result;
                StdOutLine = StdOut.ReadLineAsync();
            } else {
                stdOut = null;
            }
            string stdErr;
            if (StdErrLine.IsCompleted) {
                stdErr = StdErrLine.Result;
                StdErrLine = StdErr.ReadLineAsync();
            } else {
                stdErr = null;
            }
            bool isRunning = !Proc.HasExited;
            if (canWait && isRunning && stdOut == null && stdErr == null) {
                Task.WaitAny(new [] { StdOutLine, StdErrLine }, 1000);
                return GetData(false);
            }
            int exitCode;
            if (isRunning) {
                exitCode = -65537;
            } else {
                exitCode = Proc.ExitCode;
            }
            return new ProcessData(stdOut, stdErr, isRunning, exitCode);
        }

        public void Kill() {
            StdIn.Close();
            StdOut.Close();
            StdErr.Close();
            StdIn.Dispose();
            StdOut.Dispose();
            StdErr.Dispose();
            if (!Proc.HasExited) {
                Proc.Kill();
            }
            Proc.Dispose();
        }

        public void SupplyStdIn(Stream stream) {
            stream.CopyTo(StdIn);
            StdIn.WriteByte((byte) '\n');
        }

        public LaunchedProcess(ProcessInformation info) {
            Proc = Process.Start(info);
            StdIn = Proc.StandardInput.BaseStream;
            StdOut = Proc.StandardOutput;
            StdErr = Proc.StandardError;
            StdOutLine = StdOut.ReadLineAsync();
            StdErrLine = StdErr.ReadLineAsync();
        }
    }
}

