//
// Constants.cs
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

namespace Com.Latipium.Daemon.Platform.Windows {
    internal partial class Native {
        public const uint FORMAT_MESSAGE_ALLOCATE_BUFFER = 0x00000100;
        public const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
        public const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;
        public const ushort LANG_NEUTRAL = 0x00;
        public const ushort SUBLANG_DEFAULT = 0x01;
        public const int DNLEN = 15;
        public const int UNLEN = 256;
        public const int ERROR_INSUFFICIENT_BUFFER = 122;
        public const uint S_OK = 0x00000000;
        public const uint E_FAIL = 0x80004005;
        public const uint E_INVALIDARG = 0x80070057;
        public const uint STARTF_USESHOWWINDOW = 0x00000001;
        public const ushort SW_HIDE = 0;
        public const ushort SW_MAXIMIZE = 3;
        public const ushort SW_MINIMIZE = 6;
        public const ushort SW_SHOWNORMAL = 1;
        public const uint CREATE_NO_WINDOW = 0x08000000;
        public const uint CREATE_UNICODE_ENVIRONMENT = 0x00000400;
    }
}
