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
    public class ProcessController : ApiController {
        private static Dictionary<int, LaunchedProcess> Processes;

        public IEnumerable<int> Get() {
            return Processes.Keys;
        }

        public ProcessData Get(int id) {
            if (Processes.ContainsKey(id)) {
                return Processes[id].Data;
            } else {
                return new ProcessData();
            }
        }

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

        public ProcessData Put(int id, ProcessInformation info) {
            if (Processes.ContainsKey(id)) {
                Delete(id);
            }
            LaunchedProcess proc = new LaunchedProcess(info);
            Processes.Add(id, proc);
            return proc.Data;
        }

        public ProcessData Delete(int id) {
            if (Processes.ContainsKey(id)) {
                Processes[id].Kill();
                Processes.Remove(id);
            }
            return new ProcessData();
        }

        private static void KillAll(object sender, EventArgs e) {
            foreach (LaunchedProcess proc in Processes.Values) {
                proc.Kill();
            }
        }

        static ProcessController() {
            Processes = new Dictionary<int, LaunchedProcess>();
            AppDomain.CurrentDomain.ProcessExit += KillAll;
        }
    }
}

