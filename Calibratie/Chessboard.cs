using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SceneManager;

using Point3d = Emgu.CV.Structure.MCvPoint3D64f;
using Point3f = Emgu.CV.Structure.MCvPoint3D32f;

namespace Calibratie {
    public class ChessBoard : SObject {
        public double SquareSizemm { get; set; }
        public Size ChessboardSize { get; set; }

        public ChessBoard() : base() { }

        public ChessBoard(int width, int height, double squareszmm) {
            ChessboardSize = new Size(width, height);
            SquareSizemm = squareszmm;
        }


        public Point3f[] boardWorldCoordinated_Cv {
            get { return boardWorldCoordinates.Select(x => new Point3f((float)x.X, (float)x.Y, (float)x.Z)).ToArray(); }
        }
        public List<Vector3d> boardWorldCoordinates {
            get {
                List<Vector3d> r = new List<Vector3d>();
                for (int i = 0; i < ChessboardSize.Height; ++i)
                    for (int j = 0; j < ChessboardSize.Width; ++j) {
                        var local = new Vector3d((float)
                            (((float)j-((float)ChessboardSize.Width-1)/2) * SquareSizemm * .001),
                            (float)((i - (float)ChessboardSize.Height/2 - 0.5) * SquareSizemm * .001), 0);
                        r.Add(Vector3d.Transform(local, this.worldMat));
                    }
                return r;
            }

        }
        public List<Vector3d> boardLocalCoordinates {
            get {
                List<Vector3d> r = new List<Vector3d>();
                for (int i = 0; i < ChessboardSize.Height; ++i)
                    for (int j = 0; j < ChessboardSize.Width; ++j) {
                        var local = new Vector3d((float)(j * SquareSizemm * .001), (float)(i * SquareSizemm * .001), 0);
                        r.Add(local);
                    }
                return r;
            }
        }
        public Point3f[] boardLocalCoordinates_cv {
            get {
                List<Point3f> r = new List<Point3f>();
                for (int i = 0; i < ChessboardSize.Height; ++i)
                    for (int j = 0; j < ChessboardSize.Width; ++j) {
                        var local = new Point3f((float)(j * SquareSizemm * .001), (float)(i * SquareSizemm * .001), 0);
                        r.Add(local);
                    }
                return r.ToArray();
            }
        }
        public Point3d[] boardLocalCoordinates_cvd {
            get {
                List<Point3d> r = new List<Point3d>();
                for (int i = 0; i < ChessboardSize.Height; ++i)
                    for (int j = 0; j < ChessboardSize.Width; ++j) {
                        var local = new Point3d((float)(j * SquareSizemm * .001), (float)(i * SquareSizemm * .001), 0);
                        r.Add(local);
                    }
                return r.ToArray();
            }
        }

        public override string ToString() {
            return String.Format("{0}x{1} {2:0}mm", this.ChessboardSize.Width, this.ChessboardSize.Height,
                this.SquareSizemm);
        }
    }
}
