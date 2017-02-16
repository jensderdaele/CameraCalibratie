using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using cameracallibratie;
using ceresdotnet;
using Calibratie;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using OpenTK;
using Matrix = Emgu.CV.Matrix<double>;
using CVI = Emgu.CV.CvInvoke;
using Point2d = Emgu.CV.Structure.MCvPoint2D64f;
using Point3f = Emgu.CV.Structure.MCvPoint3D32f;
using Vector3d = Emgu.CV.Structure.MCvPoint3D64f;

namespace CalibratieForms {
    public class ZhangStereoCalibration {
        
    }
    public class ZhangCalibration {
        public class CalibImage {
            public CalibImage(string file) {
                Path = file;
            }
            public string Path;
            public bool UseInCalibration;
            public Size imageSize { get; set; }
            public string Filename { get { return System.IO.Path.GetFileName(Path); } }
            public PointF[] ImagePoints { get; set; }
            public Point2d[] ReprojectionError { get; set; }
            public Point2d[] Reprojection { get; set; }
            public double[] rvec;
            public double[] tvec;
        }

        public List<CalibImage> images = new List<CalibImage>();

        public List<CalibImage> images2 = new List<CalibImage>();
        private static Object lockme = new object();
        public void LoadImages(string dir,Size csize, List<string> filenames  = null, List<string> badfiles= null) {
            images.Clear();
            PhotoProvider prov = new PhotoProvider(dir);

            List<Task> allTasks = new List<Task>();
            SemaphoreSlim throttler = new SemaphoreSlim(2); //max 8 threads
            
            Action<Object> findCornerAction = o => {
                String imageFile = (String)o;
                var im = new CalibImage(imageFile);
                int scale = 1;
                while (scale <= 4) {
                    try {
                        Size imSize;
                        var pic = PhotoProvider.getSingleImage(im.Path, out imSize, scale);
                        var chessBoard = FindChessboardCorners(pic, csize);
                        Console.WriteLine(string.Format("found {0} at scale 1/{1}", im.Filename, scale));
                        im.ImagePoints = chessBoard.Select(f => new PointF(f.X * scale, f.Y * scale)).ToArray();
                        im.imageSize = imSize;
                        images.Add(im);
                        throttler.Release();
                        return;
                    }
                    catch {
                        if (badfiles != null) {
                            images.Add(null);
                            badfiles.Add(imageFile);
                            break;
                        }
                        Console.WriteLine(string.Format("no chessboard found: {0} at scale 1/{1}", im.Filename, scale));
                        scale *= 2;
                        continue;
                    }
                }

                throttler.Release();
            };

            var files = filenames ?? prov.getImageFiles().ToList();
            foreach (var imageFile in files) {
                throttler.Wait();
                Log.WriteLine(string.Format("searching pattern in {0}", imageFile));
                Task t = new Task(findCornerAction,imageFile);
                t.Start();
                allTasks.Add(t);
            }
            Task.WhenAll(allTasks).Wait();
        }

        public void LoadImages(List<string> files, Size csize) {
            LoadImages(null, csize, files);
        }
        public void LoadImages2(string dir, Size csize, List<string> filenames = null,List<string> badfiles= null) {
            
            //async werkt niet -> opencv reageert raar wanneer meerdere threads FindChessboardCorners() uitvoeren
            images2.Clear();
            PhotoProvider prov = new PhotoProvider(dir);

            List<Task> allTasks = new List<Task>();
            SemaphoreSlim throttler = new SemaphoreSlim(2); //max 8 threads

            Action<Object> findCornerAction = o => {
                String imageFile = (String)o;
                var im = new CalibImage(imageFile);
                int scale = 1;
                while (scale <= 4) {
                    try {
                        Size imSize;
                        var pic = PhotoProvider.getSingleImage(im.Path, out imSize, scale);
                        var chessBoard = FindChessboardCorners(pic, csize);
                        Console.WriteLine(string.Format("found {0} at scale 1/{1}", im.Filename, scale));
                        im.ImagePoints = chessBoard.Select(f => new PointF(f.X * scale, f.Y * scale)).ToArray();
                        im.imageSize = imSize;
                        images2.Add(im);
                        throttler.Release();
                        return;
                    }
                    catch {
                        if (badfiles != null) {
                            images2.Add(null);
                            badfiles.Add(imageFile);
                            break;
                        }
                        Console.WriteLine(string.Format("no chessboard found: {0} at scale 1/{1}", im.Filename, scale));
                        scale *= 2;
                        continue;
                    }
                }
                throttler.Release();
            };

            var files = filenames ?? prov.getImageFiles().ToList();
            foreach (var imageFile in files) {
                throttler.Wait();
                Log.WriteLine(string.Format("searching pattern in {0}", imageFile));
                Task t = new Task(findCornerAction, imageFile);
                t.Start();
                allTasks.Add(t);
            }
            Task.WhenAll(allTasks).Wait();
        }
        public static void calibrateStereoCalibrateCV() {
            
        }



