//
// ProcessController.cs
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
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using Com.Latipium.Daemon.Model;

namespace Com.Latipium.Daemon.Controllers {
    /// <summary>
    /// Process controller.
    /// </summary>
    public class ProcessController : ApiController {
        internal static Dictionary<int, LaunchedProcess> Processes;

        /// <summary>
        /// Performs the get request.
        /// </summary>
        public IEnumerable<int> Get() {
            return Processes.Keys;
        }

        /// <summary>
        /// Performs the get request.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public ProcessData Get(int id) {
            if (Processes.ContainsKey(id)) {
                return Processes[id].Data;
            } else {
                return new ProcessData();
            }
        }

        /// <summary>
        /// Performs the post request.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public ProcessData Post(int id) {
            if (Processes.ContainsKey(id)) {
                Task<Stream> task = Request.Content.ReadAsStreamAsync();
                task.Wait();
                Processes[id].SupplyStdIn(task.Result);
                return Processes[id].Data;
            } else {
                return new ProcessData();
            }
        }

        /// <summary>
        /// Performs the put request.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="info">Info.</param>
        public ProcessData Put(int id, ProcessInformation info) {
            if (Processes.ContainsKey(id)) {
                Delete(id);
            }
            LaunchedProcess proc = new LaunchedProcess(info);
            lock (Processes) {
                Processes.Add(id, proc);
            }
            return proc.Data;
        }

        /// <summary>
        /// Performs the delete request.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public ProcessData Delete(int id) {
            lock (Processes) {
                if (Processes.ContainsKey(id)) {
                    Processes[id].Kill();
                    Processes.Remove(id);
                }
            }
            return new ProcessData();
        }

        private static void KillAll(object sender, EventArgs e) {
            lock (Processes) {
                foreach (LaunchedProcess proc in Processes.Values) {
                    proc.Kill();
                }
            }
        }

        static ProcessController() {
            Processes = new Dictionary<int, LaunchedProcess>();
            AppDomain.CurrentDomain.ProcessExit += KillAll;
            ProcessReadingThread.Init();
        }
    }
}

