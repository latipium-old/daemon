//
// UnixPlatformProxy.cs
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
using System.Runtime.InteropServices;
using Com.Latipium.Daemon.Model;

namespace Com.Latipium.Daemon.Platform.Unix {
    internal class UnixPlatformProxy : Native, IPlatformProxy {
        public DisplayDetectData DetectDisplay(string id) {
            setutxent();
            IntPtr utxp;
            List<DisplayDetectData> displays = new List<DisplayDetectData>();
            while ((utxp = getutxent()) != IntPtr.Zero) {
                utmpx utx = (utmpx) Marshal.PtrToStructure(utxp, typeof(utmpx));
                if (utx.ut_type == USER_PROCESS && utx.ut_host[0] != 0) {
                    displays.Add(new DisplayDetectData() {
                        User = new string(utx.ut_user),
                        Display = new string(utx.ut_host)
                    });
                }
            }
            endutxent();
            switch (displays.Count) {
                case 0:
                    return new DisplayDetectData();
                case 1:
                    DisplayDetectData data = displays[0];
                    data.Detected = true;
                    return data;
                default:
                    // TODO
                    return new DisplayDetectData();
            }
        }
    }
}

