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
            ZhangSimulation s = new ZhangSimulation() {
                Camera = PinholeCamera.getTestCamera(),
            };
            s.Camera.Pos = new Vector3d(-.0422226, -.0878566, .36428266);
            s.Camera.Orient(Quaternion.FromEulerAngles(new Vector3(-.325198f, .1990075f, .4356749f)));

            ChessBoard b = new ChessBoard();
            b.SquareSizemm = 20;
            b.ChessboardSize = new Size(8, 6);


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
    }
}
