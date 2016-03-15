using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using cameracallibratie;
using WeifenLuo.WinFormsUI.Docking;

namespace CalibratieForms {
    public partial class CameraInfoWindow : DockContent {
        public PinholeCamera Camera {
            get { return _camera; }

            set {
                if (_camera != null) { _camera.PropertyChanged -= _camera_PropertyChanged; }
                _camera = value;
                if (_camera != null) { _camera.PropertyChanged += _camera_PropertyChanged; }
                updateForm(); 
            }
        }
        private PinholeCamera _camera;

        void _camera_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            updateForm();
        }


        public static List<CameraInfoWindow> AllForms = new List<CameraInfoWindow>();
        public CameraInfoWindow() {
            InitializeComponent();
            AllForms.Add(this);
            this.Name += " " + AllForms.Count;
            this.Closed += (s, a) => { AllForms.Remove(this); };
        }

        public void updateForm() {
            if (Camera == null) {
                richTextBox1.Text = "no camera";
                return;
            }
            var cm = Camera.CameraMatrix;
            richTextBox1.Text = String.Format(  "[{0} {1} {2}]\n" +
                                                "[{3} {4} {5}]\n" +
                                                "[{6} {7} {8}]"
                                                , cm.Mat[0, 0], cm.Mat[0, 1], cm.Mat[0, 2]
                                                , cm.Mat[1, 0], cm.Mat[1, 1], cm.Mat[1, 2]
                                                , cm.Mat[2, 0], cm.Mat[2, 1], cm.Mat[2, 2]);
            txt_fc1.Text = Camera.CameraMatrix.fx.ToString();
            txt_fc2.Text = Camera.CameraMatrix.fy.ToString();
            txt_cc1.Text = Camera.CameraMatrix.cx.ToString();
            txt_cc2.Text = Camera.CameraMatrix.cy.ToString();
            txt_ac.Text = Camera.CameraMatrix.ac.ToString();

            txt_kc1.Text = Camera.DistortionR1.ToString();
            txt_kc2.Text = Camera.DistortionR2.ToString();
            txt_kc3.Text = Camera.DistortionT1.ToString();
            txt_kc4.Text = Camera.DistortionT2.ToString();
            txt_kc5.Text = Camera.DistortionR3.ToString();
        }
    }
}
