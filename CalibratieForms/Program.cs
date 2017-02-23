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
using OpenTK.Graphics.OpenGL;
using PdfSharp;
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
        public static Dictionary<string, string> GetLRFrames(string dirL,string dirR,int KeyFrameL, int KeyFrameR, double LR, int intervalL,int startL, int stopL) {
            
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
            string dirL,string dirR,
            int KeyFrameL,
            int KeyFrameR, double LR, int intervalL,int startL, int stopL) {
            var files = GetLRFrames(dirL, dirR,KeyFrameL, KeyFrameR, LR, intervalL, startL, stopL).ToList();
            var detectedLFile = Path.Combine(dirL, "detected");
            var detectedRFile =  Path.Combine(dirR, "detected");
            //var L = Aruco.findArucoMarkers(files.Keys, detectedLFile,8).Select(x => new Tuple<string, List<ArucoMarker>>(x.Key, x.Value.ToList())).ToList();
            //var R = Aruco.findArucoMarkers(files.Values, detectedRFile,8).Select(x => new Tuple<string, List<ArucoMarker>>(x.Key, x.Value.ToList())).ToList();
            
            
            var r = new Dictionary<Tuple<string, List<ArucoMarker>>, Tuple<string, List<ArucoMarker>>>();
            EXT.ActionMultiThread(i => {
                Console.WriteLine("Searching markers in stereopair {0}   {1}", Path.GetFileName(files[i].Key), Path.GetFileName(files[i].Value));
                var filel = Path.Combine(dirL, "detected",Path.GetFileName(files[i].Key)+"detected.jpg");
                var filer = Path.Combine(dirR, "detected",Path.GetFileName(files[i].Value)+"detected.jpg");
                r.Add(
                    new Tuple<string, List<ArucoMarker>>(filel, ArUcoNET.Aruco.FindMarkers(files[i].Key, filel).ToList()),
                    new Tuple<string, List<ArucoMarker>>(filer, ArUcoNET.Aruco.FindMarkers(files[i].Value,filer).ToList()));
            },files.Count);
            
            
            
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



        static void drawReprojection(ref string infile, IEnumerable<PointF> points, IEnumerable<PointF> error, IEnumerable<string> txt = null) {
            var outfile = infile + "reprojection.jpg";

            drawReprojection(infile, outfile, points, error, txt);

            infile = outfile;
        }
        static void drawReprojection(string infile,string outfile, IEnumerable<PointF> points, IEnumerable<PointF> error, IEnumerable<string> txt = null) {
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
                        gfx.DrawEllipse(pen, p.X + e.X - width / 2, p.Y + e.Y - height / 2, width, height);
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
            bitmap.Save(outfile, ImageFormat.Bmp);
            gfx.Dispose();
            bitmap.Dispose();
            //File.Delete(file);
        }

        static void testSFM() {
            var detectedAruco = getLRFramesAruco(
               @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\stereo.canon\left\",
               @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\stereo.canon\right\",
               1, 1, 1, 1, 1, 18).ToTupleList();


            var dir = @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\links\2\";
            List<string> files = new List<string>();
            for (int i = 533; i < 2500; i+=30) {
                files.Add(string.Format(@"{1}\{0:00000000}.jpg", i, dir));
            }


            var detected = Aruco.findArucoMarkers(files);
            Matrix<double> camMat_left = new Matrix<double>(new[,] {
                {852.18, 0,975.84},
                {0,853.05,525.63},
                {0,0,1}
            });
            Matrix<double> dist_left = new Matrix<double>(new[] { -.2807, .124, .0005073, -.03291 });

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
         * 
         * 
         * */
        static void testEosCamera() {

            Scene scene = new Scene();

            var markers1 = IO.MarkersFromFile(@"C:\Users\jens\Desktop\calibratie\calibratieruimte_opname_123.txt");
            scene.AddRange(markers1);

            var markersscene = scene.getIE<Marker>().ToArray();

            var dirzhang = @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\5D\zhang.6.9.55\";
            ZhangCalibration zc = new ZhangCalibration();
            var camMat = new Matrix(new double[,] {
                {857.4829, 0,968.06224},
                {0,876.7182,556.371458},
                {0,0,1}
            });
            Matrix dist = new Matrix(new double[] { -.25761, .0877086, -.000256970, -.000593390 });

            //zc.LoadImages(Directory.GetFiles(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\5D\zhang.6.9.55\").ToList(), new System.Drawing.Size(6, 9));
            
            //zc.CalibrateCV(new ChessBoard(6, 9, 55), out camMat, out dist);

           
            
            //string[] files = Directory.GetFiles(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\5D\Opname1\").Take(4).ToArray();
            string[] files = Directory.GetFiles(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\gorporechts\").ToArray();

            //var eos5daruco =Aruco.findArucoMarkers(files, @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\5D\Opname1\detected\", 1);
            var eos5daruco = Aruco.findArucoMarkers(files, @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\gorporechts\detected\", 1);
            var filesdetected = files.Select(x => Path.Combine(Path.GetDirectoryName(x), "detected", Path.GetFileName(x) + "detected.jpg")).ToArray();

            var points = eos5daruco.Select(x => x.Value.Select(y => find3dmarker(y, markersscene)).Where(y=>y!=null).ToArray()).ToArray();

            List<ExtrinsicCameraParameters> extrinsics = new List<ExtrinsicCameraParameters>();
            int i = -1;
            foreach (var ptnfoto in points) {
                i++;
                var points3d = ptnfoto.Select(x => x.Item1).ToArray();
                var points2d = ptnfoto.Select(x => x.Item2).ToArray();
                var ids = ptnfoto.Select(x => x.Item3).ToArray();

                var intr = new IntrinsicCameraParameters(4);
                intr.DistortionCoeffs = dist;
                intr.IntrinsicMatrix = camMat;
                var ext = CameraCalibration.SolvePnP(points3d, points2d, intr, SolvePnpMethod.UPnP);
                var reprojection_left = CameraCalibration.ProjectPoints(points3d, ext, intr);
                extrinsics.Add(ext);
                List<PointF> residuals_left = points2d.Select((t, j) => new PointF(t.X - reprojection_left[j].X, t.Y - reprojection_left[j].Y)).ToList();

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

            //BAProlbem

            CeresCameraMultiCollectionBundler bundler = new CeresCameraMultiCollectionBundler();
            bundler.CollectionList = new List<CeresCameraCollection>();
            bundler.StandaloneCameraList = new List<CeresCamera>();
            var cerescamerlist = bundler.StandaloneCameraList;
            double[] distarr = new double[Math.Max(dist.Cols,dist.Rows)];
            System.Buffer.BlockCopy(dist.Data, distarr.Length, distarr, 0, distarr.Length);
            var cintr = new CeresIntrinsics() {
                
                BundleFlags = BundleIntrinsicsFlags.ALL,
                fx = camMat[0, 0],
			fy = camMat[0, 1],
			ppx = camMat[2, 0],
			ppy = camMat[2, 1],
            k1 = dist[0,0],
            k2 = dist[1,0],
            p1 = dist[2,0],
            p2 = dist[3,0]
            };


            Dictionary<CeresCamera, List<Tuple<MCvPoint3D32f, PointF, int>>> markersfromcerescam = new Dictionary<CeresCamera, List<Tuple<MCvPoint3D32f, PointF, int>>>();
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
            
            for (int j = 0; j < points.Length; j++) {
                var ext = extrinsics[j];
                double[] r = new double[] { ext.RotationVector[0, 0], ext.RotationVector[1, 0], ext.RotationVector[2, 0] };
                double[] t = new double[] { ext.TranslationVector[0, 0], ext.TranslationVector[1, 0], ext.TranslationVector[2, 0] };
                var cc = new CeresCamera(cintr,new CeresPointOrient() {
                    R_rod = r,
                    t = t
                });
                markersfromcerescam.Add(cc, points[j].ToList());
                cerescamerlist.Add(cc);





            }

            
            

            bundler.bundleCollections(nr => CeresCallbackReturnType.SOLVER_CONTINUE);


            for (int j = 0; j < bundler.StandaloneCameraList.Count; j++) {
                i++;
                var cc = bundler.StandaloneCameraList[j];
                var ptnfoto = points[j];
                var points3d = ptnfoto.Select(x => x.Item1).ToArray();
                var points2d = ptnfoto.Select(x => x.Item2).ToArray();
                var ids = ptnfoto.Select(x => x.Item3).ToArray();

                var intr = new IntrinsicCameraParameters(5);

                intr.DistortionCoeffs = new Matrix(new [] { cc.Internal.k1, cc.Internal.k2, cc.Internal.p1, cc.Internal.p2, cc.Internal.k3 });
                intr.IntrinsicMatrix = new Matrix(new double[,] {
                    {cc.Internal.fx, 0, cc.Internal.ppx},
                    {0, cc.Internal.fy, cc.Internal.ppy},
                    {0, 0, 1}
                });
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
            
        }

        
        static void testStereoCamera() {


            Scene scene =  new Scene();

            var markers1 = IO.MarkersFromFile(@"C:\Users\jens\Desktop\calibratie\calibratieruimte_opname_123.txt");
            scene.AddRange(markers1);

            ZhangCalibration calibleft = new ZhangCalibration();
            //calibleft.LoadImages(Directory.GetFiles(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\stereo.canon\left\zhang\").ToList(),new Size(6,9));
            CameraIntrinsics intrleft;
            //calibleft.CalibrateCV(new ChessBoard(6,9,55.03333333333333333), out intrleft);
            intrleft = new CameraIntrinsics(new double[,] {
                {3842, 0,2822},
                {0,3841,1842},
                {0,0,1}
            }){CVDIST = new Matrix(new double[]{-.1249,.0943})};



            //ZhangCalibration calibright = new ZhangCalibration();
            //calibleft.LoadImages(Directory.GetFiles(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\stereo.canon\right\zhang\").ToList(), new Size(6, 9));
            CameraIntrinsics intrright;
            //calibleft.CalibrateCV(new ChessBoard(6, 9, 55.03333333333333333), out intrright);
            intrright = new CameraIntrinsics(new double[,] {
                {4019, 0,1989},
                {0,4018,1287},
                {0,0,1}
            }) { CVDIST = new Matrix(new double[] { -.1389, .15689 }) };


            int blabla = 600;
            /*var detectedAruco = getLRFramesAruco(
                @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\stereo.canon\left\",
                @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\stereo.canon\right\",
                533, 689, 2.016214371053080730500085338795, 50, 600, 1301).ToTupleList();*/

            var detectedAruco = getLRFramesAruco(
                @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\stereo.canon\left\",
                @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\stereo.canon\right\",
                1, 1, 1, 1, 1, 18).ToTupleList();

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
                                new MCvPoint3D32f((float) scenemarker.X, (float) scenemarker.Y,(float) scenemarker.Z),
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

                if ((L.Count() >= minCount && R.Count() >= minCount) || true) {
                    points.Add(new Tuple<Tuple<string, List<Tuple<MCvPoint3D32f, PointF,int>>>, Tuple<string, List<Tuple<MCvPoint3D32f, PointF,int>>>>(
                        new Tuple<string, List<Tuple<MCvPoint3D32f, PointF,int>>>(detectedStereo.Item1.Item1,L.ToList()),
                        new Tuple<string, List<Tuple<MCvPoint3D32f, PointF,int>>>(detectedStereo.Item2.Item1,R.ToList())));
                }
            }

            PinholeCamera left = new PinholeCamera(intrleft);


            PinholeCamera right = new PinholeCamera(intrright);


            Matrix<double> camMat_left = new Matrix<double>(new double [,] {
                {954, 0,923},
                {0,960,484},
                {0,0,1}
            });
            Matrix<double> dist_left = new Matrix<double>(new[] { -.2172, .04159, 0, 0 });
            Matrix<double> dist_zero = new Emgu.CV.Matrix<double>(new double []{ -0, 0, 0, -0 });
            Matrix<double> camMat_right = new Emgu.CV.Matrix<double>(new double[,] {
                {845, 0,954},
                {0,848,522},
                {0,0,1}
            });
            Matrix<double> dist_right = new Matrix<double>(new[] {-.1886, .02519,0,0});

            

            var ess = new Matrix<double>(3, 3);
            var fundamental = new Matrix<double>(3, 3);
            MCvTermCriteria termcriteria = new MCvTermCriteria();
            
            List<MCvPoint3D32f[]> test3d = new List<MCvPoint3D32f[]>();
            List<MCvPoint3D32f[]> test3dL = new List<MCvPoint3D32f[]>();
            List<MCvPoint3D32f[]> test3dR = new List<MCvPoint3D32f[]>();
            List<PointF[]> test2dL = new List<PointF[]>();
            List<PointF[]> test2dR = new List<PointF[]>();

            List<Tuple<MCvPoint3D32f, PointF, int>[]> testL = new List<Tuple<MCvPoint3D32f, PointF, int>[]>();
            List<Tuple<MCvPoint3D32f, PointF, int>[]> testR = new List<Tuple<MCvPoint3D32f, PointF, int>[]>();

            List<string> fileL = new List<string>();
            List<string> fileR = new List<string>();

            var bundler = new CeresCameraMultiCollectionBundler();

            CeresCameraCollection CeresCameraColl = new CeresCameraCollection();
            CeresCameraColl.Cameras = new List<CeresCamera>();

            var CeresIntrLeft = new CeresIntrinsics(new[] { 939.7583, 951.02496, 918.374, 507.64828, -.26166, .096052, 0, 0, 0 }) {
                BundleFlags = (BundleIntrinsicsFlags.R1 | BundleIntrinsicsFlags.FocalLength | BundleIntrinsicsFlags.PrincipalP | BundleIntrinsicsFlags.R2)
            };
            var CeresIntrRight = new CeresIntrinsics(new[] { 841.0235, 844.1916, 972.10629, 529.56556, -.2182, .04911, 0, 0, 0 }) {
                BundleFlags = (BundleIntrinsicsFlags.R1 | BundleIntrinsicsFlags.FocalLength | BundleIntrinsicsFlags.PrincipalP | BundleIntrinsicsFlags.R2)
            };

            CeresIntrLeft = left.Intrinsics.toCeresParameter(BundleIntrinsicsFlags.INTERNAL_R1R2);
            CeresIntrRight = right.Intrinsics.toCeresParameter(BundleIntrinsicsFlags.INTERNAL_R1R2);

            var ccLeft = new CeresCamera(CeresIntrLeft,new CeresPointOrient{
                RT = new double []{0,0,0,0,0,0}
            });
            CeresCamera ccRight = null;

            
            for (int i = 0; i < points.Count; i++) {
                var L = points[i].Item1;
                var R = points[i].Item2;

                

                List<int> markeridsL = L.Item2.Select(x => x.Item3).ToList();
                List<int> markeridsR = R.Item2.Select(x => x.Item3).ToList();
                var intersection = markeridsL.Intersect(markeridsR);

                var int2dL = L.Item2.Where(x => intersection.Contains(x.Item3)).OrderBy(x => x.Item3).ToArray();
                var int2dR = R.Item2.Where(x => intersection.Contains(x.Item3)).OrderBy(x => x.Item3).ToArray();
                if (int2dL.Length < 8 || int2dR.Length < 8) {
                    //continue;
                }
                testL.Add(L.Item2.ToArray());
                testR.Add(R.Item2.ToArray());
                test3dL.Add(L.Item2.Select(x => x.Item1).ToArray());
                test3dR.Add(R.Item2.Select(x => x.Item1).ToArray());
                test3d.Add(L.Item2.Select(x => x.Item1).ToArray());
                test2dL.Add(L.Item2.Select(x => x.Item2).ToArray());
                test2dR.Add(R.Item2.Select(x => x.Item2).ToArray());

                fileL.Add(L.Item1);
                fileR.Add(R.Item1);
            }
            CeresCameraCollection baseColl = null;


            var intrL = new IntrinsicCameraParameters(4);
            intrL.DistortionCoeffs = dist_left;
            intrL.IntrinsicMatrix = camMat_left;

            var intrR = new IntrinsicCameraParameters(4);
            intrR.DistortionCoeffs = dist_zero;
            intrR.IntrinsicMatrix = camMat_right;



            Dictionary<Tuple<CeresCamera, CeresCameraCollection>, List<Tuple<MCvPoint3D32f, PointF, int>>> markersfromcerescam =
                    new Dictionary<Tuple<CeresCamera, CeresCameraCollection>, List<Tuple<MCvPoint3D32f, PointF, int>>>();

            Dictionary<Tuple<CeresCamera, CeresCameraCollection>, string> filefromcerescam =
                new Dictionary<Tuple<CeresCamera, CeresCameraCollection>, string>();

            List<Object> someobjshaha = new List<object>();

            bundler.MarkersFromCamera = (cerescam, cerescamcollection) => {
                var key = markersfromcerescam.Keys.First(x => x.Item1 == cerescam && x.Item2 == cerescamcollection);
                var pts = markersfromcerescam[key];
                return pts.Select(p => {

                    var r =  new CeresMarker() {
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
                    someobjshaha.Add(r);
                    return r;
                }).ToList();
            };

            for (int i = 0; i < testL.Count; i++) {
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
                    camMat_left, dist_left, outr_left, outt_left, false, 100, 50, .8, inliers, SolvePnpMethod.Iterative);

                var intr = new IntrinsicCameraParameters(4);
                intr.DistortionCoeffs = intrleft.Cv_DistCoeffs4;
                intr.IntrinsicMatrix = intrleft.cvmat;
                var extL = CameraCalibration.SolvePnP(points3d, points2d, intr, SolvePnpMethod.Iterative);
                var reprojection_left = CameraCalibration.ProjectPoints(points3d, extL, intr);
                List<PointF> residuals_left =
                    points2d.Select((t, j) => new PointF(t.X - reprojection_left[j].X, t.Y - reprojection_left[j].Y))
                        .ToList();

                var s = dirL;

                drawReprojection(ref s, points2d, residuals_left, ids.Select(x => x.ToString()));
                //dirL = s;
                IO.MarkersToFile(residuals_left.ToArray(), ids.ToArray(), s + "RESIDUALS.txt");
                IO.MarkersToFile(points2d, ids.ToArray(), s + "2d.txt");


                ///RIGHT CAMERA
                points3d = R.Select(x => x.Item1).ToArray();
                points2d = R.Select(x => x.Item2).ToArray();
                ids = R.Select(x => x.Item3);
                inliers = new Emgu.CV.Mat();
                b = CVI.SolvePnPRansac(new VectorOfPoint3D32F(points3d), new VectorOfPointF(points2d),
                    camMat_right, dist_right, outr_right, outt_right, false, 20, 50, .8, inliers, SolvePnpMethod.UPnP);
                
                intr = new IntrinsicCameraParameters(4);
                intr.DistortionCoeffs = intrright.Cv_DistCoeffs4;
                intr.IntrinsicMatrix = intrright.cvmat;
                var extR = CameraCalibration.SolvePnP(points3d, points2d, intr);
                var reprojection_right = CameraCalibration.ProjectPoints(points3d, extR, intr);
                List<PointF> residuals_right = points2d.Select((t, k) => new PointF(t.X - reprojection_right[k].X, t.Y - reprojection_right[k].Y))                        .ToList();
                s = dirR;
                drawReprojection(ref s, points2d, residuals_right, ids.Select(x => x.ToString()));
                //dirR = s;
                IO.MarkersToFile(points2d, ids.ToArray(), s + "2d.txt");
                IO.MarkersToFile(points3d, ids.ToArray(), s + "3d.txt");



                
                CeresPointOrient systeemCamera = new CeresPointOrient {
                    R_rod = new[] { extL.RotationVector[0, 0], extL.RotationVector[1, 0], extL.RotationVector[2, 0] },
                    t = new[] { extL.TranslationVector[0, 0], extL.TranslationVector[1, 0], extL.TranslationVector[2, 0] }
                };

                if (ccRight == null) {
                    var right_R = new Matrix(3, 3, extR.RotationVector.RotationMatrix.DataPointer);
                    var right_RT = right_R.Transpose();
                    var left_R = new Matrix(3, 3, extL.RotationVector.RotationMatrix.DataPointer);
                    var left_RT = left_R.Transpose();

                    var right_T = extR.TranslationVector;
                    var left_T = extL.TranslationVector;
                    var left_TT = -1*left_RT*left_T;
                    var right_TT = -1*right_RT*right_T;


                    var newRot = right_R*left_RT;
                    var newT = right_R*left_TT + right_T;

                    Matrix newRot_rodr = new Matrix(3, 1);
                    CVI.Rodrigues(newRot, newRot_rodr);
                    ccRight = new CeresCamera(CeresIntrRight, new CeresPointOrient() {
                        R_rod = new[] {newRot_rodr[0, 0], newRot_rodr[1, 0], newRot_rodr[2, 0]},
                        t = new[] {newT[0, 0], newT[1, 0], newT[2, 0]}
                    });
                    baseColl = new CeresCameraCollection() { };
                    baseColl.Position = systeemCamera;

                    baseColl.Cameras = new List<CeresCamera> { ccRight, ccLeft };

                    var tupleR = new Tuple<CeresCamera, CeresCameraCollection>(ccRight, baseColl);
                    var tupleL = new Tuple<CeresCamera, CeresCameraCollection>(ccLeft, baseColl);
                    markersfromcerescam.Add(tupleR, R.ToList());
                    markersfromcerescam.Add(tupleL, L.ToList());

                    filefromcerescam.Add(tupleL, dirL);
                    filefromcerescam.Add(tupleR, dirR);

                    ccLeft.External.BundleFlags = BundlePointOrientFlags.None;
                    ccRight.External.BundleFlags = BundlePointOrientFlags.ALL;
                    baseColl.Position.BundleFlags = BundlePointOrientFlags.ALL;

                    bundler.CollectionList.Add(baseColl);
                }
                else {
                    var coll = baseColl.CreateSecondPositionCopy();
                    coll.Position = systeemCamera;
                    coll.Position.BundleFlags = BundlePointOrientFlags.ALL;

                    var tupleR = new Tuple<CeresCamera, CeresCameraCollection>(ccRight, coll);
                    var tupleL = new Tuple<CeresCamera, CeresCameraCollection>(ccLeft, coll);
                    markersfromcerescam.Add(tupleR, R.ToList());
                    markersfromcerescam.Add(tupleL, L.ToList());

                    filefromcerescam.Add(tupleL, dirL);
                    filefromcerescam.Add(tupleR, dirR);
                    bundler.CollectionList.Add(coll);
                }
                someobjshaha.Add(systeemCamera);

            }

            bundler.bundleCollections(nr => CeresCallbackReturnType.SOLVER_CONTINUE);

            List<Tuple<PointF, int, CeresCamera, string, CeresCameraCollection>> allerrors =
                new List<Tuple<PointF, int, CeresCamera, string, CeresCameraCollection>>();
            
            for (int i = 0; i < bundler.CollectionList.Count; i++) {

                
                var cerescamcollection = bundler.CollectionList[i];
                var cerescaml = ccLeft;//bundler.CollectionList[i].Cameras[0];
                var cerescamr = ccRight;//bundler.CollectionList[i].Cameras[1];
                var keyl = markersfromcerescam.Keys.First(x => x.Item1 == cerescaml && x.Item2 == cerescamcollection);
                var ptsl = markersfromcerescam[keyl];
                var keyr = markersfromcerescam.Keys.First(x => x.Item1 == cerescamr && x.Item2 == cerescamcollection);
                var ptsr = markersfromcerescam[keyr];
                var dirL = filefromcerescam[keyl];
                var dirR = filefromcerescam[keyr];

                var corrL = new List<Tuple<MCvPoint3D32f, PointF, int>>();
                var corrR = new List<Tuple<MCvPoint3D32f, PointF, int>>();

                foreach (var pl in ptsl) {
                    foreach (var pr in ptsr) {
                        if (pl.Item3 == pr.Item3) {
                            corrL.Add(pl);
                            corrR.Add(pr);
                        }
                    }
                }
                var pts2dl = new VectorOfPointF(corrL.Select(x => x.Item2).ToArray());
                var pts2dr = new VectorOfPointF(corrR.Select(x => x.Item2).ToArray());

                

                var ceresmarkersl = bundler.MarkersFromCamera(cerescaml, cerescamcollection);
                var errorsl = ceresmarkersl.Select(x => {
                    var error = CeresTestFunctions.testProjectPoint(cerescaml, cerescamcollection.Position, x);
                    var r = new PointF((float)error[0], (float)error[1]);
                    allerrors.Add(new Tuple<PointF, int, CeresCamera, string, CeresCameraCollection>(
                        r, x.id, cerescaml, dirL, cerescamcollection
                     ));
                    return r;
                });

                var ceresmarkersr = bundler.MarkersFromCamera(cerescamr, cerescamcollection);
                var errorsr = ceresmarkersr.Select(x => {
                    var error = CeresTestFunctions.testProjectPoint(cerescamr, cerescamcollection.Position, x);
                    var r = new PointF((float)error[0], (float)error[1]);
                    allerrors.Add(new Tuple<PointF, int, CeresCamera, string, CeresCameraCollection>(
                        r, x.id, cerescamr, dirR, cerescamcollection
                     ));
                    return r;
                });

                var Fdir = @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\calibratie gopro\R1R2\";

                


                var F1 = new Matrix(3, 3);

                var t = new Matrix(new double[,] {
                    {0, -ccRight.External.t[2], ccRight.External.t[1]},
                    {ccRight.External.t[2], 0, -ccRight.External.t[0]},
                    {-ccRight.External.t[1], ccRight.External.t[0], 0},
                });
                var rotv = new RotationVector3D(ccRight.External.R_rod);
                var R = new Matrix(3, 3, rotv.RotationMatrix.DataPointer);
                var E = t*R;
                
                var pts2dl_undist = new VectorOfPointF();
                var pts2dr_undist = new VectorOfPointF();
                CVI.UndistortPoints(pts2dl, pts2dl_undist, cerescaml.Internal.CameraMatrixCV,
                    new Matrix(cerescaml.Internal.Dist5));
                CVI.UndistortPoints(pts2dr, pts2dr_undist, cerescamr.Internal.CameraMatrixCV,
                    new Matrix(cerescamr.Internal.Dist5));



                Mat iml = new Mat();
                Mat imr = new Mat();

                CVI.Undistort(CVI.Imread(dirL), iml, cerescaml.Internal.CameraMatrixCV, new Matrix(cerescaml.Internal.Dist5));
                CVI.Undistort(CVI.Imread(dirR), imr, cerescamr.Internal.CameraMatrixCV, new Matrix(cerescamr.Internal.Dist5));
                CVI.Imwrite(string.Format(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\calibratie gopro\R1R2\{0}.LEFT.Undist.jpg", i), iml);
                CVI.Imwrite(string.Format(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\calibratie gopro\R1R2\{0}.RIGHT.Undist.jpg", i), imr);
                
                var Kinv_r = new Matrix(3, 3);
                var Kinv_l = new Matrix(3, 3);

                CVI.Invert(cerescaml.Internal.CameraMatrixCV, Kinv_l, DecompMethod.Cholesky);
                CVI.Invert(cerescamr.Internal.CameraMatrixCV, Kinv_r, DecompMethod.Cholesky);

                var F4 = Kinv_r.Transpose() * E* Kinv_l;



                IO.WriteMatrix(F4, string.Format(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\calibratie gopro\R1R2\{0}.Fmat4.F", i));
                
                

                
                var dirLout = Path.Combine(Path.GetDirectoryName(dirL), "bundle adjustment",
                    Path.GetFileName(dirL) + "BA.jpg");
                var dirRout = Path.Combine(Path.GetDirectoryName(dirR), "bundle adjustment",
                    Path.GetFileName(dirR) + "BA.jpg");

                var extr = new ExtrinsicCameraParameters(new RotationVector3D(cerescamr.External.R_rod),
                    new Matrix(cerescamr.External.t));
                var extl = new ExtrinsicCameraParameters(new RotationVector3D(cerescaml.External.R_rod),
                    new Matrix(cerescaml.External.t));
                
                var right_R = new Matrix(3, 3, extr.RotationVector.RotationMatrix.DataPointer);
                var right_RT = right_R.Transpose();
                var left_R = new Matrix(3, 3, extl.RotationVector.RotationMatrix.DataPointer);
                var left_RT = left_R.Transpose();

                var right_T = extr.TranslationVector;
                var left_T = extl.TranslationVector;
                var left_TT = -1 * left_RT * left_T;
                var right_TT = -1 * right_RT * right_T;


                var newRot = right_R * left_RT;

                Matrix newRot_rodr = new Matrix(3, 1);
                CVI.Rodrigues(newRot, newRot_rodr); 

                List<PointF> residuals_left;// = ptsl.Select(x => x.Item2).Select((t, k) => new PointF(t.X - reporjl[k].X, t.Y - reporjl[k].Y)).ToList();
                List<PointF> residuals_right;// = ptsr.Select(x => x.Item2).Select((t, k) => new PointF(t.X - reporjr[k].X, t.Y - reporjr[k].Y)).ToList();

                residuals_left = errorsl.ToList();
                residuals_right = errorsr.ToList();

                drawReprojection(dirL,dirLout, ptsl.Select(x => x.Item2), residuals_left, ptsl.Select(x=>x.Item3.ToString()));
                drawReprojection(dirR, dirRout, ptsr.Select(x => x.Item2), residuals_right, ptsr.Select(x => x.Item3.ToString()));
            }
            List<int> ints = new List<int>();
            List<float[]> floats = new List<float[]>();

            for (int id = 0; id < 1000; id++) {
                var errors = allerrors.Where(x => x.Item2 == id);
                if (errors == null || errors.Count() <= 0) { continue; }
                ints.Add(id);
                List<float> currenterrors = new List<float>();
                int counter = 0;
                float errorswaretotal = 0;
                foreach (var error in errors) {
                    counter++;
                    errorswaretotal += (float)Math.Sqrt(error.Item1.X*error.Item1.X + error.Item1.Y*error.Item1.Y);
                }
                currenterrors.Add(counter);
                currenterrors.Add(errorswaretotal);
                floats.Add(currenterrors.ToArray());
            }


            //IO.writePoints(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\errorspermarker.txt", ints.ToArray(),floats.ToArray());

            //IO.writePoints(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\errorspermarker2.txt", ints.ToArray(), floats.ToArray());
        }

        public static Matrix toRodr(Matrix rot) {
            var r = new Matrix(3, 1);
            CVI.Rodrigues(rot, r);
            return r;
        }

        static void testFeatureDetection() {
            Log.ToConsole = true;

            var im1 = @"C:\Users\jens\Desktop\calibratie\Huawei p9\aruco\stereo test\IMG_20160721_230236.jpg";
            var im2 = @"C:\Users\jens\Desktop\calibratie\Huawei p9\aruco\stereo test\IMG_20160721_230320.jpg";
            var imout = @"C:\Users\jens\Desktop\calibratie\Huawei p9\aruco\stereo test\IMG_out.jpg";
            //StereoCalc.test(im1, im2, imout);

            long time;
            VectorOfKeyPoint test = new VectorOfKeyPoint();
            VectorOfKeyPoint key1, key2;
            VectorOfVectorOfDMatch matches = new VectorOfVectorOfDMatch();
            Mat mask, out2;
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
            Matrix F = new Matrix(3, 3);
            Matrix E = new Matrix(3, 3);
            
            

            CVI.FindFundamentalMat(
                new VectorOfPointF(listmatchbest.Select(x => x.Item2.Point).ToArray()),
                new VectorOfPointF(listmatchbest.Select(x => x.Item3.Point).ToArray()), F);
            CVI.FindEssentialMat(
                new VectorOfPointF(listmatchbest.Select(x => x.Item2.Point).ToArray()),
                new VectorOfPointF(listmatchbest.Select(x => x.Item3.Point).ToArray()), E);

            var K = new Matrix<double>(PinholeCamera.getTestCameraHuawei().Intrinsics.Mat);
            
            var W = new Matrix<double>(new double[,] {
                {0,-1,0},
                {1,0,0},
            {0,0,1}
            });

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

            var punten1px = listmatchbest.Select(x => x.Item2.Point).ToArray();
            var punten2px = listmatchbest.Select(x => x.Item3.Point).ToArray();

            VectorOfPointF pt1 = new VectorOfPointF(punten1px);
            VectorOfPointF pt2 = new VectorOfPointF(punten2px);


            var KP0 = new Matrix(new double[,] {
                {1, 0, 0, 0},
                {0, 1, 0, 0},
                {0, 0, 1, 0}
            });
            
            for (int i = 0; i < 4; i++) {


                Matrix<float> output_hom = new Matrix<float>(4, punten1px.Count());
                Matrix<float> output_3dd = new Matrix<float>(3, punten1px.Count());
                output_3dd = new Matrix<float>(punten1px.Count(),3);
                VectorOfPoint3D32F output_3d = new VectorOfPoint3D32F(1200);
                
                CVI.TriangulatePoints(KP0, KPs[i], pt1, pt2, output_hom);
                CVI.ConvertPointsFromHomogeneous(output_hom.Transpose(), output_3d);
            }
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
            //testFeatureDetection();
            //testEosCamera();
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
