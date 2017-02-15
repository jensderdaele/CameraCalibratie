using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using cameracallibratie;
using CalibratieForms.Logging;
using OpenTK;

using System.Runtime.InteropServices;
using System.Reflection;
using System.Security.Policy;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using ceresdotnet;
using Calibratie;

using ArUcoNET;
using Emgu.CV;
using Emgu.CV.Aruco;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using MathNet.Numerics;
using PdfSharp;
using SceneManager;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;

using Matrix = Emgu.CV.Matrix<double>;
using CVI = Emgu.CV.CvInvoke;
using FontFamily = System.Drawing.FontFamily;
using FontStyle = System.Drawing.FontStyle;
using Point = System.Windows.Point;
using Point2d = Emgu.CV.Structure.MCvPoint2D64f;
using Point3f = Emgu.CV.Structure.MCvPoint3D32f;
using Vector3d = Emgu.CV.Structure.MCvPoint3D64f;


namespace CalibratieForms {
    public class StereoImageFileProvider : IMultiCameraImageProvider {

        private Func<string, Mat> _fileread = s => CVI.Imread(s);
        public Func<string, Mat> FileRead {
            get { return _fileread; }
            set {
                _fileread = value;
                cachedImages.Clear();
            }

        }

        private Mat readFileCache(string f) {
            if (cachedImages.ContainsKey(f)) {
                return cachedImages[f];
            }
            else {
                var r = FileRead(f);
                cachedImages.Add(f, r);
                return r;
            }
        }

        public Dictionary<string, Mat> cachedImages = new Dictionary<string, Mat>();  
        

        private int _keyFrameL, _keyFrameR;
        private double _lr;
        private int _intervalL;
        private int _startL;
        private int _stopL;

        private Func<int, string> getFileL,getFileR;

        private CameraIntrinsics[] cameraintr = new CameraIntrinsics[2];

        public CameraIntrinsics LeftCamera { get { return cameraintr[0]; } }
        public CameraIntrinsics RightCamera { get { return cameraintr[1]; } }
        
        private int getFrameLfromindex(int index) {
            return _startL + _intervalL*index;
        }

        private int getFrameRFromL(int frameL) {
            return (int) ((frameL - _keyFrameL)*_lr + _keyFrameR);
        }

        public StereoImageFileProvider(int keyframeL, int keyframeR, int intervalL, int startL, int stopL, double lr,
            Func<int, string> frameLocL, Func<int, string> frameLocR) {
            _keyFrameL = keyframeL;
            _keyFrameR = keyframeR;
            _intervalL = intervalL;
            _startL = startL;
            _lr = lr;
            _stopL = stopL;
            getFileL = frameLocL;
            getFileR = frameLocR;
        }



        public CameraIntrinsics[] CameraIntrinsics { get { return cameraintr; }}

        public Mat this[int index, CameraIntrinsics intrinsics] {
            get {
                var lframe = getFrameLfromindex(index);
                if (lframe > _stopL) {
                    throw new IndexOutOfRangeException();
                }
                string file;
                if (LeftCamera == intrinsics) {
                    file = getFileL(lframe);
                }
                else if (RightCamera == intrinsics) {
                    file = getFileR(getFrameRFromL(lframe));
                }
                else {
                    throw new Exception();
                }
                return readFileCache(file);
            }
        }

        public ICameraImageProvider GetProviderForCamera(CameraIntrinsics index) {
            throw new NotImplementedException();
        }
    }
    static class Program {


        public static object lockme = new Object();
        // "C:\Users\jens\Desktop\calibratie\Opnames_thesis\links\2
        public static Dictionary<string, string> GetLRFrames(string dirL,string dirR,int KeyFrameL, int KeyFrameR, double LR, int intervalL,int startL, int stopL) {
            
            var r = new Dictionary<string, string>();


            int startframeL = KeyFrameL;
            int startframeR = KeyFrameR;
            int fps_links = intervalL;
            Func<int, int> frameR = frameL => (int)((frameL - startframeL) * LR + startframeR);
            


            int framel = startL;
            int framer = frameR(framel);


            while (framel < stopL) {
                r.Add(string.Format(@"{1}\{0:00000000}.jpg", framel, dirL),
                    string.Format(@"{1}\{0:00000000}.jpg", framer, dirR));
                framel += intervalL;
                framer = frameR(framel);
            }
            return r;
        }
        
       

