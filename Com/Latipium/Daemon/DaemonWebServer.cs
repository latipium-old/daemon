//
// DaemonWebServer.cs
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
using System.Net;
using System.Reflection;
using Newtonsoft.Json;
using Com.Latipium.Daemon.Apis;
using Com.Latipium.Daemon.Model;

namespace Com.Latipium.Daemon {
    public class DaemonWebServer : IDisposable {
        private const string DefaultUrl = "http://localhost:43475/";
        private HttpListener Listener;
        private Dictionary<string, IApi> Apis;
        private Dictionary<Guid, ApiClient> Clients;

        public void Dispose() {
            Dispose(true);
        }

        protected void Dispose(bool disposing) {
            if (disposing) {
                if (Listener.IsListening) {
                    Listener.Stop();
                }
                Listener.Close();
                ((IDisposable) Listener).Dispose();
            }
        }

        public ApiClient RegisterClient() {
            ApiClient client = new ApiClient();
            client.Deleted += () => Clients.Remove(client.Id);
            Clients[client.Id] = client;
            return client.Ping();
        }

        private string Handle(string url, string request, ApiClient client) {
            ResponseObject result;
            if (Apis.ContainsKey(url)) {
                try {
                    IApi api = Apis[url];
                    result = api.HandleRequest(JsonConvert.DeserializeObject(request, api.RequestType), client);
                } catch (Exception ex) {
                    WindowsService.WriteLog(ex);
                    result = new Error() {
                        Message = ex.Message,
                        Side = Side.Server
                    };
                }
            } else {
                result = new Error() {
                    Message = "API not found",
                    Side = Side.Client
                };
            }
            return JsonConvert.SerializeObject(result);
        }

        private void GetContextCallback(IAsyncResult iar) {
            HttpListenerContext ctx = Listener.EndGetContext(iar);
            Listener.BeginGetContext(GetContextCallback, null);
            if (ctx.Request.IsWebSocketRequest) {
                // TODO
            } else {
                string request;
                using (TextReader reader = new StreamReader(ctx.Request.InputStream)) {
                    request = reader.ReadToEnd();
                }
                Guid clientId = Guid.Empty;
                Guid.TryParse(ctx.Request.Headers["X-Latipium-Client-Id"] ?? "", out clientId);
                string response = Handle(ctx.Request.Url.AbsolutePath, request, Clients.ContainsKey(clientId) ? Clients[clientId].Ping() : null);
                ctx.Response.ContentType = "application/json";
                using (TextWriter writer = new StreamWriter(ctx.Response.OutputStream)) {
                    writer.Write(response);
                }
                ctx.Response.Close();
            }
        }

        public void Start() {
            Apis = Assembly.GetExecutingAssembly().GetTypes()
                .Where(t => !t.IsInterface && !t.IsAbstract && typeof(IApi).IsAssignableFrom(t))
                .Select(t => t.GetConstructor(new Type[0]).Invoke(new object[0]))
                .Cast<IApi>()
                .ToDictionary(a => a.Url);
            foreach (IApi api in Apis.Values) {
                api.Server = this;
            }
            Clients = new Dictionary<Guid, ApiClient>();
            Listener.Start();
            Listener.BeginGetContext(GetContextCallback, null);
        }

        public DaemonWebServer() {
            Listener = new HttpListener();
            Listener.Prefixes.Add(Environment.GetEnvironmentVariable("LATIPIUM_DAEMON_URL") ?? DefaultUrl);
        }

        ~DaemonWebServer() {
            Dispose(false);
        }
    }
}

