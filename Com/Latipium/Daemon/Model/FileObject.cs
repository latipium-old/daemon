//
// FileObject.cs
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

namespace Com.Latipium.Daemon.Model {
    /// <summary>
    /// File object.
    /// </summary>
    public class FileObject {
        /// <summary>
        /// The path.
        /// </summary>
        public string Path;
        /// <summary>
        /// The exists.
        /// </summary>
        public bool Exists;
        /// <summary>
        /// The contents.
        /// </summary>
        public string Contents;

        /// <summary>
        /// Initializes a new instance of the <see cref="Com.Latipium.Daemon.Model.FileObject"/> class.
        /// </summary>
        /// <param name="path">Path.</param>
        public FileObject(string path = null) {
            if (!string.IsNullOrWhiteSpace(Path = path)) {
                if ((Exists = File.Exists(path))) {
                    Contents = File.ReadAllText(path);
                }
            }
        }
    }
}

