namespace Com.Latipium.Daemon.Platform.Unix {
    partial class ConfirmDialog {
        private System.ComponentModel.IContainer components = null;
        
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.Windows.Forms.TableLayoutPanel layout;
            System.Windows.Forms.Label descLbl;
            this.tokenLbl = new System.Windows.Forms.Label();
            this.noBtn = new System.Windows.Forms.Button();
            this.yesBtn = new System.Windows.Forms.Button();
            layout = new System.Windows.Forms.TableLayoutPanel();
            descLbl = new System.Windows.Forms.Label();
            layout.SuspendLayout();
            this.SuspendLayout();
            // 
            // layout
            // 
            layout.ColumnCount = 2;
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            layout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            layout.Controls.Add(descLbl, 0, 0);
            layout.Controls.Add(this.tokenLbl, 0, 1);
            layout.Controls.Add(this.noBtn, 0, 2);
            layout.Controls.Add(this.yesBtn, 1, 2);
            layout.Dock = System.Windows.Forms.DockStyle.Fill;
            layout.Location = new System.Drawing.Point(0, 0);
            layout.Name = "layout";
            layout.RowCount = 3;
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            layout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            layout.Size = new System.Drawing.Size(358, 131);
            layout.TabIndex = 0;
            // 
            // descLbl
            // 
            descLbl.AutoSize = true;
            layout.SetColumnSpan(descLbl, 2);
            descLbl.Location = new System.Drawing.Point(3, 0);
            descLbl.Name = "descLbl";
            descLbl.Size = new System.Drawing.Size(347, 26);
            descLbl.TabIndex = 0;
            descLbl.Text = "A user on this computer is starting Latipium.  If you are this user, and the toke" +
    "n below matches, please click \"Yes,\" otherwise click \"No.\"";
            // 
            // tokenLbl
            // 
            this.tokenLbl.AutoSize = true;
            layout.SetColumnSpan(this.tokenLbl, 2);
            this.tokenLbl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tokenLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 32F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tokenLbl.Location = new System.Drawing.Point(3, 43);
            this.tokenLbl.Name = "tokenLbl";
            this.tokenLbl.Size = new System.Drawing.Size(352, 43);
            this.tokenLbl.TabIndex = 1;
            this.tokenLbl.Text = "000 000";
            this.tokenLbl.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // noBtn
            // 
            this.noBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.noBtn.DialogResult = System.Windows.Forms.DialogResult.No;
            this.noBtn.Location = new System.Drawing.Point(3, 105);
            this.noBtn.Name = "noBtn";
            this.noBtn.Size = new System.Drawing.Size(75, 23);
            this.noBtn.TabIndex = 2;
            this.noBtn.Text = "No";
            this.noBtn.UseVisualStyleBackColor = true;
            this.noBtn.Click += new System.EventHandler(this.BtnClicked);
            // 
            // yesBtn
            // 
            this.yesBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.yesBtn.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.yesBtn.Location = new System.Drawing.Point(280, 105);
            this.yesBtn.Name = "yesBtn";
            this.yesBtn.Size = new System.Drawing.Size(75, 23);
            this.yesBtn.TabIndex = 3;
            this.yesBtn.Text = "Yes";
            this.yesBtn.UseVisualStyleBackColor = true;
            this.yesBtn.Click += new System.EventHandler(this.BtnClicked);
            // 
            // ConfirmDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.noBtn;
            this.ClientSize = new System.Drawing.Size(358, 131);
            this.ControlBox = false;
            this.Controls.Add(layout);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfirmDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Latipium Launcher";
            this.TopMost = true;
            layout.ResumeLayout(false);
            layout.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label tokenLbl;
        private System.Windows.Forms.Button noBtn;
        private System.Windows.Forms.Button yesBtn;
    }
}
