using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using WeifenLuo.WinFormsUI.Docking;

namespace CalibratieForms {
    public partial class CameraSimulationFrm : DockContent {
        public PinholeCamera Camera { get { return _camera; } set { _camera = value; draw(); } }
        private PinholeCamera _camera;
        public ChessBoard Board { get { return _board; } set { _board = value; draw(); } }
        private ChessBoard _board;
        private Bitmap _bitmap;
        public CameraSimulationFrm() {
            InitializeComponent();
        }

        public void draw() {
            if (_camera == null || _board == null) return;
            drawChessboard(_camera.ProjectBoard_Cv(_board));
        }
        
        public void drawChessboard(Vector2[] points) {
            _bitmap = new Bitmap(_camera.PictureSize.Width, _camera.PictureSize.Height);
            var g = Graphics.FromImage(_bitmap);
            g.Clear(Color.White);
            
            foreach (var p in points) {
                drawCorner(_bitmap, p);
            }
            this.pictureBox1.Image = _bitmap;
            pictureBox1.Show();
            this.Update();
            pictureBox1.Update();
        }

        public static void drawCorner(Bitmap b, Vector2 point) {
            drawCorner(b,(int)point.X, (int)point.Y);
        }
        public static void drawCorner(Bitmap b, int x, int y) {
            int px = 10;
            
            for (int i = -px / 2; i < px / 2 + 1; i++) {
                for (int j = -px / 2; j < px / 2 + 1; j++) {
                    var xx = x + i;
                    var yy = y + j;
                    if (xx < 0 || yy < 0 || xx > b.Width - 1 || yy > b.Height - 1) return;
                    b.SetPixel(x + i, y + j, Color.Black);
                }
            }
        }
    }
}
