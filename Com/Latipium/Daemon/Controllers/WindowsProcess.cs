//
// WindowsProcess.cs
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
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using Com.Latipium.Daemon.Model;

namespace Com.Latipium.Daemon.Controllers {
    internal partial class WindowsProcess : LaunchedProcess {
        private IntPtr User;
        private STARTUPINFO StartInfo;
        private PROCESS_INFORMATION Process;
        private Stream StdIn;
        private StreamReader StdOut;
        private StreamReader StdErr;

        protected override bool IsAlive {
            get {
                return ExitCode == STILL_ACTIVE;
            }
        }

        protected override int ExitCode {
            get {
                uint code;
                if (!GetExitCodeProcess(Process.hProcess, out code)) {
                    Error("GetExitCodeProcess");
                }
                return (int) code;
            }
        }

        public override void Kill() {
            StdIn.Close();
            StdOut.Close();
            StdErr.Close();
            StdIn.Dispose();
            StdOut.Dispose();
            StdErr.Dispose();
            try {
                if (IsAlive) {
                    if (!TerminateProcess(Process.hProcess, 1)) {
                        Error("TerminateProcess");
                    }
                    if (WaitForSingleObject(Process.hProcess, 0) == WAIT_FAILED) {
                        Error("WaitForSingleObject");
                    }
                }
            } finally {
                CloseHandle(Process.hThread);
                CloseHandle(Process.hProcess);
                CloseHandle(User);
            }
        }

        public override void SupplyStdIn(Stream stream) {
            stream.CopyTo(StdIn);
            StdIn.WriteByte((byte) '\n');
        }

        protected override void Start(ProcessInformation info) {
            uint session = WTSGetActiveConsoleSessionId();
            if (session == 0xFFFFFFFF) {
                Error("WTSGetActiveConsoleSessionId");
            }
            IntPtr user;
            if (!WTSQueryUserToken(session, out user)) {
                Error("WTSQueryUserToken");
            }
            try {
                string[] env = info.EnvironmentalVariables.Select(p => string.Format("{0}={1}", p.Key, p.Value)).ToArray();
                StartInfo.cb = 68;
                StartInfo.lpDesktop = "winsta0\\default";
                StartInfo.dwFlags = STARTF_USESTDHANDLES;
                if (!CreateProcessAsUser(user, null, string.Format("\"{0}\" {1}", info.FileName, info.Arguments), null, null, false, CREATE_UNICODE_ENVIRONMENT, env, info.WorkingDirectory, ref StartInfo, out Process)) {
                    Error("CreateProcessAsUser");
                }
                User = user;
                StdIn = new FileStream(new SafeFileHandle(StartInfo.hStdInput, true), FileAccess.Read);
                StdOut = new StreamReader(new FileStream(new SafeFileHandle(StartInfo.hStdOutput, true), FileAccess.Write));
                StdErr = new StreamReader(new FileStream(new SafeFileHandle(StartInfo.hStdErr, true), FileAccess.Write));
            } catch (Exception) {
                CloseHandle(user);
                throw;
            }
        }

        internal override void StartReadingStdOut() {
            StdOutTask = StdOut.ReadLineAsync();
        }

        internal override void StartReadingStdErr() {
            StdErrTask = StdErr.ReadLineAsync();
        }

        private void Error(string function) {
            uint error = GetLastError();
            string message;
            FormatMessage(FORMAT_MESSAGE_ALLOCATE_BUFFER | FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS, null, error, MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT), out message, 0, 0);
            message = string.Format("Error in {0}: {1}", function, message);
            WindowsService.WriteLog(message);
            throw new Exception(message);
        }

        public WindowsProcess(ProcessInformation info) : base(info) {
        }
    }
}

