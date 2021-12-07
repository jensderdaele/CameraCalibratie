#define LINQ

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using cameracallibratie;
using System.Windows;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using ceresdotnet;
using Calibratie;

using ArUcoNET;
using PhotoscanIO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Flann;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.XFeatures2D;
using GeoAPI.CoordinateSystems;
using OpenTK.Graphics.OpenGL;
using ProjNet.Converters.WellKnownText;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using Color = System.Drawing.Color;
using Pen = System.Drawing.Pen;

using Matrix = Emgu.CV.Matrix<double>;
using CVI = Emgu.CV.CvInvoke;
using FontFamily = System.Drawing.FontFamily;
using FontStyle = System.Drawing.FontStyle;
using Point = System.Windows.Point;
using Point2d = Emgu.CV.Structure.MCvPoint2D64f;
using Point3f = Emgu.CV.Structure.MCvPoint3D32f;
using Size = System.Drawing.Size;
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
        public int Length { get; private set; }

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

        #region andere

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

        #endregion

        #region trenchview

        public static void TestFeatureDetection() {
            var dir = @"D:\3dtrenchview\autofeature detection\test data";

            List<Feature2D> featureDetectionAlgorithms = new List<Feature2D> {
                new Emgu.CV.XFeatures2D.SIFT(),
                new Emgu.CV.XFeatures2D.SURF(400),
                new Emgu.CV.Features2D.AKAZE(),
                //new AgastFeatureDetector(), no initialize
                new Brisk(),
                //new Emgu.CV.XFeatures2D.DAISY(), crash @ Detect
                new GFTTDetector(),
                new Emgu.CV.Features2D.KAZE(),
                //new Freak(), crash @ Detect
                //new Emgu.CV.Features2D.MSERDetector(),
                //new LATCH(), crash @ detected
                //new LUCID(), crash @ detected
                new Emgu.CV.Features2D.ORBDetector(),
                new FastDetector(),
                //new BriefDescriptorExtractor(), crash @ detected
            };



            foreach (var algorithm in featureDetectionAlgorithms) {
                foreach (var file in Directory.EnumerateFiles(dir)) {
                    var im = CVI.Imread(file);
                    var algName = algorithm.GetType().Name;
                    var keypoints = algorithm.Detect(im);
                    var imout = new Image<Rgb, Int32>(im.Size);
                    Emgu.CV.Features2D.Features2DToolbox.DrawKeypoints(im, new Emgu.CV.Util.VectorOfKeyPoint(keypoints), imout, new Bgr(Color.Gold), Features2DToolbox.KeypointDrawType.DrawRichKeypoints);
                    Directory.CreateDirectory(Path.Combine(dir, algName));
                    imout.Save(Path.Combine(dir, algName, Path.GetFileName(file)));

                }
            }

        }
        public static void TestFeatureDetectionGCP() {
            ArUcoNET.SIFTTEST.test();
            var img1s = @"D:\3dtrenchview\autofeature detection\test data\G0026845.JPG";

            Rectangle roi = new Rectangle(1125, 1941, 517, 321);
            PointF gcp1 = new PointF(1310,2074);
            var img1 = new Image<Rgb, Byte>(new Bitmap(img1s));
            //var img1 = CVI.Imread(img1s);

            img1.ROI = roi;
            //CVI.cvSetImageROI(img1, roi);



            var img2s = @"D:\3dtrenchview\autofeature detection\test data\G0026846.JPG";
            var img2 = CVI.Imread(img2s);

            Feature2D algorithm = new Emgu.CV.XFeatures2D.SIFT();
            VectorOfKeyPoint kpts1 = new VectorOfKeyPoint();
            VectorOfKeyPoint kpts1des = new VectorOfKeyPoint();
            Mat d = new Mat();
            Mat d2 = new Mat();
            algorithm.DetectAndCompute(img1, null, kpts1, d, false);



            BriefDescriptorExtractor descriptor = new BriefDescriptorExtractor(64);
            //descriptor.Compute(img1, kpts1, d);



            VectorOfKeyPoint kpts2 = new VectorOfKeyPoint();
            algorithm.DetectRaw(img2, kpts2);
            descriptor.Compute(img2, kpts2, d2);
            
            Emgu.CV.Features2D.FlannBasedMatcher flann = new FlannBasedMatcher(new KdTreeIndexParams(5),new SearchParams(50));
            
        }
        public static void TestBlurDetection() {
            var dir = @"D:\3dtrenchview\opnames\17.03.21.Roeselare.Veldstraat\4\";
            var files = Directory.GetFiles(dir).ToList().GetRange(0, 25);
            foreach (var file in files) {
                for (int i = 0; i < 10; i++) {
                    Console.Write(Path.GetFileName(file).PadRight(16));
                    var im = CVI.Imread(file);
                    var r = Blur.BlurDetect(im, 80 + 20 * i);
                    im.Dispose();
                }
            }
        }
        #endregion

        #region selfcalibration

        static void testSFM() {
            var detectedAruco = getLRFramesAruco(
               @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\stereo.canon\left\",
               @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\stereo.canon\right\",
               1, 1, 1, 1, 1, 2).ToTupleList().ToList();

            //initiele triangulatie
            var stereopair = detectedAruco[0];

            var ptsl = stereopair.Item1.Item2;
            var ptsr = stereopair.Item2.Item2;

            var dirL = stereopair.Item1.Item1;
            var dirR = stereopair.Item2.Item1;

            var corrL = new List<ArucoMarker>();
            var corrR = new List<ArucoMarker>();

            foreach (var pl in ptsl) {
                foreach (var pr in ptsr) {
                    if (pl.ID == pr.ID) {
                        corrL.Add(pl);
                        corrR.Add(pr);
                    }
                }
            }

            Matrix F = new Matrix(3, 3);
            var W = new Matrix(new double[,] {
                {0, -1, 0},
                {1, 0, 0},
                {0, 0, 1}
            });



            var L2d = new VectorOfPointF(corrL.Select(x => x.PointF).ToArray());
            var R2d = new VectorOfPointF(corrR.Select(x => x.PointF).ToArray());

            CVI.FindFundamentalMat(L2d, R2d, F);

            var E = CameraIntrinsics.EOS1000D.cvmat.Transpose() * F * CameraIntrinsics.EOS5DMARKII.cvmat;
            PinholeCamera left = new PinholeCamera(CameraIntrinsics.EOS5DMARKII);
            PinholeCamera right = new PinholeCamera(CameraIntrinsics.EOS1000D);

            SVD<double> svd = new SVD<double>(E);


            var K = right.Intrinsics.cvmat;

            var decomp = new SVD<double>(E);
            var U = decomp.U;
            var Vt = decomp.Vt;

            var R1 = U * W * Vt;
            var R2 = U * W.Transpose() * Vt;
            var T1 = 1 * U.GetCol(2);
            var T2 = -1 * U.GetCol(2);

            var rot = R1.toRotationVector();
            var rot2 = R2.toRotationVector();
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

            foreach (var kP in KPs) {
                var out4d = new Matrix(4, corrL.Count);
                var out4d2 = new Matrix(corrL.Count, 4);
                CVI.TriangulatePoints(left.ProjectionMatrix, kP, L2d, R2d, out4d);
                CVI.TriangulatePoints(left.ProjectionMatrix, kP, L2d, R2d, out4d2);
                CVI.TriangulatePoints(left.ProjectionMatrix, kP, R2d, L2d, out4d2);
                CVI.TriangulatePoints(left.ProjectionMatrix, kP, R2d, L2d, out4d);
            }
        }

        #endregion
        
        #region gewone kalibratie
        public static Dictionary<string, string> GetLRFrames(string dirL, string dirR, int KeyFrameL, int KeyFrameR, double LR, int intervalL, int startL, int stopL) {

            var r = new Dictionary<string, string>();
            
            int startframeL = KeyFrameL;
            int startframeR = KeyFrameR;
            int fps_links = intervalL;
            Func<int, int> frameR = frameL => (int)((frameL - startframeL) * LR + startframeR);



            int framel = startL;
            int framer = frameR(framel);


            while (framel < stopL) {
                r.Add(string.Format(@"{1}\{0:0}.jpg", framel, dirL),
                    string.Format(@"{1}\{0:0}.jpg", framer, dirR));
                framel += intervalL;
                framer = frameR(framel);
            }
            return r;
        }
        public static Dictionary<Tuple<string, List<ArucoMarker>>, Tuple<string, List<ArucoMarker>>> getLRFramesAruco(
            string dirL, string dirR,
            int KeyFrameL,
            int KeyFrameR, double LR, int intervalL, int startL, int stopL) {
            var files = GetLRFrames(dirL, dirR, KeyFrameL, KeyFrameR, LR, intervalL, startL, stopL).ToList();
            var detectedLFile = Path.Combine(dirL, "detected");
            var detectedRFile = Path.Combine(dirR, "detected");
            //var L = Aruco.findArucoMarkers(files.Keys, detectedLFile,8).Select(x => new Tuple<string, List<ArucoMarker>>(x.Key, x.Value.ToList())).ToList();
            //var R = Aruco.findArucoMarkers(files.Values, detectedRFile,8).Select(x => new Tuple<string, List<ArucoMarker>>(x.Key, x.Value.ToList())).ToList();


            var r = new Dictionary<Tuple<string, List<ArucoMarker>>, Tuple<string, List<ArucoMarker>>>();
            EXT.ActionMultiThread(i => {
                Console.WriteLine("Searching markers in stereopair {0}   {1}", Path.GetFileName(files[i].Key), Path.GetFileName(files[i].Value));
                var filel = Path.Combine(dirL, "detected", Path.GetFileName(files[i].Key) + "detected.jpg");
                var filer = Path.Combine(dirR, "detected", Path.GetFileName(files[i].Value) + "detected.jpg");
                r.Add(
                    new Tuple<string, List<ArucoMarker>>(filel, ArUcoNET.Aruco.FindMarkers(files[i].Key, filel).ToList()),
                    new Tuple<string, List<ArucoMarker>>(filer, ArUcoNET.Aruco.FindMarkers(files[i].Value, filer).ToList()));
            }, files.Count);



            /*for (int i = 0; i < files.Count; i++) {
                Console.WriteLine("Searching markers in stereopair {0}   {1}", Path.GetFileName(files[i].Key), Path.GetFileName(files[i].Value));
                r.Add(
                    new Tuple<string, List<ArucoMarker>>(files[i].Key,ArUcoNET.Aruco.FindMarkers(files[i].Key).ToList()),
                    new Tuple<string, List<ArucoMarker>>(files[i].Value,ArUcoNET.Aruco.FindMarkers(files[i].Value).ToList()));
                //Console.WriteLine("stereopair {0} {1}", Path.GetFileName(files.Keys.ElementAt(i)), Path.GetFileName(files.Values.ElementAt(i)));
                //r.Add(L[i], R[i]);
            }*/
            return r;
        }
        #endregion

        #region drawing



        static void drawReprojection(ref string infile, IEnumerable<PointF> points, IEnumerable<PointF> error, IEnumerable<string> txt = null) {
            var outfile = infile + "reprojection.jpg";

            drawReprojection(infile, outfile, points, error, txt);

            infile = outfile;
        }
        static void drawMarkers(string infile, string outfile, IEnumerable<Marker2d> markers) {
            var bitmap = Bitmap.FromFile(infile);
            var gfx = Graphics.FromImage(bitmap);
            var pen = new Pen(Color.Coral);

            IEnumerable<PointF> points = markers.Select(x => x.PointF);
            IEnumerable<string> txt = markers.Select(x => x.ID.ToString());

            var pointFs = points as PointF[] ?? points.ToArray();
            string[] txtarr = null;
            if (txt != null) txtarr = txt as string[] ?? txt.ToArray();
            var rect = new Rect(new Point(0, 0), new Point(bitmap.Width, bitmap.Height));
            var font = new Font(FontFamily.GenericMonospace, 18, FontStyle.Bold);
            for (int i = 0; i < pointFs.Count(); i++) {
                var p = pointFs[i];

                float width = 10;
                float height = 10;
                try {
                    pen.Brush = new SolidBrush(pen.Color);
                    gfx.DrawEllipse(pen, p.X - width / 2, p.Y - height / 2, width, height);
                    pen.Width = 3;
                    if (txt != null) { gfx.DrawString(txtarr[i], font, new SolidBrush(Color.Chocolate), p.X, p.Y); }
                }
                catch {

                }
            }
            bitmap.Save(outfile, ImageFormat.Bmp);
            gfx.Dispose();
            bitmap.Dispose();
        }
        static void drawReprojection(string infile, string outfile, IEnumerable<PointF> points, IEnumerable<PointF> error, IEnumerable<string> txt = null) {
            var bitmap = Bitmap.FromFile(infile);
            var gfx = Graphics.FromImage(bitmap);
            var pen = new Pen(Color.Coral);

            var pointFs = points as PointF[] ?? points.ToArray();
            var enumerable = error as PointF[] ?? error.ToArray();
            string[] txtarr = null;
            if (txt != null)
                txtarr = txt as string[] ?? txt.ToArray();
            var rect = new Rect(new Point(0, 0), new Point(bitmap.Width, bitmap.Height));
            var font = new Font(FontFamily.GenericMonospace, 18, FontStyle.Bold);
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
                        gfx.DrawEllipse(pen, p.X - width / 2, p.Y - height / 2, width, height);// p.X + e.X - width / 2, p.Y + e.Y - height / 2
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
            bitmap.Save(outfile, ImageFormat.Jpeg);
            gfx.Dispose();
            bitmap.Dispose();
            //File.Delete(file);
        }

        #endregion
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(System.Windows.Forms.Keys vKey);


        #region simulaties

        public static void multicamsim_ruisinvloed() {
            List<CeresSimulation> sims = new List<CeresSimulation>();

            Matrix<double> graphgem = null;
            var std3dlist = new double[] {.001, .0025, .005, .01, .025, .05, .1}; //meter
            var afstandlist = new double[] {1, 3, 5, 10}; //meter
            foreach (var afst in afstandlist) {
                foreach (var std3d in std3dlist) {
                    
                    int redocounter = 0;
                    Matrix<double> graph = null;
                    int totaalredo = 500;
                    for (int redo = 0; redo < totaalredo; redo++) {
                        redocounter++;
                        var sim = new CeresSimulation {
                            scene = Util.testScene_featuressimulation(afst,0, 100) 
                        };
                        sims.Add(sim);

                        sim.WorldRuisProvider = new GaussRuisProvider(0, std3d);

                        sim.SolveMultiCollection();
                        
                        //data voor matlab
                        var m = new Matrix(new[] {
                            std3d,
                            afst,
                            sim.lastReproj,
                        });

                        graph = graph ?? new Matrix<double>(m.Rows, 1);
                        graph += m;
                    }
                    graph /= redocounter;
                    graphgem = graphgem == null ? graph : graphgem.ConcateHorizontal(graph);

                }
                Matlab.SendMatrix(graphgem, "TESTRUISGOPROHERO3");
            }
        }

        public static void multicamsim_fotosinvloed() {
            List<CeresSimulation> sims = new List<CeresSimulation>();
            Matrix<double> graphgem = null;

            var featurecount = 20; //20 features per foto
            var fotoslist = new int[] { 2,3,4,5,6,8,10,12,15 }; //x-waarden (9)
            string matlab_matrixname = String.Format("TESTFOTOSVARIATIONGOPROHERO3_std{0}_dist{1}_std3d{2}featcount{3}", std, 5, 0, featurecount);

            foreach (var fotocount in fotoslist) {
                int redocounter = 0;
                Matrix<double> graph = null;
                int totaalredo = 1000;
                for (int redo = 0; redo < totaalredo; redo++) {
                    redocounter++;
                    var sim = new CeresSimulation {
                        //scene met 1 camera & features op 5+-0m 
                        scene = Util.testScene_fotossimulation(1, 0, featurecount, fotocount)
                    };
                    sims.Add(sim);

                    //Ruis instellen
                    sim.PixelRuisProvider = new GaussRuisProvider(0, std);
                    //sim.CameraModifier = new CameraModifier();

                    sim.SolveMultiCollection();

                    var camera_original = sim.OriginalValues.First().Value;//er is maar 1 interne param
                    var camera_calibrated = sim.OriginalValues.First().Key;//dus First() is ok
                    var intr_or = camera_original.Intrinsics;
                    var intr_cal = camera_calibrated.Intrinsics;

                    var camxerror = camera_calibrated.X - camera_original.X;
                    var camyerror = camera_calibrated.Y - camera_original.Y;
                    var camzerror = camera_calibrated.Z - camera_original.Z;

                    //data voor matlab; abs() nodig -> anders is gemiddelde 0
                    var m = new Matrix(new[]{
                        fotocount,featurecount,sim.lastReproj,
                        Math.Abs(intr_cal.fx - intr_or.fx),
                        Math.Abs(intr_cal.fy - intr_or.fy),
                        Math.Abs(intr_cal.cx-intr_or.cx),
                        Math.Abs(intr_cal.cy-intr_or.cy),
                        Math.Abs(intr_cal.DistortionR1-intr_or.DistortionR1),
                        Math.Abs(intr_cal.DistortionR2-intr_or.DistortionR2),
                        Math.Abs(intr_cal.DistortionR3-intr_or.DistortionR3),
                        Math.Abs(intr_cal.DistortionT1-intr_or.DistortionT1),
                        Math.Abs(intr_cal.DistortionT2-intr_or.DistortionT2),
                        Math.Abs(camxerror),Math.Abs(camyerror),Math.Abs(camzerror),
                        Math.Sqrt(camxerror*camxerror + camyerror*camyerror + camzerror*camzerror)});

                    graph = graph ?? new Matrix<double>(m.Rows, 1);
                    graph += m;
                }
                graph /= redocounter; //gemiddelde
                graphgem = graphgem == null ? graph : graphgem.ConcateHorizontal(graph);
                
                Matlab.SendMatrix(graphgem, matlab_matrixname); //naar matlab

            }
            var camoriginal = sims.First().OriginalValues.First().Value;

            var introriginal = sims.First().OriginalValues.First().Value.Intrinsics;
            var matoriginal = new Matrix(new[]{1,1,1,introriginal.fx, introriginal.fy, introriginal.cx, introriginal.cy,
                introriginal.DistortionR1, introriginal.DistortionR2, introriginal.DistortionR3,
                introriginal.DistortionT1, introriginal.DistortionT2,
                camoriginal.X,camoriginal.Y,camoriginal.Z,0});

            graphgem = graphgem.ConcateHorizontal(matoriginal); //laatste tabel = originele waarden
            Matlab.SendMatrix(graphgem, matlab_matrixname); //naar matlab
        }

        private static double std = 4;
        public static void multicamsim_featuresinvloed() {
            List<CeresSimulation> sims = new List<CeresSimulation>();
            Matrix<double> graphgem = null;
            //x-waarden
            var featurelist = new int[] {6, 8, 10, 12, 14, 16, 18, 20, 30, 40, 50, 60, 80};
            foreach (var featurecount in featurelist) {
                int redocounter = 0;
                Matrix<double> graph = null;
                int totaalredo = 2000; //- (int)(400 * ((double)(featurecount - featurelist.First()) / featurelist.Last()));
                for (int redo = 0; redo < totaalredo; redo++) {
                    redocounter++;
                    var sim = new CeresSimulation {
                        //scene met 1 camera & features op 5+-.5m 
                        scene = Util.testScene_featuressimulation(5,0.5,featurecount)
                    };
                    sims.Add(sim);

                    //Ruis instellen
                    sim.PixelRuisProvider = new GaussRuisProvider(0, std);
                    //sim.CameraModifier = new CameraModifier();
                    //sim.WorldRuisProvider = new GaussRuisProvider(0,.01);

                    sim.SolveMultiCollection();

                    var camera_original = sim.OriginalValues.First().Value;//er is maar 1 interne param
                    var camera_calibrated = sim.OriginalValues.First().Key;//dus First() is ok
                    var intr_or = camera_original.Intrinsics;
                    var intr_cal = camera_calibrated.Intrinsics;

                    var camxerror = camera_calibrated.X - camera_original.X;
                    var camyerror = camera_calibrated.Y - camera_original.Y;
                    var camzerror = camera_calibrated.Z - camera_original.Z;

                    //data voor matlab; abs() nodig -> anders is gemiddelde 0
                    var m =  new Matrix(new[]{
                        featurecount,sim.lastReproj,
                        Math.Abs(intr_cal.fx - intr_or.fx),
                        Math.Abs(intr_cal.fy - intr_or.fy),
                        Math.Abs(intr_cal.cx-intr_or.cx),
                        Math.Abs(intr_cal.cy-intr_or.cy),
                        Math.Abs(intr_cal.DistortionR1-intr_or.DistortionR1),
                        Math.Abs(intr_cal.DistortionR2-intr_or.DistortionR2),
                        Math.Abs(intr_cal.DistortionR3-intr_or.DistortionR3),
                        Math.Abs(intr_cal.DistortionT1-intr_or.DistortionT1),
                        Math.Abs(intr_cal.DistortionT2-intr_or.DistortionT2),
                        Math.Abs(camxerror),Math.Abs(camyerror),Math.Abs(camzerror),
                        Math.Sqrt(camxerror*camxerror + camyerror*camyerror + camzerror*camzerror)});

                    graph = graph ?? new Matrix<double>(m.Rows, 1);
                    graph += m;
                }
                graph /= redocounter; //gemiddelde
                graphgem = graphgem == null ? graph : graphgem.ConcateHorizontal(graph);
                
            }
            var camoriginal = sims.First().OriginalValues.First().Value;

            var introriginal = sims.First().OriginalValues.First().Value.Intrinsics;
            var matoriginal = new Matrix(new[]{1,1,introriginal.fx, introriginal.fy, introriginal.cx, introriginal.cy,
                introriginal.DistortionR1, introriginal.DistortionR2, introriginal.DistortionR3,
                introriginal.DistortionT1, introriginal.DistortionT2,
                camoriginal.X,camoriginal.Y,camoriginal.Z,0});

            graphgem = graphgem.ConcateHorizontal(matoriginal); //laatste tabel = originele waarden
            Matlab.SendMatrix(graphgem, "TESTFEATURESVARIATIONGOPROHERO3_std"+std+"_dist"+5+"_std3d0"); //naar matlab
            
            std += 3;
            multicamsim_featuresinvloed(); //& opnieuw met meer ruis
        }

        #endregion
        

        static Matrix<double> testPhotoscanR(ceresdotnet.BundleIntrinsicsFlags intr_flags, ceresdotnet.DistortionModel model) {
            AgisoftProject project = new AgisoftProject(@"D:\3dtrenchview\opnames\veldstraatnocompressiontest\Roeselare.Veldstraat.psx");

            var chunk = project.Chunks.Last();
            var pc = chunk.readPointCloud();
            
            var points = pc.ReadPoints().ToList();
            var comparer = new MarkerIDComparer();
            points.Sort(comparer);

            var bundler = new MultiCameraBundler();






            Dictionary<CeresCamera, IEnumerable<CeresMarker>> markerdict = new Dictionary<CeresCamera, IEnumerable<CeresMarker>>();
            var maxImages = 2000;
            int imagecount = 0;
            int skipimages = 0;
            foreach (var projection in pc.Projections) {
                if (++imagecount > maxImages) {
                    break;
                }
                if (skipimages++ != 0) {
                    if (skipimages == 10) {
                        skipimages = 0;
                    }
                    //continue;
                }
                var cameraid = projection.Item1;
                var camera = chunk.GetCameraFromID(cameraid);
                var markers = pc.ReadMarkersForCameraID(cameraid).ToArray();
                
                camera.Intrinsics.skew = 0;

                camera.Intrinsics.Distortionmodel = model;

                bundler.AddCamera(camera);
                bundler.SetBundleFlags(camera.Internal, intr_flags);
                bundler.SetBundleFlags(camera.External, BundlePointOrientFlags.ALL);
                var observations = bundler.GetObservationList(camera);
                

                //var foundmarkers = new List<Tuple<Marker2d, IMarker3d>>();
                //List<PointF> reprojections = new List<PointF>();
                foreach (var marker2D in markers) {
                    var found = points.BinarySearch(new Marker(marker2D.ID, 0, 0, 0), comparer);
                    if (found >= 0) {
                        //foundmarkers.Add(new Tuple<Marker2d, IMarker3d>(marker2D, points[found]));
                        observations.Add(new CeresMarker()
                        {
                            id = 0,
                            x = marker2D.X,
                            y = marker2D.Y,
                            Location = points[found]
                        });
                    }
                }
            }


            foreach (var gcp in chunk.GCPs)
            {
                var obs = chunk.Frame.markerid_observations.FirstOrDefault(x => x.Key == gcp.Id);
                if (obs.Value != null && obs.Value.Count >= 2)
                {
                    var pts2d = obs.Value.Select(x => new PointF(x.x, x.y)).ToArray();
                    var pts2dvec = new VectorOfPointF(pts2d);
                    var pts2dvec_undst = new VectorOfPointF();
                    var projectionmats = obs.Value.Select(x => chunk.Cameras.First(cam => cam.CameraID == x.camera_id).ProjectionMatrix).ToArray();
                    var camera = chunk.Cameras.First(x => x.CameraID == obs.Value.First().camera_id);
                    var marker3d = new Marker(0x40000000 + obs.Key, 0, 0, 0);

                    CvInvoke.UndistortPoints(pts2dvec, pts2dvec_undst, camera.Intrinsics.cvmat, camera.Intrinsics.CVDIST, null, camera.Intrinsics.cvmat);
                    CVNative.CVNative.triangulateNViews(pts2dvec_undst.ToArray(), projectionmats, marker3d.Pos);

                    gcp.AdjustedPosition = marker3d;
                    bundler.AddGCP(gcp);
                }
            }
            
            
            string cameraparamsstring = "";
            string reporjectionsstring = "";
            string meancamereapos = "";

            double lastError = 0;

            double lastCost = 0;
            bundler.Iteration += (sender, summary) => {
                if (!summary.step_is_successful)
                    return CeresCallbackReturnType.SOLVER_CONTINUE;

                bundler.GetNativeParamblock(chunk.Cameras.First().Intrinsics, out CeresIntrinsics ceresintr);
                var r = CeresCallbackReturnType.SOLVER_CONTINUE;
                int nr = summary.iteration;
                var flags = ceresintr.BundleFlags;
                var camerastr = "(" + nr + ") CAMERA PARAMS";

                camerastr += flags.HasFlag(BundleIntrinsicsFlags.FocalLength) ? " fx: " + ceresintr.fx + " fy: " + ceresintr.fy : "";
                camerastr += flags.HasFlag(BundleIntrinsicsFlags.PrincipalP) ? " cx: " + ceresintr.ppx + " cy: " + ceresintr.ppy : "";
                camerastr += flags.HasFlag(BundleIntrinsicsFlags.SKEW) ? " skew: " + ceresintr.skew : "";

                camerastr += flags.HasFlag(BundleIntrinsicsFlags.R1) ? " k1: " + ceresintr.k1 : "";
                camerastr += flags.HasFlag(BundleIntrinsicsFlags.R2) ? " k2: " + ceresintr.k2 : "";
                camerastr += flags.HasFlag(BundleIntrinsicsFlags.R3) ? " k3: " + ceresintr.k3 : "";
                camerastr += flags.HasFlag(BundleIntrinsicsFlags.R4) ? " k4: " + ceresintr.k4 : "";
                camerastr += flags.HasFlag(BundleIntrinsicsFlags.R5) ? " k5: " + ceresintr.k5 : "";
                camerastr += flags.HasFlag(BundleIntrinsicsFlags.R6) ? " k6: " + ceresintr.k6 : "";

                camerastr += flags.HasFlag(BundleIntrinsicsFlags.P1) ? " p1: " + ceresintr.p1 : "";
                camerastr += flags.HasFlag(BundleIntrinsicsFlags.P2) ? " p2: " + ceresintr.p2 : "";
                camerastr += flags.HasFlag(BundleIntrinsicsFlags.P3) ? " p3: " + ceresintr.p3 : "";
                camerastr += flags.HasFlag(BundleIntrinsicsFlags.P4) ? " p4: " + ceresintr.p4 : "";

                camerastr += flags.HasFlag(BundleIntrinsicsFlags.S1) ? " s1: " + ceresintr.s1 : "";
                camerastr += flags.HasFlag(BundleIntrinsicsFlags.S2) ? " s2: " + ceresintr.s2 : "";


                cameraparamsstring += camerastr + Environment.NewLine;
                //Console.WriteLine(camerastr);
                double AllReprojections = 0;
                double totalmarkercount = 0;

                double meanPosX = 0;
                double meanPosY = 0;
                double meanPosZ = 0;
                

                AllReprojections /= totalmarkercount;
                reporjectionsstring += String.Format("({0}) Error: {1}", nr, AllReprojections) + Environment.NewLine;
                meancamereapos += String.Format("({0}) pos: {1}  {2}  {3}", nr, AllReprojections, meanPosX, meanPosY, meanPosZ) + Environment.NewLine;
                //Console.WriteLine("({0}) reprojerror: {1}   mean cam pos: x({2}) y({3}) z({4})", nr, AllReprojections, meanPosX, meanPosY, meanPosZ);


                if (Math.Abs(lastError - AllReprojections) / AllReprojections < .0001)
                {
                    r = CeresCallbackReturnType.SOLVER_TERMINATE_SUCCESSFULLY;
                }
                lastError = AllReprojections;
                lastCost = summary.cost;


                if (GetAsyncKeyState(Keys.Alt) == 1)
                {
                    return CeresCallbackReturnType.SOLVER_TERMINATE_SUCCESSFULLY;
                }
                return r;
            };


            bundler.BuildProblem();
            bundler.SolveProblem();

            Console.WriteLine(cameraparamsstring);
            Console.WriteLine(reporjectionsstring);

            return null;

        }

       
       

        
        
        private static IEnumerable<KeyValuePair<int, string>> LoadXml(string xmlPath) {
            var stream = System.IO.File.OpenRead(xmlPath);

            Console.WriteLine("Reading '{0}'.", xmlPath);
            var sw = new Stopwatch();
            sw.Start();

#if !LINQ
            var document = new XmlDocument();
            document.Load(XmlReader.Create(stream));
            var nodes = document.SelectNodes("/SpatialReference/ReferenceSystem");
            if (nodes == null)
                yield break;
            var rs = nodes.Cast<XmlElement>();

            foreach (var node in rs) {
                var sridElement = node["SRID"];
                if (sridElement != null) {
                    var srid = int.Parse(sridElement.InnerText);
                    yield return new KeyValuePair<int, string>(srid, node.ChildNodes[2].InnerText);
                }
            }
#else
            var document = XDocument.Load(stream);

            var rs = from tmp in document.Elements("SpatialReference").Elements("ReferenceSystem") select tmp;

            foreach (var node in rs)
            {
                var sridElement = node.Element("SRID");
                if (sridElement != null)
                {
                    var srid = int.Parse(sridElement.Value);
                    yield return new KeyValuePair<int, string>(srid, node.LastNode.ToString());
                }
            }
#endif

            sw.Stop();
            Console.WriteLine("Read '{1}' in {0:N0}ms", sw.ElapsedMilliseconds, xmlPath);
        }
       
        public static Tuple<MCvPoint3D32f, PointF, int> find3dmarker(Marker2d marker2d, IEnumerable<Marker> markers3d) {
            foreach (var marker3D in markers3d) {
                if (marker2d.ID == marker3D.ID) {
                    return new Tuple<MCvPoint3D32f, PointF, int>(new MCvPoint3D32f((float)marker3D.X,(float)marker3D.Y,(float)marker3D.Z),marker2d.PointF,marker2d.ID);
                }
            }
            return null;
        }
        /*
         * {3689.7165,0,2816.1568}, 
           {0,3693.6683,1839.1889},
           {0,0,1}
         * */

        public static void writeMarkersToVRML(IEnumerable<Marker> markers) {
            string s = "";
            foreach (var marker in markers) {
                s += String.Format(
                    "Seperator {{ \nTransform {{\n translation {0} {1} {2}\n }} \nText2 {{\n string \"{3}\" \n}}}}\n"
                    , marker.X.ToString(CultureInfo.InvariantCulture), marker.Y.ToString(CultureInfo.InvariantCulture),
                    marker.Z.ToString(CultureInfo.InvariantCulture), marker.ID.ToString());
            }
        }

        static void testEosCamera() {
            /*
            string fotodir,zhangdir;
            Matrix<double> camMat, dist;
            CameraIntrinsics intrinsics;
            Scene scene = new Scene();
            //fotodir = @"D:\opmeting multicam\EOS\";//@"D:\calibratiruimte metingen\GOPRO3KUL\zeer goed"
            //fotodir = @"D:\calibratiruimte metingen\GOPRO3KUL\zeer goed\slecht";
            //fotodir = @"D:\calibratiruimte metingen\GOPRO3KUL\zeer goed";
            fotodir = @"D:\calibratiruimte metingen\GOPRO3KUL\beide";
            //fotodir = @"D:\opmeting multicam\canan 1100d\single";
            var fotodirdetected = Path.Combine(fotodir, @"detected");

            zhangdir = @"D:\opmeting multicam\gopro zhang";
            //zhangdir = @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\5D\zhang.6.9.55\";

            var markers = IO.MarkersFromFile(@"D:\calibratiruimte metingen\calibjens-gekuist.txt");
            writeMarkersToVRML(markers);
            scene.AddRange(markers);

            var markersscene = scene.get<Marker>();

            ZhangCalibration zc = new ZhangCalibration();
            //zc.LoadImages(Directory.GetFiles(zhangdir).ToList(), new System.Drawing.Size(6, 9));
            //zc.CalibrateCV(new ChessBoard(6, 9, 55), out camMat, out  dist);
            //intrinsics = new CameraIntrinsics(camMat) {CVDIST = dist};

            intrinsics = CameraIntrinsics.GOPROFOTOWIDE;
            //intrinsics = CameraIntrinsics.EOS5DMARKII;
            //intrinsics = CameraIntrinsics.EOS1000D;

            camMat = intrinsics.cvmat;
            dist = intrinsics.CVDIST;
            
            
            //string[] files = Directory.GetFiles(@"D:\thesis\opnames\5D\Opname1\").Take(4).ToArray();
            string[] files = Directory.GetFiles(fotodir).Where(x=>x.EndsWith(".jpg",StringComparison.CurrentCultureIgnoreCase)).ToArray();

            //var eos5daruco =Aruco.findArucoMarkers(files, @"D:\thesis\opnames\5D\Opname1\detected\", 1);
            var eos5daruco = Aruco.findArucoMarkers(files, fotodirdetected, 1);
            var filesdetected = files.Select(x => Path.Combine(Path.GetDirectoryName(x), "detected", Path.GetFileName(x) + "detected.jpg")).ToArray();

            var points = eos5daruco.Select(x => x.Value.Select(y => find3dmarker(y, markersscene)).Where(y=>y!=null).ToArray()).ToArray();

            List<PinholeCamera> cameras = new List<PinholeCamera>();


            CeresCameraMultiCollectionBundler bundler = new CeresCameraMultiCollectionBundler() {
                CollectionList = new List<CeresCameraCollection>(),
                StandaloneCameraList = new List<CeresCamera>()
            };
            var cerescamerlist = bundler.StandaloneCameraList;

            var cintr = intrinsics.toCeresParameter();


            Dictionary<CeresCamera, List<Tuple<MCvPoint3D32f, PointF, int>>> markersfromcerescam = new Dictionary<CeresCamera, List<Tuple<MCvPoint3D32f, PointF, int>>>();


            int i = -1;
            foreach (var ptnfotoArr in points) {
                i++;
                var ptnfoto = ptnfotoArr.ToList();
                if (ptnfoto.Count == 0) {
                    continue;
                }
                var points3d = ptnfoto.Select(x => x.Item1).ToArray();
                var points2d = ptnfoto.Select(x => x.Item2).ToArray();
                var ids = ptnfoto.Select(x => x.Item3).ToArray();

                var intr = new IntrinsicCameraParameters(5){
                    DistortionCoeffs = intrinsics.CVDIST,
                    IntrinsicMatrix = intrinsics.cvmat
                };
                Matrix<double> RodrR = new Matrix(3, 1);
                Matrix<double> Rot = new Matrix(3, 3);
                Matrix<double> transl = new Matrix(3, 1);
                CVI.SolvePnP(points3d, points2d, intr.IntrinsicMatrix, intr.DistortionCoeffs, RodrR, transl, false, SolvePnpMethod.Dls);
                //CVI.SolvePnP(points3d, points2d, intr.IntrinsicMatrix, intr.DistortionCoeffs, RodrR, transl, false, SolvePnpMethod.Iterative);
                //CVI.SolvePnP(points3d, points2d, intr.IntrinsicMatrix, intr.DistortionCoeffs, RodrR, transl, false, SolvePnpMethod.EPnP);
                //CVI.SolvePnP(points3d, points2d, intr.IntrinsicMatrix, intr.DistortionCoeffs, RodrR, transl, false, SolvePnpMethod.P3P);
                //CVI.SolvePnP(points3d, points2d, intr.IntrinsicMatrix, intr.DistortionCoeffs, RodrR, transl, false, SolvePnpMethod.UPnP);
                //CVI.SolvePnPRansac(new VectorOfPoint3D32F(points3d.Select(x => new MCvPoint3D32f(x.X, x.Y, x.Z)).ToArray()),new VectorOfPointF(points2d), intr.IntrinsicMatrix, intr.DistortionCoeffs, RodrR, transl, false, 20,20, .8, new VectorOfInt(), SolvePnpMethod.UPnP);
                CVI.Rodrigues(RodrR, Rot);
                var ext = CameraCalibration.SolvePnP(points3d, points2d, intr, SolvePnpMethod.Dls);
                var reprojection_left = CameraCalibration.ProjectPoints(points3d, ext, intr);
                List<PointF> residuals_left = points2d.Select((t, j) => new PointF(t.X - reprojection_left[j].X, t.Y - reprojection_left[j].Y)).ToList();
                int fotoindex = 0;
                for (int j = 0; j < residuals_left.Count; j++) {
                    if (residuals_left[j].X + residuals_left[j].Y > 50) {
                        ptnfoto.RemoveAt(fotoindex);
                        fotoindex--;
                    }
                    fotoindex++;
                }

                var r = ext.RotationVector.RotationMatrix;
                var c = new PinholeCamera(intrinsics);
                c.Rot_transform = Rot;//new Matrix(3, 3, r.DataPointer);
                c.Pos_transform = transl;//ext.TranslationVector;
                cameras.Add(c);
                var cc = c.toCeresCamera();
                cerescamerlist.Add(cc);
                cc.External.BundleFlags = BundlePointOrientFlags.ALL;
                markersfromcerescam.Add(cc, ptnfoto.ToList());


                var s = filesdetected[i];
                IO.MarkersToFile(residuals_left.ToArray(), ids.ToArray(), s + "RESIDUALS.txt");
                drawReprojection(ref s, reprojection_left, residuals_left, ids.Select(x => x.ToString()));
                IO.MarkersToFile(points2d, ids.ToArray(), s + "2dDist.txt");
                var undist2d = new VectorOfPointF();
                //CVI.UndistortPoints(new VectorOfPointF(points2d), undist2d, camMat, dist);
                //IO.MarkersToFile(undist2d.ToArray(), ids.ToArray(), s + "2dUndistoredCameraCoord.txt");
                //CVI.PerspectiveTransform(undist2d, undist2d, camMat);
                //IO.MarkersToFile(undist2d.ToArray(), ids.ToArray(), s + "2dUndistored.txt");
                //IO.MarkersToFile(points3d, ids.ToArray(), s + "3d.txt");

                //var outim1 = CVI.Imread(s);
                //CVI.Undistort(CVI.Imread(s), outim1, camMat, dist);
                //CVI.Imwrite(s + "undistorted.jpg", outim1);
            }
            


            bundler.MarkersFromCamera = (cerescam, cerescamcollection) => {
                var pts = markersfromcerescam[cerescam];
                return pts.Select(p => {
                    return new CeresMarker() {
                        id = p.Item3,
                        x = p.Item2.X,
                        y = p.Item2.Y,
                        Location = new CeresPoint() {
                            BundleFlags = BundleWorldCoordinatesFlags.None,
                            X = p.Item1.X,
                            Y = p.Item1.Y,
                            Z = p.Item1.Z
                        }
                    };
                }).ToList();
            };
            var cerescallback = new Iteration((nr,o) => {
                Console.WriteLine(cintr.ToString());
                var r = CeresCallbackReturnType.SOLVER_CONTINUE;
                return r;
            });


            bundler.bundleCollections(cerescallback);


            for (int j = 0; j < bundler.StandaloneCameraList.Count; j++) {
                var cc = bundler.StandaloneCameraList[j];
                var ptnfoto = points[j];
                var points3d = ptnfoto.Select(x => x.Item1).ToArray();
                var points2d = ptnfoto.Select(x => x.Item2).ToArray();
                var ids = ptnfoto.Select(x => x.Item3).ToArray();

                if (points3d.Length == 0) {
                    continue;
                }

                var intr = new IntrinsicCameraParameters(5) 
                {
                    DistortionCoeffs = new Matrix(new[] { cc.Internal.k1, cc.Internal.k2, cc.Internal.p1, cc.Internal.p2, cc.Internal.k3 }),
                    IntrinsicMatrix = cc.Internal.CameraMatrixCV

                };
                var ext = new ExtrinsicCameraParameters(new RotationVector3D(cc.External.R_rod),new Matrix(cc.External.t));
                var reprojection_left = CameraCalibration.ProjectPoints(points3d, ext, intr);
                List<PointF> residuals_left = points2d.Select((t, k) => new PointF(t.X - reprojection_left[k].X, t.Y - reprojection_left[k].Y)).ToList();

                var s = filesdetected[j];
                drawReprojection(ref s, reprojection_left, residuals_left, ids.Select(x => x.ToString()));
                var undist2d = new VectorOfPointF();
                CVI.UndistortPoints(new VectorOfPointF(points2d), undist2d, camMat, dist);
                CVI.PerspectiveTransform(undist2d, undist2d, camMat);

                var outim1 = CVI.Imread(s);
                CVI.Undistort(CVI.Imread(s), outim1, camMat, dist);
                CVI.Imwrite(s + "undistortedBUNDLEADJSUTMENT.jpg", outim1);
                
            }
            */
        }

        private static void testPhotoscanAllModels() {
            /*
            var flags =
                BundleIntrinsicsFlags.FocalLength |
                BundleIntrinsicsFlags.PrincipalP;
            //BundleIntrinsicsFlags.R1 |
            //BundleIntrinsicsFlags.R2 |
            //BundleIntrinsicsFlags.R3 |
            //BundleIntrinsicsFlags.R4 |
            //BundleIntrinsicsFlags.R5 |
            //BundleIntrinsicsFlags.R6 |
            //BundleIntrinsicsFlags.P1 |
            //BundleIntrinsicsFlags.P2;

            Matrix<double> mat = null;

            List<Tuple<DistortionModel, BundleIntrinsicsFlags>> todolist = new List<Tuple<DistortionModel, BundleIntrinsicsFlags>>();
            todolist.Add(
                new Tuple<DistortionModel, BundleIntrinsicsFlags>(
                    DistortionModel.Standard, flags));
            todolist.Add(
                new Tuple<DistortionModel, BundleIntrinsicsFlags>(
                    DistortionModel.Standard, flags | BundleIntrinsicsFlags.R1));
            todolist.Add(
                new Tuple<DistortionModel, BundleIntrinsicsFlags>(
                    DistortionModel.Standard, flags | BundleIntrinsicsFlags.R1 | BundleIntrinsicsFlags.R2));
            todolist.Add(
                new Tuple<DistortionModel, BundleIntrinsicsFlags>(
                    DistortionModel.Standard, flags | BundleIntrinsicsFlags.R1 | BundleIntrinsicsFlags.R2 | BundleIntrinsicsFlags.R3));
            todolist.Add(
                new Tuple<DistortionModel, BundleIntrinsicsFlags>(
                    DistortionModel.AgisoftPhotoscan, flags | BundleIntrinsicsFlags.R1 | BundleIntrinsicsFlags.R2 | BundleIntrinsicsFlags.R3 | BundleIntrinsicsFlags.R4));
            todolist.Add(
                new Tuple<DistortionModel, BundleIntrinsicsFlags>(
                    DistortionModel.OpenCVAdvanced, flags | BundleIntrinsicsFlags.R1 | BundleIntrinsicsFlags.R2 | BundleIntrinsicsFlags.R3 | BundleIntrinsicsFlags.R4 | BundleIntrinsicsFlags.R5 | BundleIntrinsicsFlags.R6));
            foreach (var tuple in todolist) {
                var m = testPhotoscanR(tuple.Item2, tuple.Item1);
                var m2 = testPhotoscanR(tuple.Item2 | BundleIntrinsicsFlags.P1 | BundleIntrinsicsFlags.P2, tuple.Item1);

                var rflags = tuple.Item2 ^ flags;
                var allRflags = BundleIntrinsicsFlags.R1 | BundleIntrinsicsFlags.R2 | BundleIntrinsicsFlags.R3 |
                                BundleIntrinsicsFlags.R4 | BundleIntrinsicsFlags.R5 | BundleIntrinsicsFlags.R6;
                var allflags = BundleIntrinsicsFlags.ALL_PHOTOSCAN | BundleIntrinsicsFlags.ALL_STANDARD |
                               BundleIntrinsicsFlags.ALL_OPENCVADVANCED; 

                mat = mat?.ConcateHorizontal(m);
                mat = mat ?? m;
                mat = mat.ConcateHorizontal(m2);

                if (tuple.Item1 != DistortionModel.Standard) {
                    var m3 = testPhotoscanR(allflags ^ allRflags ^ rflags, tuple.Item1);
                    mat = mat.ConcateHorizontal(m3);
                }
            }
            Matlab.SendMatrix(mat, "photoscanBAoutput");
            */
        }



        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void  Main() {
            multicamsim_fotosinvloed();
            multicamsim_featuresinvloed();
            TestFeatureDetectionGCP();
            
          



            ZhangCalibration stereotest = new ZhangCalibration();
            List<string> badl = new List<string>();
            List<string> badr = new List<string>();
            List<int> badindices = new List<int>();

            for (int i = 0; i < stereotest.images.Count; i++) {
                if (stereotest.images[i] == null || stereotest.images2[i] == null) {
                    stereotest.images.RemoveAt(i);
                    stereotest.images2.RemoveAt(i);
                    i--;
                }
                
            }


            
            Version version = Environment.Version;
            int build = version.Build;
            int major = version.Major;
            int minor = version.Minor;
            int revision = Environment.Version.Revision;
            Console.Write(".NET Framework version: ");
            System.Console.WriteLine("{0}.{1}.{2}.{3}", 
            build, major, minor, revision);

            //CeresSimulation.ceresSolveAruco();


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

        private static void testplyfile() {
            var file = @"D:\3dtrenchview\opnames\veldstraatnocompressiontest\Roeselare.Veldstraat.files\21\0\point_cloud\p210.ply";
            PLY_Pointcloudreader plyreader = new PLY_Pointcloudreader(file);
            var element = plyreader.readNextElement();
            var allelement = plyreader.readAllElements();
        }


    }
}
