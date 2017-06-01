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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Com.Latipium.Daemon.Platform.Windows {
    internal partial class Native {
        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern uint WTSGetActiveConsoleSessionId();

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern uint GetLastError();

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern uint FormatMessage(uint dwFlags, object lpSource, uint dwMessageId, uint dwLanguageId, [Out] out string lpBuffer, uint nSize, uint Arguments);

        [DllImport("Wtsapi32.dll", SetLastError = true)]
        public static extern bool WTSQueryUserToken(uint SessionId, [Out] out IntPtr phToken);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool ProcessIdToSessionId(uint dwProcessId, out uint pSessionId);

        [DllImport("Kernel32.dll", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr hObject);

        [DllImport("Advapi32.dll", SetLastError = true)]
        public static extern bool GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength, [Out] out uint ReturnLength);

        [DllImport("Advapi32.dll", SetLastError = true)]
        public static extern bool LookupAccountSid(string lpSystemName, IntPtr lpSid, StringBuilder lpName, ref uint cchName, StringBuilder lpReferencedDomainName, ref uint cchReferencedDomainName, [Out] out int peUse);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort MAKELANGID(ushort p, ushort s) {
            return (ushort)((s << 10) | p);
        }
    }
}
