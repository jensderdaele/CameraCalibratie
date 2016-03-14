using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using cameracallibratie;
using CalibratieForms.Windows;
using OpenTK;
using WeifenLuo.WinFormsUI.Docking;
using Size = OpenCvSharp.Size;

namespace CalibratieForms {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();

            ZhangSimulationForm simfrm = new ZhangSimulationForm();
            simfrm.Show(dockPanel1);

            CameraSimulationFrm cfrm = new CameraSimulationFrm();
            cfrm.Show(dockPanel1, DockState.DockRight);

            CameraInfoWindow cifo = new CameraInfoWindow();
            var w = cifo.Width;
            cifo.Show(dockPanel1, DockState.DockRight);
            cifo.Width = w;

            CameraInfoWindow cifo2 = new CameraInfoWindow();
            w = cifo2.Width;
            cifo2.Show(dockPanel1, DockState.DockRight);
            cifo2.Width = w;

            LogForm logform = new LogForm();
            logform.Show(dockPanel1,DockState.DockBottom);

            PinholeCamera c = PinholeCamera.getTestCamera();
            cifo.Camera = c;

            simfrm.InitialCameraWindow = cifo;
            simfrm.CalibratedCameraWindow = cifo2;
            simfrm.cameraFrm = cfrm;

            
            Log.AddReader(logform);

        }

        private void button1_Click(object sender, EventArgs e) {
            ZhangSimulation s = new ZhangSimulation() {
                Camera = PinholeCamera.getTestCamera(),
            };
            s.Camera.Pos = new Vector3d(-.0422226, -.0878566, .36428266);
            s.Camera.Orient(Quaternion.FromEulerAngles(new Vector3(-.325198f, .1990075f, .4356749f)));

            ChessBoard b = new ChessBoard();
            b.SquareSizemm = 20;
            b.ChessboardSize = new Size(8, 6);

            var punten2d = s.Calc2DProjection(b);

        }
    }
}
