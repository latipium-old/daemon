//
// Structs.cs
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

namespace Com.Latipium.Daemon.Platform.Unix {
    internal partial class Native {
#pragma warning disable 0649
        protected struct exit_status {
            public short e_termination;
            public short e_exit;
        }

        protected struct timeval {
            public int tv_sec;
            public int tv_usec;
        }

        protected struct utmpx {
            public short ut_type;
            public int ut_pid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = UT_LINESIZE)]
            public char[] ut_line;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public char[] ut_id;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = UT_NAMESIZE)]
            public char[] ut_user;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = UT_HOSTSIZE)]
            public char[] ut_host;
            public exit_status ut_exit;
            public int ut_session;
            public timeval ut_tv;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public int[] ut_addr_v6;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public char[] __unused;
        }
#pragma warning restore 0649
    }
}

