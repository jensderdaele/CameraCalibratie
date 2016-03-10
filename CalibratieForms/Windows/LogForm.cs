using System;
using WeifenLuo.WinFormsUI.Docking;

namespace CalibratieForms.Windows {
    public partial class LogForm : DockContent, ILog {
        public LogForm() {
            InitializeComponent();
        }

        public void WriteLine(string entry) {
            richTextBox1.Invoke((Action)(() => {
                richTextBox1.AppendText(entry + Environment.NewLine);
                //rbLogBox.ScrollToCaret();
            }));
        }
    }
}
