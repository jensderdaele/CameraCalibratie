using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComponentOwl.BetterListView;
using OpenTK;
using SceneManager;
using WeifenLuo.WinFormsUI.Docking;

namespace CalibratieForms {
    public partial class ZhangSimulationForm : DockContent {
        public CameraInfoWindow InitialCameraWindow { get;set; }
        public CameraInfoWindow CalibratedCameraWindow { get; set; }
        public ObservableCollection<ZhangSimulation> Simulations { get { return _simulations; } }
        private ObservableCollection<ZhangSimulation> _simulations = new ObservableCollection<ZhangSimulation>();
        public reprojectionForm ReprojectionWindow { get; set; }
        public ZhangSimulationForm() {
            InitializeComponent();
            _simulations.CollectionChanged += _simulations_CollectionChanged;
        }

        void _simulations_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            drawList();
        }


        public void drawList() {
            lv_Zhang.Clear();
            foreach (var zhangSimulation in Simulations) {
                var item = new BetterListViewItem();
                var sub = new BetterListViewSubItem();
                lv_Zhang.Items.Add(getSimulationItem(zhangSimulation));
            }
        }

        private static BetterListViewItem getSimulationItem(ZhangSimulation s) {
            var item = new BetterListViewItem(new String[]{
                s.Camera.PictureSizeST,
                s.Camera.ToString(),
                s.calcMeanDist().ToString(),
                s.AvgReprojectionError.ToString()
            });
            item.Tag = s;
            return item;
        }
        //chessboard,angle,dist,reprojectionerror
        private static BetterListViewItem getDetailItem(ZhangSimulation s,ChessBoard b) {
            var item = new BetterListViewItem(new String[]{
                String.Format("{0}x{1}({2}mm)",b.ChessboardSize.Width,b.ChessboardSize.Height,b.SquareSizemm),
                calcAngle(s.Camera,b).ToString(),
                (s.Camera.Pos-b.Pos).Length.ToString(),
                "not implemented"
            });
            item.Tag = s;
            return item;
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
            
        }

        private void lv_ZhangDetail_SelectedIndexChanged(object sender, EventArgs e) {

            ZhangSimulation simulation;
            ChessBoard board;
            try {
                simulation = (ZhangSimulation) lv_ZhangDetail.SelectedItems[0].Tag;
                board = simulation.Chessboards[lv_ZhangDetail.SelectedIndices[0]];
            }
            catch {return;}
            if (ReprojectionWindow != null) {
                ReprojectionWindow.drawChessboard(simulation.Calc2DProjectionBitmap(board));
            }

        }

        private void redrawZhangDetailList() {
            lv_ZhangDetail.Clear();
            ZhangSimulation simulation;
            try {
                simulation = (ZhangSimulation)lv_Zhang.SelectedItems[0].Value;
            }
            catch {
                return;
            }
            
        }


        private static double calcAngle(SObject obj1, SObject obj2) {
            var q1 = new Quaterniond(obj1.Dir);
            var q2 = new Quaterniond(obj2.Dir);
            var q = (q1.Normalized() - q2.Normalized()).Normalized();
            Vector3d axis;
            double angle;
            q.ToAxisAngle(out axis, out angle);
            return angle;
        }

        private void button1_Click(object sender, EventArgs e) {
            var c = PinholeCamera.getTestCamera();
            var b = new ChessBoard(8,6,20);
            var s = ZhangSimulation.CreateSimulation(c, b, 12,
                count => Util.gaussDistr(count, .5, .15, .2, 1),
                count => Util.gaussDistr(count, 0, Math.PI / 4, -Math.PI / 2, Math.PI / 2)
                );
            _simulations.Add(s);
        }
    }
}
