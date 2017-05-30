//
// AbstractApi.cs
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
using System.Net;
using Com.Latipium.Daemon.Model;

namespace Com.Latipium.Daemon.Apis {
    public abstract class AbstractApi<TRequest, TResponse> : IApi where TResponse : ResponseObject {
        public string Url {
            get;
            private set;
        }

        public Type RequestType {
            get {
                return typeof(TRequest);
            }
        }

        public Type ResponseType {
            get {
                return typeof(TResponse);
            }
        }

        public abstract TResponse Handle(TRequest req, ApiClient client);

        public ResponseObject _Handle(object req, ApiClient client) {
            return Handle((TRequest) req, client);
        }

        protected AbstractApi(string url) {
            Url = url;
        }
    }
}

