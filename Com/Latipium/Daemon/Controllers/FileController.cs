//
// FileController.cs
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
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Com.Latipium.Daemon.Model;

namespace Com.Latipium.Daemon.Controllers {
    /// <summary>
    /// File controller.
    /// </summary>
    public class FileController : ApiController {
        /// <summary>
        /// Performs the get request.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public FileObject Get(string id) {
            Request.Check();
            FileObject obj = new FileObject(id.ExpandParameter());
            if (Request.RequestUri.Query.Contains("empty")) {
                obj.Contents = null;
            }
            return obj;
        }

        /// <summary>
        /// Performs the post request.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public FileObject Post(string id) {
            Request.Check();
            Task<string> task = Request.Content.ReadAsStringAsync();
            task.Wait();
            WebRequest req = WebRequest.Create(task.Result);
            if (req is HttpWebRequest) {
                HttpWebRequest http = (HttpWebRequest) req;
                http.AllowAutoRedirect = true;
                http.UserAgent = "Latipium Launcher Daemon (https://github.com/latipium/daemon)";
            }
            using (WebResponse res = req.GetResponse()) {
                using (Stream net = res.GetResponseStream()) {
                    using (FileStream file = new FileStream(id.ExpandParameter(), FileMode.Create, FileAccess.Write)) {
                        net.CopyTo(file);
                    }
                }
            }
            FileObject obj = Get(id);
            obj.Contents = null;
            return obj;
        }

        /// <summary>
        /// Performs the put request.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public FileObject Put(string id) {
            Request.Check();
            using (FileStream stream = File.Open(id.ExpandParameter(), FileMode.Create, FileAccess.Write)) {
                Request.Content.CopyToAsync(stream).Wait();
            }
            FileObject obj = Get(id);
            obj.Contents = null;
            return obj;
        }

        /// <summary>
        /// Performs the delete request.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public FileObject Delete(string id) {
            Request.Check();
            File.Delete(id.ExpandParameter());
            return Get(id);
        }
    }
}

