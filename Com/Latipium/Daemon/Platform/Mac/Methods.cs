//
// Methods.cs
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
using System.Runtime.InteropServices;

namespace Com.Latipium.Daemon.Platform.Mac {
    internal partial class Native {
        [DllImport("/System/Library/Frameworks/Foundation.framework/Foundation")]
        public static extern IntPtr NSHomeDirectoryForUser(IntPtr userName);

        [DllImport("/usr/lib/libobjc.dylib")]
        public static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport("/usr/lib/libobjc.dylib")]
        public static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, string arg1);

        [DllImport("/usr/lib/libobjc.dylib")]
        public static extern IntPtr objc_msgSend(IntPtr receiver, IntPtr selector, uint arg1);

        [DllImport("/usr/lib/libobjc.dylib")]
        public static extern IntPtr objc_getClass(string name);

        [DllImport("/usr/lib/libobjc.dylib")]
        public static extern IntPtr sel_registerName(string name);

        [DllImport("libc")]
        public static extern IntPtr getpwnam(string name);

        public static readonly IntPtr stringWithUTF8String = sel_registerName("stringWithUTF8String:");
        public static readonly IntPtr cStringUsingEncoding = sel_registerName("cStringUsingEncoding:");
        public static readonly IntPtr release = sel_registerName("release");
    }
}

