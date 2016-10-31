using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using cameracallibratie;
using Calibratie;
using CalibratieForms.Windows;
using OpenTK;
using WeifenLuo.WinFormsUI.Docking;
using Size = OpenCvSharp.Size;

using MathWorks.MATLAB.NET.Arrays;
using MathWorks.MATLAB.NET.Utility;
using OpenCvSharp;
using Vector = MathNet.Numerics.LinearAlgebra.Complex.Vector;


namespace CalibratieForms {
    public partial class Form1 : Form {
        public Form1() {


            InitializeComponent();

            ZhangSimulationForm simfrm = new ZhangSimulationForm();
            simfrm.Show(dockPanel1);

            LogForm logform = new LogForm();
            logform.Show(simfrm.Pane, DockAlignment.Bottom,.2);

            CameraInfoWindow cifo = new CameraInfoWindow();
            cifo.Show(simfrm.Pane, DockAlignment.Right,.5);

            CameraSimulationFrm cfrm = new CameraSimulationFrm();
            cfrm.Show(cifo.Pane, DockAlignment.Bottom, .35);

            
            CameraInfoWindow cifo2 = new CameraInfoWindow();
            cifo2.Show(cifo.Pane, DockAlignment.Right,.5);

            
            PinholeCamera c = PinholeCamera.getTestCamera();
            cifo.Camera = c;

            simfrm.InitialCameraWindow = cifo;
            simfrm.CalibratedCameraWindow = cifo2;
            simfrm.cameraFrm = cfrm;

            
            Log.AddReader(logform);

        }



        private void button1_Click(object sender, EventArgs e) {
        }

        private void cameraInfoToolStripMenuItem_Click(object sender, EventArgs e) {
            CameraInfoWindow window = new CameraInfoWindow();
            window.AllowEndUserDocking = true;
            window.Show(dockPanel1,DockState.Float);
        }

        private void zhangSimulationToolStripMenuItem_Click(object sender, EventArgs e) {
            ZhangSimulationForm window = new ZhangSimulationForm();
            window.Show(dockPanel1, DockState.Float);
        }

        private void cameraSimulationToolStripMenuItem_Click(object sender, EventArgs e) {
            CameraSimulationFrm window = new CameraSimulationFrm();
            window.Show(dockPanel1, DockState.Float);
        }

        private void logWindowToolStripMenuItem_Click(object sender, EventArgs e) {
            var window = new Windows.LogForm();
            window.Show(dockPanel1, DockState.Float);
            Log.AddReader(window);
        }

        private void button2_Click(object sender, EventArgs e) {
            var markers = ArUcoNET.Aruco.FindMarkers(@"C:\Users\jens\Desktop\calibratie\Huawei p9\test.jpg");
        }

        private void button3_Click(object sender, EventArgs e) {
            var scene = Util.bundleAdjustScene();

            //CeresSimulation.ceresSolveAruco();
            
            var sim = new CeresSimulation();
            sim.scene = scene;

            sim.Solve();


        }
    }
}
