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
    public partial class reprojectionForm : Form {

        public void drawChessboard(Vector2[] points, string saveLoc = @"C:\Users\jens\Desktop\calibratie\") {
            Bitmap b = new Bitmap(3072,2304);
            foreach (var p in points) {
                b.drawCorner(p);
            }
            this.pictureBox1.Image = b;
            pictureBox1.Show();
            b.Save(saveLoc+"test.jpg", ImageFormat.Jpeg);
            this.Update();
            pictureBox1.Update();
        }
        public reprojectionForm() {
            InitializeComponent();
        }
    }

    public static class EXT {
        public static void drawCorner(this Bitmap b,Vector2 point) {
            b.drawCorner((int) point.X, (int) point.Y);
        }
        public static void drawCorner(this Bitmap b, int x, int y) {
            int px = 10;
            for (int i = -px/2; i < px/2 + 1; i++) {
                for (int j = -px/2; j < px/2 + 1; j++) {
                    b.SetPixel(x+i,y+j,Color.White);
                }
            }
        }
    }
}