        public static Dictionary<Tuple<string, List<ArucoMarker>>, Tuple<string, List<ArucoMarker>>> getLRFramesAruco(
            string dirL,string dirR,
            int KeyFrameL,
            int KeyFrameR, double LR, int intervalL,int startL, int stopL) {
            var files = GetLRFrames(dirL, dirR,KeyFrameL, KeyFrameR, LR, intervalL, startL, stopL);
            var detectedLFile = Path.Combine(dirL, "detected");
            var detectedRFile =  Path.Combine(dirR, "detected");
            var L = Aruco.findArucoMarkers(files.Keys, detectedLFile).Select(x => new Tuple<string, List<ArucoMarker>>(x.Key, x.Value.ToList())).ToList();
            var R = Aruco.findArucoMarkers(files.Values, detectedRFile).Select(x => new Tuple<string, List<ArucoMarker>>(x.Key, x.Value.ToList())).ToList();
            
            
            var r = new Dictionary<Tuple<string, List<ArucoMarker>>, Tuple<string, List<ArucoMarker>>>();
            for (int i = 0; i < L.Count; i++) {
                r.Add(L[i], R[i]);
            }
            return r;
        }



        static void drawReprojection(ref string file, IEnumerable<PointF> points, IEnumerable<PointF> error,IEnumerable<string> txt = null) {
            var bitmap = Bitmap.FromFile(file);
            var gfx = Graphics.FromImage(bitmap);
            var pen = new Pen(Color.Coral);

            var pointFs = points as PointF[] ?? points.ToArray();
            var enumerable = error as PointF[] ?? error.ToArray();
            string[] txtarr = null;
            if (txt != null)txtarr = txt as string[] ?? txt.ToArray();
            var rect = new Rect(new Point(0, 0), new Point(bitmap.Width, bitmap.Height));
            var font = new Font(FontFamily.GenericMonospace, 18,FontStyle.Bold);
            for (int i = 0; i < pointFs.Count(); i++) {
                var p = pointFs[i];
                var e = enumerable[i];

                float width = 10;
                float height = 10;
                var test = new LineEquation(new Point(p.X, p.Y), new Point(p.X + e.X, p.Y + e.Y));
                var intersect = test.GetIntersectionWithLineForRay(rect);
                if (intersect != null) {
                    try {
                        pen.Brush = new SolidBrush(pen.Color);
                        gfx.DrawEllipse(pen, p.X - width/2, p.Y - height/2, width, height);
                        pen.Width = 3;
                        gfx.DrawLine(pen, p.X, p.Y, p.X + e.X, p.Y + e.Y);
                        if (txt != null) { gfx.DrawString(txtarr[i], font, new SolidBrush(Color.Chocolate), p.X, p.Y); }
                    }
                    catch {
                        
                    }
                }
                else {
                }

                //gfx.DrawLine(pen, p.X, p.Y, p.X - e.X, p.Y - e.Y);
                //gfx.DrawLine(pen, p.X, p.Y, p.X + e.X, p.Y - e.Y);
                //gfx.DrawLine(pen, p.X, p.Y, p.X - e.X, p.Y + e.Y);
            }
            file = file + "reprojection.jpg";
            bitmap.Save(file,ImageFormat.Bmp);
            gfx.Dispose();
            bitmap.Dispose();
            //File.Delete(file);
        }

