//
// ProcessInformation.cs
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

namespace Com.Latipium.Daemon.Model {
    /// <summary>
    /// Process information.
    /// </summary>
    public class ProcessInformation {
        /// <summary>
        /// The arguments.
        /// </summary>
        public string Arguments;
        /// <summary>
        /// The environmental variables.
        /// </summary>
        public Dictionary<string, string> EnvironmentalVariables;
        /// <summary>
        /// The name of the file.
        /// </summary>
        public string FileName;
        /// <summary>
        /// The working directory.
        /// </summary>
        public string WorkingDirectory;

        /// <param name="info">Info.</param>
        public static implicit operator ProcessStartInfo(ProcessInformation info) {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.Arguments = info.Arguments;
            psi.CreateNoWindow = true;
            psi.EnvironmentVariables.Clear();
            foreach (string varname in info.EnvironmentalVariables.Keys) {
                psi.EnvironmentVariables.Add(varname, info.EnvironmentalVariables[varname]);
            }
            psi.FileName = info.FileName;
            psi.RedirectStandardError = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            psi.WorkingDirectory = info.WorkingDirectory;
            return psi;
        }
    }
}

