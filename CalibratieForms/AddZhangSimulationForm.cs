﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CalibratieForms {
    public partial class AddZhangSimulationForm : Form {
        public AddZhangSimulationForm() {
            InitializeComponent();
        }
    }

    [AttributeUsage(AttributeTargets.Delegate)]
    public class ChessAngleDelegate : System.Attribute {
        public readonly string UIText;

        public ChessAngleDelegate(string UIText) {
            this.UIText = UIText;
        }
       
    }
}
