using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using cameracallibratie;
using CalibratieForms.Annotations;
using OpenTK;
using ceresdotnet;
using Calibratie;


using Matrix = Emgu.CV.Matrix<double>;
using CVI = Emgu.CV.CvInvoke;
using Point2d = Emgu.CV.Structure.MCvPoint2D64f;
using Point3f = Emgu.CV.Structure.MCvPoint3D32f;
using Vector3d = Emgu.CV.Structure.MCvPoint3D64f;

namespace CalibratieForms {
    public class ZhangSimulation : INotifyPropertyChanged {
        #region ctors
        public static ZhangSimulation CreateSimulation(PinholeCamera c,ChessBoard board,int pictureCount, 
            Func<int,double[]> distanceGenerator ,
            Func<int,double[]> Angles) {
            throw new NotImplementedException();
            /*
            ZhangSimulation simulation = new ZhangSimulation();

            var distances = distanceGenerator(pictureCount);
            var angles = Angles(pictureCount);
            Random r = new Random();
            var randomvec = new Vector3d(r.NextDouble(),r.NextDouble(),r.NextDouble()).Normalized(); //IS NIET RANDOM
            for (int i = 0; i < pictureCount; i++) {
                var pos = c.Pos + c.Dir*distances[i];
                var q1 = Quaterniond.FromAxisAngle(Vector3d.Cross(c.Dir, randomvec), angles[i]);
                var q2 = Quaterniond.FromAxisAngle(c.Dir, r.NextDouble()*Math.PI*2);
                var q = q1 + q2; 
                var b = new ChessBoard();
                b.ChessboardSize = board.ChessboardSize;
                b.SquareSizemm = board.SquareSizemm;
                b.Pos = pos;
                b.Orient(q);
                simulation.Chessboards.Add(b);
            }

            simulation.Camera = c;
            
            return simulation;
             * */
        }
#endregion

        #region props
        //Before calc
        protected List<ChessBoard> _chessboards = new List<ChessBoard>();
        public List<ChessBoard> Chessboards { get { return _chessboards; } }

        public PinholeCamera Camera { get; set; }
        //After calc
        public double AvgReprojectionError { get; private set; }

        public PinholeCamera CalibratedCamera { get; private set; }
        public Matrix[] Calibratedrvecs { get; private set; }
        public Matrix[] Calibratedtvecs { get; private set; }

        public bool Solved { get { return CalibratedCamera != null; } }

        public List<double> ReporjectionErrorRMS = new List<double>();

        #endregion

        
        public double calcMeanDist() {
            return Chessboards.Sum(chessboard => (chessboard.Pos - Camera.Pos).Norm/Chessboards.Count);
        }

        private IEnumerable<IEnumerable<Point3f>> CvWorldChessPointsf {
            get {
                return _chessboards.Select(chessboard => chessboard.boardWorldCoordinates.Select(x => new Point3f((float)x.X, (float)x.Y, (float)x.Z)));
            }
        }
        private IEnumerable<IEnumerable<Point3f>> CvLocalChessPointsf {
            get {
                return _chessboards.Select(chessboard => chessboard.boardLocalCoordinates_cv);
            }
        }

        [Obsolete("werkt niet")]
        public void calculateCv2() {
            throw new NotImplementedException();
            /*
            Log.WriteLine("zhang simulatie berekenen start");
            List<List<PointF>> imagePoints = new List<List<PointF>>();
            foreach (var chessboard in Chessboards) {
                PointF[] projected;
                get2DProjection_OpenCv(chessboard, out projected);
                imagePoints.Add(projected.ToList());
            }
            Vec3d[] rvecs,tvecs;

            var camera = new PinholeCamera();
            camera.PictureSize = Camera.PictureSize;
            Log.WriteLine("Cv2.CalibrateCamera");
            try {
                CVI.CalibrateCamera(CvLocalChessPointsf, imagePoints, Camera.PictureSize,
                    camera.CameraMatrix.Mat,
                    camera.Cv_DistCoeffs5, out rvecs, out tvecs);
            }
            catch (Exception e) {
                Log.WriteLine("Cv2.CalibrateCamera error: " + e.Message);
                return;
            }
            Calibratedrvecs = rvecs;
            Calibratedtvecs = tvecs;
            CalibratedCamera = camera;
            Log.WriteLine("zhang simulatie berekenen einde");
             * */
        }

