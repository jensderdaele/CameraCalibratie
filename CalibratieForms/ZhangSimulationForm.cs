using System;
using System.Collections.Generic;
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

namespace CalibratieForms {
    public partial class ZhangSimulationForm : Form {
        public CameraInfoWindow InitialCameraWindow { get;set; }
        public CameraInfoWindow CalibratedCameraWindow { get; set; }
        public List<ZhangSimulation> Simulations { get; private set; }
        public reprojectionForm ReprojectionWindow { get; set; }
        public ZhangSimulationForm() {
            InitializeComponent();
            BetterListViewItem item = new BetterListViewItem();
        }

        private void ZhangSimulationForm_Load(object sender, EventArgs e) {

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
            item.Value = s;
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
            item.Value = s;
            return item;
        }

        private void lv_Zhang_SelectedIndexChanged(object sender, EventArgs e) {
            ZhangSimulation simulation;
            try {
                simulation = (ZhangSimulation) lv_Zhang.SelectedItems[0].Value;
            }
            catch {
                return;
            }
            if (InitialCameraWindow != null) { InitialCameraWindow.Camera = simulation.Camera; }
            if (CalibratedCameraWindow != null) { CalibratedCameraWindow.Camera = simulation.CvCalibratedCamera; }
            
        }

        private void lv_ZhangDetail_SelectedIndexChanged(object sender, EventArgs e) {

            ZhangSimulation simulation;
            ChessBoard board;
            try {
                simulation = (ZhangSimulation) lv_ZhangDetail.SelectedItems[0].Value;
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

        }
    }
}
