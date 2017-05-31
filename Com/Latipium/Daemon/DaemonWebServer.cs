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
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Com.Latipium.Daemon.Api.Model;
using Com.Latipium.Daemon.Apis;
using Com.Latipium.Daemon.Model;

namespace Com.Latipium.Daemon {
    public class DaemonWebServer : IDisposable {
        private const string DefaultUrl = "http://localhost:43475/";
        private const int MaxReceiveSize = 8192;
        private HttpListener Listener;
        private Dictionary<string, IApi> Apis;
        private Dictionary<Guid, ApiClient> Clients;
        private CancellationTokenSource CancellationTokenSource;
        public readonly string BaseUrl;
        public bool IsRunning {
            get;
            private set;
        }

        public void Dispose() {
            Dispose(true);
        }

        protected void Dispose(bool disposing) {
            if (disposing) {
                IsRunning = false;
                if (Listener.IsListening) {
                    Listener.Stop();
                }
                Listener.Close();
                ((IDisposable) Listener).Dispose();
                CancellationTokenSource.Cancel();
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
                } catch (ClientException ex) {
                    WindowsService.WriteLog(ex);
                    result = ex.Error;
                } catch (ModuleException ex) {
                    WindowsService.WriteLog(ex);
                    result = ex.Error;
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

        private void WebsocketRead(Task<WebSocketReceiveResult> task, WebSocket ws, ArraySegment<byte> buffer) {
            switch (task.Result.MessageType) {
                case WebSocketMessageType.Binary:
                    ws.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Binary messages are not supported", CancellationTokenSource.Token);
                    break;
                case WebSocketMessageType.Text:
                    if (task.Result.EndOfMessage) {
                        string message = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, task.Result.Count);
                        WebSocketRequest req = JsonConvert.DeserializeObject<WebSocketRequest>(message);
                        ApiClient client = Clients.ContainsKey(req.ClientId) ? Clients[req.ClientId].Ping() : null;
                        WebSocketResponse res = new WebSocketResponse() {
                            Responses = new string[req.Tasks.Length]
                        };
                        for (int i = 0; i < req.Tasks.Length; ++i) {
                            res.Responses[i] = Handle(req.Tasks[i].Url, req.Tasks[i].Request, client);
                        }
                        ArraySegment<byte> sendBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(res)));
                        ws.SendAsync(sendBuffer, WebSocketMessageType.Text, true, CancellationTokenSource.Token).ContinueWith(t => {
                            if (!t.IsCanceled && !t.IsFaulted) {
                                ws.ReceiveAsync(buffer, CancellationTokenSource.Token).ContinueWith(tsk => WebsocketRead(tsk, ws, buffer));
                            }
                        });
                    } else {
                        ws.CloseAsync(WebSocketCloseStatus.MessageTooBig, "Message too big", CancellationTokenSource.Token);
                    }
                    break;
                case WebSocketMessageType.Close:
                    ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed", CancellationTokenSource.Token);
                    break;
            }
        }

        private async void GetContextCallback(IAsyncResult iar) {
            if (IsRunning) {
                HttpListenerContext ctx = Listener.EndGetContext(iar);
                Listener.BeginGetContext(GetContextCallback, null);
                if (ctx.Request.IsWebSocketRequest) {
                    HttpListenerWebSocketContext wsctx = await ctx.AcceptWebSocketAsync("latipium");
                    ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[MaxReceiveSize]);
#pragma warning disable 4014
                    wsctx.WebSocket.ReceiveAsync(buffer, CancellationTokenSource.Token).ContinueWith(task => WebsocketRead(task, wsctx.WebSocket, buffer));
#pragma warning restore 4014
                } else {
                    string request;
                    using (TextReader reader = new StreamReader(ctx.Request.InputStream)) {
                        request = reader.ReadToEnd();
                    }
                    Guid clientId = Guid.Empty;
                    Guid.TryParse(ctx.Request.Headers["X-Latipium-Client-Id"] ?? "", out clientId);
                    string response = Handle(ctx.Request.Url.AbsolutePath.Replace("//", "/"), request, Clients.ContainsKey(clientId) ? Clients[clientId].Ping() : null);
                    ctx.Response.ContentType = "application/json";
                    using (TextWriter writer = new StreamWriter(ctx.Response.OutputStream)) {
                        writer.Write(response);
                    }
                    ctx.Response.Close();
                }
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
            CancellationTokenSource = new CancellationTokenSource();
            Listener.Start();
            Listener.BeginGetContext(GetContextCallback, null);
            IsRunning = true;
        }

        public DaemonWebServer() {
            Listener = new HttpListener();
            Listener.Prefixes.Add(BaseUrl = Environment.GetEnvironmentVariable("LATIPIUM_DAEMON_URL") ?? DefaultUrl);
        }

        ~DaemonWebServer() {
            Dispose(false);
        }
    }
}