        static void testSFM() {
            /*
            var dir = @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\links\2\";
            List<string> files = new List<string>();
            for (int i = 533; i < 2500; i+=30) {
                files.Add(string.Format(@"{1}\{0:00000000}.jpg", i, dir));
            }


            var detected = Aruco.findArucoMarkers(files);
            Matrix<double> camMat_left = new Emgu.CV.Matrix<double>(new[,] {
                {852.18, 0,975.84},
                {0,853.05,525.63},
                {0,0,1}
            });
            Matrix<double> dist_left = new Emgu.CV.Matrix<double>(new[] { -.2807, .124, .0005073, -.03291 });

            var K = camMat_left;
            foreach (KeyValuePair<string, IEnumerable<ArucoMarker>> kvp in detected) {
                var file = kvp.Key;
                var markers = kvp.Value;

            }

            Matrix essential = K.Transpose() * F * K;
            var decomp = new SVD<double>(essential);
            var U = decomp.U;
            var Vt = decomp.Vt;

            var R1 = U * W * Vt;
            var R2 = U * W.Transpose() * Vt;
            var T1 = U.GetCol(2);
            var T2 = -1 * U.GetCol(2);

            Matrix[] Ps = new Matrix[4];

            for (int i = 0; i < 4; i++)
                Ps[i] = new Matrix(3, 4);

            CVI.HConcat(R1, T1, Ps[0]);
            CVI.HConcat(R1, T2, Ps[1]);
            CVI.HConcat(R2, T1, Ps[2]);
            CVI.HConcat(R2, T2, Ps[3]);

            var KPs = new Matrix[4];
            KPs[0] = K * Ps[0];
            KPs[1] = K * Ps[1];
            KPs[2] = K * Ps[2];
            KPs[3] = K * Ps[3];


            var KP0 = K * new Matrix(new double[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 } });

            for (int i = 0; i < 4; i++) {


                Matrix<float> output_hom = new Matrix<float>(4, punten1px.Size);
                VectorOfPoint3D32F output_3d = new VectorOfPoint3D32F();

                CVI.TriangulatePoints(KP0, KPs[i], punten1px, punten2px, output_hom);
                CVI.ConvertPointsFromHomogeneous(output_hom, output_3d);






            }


            CVI.FindEssentialMat();
            CVI.TriangulatePoints();
             * */
        }

        public static Tuple<MCvPoint3D32f, PointF, int> find3dmarker(Marker2d marker2d, IEnumerable<Marker> markers3d) {
            foreach (var marker3D in markers3d) {
                if (marker2d.ID == marker3D.ID) {
                    return new Tuple<MCvPoint3D32f, PointF, int>(new MCvPoint3D32f((float)marker3D.X,(float)marker3D.Y,(float)marker3D.Z),marker2d.PointF,marker2d.ID);
                }
            }
            return null;
        }