        private static object lockme = new Object();


        [Obsolete("werkt niet")]
        public void calculateCv2Async() {
            throw new NotImplementedException();
            /*
            new Thread(() => {
                Log.WriteLine("zhang simulatie new thread berekenen start");
                List<List<PointF>> imagePoints = new List<List<Point2f>>();
                foreach (var chessboard in Chessboards) {
                    PointF[] projected;
                    get2DProjection_OpenCv(chessboard, out projected);
                    imagePoints.Add(projected.ToList());
                }
                Vec3d[] rvecs, tvecs;

                var camera = new PinholeCamera();
                camera.PictureSize = Camera.PictureSize;
                lock (lockme) {
                    Log.WriteLine("Cv2.CalibrateCamera LOCK");
                    try {
                        double[] distortie = new double[5];
                        CVI.CalibrateCamera(CvLocalChessPointsf, imagePoints, Camera.PictureSize,
                            camera.CameraMatrix.Mat,
                            distortie, out rvecs, out tvecs);
                        camera.Cv_DistCoeffs5 = distortie;
                    }
                    catch (Exception e) {
                        Log.WriteLine("Cv2.CalibrateCamera error: " + e.Message);
                        return;
                    }
                }
                
                Calibratedrvecs = rvecs;
                Calibratedtvecs = tvecs;
                CalibratedCamera = camera;

                calcReprojectionError();
                Log.WriteLine("zhang simulatie berekenen einde (succes)");
                OnPropertyChanged();
            }).Start();
             * */
        }

        

        private void calcReprojectionError() {
            ReporjectionErrorRMS.Clear();
            foreach (var b in _chessboards) {
                var original = Camera.ProjectBoard_Cv(b);
                var reproj = CalibratedCamera.ProjectBoard_Cv(b);
                var diff = new Vector2[original.Length];
                for (int i = 0; i < original.Length; i++) {
                    diff[i] = original[i] - reproj[i];
                }
                var totalErr = diff.Sum(x => x.Length);
                totalErr /= diff.Length;
                ReporjectionErrorRMS.Add(totalErr);
            }
            AvgReprojectionError = ReporjectionErrorRMS.Sum(x => x) / ReporjectionErrorRMS.Count;
            
            
        }

        public delegate void emptyDelegate();

        public void toCeresInput(out List<CeresMarker> markers, out List<CeresCamera> cameras,
            out List<CeresPoint> points) {
            throw new NotImplementedException();
            /*
            markers = new List<CeresMarker>();
            int imageNr = 0;
            foreach (var chessboard in Chessboards) {
                Point2f[] projected;
                get2DProjection_OpenCv(chessboard, out projected);
                markers.AddRange(projected.Select((p,track) => new CeresMarker(imageNr, track, p.X, p.Y)));
                imageNr++;
            }
            cameras = Chessboards.Select((x,i)=> new  CeresCamera(new Matrix3d(), new Vector3d(),i)).ToList();
            points = Chessboards[0].boardLocalCoordinates.Select((x, i) => new CeresPoint(x, i)).ToList();
             * */
        }

        #region 2dprojection
        private void get2DProjection_OpenCv(ChessBoard b, out PointF[] projected) {
            double[,] jabobian;
            projected = CVI.ProjectPoints(b.boardWorldCoordinated_Cv, Camera.Rvecs, Camera.Tvecs, Camera.Intrinsics.cvmat, Camera.Cv_DistCoeffs4);
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanged;
            if (handler != null) { handler(this, new PropertyChangedEventArgs(propertyName)); }
        }
    }
}