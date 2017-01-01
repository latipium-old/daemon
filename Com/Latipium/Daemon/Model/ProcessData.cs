//
// ProcessData.cs
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

namespace Com.Latipium.Daemon.Model {
    /// <summary>
    /// Process data.
    /// </summary>
    public class ProcessData {
        /// <summary>
        /// The std out.
        /// </summary>
        public string[] StdOut;
        /// <summary>
        /// The std error.
        /// </summary>
        public string[] StdErr;
        /// <summary>
        /// The is running.
        /// </summary>
        public bool IsRunning;
        /// <summary>
        /// The exit code.
        /// </summary>
        public int ExitCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="Com.Latipium.Daemon.Model.ProcessData"/> class.
        /// </summary>
        /// <param name="stdOut">Std out.</param>
        /// <param name="stdErr">Std error.</param>
        /// <param name="isRunning">If set to <c>true</c> is running.</param>
        /// <param name="exitCode">Exit code.</param>
        public ProcessData(string[] stdOut = null, string[] stdErr = null, bool isRunning = false, int exitCode = -65536) {
            StdOut = stdOut;
            StdErr = stdErr;
            IsRunning = isRunning;
            ExitCode = exitCode;
        }
    }
}

