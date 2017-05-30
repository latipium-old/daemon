using System;
using System.Windows.Forms;

namespace Com.Latipium.Daemon.Platform.Unix {
    public partial class ConfirmDialog : Form {
        public string Token {
            get {
                return tokenLbl.Text;
            }
            set {
                if (value.Length > 4 && value.Length % 2 == 0) {
                    tokenLbl.Text = string.Concat(value.Substring(0, value.Length / 2), " ", value.Substring(value.Length / 2));
                } else {
                    tokenLbl.Text = value;
                }
            }
        }

        public ConfirmDialog() {
            InitializeComponent();
        }

        private void BtnClicked(object sender, EventArgs e) {
            Environment.ExitCode = sender == yesBtn ? 0 : 1;
            Application.Exit();
        }
    }
}
