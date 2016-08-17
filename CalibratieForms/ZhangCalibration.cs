using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using cameracallibratie;
using Calibratie;
using OpenCvSharp;

namespace CalibratieForms {
    public class ZhangCalibration {
        public class CalibImage {
            public CalibImage(string file) {
                Path = file;
            }
            public string Path;
            public bool UseInCalibration;
            public Size imageSize { get; set; }
            public string Filename { get { return System.IO.Path.GetFileName(Path); } }
            public Point2f[] ImagePoints { get; set; }
            public Point2d[] ReprojectionError { get; set; }
            public Point2d[] Reprojection { get; set; }
            public double[] rvec;
            public double[] tvec;
        }

        public List<CalibImage> images = new List<CalibImage>();
        private static Object lockme = new object();
        public void LoadImages(string dir,Size csize) {
            //async werkt niet -> opencv reageert raar wanneer meerdere threads FindChessboardCorners() uitvoeren
            images.Clear();
            PhotoProvider prov = new PhotoProvider(dir);

            List<Task> allTasks = new List<Task>();
            SemaphoreSlim throttler = new SemaphoreSlim(100); //max 8 threads

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
                        im.ImagePoints = chessBoard.Select(f => new Point2f(f.X * scale, f.Y * scale)).ToArray();
                        im.imageSize = imSize;
                        images.Add(im);
                        throttler.Release();
                        return;
                    }
                    catch {
                        Console.WriteLine(string.Format("no chessboard found: {0} at scale 1/{1}", im.Filename, scale));
                        scale *= 2;
                        continue;
                    }
                }
                throttler.Release();
            };

            var files = prov.getImageFiles().ToList();
            foreach (var imageFile in files) {
                throttler.Wait();
                Log.WriteLine(string.Format("searching pattern in {0}", imageFile));
                Task t = new Task(findCornerAction,imageFile);
                t.Start();
                allTasks.Add(t);
            }
            Task.WhenAll(allTasks).Wait();
        }

        public void CalibrateCV(ChessBoard cb, out Mat cameraMat, out Mat distCoeffs) {

            var worldCoordinates = cb.boardLocalCoordinates_cv;

            //Mat distCoeffs = Mat.Zeros(5, 1, (MatType)MatType.CV_64F);

            List<List<Point3f>> worldpoints = new List<List<Point3f>>();
            for (int i = 0; i < images.Count; i++) {
                worldpoints.Add(cb.boardLocalCoordinates_cv.ToList());
            }

            double[,] cameraMat2 = new double[3, 3];
            
            var imagepoints = images.Select(x => x.ImagePoints);


            List<Mat> imagepointscv = imagepoints.Select(imagepoint => new MatOfPoint2f(1, imagepoint.Length, imagepoint.ToArray())).Cast<Mat>().ToList();
            List<Mat> worldpointscv = worldpoints.Select(worldpoint => new MatOfPoint3f(1, worldpoint.Count, worldpoint.ToArray())).Cast<Mat>().ToList();

            Mat cmcv = new Mat();
            Mat distcoeffs = new Mat();
            Mat[] rvecs, tvecs;
            Cv2.CalibrateCamera(worldpointscv, imagepointscv, images.First().imageSize, cmcv, distcoeffs, out rvecs, out tvecs);
            distCoeffs = distcoeffs;
            cameraMat = cmcv;


            cmcv.GetArray(0, 0, cameraMat2);

            /*
            for (int i = 0; i < images.Count; i++) {
                List<Point3d> worldpointsd = worldpoints[i].Select(p => new Point3d(p.X, p.Y, p.Z)).ToList();
                Point2d[] reproj;

                double[] rv = new double[] { rvecs2[i].Item0, rvecs2[i].Item1, rvecs2[i].Item2 };
                double[] tv = new double[] { tvecs2[i].Item0, tvecs2[i].Item1, tvecs2[i].Item2 };
                double[,] jacobian;
                Mat output = 
                Cv2.ProjectPoints(worldpointscv[i],rvecs[i],tvecs[i],cmcv,distCoeffs);
                Cv2e.ProjectPoints(worldpointsd, rv, tv, cameraMat2, distc5, out reproj, out jacobian);
                images[i].Reprojection = reproj;
                images[i].ReprojectionError =
                    images[i].ImagePoints.Select((x, j) => { return new Point2d(x.X, x.Y) - images[i].Reprojection[j]; })
                        .ToArray();
            }*/
        }

        public void Calibrate(ChessBoard cb, out double[,] cameramatrix, out double[] distort5) {
            
            var worldCoordinates = cb.boardLocalCoordinates_cv;

            Mat cameraMat = Mat.Eye(3, 3, MatType.CV_64F);
            Mat distCoeffs = Mat.Zeros(5, 1, (MatType)MatType.CV_64F);

            List<List<Point3f>> worldpoints = new List<List<Point3f>>();
            for (int i = 0; i < images.Count; i++) {
                worldpoints.Add(cb.boardLocalCoordinates_cv.ToList());
            }

            double[,] cameraMat2 = new double[3, 3];
            Vec3d[] rvecs2, tvecs2;
            var distc5 = new double[5];

            
            var error = Cv2.CalibrateCamera(worldpoints, images.Select(x=>x.ImagePoints), images.FirstOrDefault().imageSize, cameraMat2, distc5, out rvecs2, out tvecs2);

            cameramatrix = cameraMat2;
            distort5 = distc5;
            for (int i = 0; i < images.Count; i++) {
                List<Point3d> worldpointsd = worldpoints[i].Select(p => new Point3d(p.X, p.Y, p.Z)).ToList();
                Point2d[] reproj;

                double[] rv = new double[] { rvecs2[i].Item0, rvecs2[i].Item1, rvecs2[i].Item2 };
                double[] tv = new double[] { tvecs2[i].Item0, tvecs2[i].Item1, tvecs2[i].Item2 };
                double[,] jacobian;
                Cv2e.ProjectPoints(worldpointsd, rv, tv, cameraMat2, distc5, out reproj, out jacobian);
                images[i].Reprojection = reproj;
                images[i].ReprojectionError =
                    images[i].ImagePoints.Select((x, j) => { return new Point2d(x.X, x.Y) - images[i].Reprojection[j]; })
                        .ToArray();
            }
        }

        public void Calibrate(ChessBoard cb, out PinholeCamera c) {
            double[] dist;
            double[,] cmat;
            Calibrate(cb,out cmat, out dist);
            c = new PinholeCamera(new CameraMatrix(cmat)){Cv_DistCoeffs5 = dist};
        }

        public void scatterPlot() {
            Matlab.ScatterPlot(this.images.Select(x=>x.ReprojectionError).ToList(), "testplot");
        }
        public static async void calibImages(string dir = @"C:\Users\jens\Desktop\calibratie\Fotos_gedownload_door_AirDroid (1)\zhang\5x7\") {
            CalibPictureProvider pictureProvider = new PhotoProvider(dir);
            
            int scaledown = 4;
            //var picSize = new Size(5184, 3456);
            var picSize = new Size(4160 / scaledown, 3120 / scaledown);
            var csize = new Size(7, 5);
            var cb = new ChessBoard {
                SquareSizemm = 30,
                ChessboardSize = csize
            };

            List<Point2f[]> imagepoints = new List<Point2f[]>();

            var prov = new PhotoProvider(dir);
            prov.ScaleDown = 4;
            var cvPictures = prov.picturesCvArray.ToList();
            var imagefileslist = prov.getImageFiles().ToList();

            List<Task> allTasks = new List<Task>();
            SemaphoreSlim throttler = new SemaphoreSlim(8); //max 8 async threads

            for (int i = 0; i < cvPictures.Count; i++) {
                var pic = cvPictures[i];
                var file = imagefileslist[i];
                await throttler.WaitAsync();
                allTasks.Add(Task.Run(() => {
                    try {
                        var chessBoard = FindChessboardCorners(pic, csize);
                        Log.WriteLine("found " + file);
                        imagepoints.Add(chessBoard.Select(f => new Point2f(f.X * scaledown, f.Y * scaledown)).ToArray());
                    }
                    catch {
                        Log.WriteLine("no chessboard found: " + file);
                    }
                    finally {
                        throttler.Release();
                    }

                }));
            }
            await Task.WhenAll(allTasks);



            var worldCoordinates = cb.boardLocalCoordinates_cv;

            Mat cameraMat = Mat.Eye(3, 3, MatType.CV_64F);
            Mat distCoeffs = Mat.Zeros(8, 1, (MatType)MatType.CV_64F);

            List<List<Point3f>> worldpoints = new List<List<Point3f>>();
            foreach (Point2f[] t in imagepoints) {
                worldpoints.Add(cb.boardLocalCoordinates_cv.ToList());
            }


            double[,] cameraMat2 = new double[3, 3];
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
                Cv2e.ProjectPoints(worldpointsd, rv, tv, cameraMat2, distc5, out reproj, out jacobian);
                reprojectedList.Add(reproj);
            }

            var diff = new List<Point2d[]>();
            for (int i = 0; i < reprojectedList.Count; i++) {
                Point2d[] d = new Point2d[reprojectedList[i].Length];
                for (int j = 0; j < reprojectedList[i].Length; j++) {
                    d[j] = reprojectedList[i][j] - new Point2d(imagepoints[i][j].X, imagepoints[i][j].Y);
                }
                diff.Add(d);
                //Matlab.ScatterPlot(d);
            }
            Matlab.ScatterPlot(diff, "testplot");
        }

        public static Point2f[] FindChessboardCorners(InputArray image, Size ChessboardSize) {
            Point2f[] corners;
            if (Cv2.FindChessboardCorners(image, ChessboardSize, out corners)) {
                return corners;
            }
            throw new Exception("No Corners Found");

        }
    }
}
