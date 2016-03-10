using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using cameracallibratie;
using CalibratieForms.Logging;
using OpenCvSharp;
using OpenTK;
using Size = OpenCvSharp.Size;

namespace CalibratieForms {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //test2();
            Application.Run(new Form1());
        }

        public static void test2() {
            
        }

        public static void testZhangCalib() {
            ZhangSimulation s = new ZhangSimulation() {
                 Camera = PinholeCamera.getTestCamera(),
            };
            s.Camera.Pos = new Vector3d(2,0,0);
            s.Camera.Orient(new Vector3d(0,0,0),new Vector3d(0,0,1));
            
            for (int i = 0; i < 10; i++) {
                ChessBoard c = new ChessBoard();
                c.ChessboardSize = new Size(8,6);
                c.SquareSizemm = 100;
                c.Pos=new Vector3d(0,0,0);
                c.Orient(new Vector3d(0+i, 0+i, 1).Normalized(), new Vector3d(0+i, 1, 0).Normalized());
                s.Chessboards.Add(c);
            }

            s.calculateCv2();


            
            
        }
        public static Point2f[] FindChessboardCorners(InputArray image,Size ChessboardSize) {
            Point2f[] corners;
            if (Cv2.FindChessboardCorners(image, ChessboardSize, out corners)) {
                return corners;
            }
            throw new Exception("No Corners Found");
        }
        public static void calibImages(string dir = @"C:\Users\jens\Desktop\calibratie\fotos\") {
            var csize = new Size(8, 6);
            List<Point2f[]> imagepoints = new List<Point2f[]>();

            var prov = new PhotoProvider(dir);
            var cvPictures = prov.picturesCvArray.ToList();
            var imagefileslist = prov.getImageFiles().ToList();
            for (int i = 0; i < cvPictures.Count; i++) {
                Log.WriteLine("chessboardcorners zoeken {0}/{1}: ", i, cvPictures.Count);
                try {
                    var chessBoard = FindChessboardCorners(cvPictures[i],csize);
                    Log.WriteLine("found " + imagefileslist[i]);
                    imagepoints.Add(chessBoard);
                }
                catch {
                    Log.WriteLine("no chessboard found: " + imagefileslist[i]);
                }
            }
            var picSize = new Size(3072, 2304);
            var cb = new ChessBoard {
                SquareSizemm = 20,
                ChessboardSize = new Size(8, 6)
            };
            var worldCoordinates = cb.boardLocalCoordinates_cv;

            Mat cameraMat = Mat.Eye(3, 3, MatType.CV_64F);
            Mat distCoeffs = Mat.Zeros(8, 1, (MatType)MatType.CV_64F);
            
            List<List<Point3f>> worldpoints = new List<List<Point3f>>();
            foreach (Point2f[] t in imagepoints) {
                worldpoints.Add(cb.boardLocalCoordinates_cv.ToList());
            }


            double[,] cameraMat2 = new double[3,3];
            Vec3d[] rvecs2, tvecs2;
            var distc5 = new double[5];

            var error = Cv2.CalibrateCamera(worldpoints, imagepoints, picSize, cameraMat2, distc5, out rvecs2, out tvecs2);

            List<Point2d[]> reprojectedList = new List<Point2d[]>();
            for (int i = 0; i < worldpoints.Count; i++) {
                List<Point3d> worldpointsd = worldpoints[i].Select(p => new Point3d(p.X, p.Y, p.Z)).ToList();
                Point2d[] reproj;
                double[] rv = new double[] { rvecs2[i].Item0, rvecs2[i].Item1, rvecs2[i].Item2 };
                double[] tv = new double[] { tvecs2[i].Item0, tvecs2[i].Item1, tvecs2[i].Item2 };
                double[,] jacobian;
                Cv2.ProjectPoints(worldpointsd, rv, tv, cameraMat2, distc5, out reproj,out jacobian);
                reprojectedList.Add(reproj);
            }

            var diff = new List<Point2d[]>();
            for (int i = 0; i < reprojectedList.Count; i++) {
                Point2d[] d = new Point2d[reprojectedList[i].Length];
                for (int j = 0; j < reprojectedList[i].Length; j++) {
                    d[j] = reprojectedList[i][j] - new Point2d(imagepoints[i][j].X, imagepoints[i][j].Y);
                }
                diff.Add(d);
            }
        }

        public static Image drawCameraImage(PinholeCamera c) {
            throw new NotImplementedException();
        }

        //Target recognition
    }
}