        static void testEosCamera() {
            Scene scene = new Scene();

            var markers1 = IO.MarkersFromFile(@"C:\Users\jens\Desktop\calibratie\calibratieruimte_opname_123.txt");
            scene.AddRange(markers1);

            var markersscene = scene.getIE<Marker>().ToArray();

            var dirzhang = @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\5D\zhang.6.9.55\";
            ZhangCalibration zc = new ZhangCalibration();
            zc.LoadImages(Directory.GetFiles(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\5D\zhang.6.9.55\").ToList(),new System.Drawing.Size(6,9));
            var camMat = new Matrix(new double[,] {
                {3689.7165,0,2816.1568}, 
                {0,3693.6683,1839.1889},
                {0,0,0}
            });
            Matrix dist;
            zc.CalibrateCV(new ChessBoard(6, 9, 55), out camMat, out dist);


           
            
            string[] files = Directory.GetFiles(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\5D\Opname1\").ToArray();
            var eos5daruco =
                Aruco.findArucoMarkers(files, @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\5D\Opname1\detected\", 1);
            var filesdetected = files.Select(x => Path.Combine(Path.GetDirectoryName(x), "detected", Path.GetFileName(x) + "detected.jpg")).ToArray();

            var points = eos5daruco.Select(x => x.Value.Select(y => find3dmarker(y, markersscene)).Where(y=>y!=null).ToArray()).ToArray();
            int i = -1;
            foreach (var ptnfoto in points) {
                i++;
                var points3d = ptnfoto.Select(x => x.Item1).ToArray();
                var points2d = ptnfoto.Select(x => x.Item2).ToArray();
                var ids = ptnfoto.Select(x => x.Item3).ToArray();

                var outr_left = new Mat();
                var outt_left = new Mat();
                var inliers = new Mat();
                var b = CVI.SolvePnPRansac(new VectorOfPoint3D32F(points3d), new VectorOfPointF(points2d),
                    camMat, dist, outr_left, outt_left, false, 100, 50, .8, inliers, SolvePnpMethod.UPnP);
                var projection = CVI.ProjectPoints(points3d, outr_left, outt_left, camMat, dist);

                var intr = new IntrinsicCameraParameters(4);
                intr.DistortionCoeffs = dist;
                intr.IntrinsicMatrix = camMat;
                var ext = CameraCalibration.SolvePnP(points3d, points2d, intr, SolvePnpMethod.UPnP);
                var reprojection_left = CameraCalibration.ProjectPoints(points3d, ext, intr);

                List<PointF> residuals_left = points2d.Select((t, j) => new PointF(t.X - reprojection_left[j].X, t.Y - reprojection_left[j].Y)).ToList();

                //reprojection_left = CVI.ProjectPoints(L.Select(x => x.Item1).ToArray(), outr_left, outt_left, camMat_left, dist_left);

                var s = filesdetected[i];
                IO.MarkersToFile(residuals_left.ToArray(), ids.ToArray(), s + "RESIDUALS.txt");
                drawReprojection(ref s, reprojection_left, residuals_left, ids.Select(x => x.ToString()));
                IO.MarkersToFile(points2d, ids.ToArray(), s + "2dDist.txt");
                var undist2d = new VectorOfPointF();
                CVI.UndistortPoints(new VectorOfPointF(points2d), undist2d, camMat, dist);
                IO.MarkersToFile(undist2d.ToArray(), ids.ToArray(), s + "2dUndistoredCameraCoord.txt");
                CVI.PerspectiveTransform(undist2d, undist2d, camMat);
                IO.MarkersToFile(undist2d.ToArray(), ids.ToArray(), s + "2dUndistored.txt");
                IO.MarkersToFile(points3d, ids.ToArray(), s + "3d.txt");

                var outim1 = CVI.Imread(s);
                CVI.Undistort(CVI.Imread(s), outim1, camMat, dist);
                CVI.Imwrite(s + "undistorted.jpg", outim1);
            }

        }
        
        static void testStereoCamera() {

            Scene scene =  new Scene();

            var markers1 = IO.MarkersFromFile(@"C:\Users\jens\Desktop\calibratie\calibratieruimte_opname_123.txt");
            scene.AddRange(markers1);
            


            

            var detectedAruco = getLRFramesAruco(
                @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\links\2\",
                @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\rechts\2\",
                533, 689, 2.016214371053080730500085338795, 100, 533, 534).ToTupleList();

            var markersscene = scene.getIE<Marker>().ToArray();
            

            var points = new List<Tuple<
                    Tuple<string, List<Tuple<MCvPoint3D32f, PointF,int>>>,
                    Tuple<string, List<Tuple<MCvPoint3D32f, PointF,int>>>>>();



            Action<IEnumerable<Tuple<Tuple<string, List<ArucoMarker>>,
                Tuple<string, List<ArucoMarker>>>>> intersect = list => {
                    foreach (var stereoPair in list) {
                        var L = stereoPair.Item1;
                        var R = stereoPair.Item2;
                        L.Item2.IntersectLists(R.Item2, ((marker, arucoMarker) => marker.ID == arucoMarker.ID));
                        
                    }
                };


            var detectedStereos = detectedAruco as Tuple<Tuple<string, List<ArucoMarker>>, Tuple<string, List<ArucoMarker>>>[] ?? detectedAruco.ToArray();
            //intersect(detectedStereos);
            
            Func<IEnumerable<Marker>, IEnumerable<ArucoMarker>, IEnumerable<Tuple<MCvPoint3D32f, PointF,int>>> find =
                (markers3d, arucomarkers) => {
                    List<Tuple<MCvPoint3D32f, PointF,int>> r = new List<Tuple<MCvPoint3D32f, PointF,int>>();
                    foreach (var arucomarker in arucomarkers) {
                        var scenemarker = markers3d.FirstOrDefault(x => x.ID == arucomarker.ID);
                        if (scenemarker != null) {
                            r.Add(new Tuple<MCvPoint3D32f, PointF,int>(
                                new MCvPoint3D32f((float) scenemarker.Pos.X, (float) scenemarker.Pos.Y,(float) scenemarker.Pos.Z),
                                new PointF(arucomarker.Corner1.X, arucomarker.Corner1.Y),
                                arucomarker.ID));
                        }
                    }
                    return r;
                };

            foreach (Tuple<Tuple<string, List<ArucoMarker>>, Tuple<string, List<ArucoMarker>>> detectedStereo in detectedStereos) {
                //per fotopaar
                var minCount = 10;
                var L = find(markersscene, detectedStereo.Item1.Item2);
                var R = find(markersscene, detectedStereo.Item2.Item2);

                if (L.Count() >= minCount && R.Count() >= minCount) {
                    points.Add(new Tuple<Tuple<string, List<Tuple<MCvPoint3D32f, PointF,int>>>, Tuple<string, List<Tuple<MCvPoint3D32f, PointF,int>>>>(
                        new Tuple<string, List<Tuple<MCvPoint3D32f, PointF,int>>>(detectedStereo.Item1.Item1,L.ToList()),
                        new Tuple<string, List<Tuple<MCvPoint3D32f, PointF,int>>>(detectedStereo.Item2.Item1,R.ToList())));
                }
            }
            


            Matrix<double> camMat_left = new Emgu.CV.Matrix<double>(new[,] {
                {852.18, 0,975.84},
                {0,853.05,525.63},
                {0,0,1}
            });
            Matrix<double> dist_left = new Emgu.CV.Matrix<double>(new[] { -.2807, .124, .0005073, -.03291 });
            //dist_left = new Emgu.CV.Matrix<double>(new double[] { -0, 0, 0, -0 });
            Matrix<double> dist_zero = new Emgu.CV.Matrix<double>(new double []{ -0, 0, 0, -0 });
            Matrix<double> camMat_right = new Emgu.CV.Matrix<double>(new[,] {
                {965.89, 0,918.07},
                {0,966.58,481.03},
                {0,0,1}
            });
            Matrix<double> dist_right = new Emgu.CV.Matrix<double>(new[] { -.29344, .15113, .001330, -.048524 });
            //dist_right = new Emgu.CV.Matrix<double>(new double[] { -0, 0, 0, -0 });


            var rot = new Matrix<double>(3, 3);
            var trans = new Matrix<double>(3, 1);
            var ess = new Matrix<double>(3, 3);
            var fundamental = new Matrix<double>(3, 3);
            MCvTermCriteria termcriteria = new MCvTermCriteria();

            List<MCvPoint3D32f[]> test3d = new List<MCvPoint3D32f[]>();
            List<PointF[]> test2dL = new List<PointF[]>();
            List<PointF[]> test2dR = new List<PointF[]>();

            List<Tuple<MCvPoint3D32f, PointF, int>[]> testL = new List<Tuple<MCvPoint3D32f, PointF, int>[]>();
            List<Tuple<MCvPoint3D32f, PointF, int>[]> testR = new List<Tuple<MCvPoint3D32f, PointF, int>[]>();

            List<string> fileL = new List<string>();
            List<string> fileR = new List<string>();
            
            for (int i = 0; i < points.Count; i++) {
                var L = points[i].Item1;
                var R = points[i].Item2;

                

                List<int> markeridsL = L.Item2.Select(x => x.Item3).ToList();
                List<int> markeridsR = R.Item2.Select(x => x.Item3).ToList();
                var intersection = markeridsL.Intersect(markeridsR);

                var int2dL = L.Item2.Where(x => intersection.Contains(x.Item3)).OrderBy(x => x.Item3).ToArray();
                var int2dR = R.Item2.Where(x => intersection.Contains(x.Item3)).OrderBy(x => x.Item3).ToArray();
                if (int2dL.Length < 8 || int2dR.Length < 8) {
                    continue;
                }
                testL.Add(int2dL);
                testR.Add(int2dR);
                for (int j = 0; j < int2dL.Length; j++) {
                    var id2dl = int2dL[j].Item3;
                    var id2dr = int2dR[j].Item3;
                    if (id2dr != id2dl) {
                        int tst = 54;
                    }
                }
                for (int j = 0; j < int2dL.Length; j++) {
                    var p1 = int2dL[j].Item1;
                    var p2 = int2dR[j].Item1;
                    if (p1.X != p2.X || p1.Y != p2.Y || p1.Z != p2.Z) {
                        int tst = 54;
                    }
                }
                fileL.Add(points[i].Item1.Item1);
                fileR.Add(points[i].Item2.Item1);
                test3d.Add(int2dL.Select(x => x.Item1).ToArray());
                test2dL.Add(int2dL.Select(x => x.Item2).ToArray());
                test2dR.Add(int2dR.Select(x => x.Item2).ToArray());

            }
            for (int i = 0; i < test3d.Count; i++) {
                if (test3d[i].Count() < 8 || test2dL[i].Count() < 8 || test2dR[i].Count() < 8) {

                    test3d.RemoveAt(i);
                    test2dL.RemoveAt(i);
                    test2dR.RemoveAt(i);
                    i--;
                }
            }


            /*CVI.StereoCalibrate(test3d.ToArray(), test2dL.ToArray(), test2dR.ToArray(),
                camMat_left, dist_left, camMat_right, dist_right,
                new System.Drawing.Size(1920, 1080), rot, trans, ess, fundamental,
                CalibType.UserIntrinsicGuess, termcriteria);*/


            for (int i = 0; i < points.Count; i++) {
                //var L = points[i].Item1.Item2;
                //var R = points[i].Item2.Item2;
                var L = testL[i];
                var R = testR[i];
                var dirL = fileL[i];
                var dirR = fileR[i];

                var outr_right = new Matrix<double>(1, 3);
                var outr_left = new Mat();

                var outt_right = new Matrix<double>(1, 3);
                var outt_left = new Mat();

                var points3d = L.Select(x => x.Item1).ToArray();
                var points2d = L.Select(x => x.Item2).ToArray();
                var ids = L.Select(x => x.Item3);

                var inliers = new Mat();
                var b = CVI.SolvePnPRansac(new VectorOfPoint3D32F(points3d), new VectorOfPointF(points2d),
                    camMat_left, dist_left, outr_left, outt_left, false, 100, 50,.8, inliers, SolvePnpMethod.UPnP);
                var projection = CVI.ProjectPoints(points3d, outr_left, outt_right, camMat_left, dist_left);
                
                var intr = new IntrinsicCameraParameters(4);
                intr.DistortionCoeffs = dist_left;
                intr.IntrinsicMatrix = camMat_left;
                var ext = CameraCalibration.SolvePnP(points3d, points2d, intr,SolvePnpMethod.UPnP);
                var reprojection_left = CameraCalibration.ProjectPoints(points3d, ext, intr);
                
                List<PointF> residuals_left = points2d.Select((t, j) => new PointF(t.X - reprojection_left[j].X, t.Y - reprojection_left[j].Y)).ToList();
                
                //reprojection_left = CVI.ProjectPoints(L.Select(x => x.Item1).ToArray(), outr_left, outt_left, camMat_left, dist_left);

                var s = dirL;
                IO.MarkersToFile(residuals_left.ToArray(), ids.ToArray(), s + "RESIDUALS.txt");
                drawReprojection(ref s, reprojection_left, residuals_left, ids.Select(x=>x.ToString()));
                IO.MarkersToFile(points2d, ids.ToArray(), s + "2dDist.txt");
                var undist2d = new VectorOfPointF();
                CVI.UndistortPoints(new VectorOfPointF(points2d), undist2d, camMat_left,dist_left);
                IO.MarkersToFile(undist2d.ToArray(), ids.ToArray(), s + "2dUndistoredCameraCoord.txt");
                CVI.PerspectiveTransform(undist2d, undist2d, camMat_left);
                IO.MarkersToFile(undist2d.ToArray(), ids.ToArray(), s + "2dUndistored.txt");
                IO.MarkersToFile(points3d, ids.ToArray(), s + "3d.txt");

                var outim1 = CVI.Imread(s);
                CVI.Undistort(CVI.Imread(s), outim1, camMat_left, dist_left);
                CVI.Imwrite(s + "undistorted.jpg", outim1);
                //var points2dundist = new VectorOfPointF();
                //CVI.UndistortPoints(new VectorOfPointF(points2d), points2dundist, camMat_left, dist_left);
                //IO.MarkersToFile(points2dundist.ToArray(), detectedArucoArr[i].Item1.Item2.Select(x => x.ID).ToArray(), s + "undistorted.jpg" + ".txt");

                points3d = R.Select(x => x.Item1).ToArray();
                points2d = R.Select(x => x.Item2).ToArray();
                ids = R.Select(x => x.Item3);
                inliers = new Emgu.CV.Mat();
                b = CVI.SolvePnPRansac(new VectorOfPoint3D32F(points3d), new VectorOfPointF(points2d),
                    camMat_right, dist_right, outr_right, outt_right, false, 20, 50, .8, inliers,SolvePnpMethod.UPnP);

                intr = new IntrinsicCameraParameters(4);
                intr.DistortionCoeffs = dist_zero;
                intr.IntrinsicMatrix = camMat_right;
                ext = Emgu.CV.CameraCalibration.SolvePnP(points3d, points2d, intr);
                var reprojection_right = CameraCalibration.ProjectPoints(points3d, ext, intr);

                //var reprojection_right = CVI.ProjectPoints(R.Select(x => x.Item1).ToArray(), outr_right, outt_right, camMat_right, dist_right);
                List<PointF> residuals_right = points2d.Select((t, k) => new PointF(t.X - reprojection_right[k].X, t.Y - reprojection_right[k].Y)).ToList();
                s = dirR;
                drawReprojection(ref s, reprojection_right, residuals_right, ids.Select(x=>x.ToString()));
                IO.MarkersToFile(points2d, ids.ToArray(), s + "2dDist.txt");
                undist2d = new VectorOfPointF();
                CVI.UndistortPoints(new VectorOfPointF(points2d), undist2d, camMat_right, dist_right);
                IO.MarkersToFile(undist2d.ToArray(), ids.ToArray(), s + "2dUndistoredCameraCoord.txt");
                CVI.PerspectiveTransform(undist2d, undist2d, camMat_right);
                IO.MarkersToFile(undist2d.ToArray(), ids.ToArray(), s + "2dUndistored.txt");
                IO.MarkersToFile(points3d, ids.ToArray(), s + "3d.txt");
                var outim = CVI.Imread(s);
                CVI.Undistort(CVI.Imread(s), outim, camMat_right, dist_right);
                CVI.Imwrite(s + "undistorted.jpg", outim);
                //points2dundist = new VectorOfPointF();
                //CVI.UndistortPoints(new VectorOfPointF(points2d), points2dundist, camMat_right, dist_right);
                //IO.MarkersToFile(points2dundist.ToArray(), detectedArucoArr[i].Item2.Item2.Select(x => x.ID).ToArray(), s + "undistorted.jpg" + ".txt");


                //leftrots.Add(outr_left);
                //rightrots.Add(outr_right);
                //leftTrans.Add(outt_left);
                //rightTrans.Add(outt_right);

                var r =new Matrix<double>(3, 3);
                //CVI.Transpose(outr_left.RotationMatrix, r);

                //var posl = r*outt_left;

                //CVI.Transpose(outr_right.RotationMatrix, r);
                //var posr = r*outt_right;
                //var dst = posl - posr;
                //double d = dst[0, 0] * dst[0, 0];
                //d += dst[1, 0] * dst[1, 0];
                //d += dst[2, 0] * dst[2, 0];

                //d = Math.Sqrt(d);

                //dstanc.Add(d);

                //cameradist.Add(dst);
            }
            for (int i = 0; i < points.Count; i++) {
                var L = points[i].Item1.Item2;
                var R = points[i].Item2.Item2;

                
            }
            //CVI.FindFundamentalMat();
            

            
        
        }

        static void testFeatureDetection() {
            Log.ToConsole = true;

            var im1 = @"C:\Users\jens\Desktop\calibratie\Huawei p9\aruco\stereo test\IMG_20160721_230236.jpg";
            var im2 = @"C:\Users\jens\Desktop\calibratie\Huawei p9\aruco\stereo test\IMG_20160721_230320.jpg";
            var imout = @"C:\Users\jens\Desktop\calibratie\Huawei p9\aruco\stereo test\IMG_out.jpg";
            //StereoCalc.test(im1, im2, imout);

            long time;
            Emgu.CV.Util.VectorOfKeyPoint test = new VectorOfKeyPoint();
            Emgu.CV.Util.VectorOfKeyPoint key1, key2;
            Emgu.CV.Util.VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
            Emgu.CV.Mat mask, out2;
            Emgufeat.FindMatch(Emgu.CV.CvInvoke.Imread(im1), Emgu.CV.CvInvoke.Imread(im2), out time, out key1, out key2, matches, out mask, out out2);

            List<Tuple<MDMatch, MKeyPoint, MKeyPoint>> listmatchbest = new List<Tuple<MDMatch, MKeyPoint, MKeyPoint>>();
            for (int i = 0; i < matches.Size; i++) {
                var arrayOfMatches = matches[i].ToArray();
                if (mask.GetData(i)[0] == 0)
                    continue;
                Tuple<MDMatch, MKeyPoint, MKeyPoint> best = new Tuple<MDMatch, MKeyPoint, MKeyPoint>(new MDMatch { Distance = Single.MaxValue }, new MKeyPoint(), new MKeyPoint());
                foreach (var match in arrayOfMatches) {
                    var matchingModelKeyPoint = key1[match.TrainIdx];
                    var matchingObservedKeyPoint = key2[match.QueryIdx];
                    if (best.Item1.Distance > match.Distance) {
                        best = new Tuple<MDMatch, MKeyPoint, MKeyPoint>(match, matchingModelKeyPoint, matchingObservedKeyPoint);
                    }
                }
                listmatchbest.Add(best);
            }
            Emgu.CV.Mat F = new Emgu.CV.Mat();

            Emgu.CV.CvInvoke.FindFundamentalMat(
                new Emgu.CV.Util.VectorOfPointF(listmatchbest.Select(x => x.Item2.Point).ToArray()),
                new Emgu.CV.Util.VectorOfPointF(listmatchbest.Select(x => x.Item3.Point).ToArray()), F
                );

            var K = new Emgu.CV.Matrix<double>(PinholeCamera.getTestCameraHuawei().Intrinsics.Mat);

            var W = new Emgu.CV.Matrix<double>(new double[] {
                0,-1,0,
                1,0,0,
                0,0,1
            });
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void  Main() {
            /*Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            */
            testEosCamera();
            testStereoCamera();

            ZhangCalibration c;
            
          



            ZhangCalibration stereotest = new ZhangCalibration();
            List<string> badl = new List<string>();
            List<string> badr = new List<string>();
            //stereotest.LoadImages(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\rechts\1\", new Size(6, 9), links, badl);
            //stereotest.LoadImages2(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\rechts\1\", new Size(6, 9), rechts, badr);

            List<int> badindices = new List<int>();

            for (int i = 0; i < stereotest.images.Count; i++) {
                if (stereotest.images[i] == null || stereotest.images2[i] == null) {
                    stereotest.images.RemoveAt(i);
                    stereotest.images2.RemoveAt(i);
                    i--;
                }
                
            }



            

            /*
            ZhangCalibration c = new ZhangCalibration();
            var csize = new Size(6, 8);
            c.LoadImages(@"C:\Users\jens\Desktop\calibratie\fotos\", csize);
            PinholeCamera cam;
            c.Calibrate(new ChessBoard(6,8,30),out cam);
            Console.ReadLine();*/
            
            //var pic = PhotoProvider.getSingleBitmap(@"C:\Users\jens\Desktop\canon 60d\patterns\5x7\IMG_3235.JPG", 4);
            //pic.Save(@"C:\Users\jens\Desktop\canon 60d\patterns\5x7\TEFEF.jpg");
            //@"C:\Users\jens\Desktop\calibratie\Fotos_gedownload_door_AirDroid (1)\zhang\7x9\"
            //arucotest2.arucotest();
            Version version = System.Environment.Version;
            int build = version.Build;
            int major = version.Major;
            int minor = version.Minor;
            int revision = System.Environment.Version.Revision;
            Console.Write(".NET Framework version: ");
            System.Console.WriteLine("{0}.{1}.{2}.{3}", 
            build, major, minor, revision);

            //CeresSimulation.ceresSolveAruco();
            string f = @"C:\Users\jens\Desktop\calibratie\foto2\";


            //ArUcoNET.Aruco.CreateMarker(1, 400, "1.400.jpg");

            
            var dir = @"C:\Users\jens\Desktop\calibratie\canon sigma lens\test aruco platen\";
            dir = @"C:\Users\jens\Desktop\calibratie\foto2\";
            
            var dirDetected = Path.Combine(dir, @"\detected\");
            Directory.CreateDirectory(dirDetected);

            PhotoProvider prov = new PhotoProvider(dir);
            var files = Directory.GetFiles(dir);
                
            var res = Aruco.findArucoMarkers(files, Path.Combine(Path.GetDirectoryName(files[0]), "detected"),8);


            

            return;
        }

        public static void testRectify() {
            var f = @"C:\Users\jens\Desktop\calibratie\Huawei p9\test2.jpg";
            Dictionary<int, Point2d> markerPos = new Dictionary<int, Point2d>();
            List<Point2d> wereld = new List<Point2d>();
            List<Point2d> pixel = new List<Point2d>();
            var markers = ArUcoNET.Aruco.FindMarkers(f);
            foreach (var m in markers) {
                if (markerPos.ContainsKey(m.ID)) {
                    wereld.Add(markerPos[m.ID]);
                    //pixel.Add(m.Corner1.to2d());
                }
            }
            //var H = Cv2.FindHomography(pixel, wereld, HomographyMethods.LMedS);
        }


    }
}
