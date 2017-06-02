//
// ModuleServiceApi.cs
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
using System.IO;
using System.Threading;
using Com.Latipium.Daemon.Api.Model;
using Com.Latipium.Daemon.Model;

namespace Com.Latipium.Daemon.Apis {
    public class ModuleServiceApi : AbstractApi<ModuleServiceRequest, ModuleServiceResponse> {
        private static readonly TimeSpan RequestTimeout = TimeSpan.FromMinutes(1);

        public override ModuleServiceResponse Handle(ModuleServiceRequest req, ApiClient client) {
            if (client == null || client.Display == null || !Directory.Exists(client.LatipiumDir)) {
                throw new ClientException("Client is not authenticated");
            }
            if (client.LoadedModules.ContainsKey(req.ModuleId)) {
                ApiClient moduleClient = Server.GetClient(client.LoadedModules[req.ModuleId]);
                Guid workId = Guid.NewGuid();
                ModuleTask task = moduleClient.ToDoList[workId] = new ModuleTask() {
                    Request = req
                };
                Thread thread = Thread.CurrentThread;
                string result = null;
                task.Result += res => {
                    result = res;
                    thread.Interrupt();
                };
                try {
                    Thread.Sleep(RequestTimeout);
                    throw new ModuleException("Request timed out");
                } catch (ThreadInterruptedException) {
                }
                return new ModuleServiceResponse() {
                    ModuleResult = result
                };
            } else {
                throw new ClientException("Module is not loaded");
            }
        }

        public ModuleServiceApi() : base("/module/call") {
        }
    }
}

