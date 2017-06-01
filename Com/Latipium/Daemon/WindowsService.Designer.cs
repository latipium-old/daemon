//
// WindowsService.Designer.cs
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

namespace Com.Latipium.Daemon {
    partial class WindowsService {
        private System.ComponentModel.IContainer components = null;
        
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        
        private void InitializeComponent() {
            this.Log = new System.Diagnostics.EventLog();
            ((System.ComponentModel.ISupportInitialize)(this.Log)).BeginInit();
            // 
            // Log
            // 
            this.Log.Log = "Application";
            this.Log.Source = "Latipium";
            // 
            // WindowsService
            // 
            this.CanHandleSessionChangeEvent = true;
            this.CanShutdown = true;
            this.ServiceName = "Latipium";
            ((System.ComponentModel.ISupportInitialize)(this.Log)).EndInit();

        }

        #endregion

        private System.Diagnostics.EventLog Log;
    }
}
