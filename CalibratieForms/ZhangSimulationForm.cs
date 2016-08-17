using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ceresdotnet;
using Calibratie;
using ComponentOwl.BetterListView;
using OpenTK;
using SceneManager;
using WeifenLuo.WinFormsUI.Docking;

namespace CalibratieForms {
    public partial class ZhangSimulationForm : DockContent {
        public CameraInfoWindow InitialCameraWindow { get;set; }
        public CameraInfoWindow CalibratedCameraWindow { get; set; }
        
        public LVList<ZhangSimulation> Simulations { get { return _simulations; } }
        public LVList<ZhangSimulation> _simulations = new LVList<ZhangSimulation>(); 

        public CameraSimulationFrm cameraFrm { get; set; }

        public static List<ZhangSimulationForm> AllForms = new List<ZhangSimulationForm>();
        public ZhangSimulationForm() {
            InitializeComponent();
            AllForms.Add(this);
            this.Closed += (s, a) => { AllForms.Remove(this); };

            _simulations.CollumnDisplay2 = (s,item) => {
                item.Text = s.Camera.PictureSizeST;
                item.SubItems.AddRange(new[] {
                    s.Camera.ToString(), 
                    s.calcMeanDist().ToString(), 
                    s.AvgReprojectionError.ToString(),
                });
                item.BackColor = s.Solved ? Color.LightGreen : Color.LightCoral;
            };
            _simulations.ParentLV = lv_Zhang;
        }



        private static List<BetterListViewItem> getSimulationDetailItem(ZhangSimulation s) {
            var r = new List<BetterListViewItem>();
            if (s == null || s.Chessboards.Count == 0) {
                var item = new BetterListViewItem(new[] {
                    s == null ? "No Simulation Selected" : "No Chessboards!"
                });
                item.Tag = null;
                r.Add(item);
                return r;
            }
            var boards = s.Chessboards;
            for (int i = 0; i < boards.Count; i++) {
                var item = new BetterListViewItem(new[] {
                    boards[i].ToString(),
                    calcAngle(s.Camera,boards[i]).ToString(),
                    (s.Camera.Pos-boards[i].Pos).LengthFast.ToString(),
                    s.ReporjectionErrorRMS.Count != boards.Count ? "niet berekend" : s.ReporjectionErrorRMS[i].ToString()
                });
                item.Tag = boards[i];
                r.Add(item);
            }
            return r;
        }

        private void lv_Zhang_SelectedIndexChanged(object sender, EventArgs e) {
            ZhangSimulation simulation;
            try {
                simulation = (ZhangSimulation) lv_Zhang.SelectedItems[0].Tag;
            }
            catch {
                return;
            }
            if (InitialCameraWindow != null) { InitialCameraWindow.Camera = simulation.Camera; }
            if (CalibratedCameraWindow != null) { CalibratedCameraWindow.Camera = simulation.CalibratedCamera; }

            redrawZhangDetailList();
        }

        private void lv_ZhangDetail_SelectedIndexChanged(object sender, EventArgs e) {

            ZhangSimulation simulation;
            ChessBoard board;
            try {
                simulation = (ZhangSimulation) lv_Zhang.SelectedItems[0].Tag;
                board = (ChessBoard)lv_ZhangDetail.SelectedItems[0].Tag;
                //board = simulation.Chessboards[lv_ZhangDetail.SelectedIndices[0]];
            }
            catch {return;}
            if (cameraFrm != null) {
                cameraFrm.Board = board;
                cameraFrm.Camera = simulation.Camera;
            }

        }

        private void redrawZhangDetailList() {
            lv_ZhangDetail.Items.Clear();
            ZhangSimulation simulation;
            try {
                simulation = (ZhangSimulation)lv_Zhang.SelectedItems[0].Tag;
            }
            catch {
                return;
            }
            lv_ZhangDetail.Items.AddRange(getSimulationDetailItem(simulation));

        }

        private static double calcAngle(SObject obj1, SObject obj2) {
            var q1 = Vector3d.Dot(obj1.Dir, obj2.Dir);
            var alpha = Math.Acos(q1);
            double angle = alpha/Math.PI*180;
            return angle;
        }

        private void button1_Click(object sender, EventArgs e) {
            var c = PinholeCamera.getTestCamera();
            var b = new ChessBoard(10,4,20);
            var s = ZhangSimulation.CreateSimulation(c, b, 5,
                count => Util.gaussDistr(count, .7, .2, .20, 1),
                count => Util.gaussDistr(count, 0, Math.PI / 4, -Math.PI / 2, Math.PI / 2)
                );
            _simulations.Add(s);
        }


        #region contextStrip
        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e) {
            foreach (var cameraInfoWindow in CameraInfoWindow.AllForms) {
                var item1 = new ToolStripMenuItem() {
                    Text = cameraInfoWindow.Name,
                };
                item1.Click += (s, args) => {
                    InitialCameraWindow = cameraInfoWindow;
                };

                var item2 = new ToolStripMenuItem() {
                    Text = cameraInfoWindow.Name,
                };
                item2.Click += (s, args) => {
                    CalibratedCameraWindow = cameraInfoWindow;
                };
                CameraInfoToolStripMenuItem.DropDownItems.Add(item1);
                calibratedCameraInfoToolStripMenuItem.DropDownItems.Add(item2);
            }

            foreach (var cameraSimulationFrm in CameraSimulationFrm.AllForms) {
                var item = new ToolStripMenuItem() {
                    Text = cameraSimulationFrm.Name,
                };
                item.Click += (s, args) => {
                    cameraFrm = cameraSimulationFrm;
                };
                cameraSimulationViewToolStripMenuItem.DropDownItems.Add(item);
            }
            
        }

        private void contextMenuStrip1_Closing(object sender, ToolStripDropDownClosingEventArgs e) {
            cameraSimulationViewToolStripMenuItem.DropDownItems.Clear();
            calibratedCameraInfoToolStripMenuItem.DropDownItems.Clear();
            CameraInfoToolStripMenuItem.DropDownItems.Clear();
        }

        private void lv_Zhang_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Right) {
                contextMenuStrip1.Show(e.Location);
            }
        }
        #endregion

        private void button2_Click(object sender, EventArgs e) {
            var items = lv_Zhang.Items.Where(x=>x.Checked).Select(x => (ZhangSimulation) x.Tag);
            foreach (var zhangSimulation in items) {
                zhangSimulation.calculateCv2Async();
            }
        }

        private void button3_Click(object sender, EventArgs e) {
            var item = lv_Zhang.Items.Where(x => x.Checked).Select(x => (ZhangSimulation)x.Tag).First();
            List<CeresCamera> cameras;
            List<CeresMarker> markers;
            List<CeresPoint> points;
            item.toCeresInput(out markers, out cameras, out points);
            Matrix3d d;

            
            double[] intrinsics = item.Camera.toCeresIntrinsics9();
            var correct = (double[])intrinsics.Clone();
            var before = (double[])intrinsics.Clone();
            var r = new Random();
            for (int i = 0; i < intrinsics.Length; i++) {
                intrinsics[i] *= 1 + (r.NextDouble() - .5) / 10;
            }

            ceresdotnet.BundleProblem.EuclideanBundleCommonIntrinsics(markers, 0, 0, intrinsics, cameras, points);

            var diff = before.Select((x, i) => x - intrinsics[i]).ToArray();
            

        }
    }
}
