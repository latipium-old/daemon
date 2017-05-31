//
// ApiClient.cs
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
using System.Threading;
using System.Threading.Tasks;
using Com.Latipium.Daemon.Api.Model;
using Com.Latipium.Daemon.Platform;

namespace Com.Latipium.Daemon.Model {
    public class ApiClient {
        public static readonly TimeSpan DeletionTimeout = TimeSpan.FromMinutes(5);

        private CancellationTokenSource CancellationTokenSource = null;
        public event Action Deleted;
        public Guid Id = Guid.NewGuid();
        public ClientType Type;
        private DisplayDetectData _Display;
        public DisplayDetectData Display {
            get {
                return _Display;
            }
            set {
                _Display = value;
                LatipiumDir = PlatformFactory.Proxy.FindLatipiumDir(value.User);
            }
        }
        public string LatipiumDir {
            get;
            private set;
        }
        public Dictionary<string, Guid> LoadedModules = new Dictionary<string, Guid>();
        public Dictionary<Guid, ModuleTask> ToDoList = new Dictionary<Guid, ModuleTask>();

        public void TimeOut() {
            if (Deleted != null) {
                Deleted();
            }
        }

        public ApiClient Ping() {
            if (CancellationTokenSource != null) {
                CancellationTokenSource.Cancel();
            }
            CancellationTokenSource = new CancellationTokenSource();
            Task.Delay(DeletionTimeout, CancellationTokenSource.Token).ContinueWith(t => {
                if (!t.IsCanceled) {
                    TimeOut();
                }
            });
            return this;
        }
    }
}

