using System;
using System.Collections.Generic;
using WeifenLuo.WinFormsUI.Docking;

namespace CalibratieForms.Windows {
    public partial class LogForm : DockContent, ILog {
        public static List<LogForm> AllForms = new List<LogForm>();
        public LogForm() {
            InitializeComponent();
            AllForms.Add(this);
            this.Closed += (s, a) => { AllForms.Remove(this);};
        }

        public void WriteLine(string entry) {
            richTextBox1.Invoke((Action)(() => {
                richTextBox1.AppendText(entry + Environment.NewLine);
                richTextBox1.ScrollToCaret();
            }));
        }
    }
}
