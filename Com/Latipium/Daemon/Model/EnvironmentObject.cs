//
// EnvironmentObject.cs
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
using System.Collections;
using System.Collections.Generic;

namespace Com.Latipium.Daemon.Model {
    public class EnvironmentObject {
        public string OS;
        public IDictionary Variables;
        public bool Is64Bit;
        public Dictionary<string, string> SpecialFolders;

        public EnvironmentObject() {
            try {
            switch (Environment.OSVersion.Platform) {
                case PlatformID.MacOSX:
                    OS = "mac";
                    break;
                case PlatformID.Unix:
                    OS = "linux";
                    break;
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                case PlatformID.Xbox:
                    OS = "windows";
                    break;
            }
            Variables = Environment.GetEnvironmentVariables();
            Is64Bit = Environment.Is64BitOperatingSystem;
            SpecialFolders = new Dictionary<string, string>();
            foreach (Environment.SpecialFolder folder in Enum.GetValues(typeof(Environment.SpecialFolder))) {
                if (!SpecialFolders.ContainsKey(folder.ToString())) {
                    SpecialFolders.Add(folder.ToString(), Environment.GetFolderPath(folder));
                }
            }
            } catch ( Exception ex ) {
                Console.WriteLine(ex);
                throw ex;
            }
        }
    }
}