        public void CalibrateCV(ChessBoard cb, out Matrix cameraMat, out Matrix distCoeffs) {

            var worldCoordinates = cb.boardLocalCoordinates_cv;
            
            List<List<Point3f>> worldpoints = new List<List<Point3f>>();
            for (int i = 0; i < images.Count; i++) {
                worldpoints.Add(cb.boardLocalCoordinates_cv.ToList());
            }

            double[,] cameraMat2 = new double[3, 3];
            
            var imagepoints = images.Select(x => x.ImagePoints);



            Matrix cameramat = new Matrix(3,3);
            Matrix distcoeffs = new Matrix(1,4);
            Mat[] rvecs, tvecs;
            CVI.CalibrateCamera(worldpoints.Select(x=>x.ToArray()).ToArray(), imagepoints.ToArray(), images.First().imageSize,
                cameramat, distcoeffs, CalibType.Default, new MCvTermCriteria(), 
                out rvecs, out tvecs);
            cameraMat = cameramat;
            distCoeffs = distcoeffs;
        }

        [Obsolete]
        public void CalibrateCV(ChessBoard cb, out PinholeCamera[] cameras) {

            var worldCoordinates = cb.boardLocalCoordinates_cv;

            List<List<Point3f>> worldpoints = new List<List<Point3f>>();
            for (int i = 0; i < images.Count; i++) {
                worldpoints.Add(cb.boardLocalCoordinates_cv.ToList());
            }

            double[,] cameraMat2 = new double[3, 3];

            var imagepoints = images.Select(x => x.ImagePoints);



            Matrix cameramat = new Matrix(3, 3);
            Matrix distcoeffs = new Matrix(4, 1);
            Mat[] rvecs, tvecs;

            CVI.CalibrateCamera(worldpoints.Select(x => x.ToArray()).ToArray(), imagepoints.ToArray(), images.First().imageSize,
                cameramat, distcoeffs, CalibType.Default, new MCvTermCriteria(),
                out rvecs, out tvecs);

            cameras = new PinholeCamera[images.Count];
            for (int i = 0; i < rvecs.Length; i++) {
                var rvec = rvecs[i];
                var tvec = tvecs[i];
                cameras[i] = new PinholeCamera();
                var cam = cameras[i];
                var rot = new RotationVector3D();
                rvec.CopyTo(rot);

                var worldMat = new Matrix4d();



            }
            
            
            for (int i = 0; i < cameras.Length; i++) {
                worldpoints.Add(cb.boardLocalCoordinates_cv.ToList());
            }
        }



        public void scatterPlot() {
            Matlab.ScatterPlot(this.images.Select(x=>x.ReprojectionError).ToList(), "testplot");
        }


        public static PointF[] FindChessboardCorners(IInputArray image, Size ChessboardSize) {
            //PointF[] corners;
            VectorOfPointF corners = new VectorOfPointF();
            if (CVI.FindChessboardCorners(image, ChessboardSize, corners)) {
                return corners.ToArray();
            }
            throw new Exception("No Corners Found");
        }

        public IObservation[] getObservations(PinholeCamera c) {
            throw new NotImplementedException();
        }
    }
}
