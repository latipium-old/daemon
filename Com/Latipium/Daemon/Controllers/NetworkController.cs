//
// NetworkController.cs
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
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Com.Latipium.Daemon.Controllers {
    /// <summary>
    /// Network controller.
    /// </summary>
    public class NetworkController : ApiController {
        /// <summary>
        /// Performs the get request.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public string Get(string id) {
            using (HttpClient client = new HttpClient()) {
                Task<string> task = client.GetStringAsync(id.ExpandParameter());
                task.Wait();
                return task.Result;
            }
        }

        /// <summary>
        /// Performs the post request.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public string Post(string id) {
            using (HttpClient client = new HttpClient()) {
                Task<HttpResponseMessage> task = client.PostAsync(id.ExpandParameter(), Request.Content);
                task.Wait();
                Task<string> res = task.Result.Content.ReadAsStringAsync();
                res.Wait();
                return res.Result;
            }
        }
    }
}

