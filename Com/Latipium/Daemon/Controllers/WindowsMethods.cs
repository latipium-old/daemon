//
// WindowsApis.cs
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

namespace Com.Latipium.Daemon.Controllers {
    partial class WindowsProcess {
        [DllImport("Kernel32.dll")]
        private static extern uint WTSGetActiveConsoleSessionId();

        [DllImport("Kernel32.dll")]
        private static extern uint GetLastError();

        [DllImport("Kernel32.dll")]
        private static extern uint FormatMessage(uint dwFlags, object lpSource, uint dwMessageId, uint dwLanguageId, [MarshalAs(UnmanagedType.LPTStr), Out] out string lpBuffer, uint nSize, uint Arguments);

        [DllImport("Wtsapi32.dll")]
        private static extern bool WTSQueryUserToken(uint SessionId, [Out] out IntPtr phToken);

        [DllImport("Kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("Advapi32.dll")]
        private static extern bool CreateProcessAsUser(IntPtr hToken, string lpApplicationName, [MarshalAs(UnmanagedType.LPTStr)] string lpCommandLine, Object lpProcessAttributes, Object lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, string[] lpEnvironment, [MarshalAs(UnmanagedType.LPTStr)] string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, [Out] out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("Kernel32.dll")]
        private static extern bool GetExitCodeProcess(IntPtr hProcess, [Out] out uint lpExitCode);

        [DllImport("Kernel32.dll")]
        private static extern bool TerminateProcess(IntPtr hProcess, uint uExitCode);

        [DllImport("Kernel32.dll")]
        private static extern uint WaitForSingleObject(IntPtr hProcess, uint dwMilliseconds);
    }
}

