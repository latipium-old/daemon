//
// FinishModuleWorkApi.cs
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
using Com.Latipium.Daemon.Api.Model;
using Com.Latipium.Daemon.Model;

namespace Com.Latipium.Daemon.Apis {
    public class FinishModuleWorkApi : AbstractApi<ModuleResults, ResponseObject> {
        public override ResponseObject Handle(ModuleResults req, ApiClient client) {
            if (client == null || client.Display == null || !Directory.Exists(client.LatipiumDir)) {
                throw new ClientException("Client is not authenticated");
            }
            if (client.Type != ClientType.Module) {
                throw new ClientException("Invalid client type");
            }
            foreach (KeyValuePair<Guid, string> result in req.Results) {
                ModuleTask task = client.ToDoList.Where(p => p.Key == result.Key).Select(p => p.Value).FirstOrDefault();
                if (task == null) {
                    throw new ModuleException("Unknown task");
                } else {
                    task.OnResult(result.Value);
                }
            }
            return new ResponseObject();
        }

        public FinishModuleWorkApi() : base("/module/work/finish") {
        }
    }
}

