//
// CorsHandler.cs
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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Com.Latipium.Daemon {
    public class CorsHandler : DelegatingHandler {
        private static readonly string[] AuthorizedOrigins = new [] {
            "https://latipium.com",
            "https://www.latipium.com",
            "http://localhost",
            "http://localhost:4000"
        };
        private static readonly string[] AuthorizedHeaders = new [] {
            "DNT",
            "Keep-Alive",
            "User-Agent",
            "X-Requested-With",
            "If-Modified-Since",
            "Cache-Control",
            "Content-Type"
        };
        private static readonly string[] AuthorizedMethods = new [] {
            "DELETE",
            "GET",
            "OPTIONS",
            "POST",
            "PUT"
        };

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            return Task.Run(() => {
                HttpResponseMessage res;
                if (request.Method == HttpMethod.Options) {
                    res = new HttpResponseMessage(HttpStatusCode.OK);
                } else {
                    Task<HttpResponseMessage> task = base.SendAsync(request, cancellationToken);
                    task.Wait();
                    res = task.Result;
                }
                if (request.Headers.Contains("Origin")) {
                    string origin = request.Headers.GetValues("Origin").First();
                    if (AuthorizedOrigins.Contains(origin)) {
                        res.Headers.Add("Access-Control-Allow-Origin", origin);
                    }
                }
                if (request.Headers.Contains("Access-Control-Request-Headers")) {
                    res.Headers.Add("Access-Control-Allow-Headers", request.Headers.GetValues("Access-Control-Request-Headers")
                                            .SelectMany(
                                                s => s.Split(new [] {
                            ", ",
                            ","
                        }, StringSplitOptions.RemoveEmptyEntries))
                                            .Where(
                                                s => AuthorizedHeaders.Contains(s))
                                            .Aggregate(
                                                (a, b) => string.Concat(a, ",", b)));
                }
                if (request.Headers.Contains("Access-Control-Request-Methods")) {
                    res.Headers.Add("Access-Control-Allow-Methods", request.Headers.GetValues("Access-Control-Request-Methods")
                                            .SelectMany(
                                                s => s.Split(new [] {
                            ", ",
                            ","
                        }, StringSplitOptions.RemoveEmptyEntries))
                                            .Where(
                                                s => AuthorizedMethods.Contains(s))
                                            .Aggregate(
                                                (a, b) => string.Concat(a, ",", b)));
                }
                res.Headers.Add("Access-Control-Max-Age", "600");
                return res;
            }, cancellationToken);
        }
    }
}

