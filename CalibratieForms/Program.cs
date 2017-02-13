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
using Point2d = Emgu.CV.Structure.MCvPoint2D64f;
using Point3f = Emgu.CV.Structure.MCvPoint3D32f;
using Vector3d = Emgu.CV.Structure.MCvPoint3D64f;


namespace CalibratieForms {

    static class Program {

        public static object lockme = new Object();
        // "C:\Users\jens\Desktop\calibratie\Opnames_thesis\links\2
        public static Dictionary<string, string> GetLRFrames(string dirL,string dirR,int KeyFrameL, int KeyFrameR, double LR, int intervalL,int startL, int stopL) {
            var r = new Dictionary<string, string>();


            int startframeL = KeyFrameL;
            int startframeR = KeyFrameR;
            int fps_links = 30;
            Func<int, int> frameR = frameL => (int)((frameL - startframeL) * LR + startframeR);
            


            int framel = startL;
            int framer = frameR(framel);


            while (framel < 2500) {
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

        public static IEnumerable<Tuple<T3, T4>> Select<T1, T2, T3, T4>(this IEnumerable<Tuple<T1, T2>> l, Func<T1, T3> selectorT1, Func<T2, T4> selectorT2) {
            return l.Select(tuple => new Tuple<T3, T4>(selectorT1(tuple.Item1), selectorT2(tuple.Item2)));
        }

        public abstract class Calibrator {
            /*
             * scene moddeling
             * 
             * Feature Detector
             * 
             * 3D point supplier
             * 
             * Matching 3D - feature
             * 
             * Initial calibration algorithm
             * 
             * initial Scene
             * 
             * Bundle Adjustment
             * 
             * */

        }

        static void drawReprojection(string file, IEnumerable<PointF> points, IEnumerable<PointF> error) {
            var bitmap = Bitmap.FromFile(file);
            var gfx = Graphics.FromImage(bitmap);
            var pen = new Pen(Color.Coral);

            var pointFs = points as PointF[] ?? points.ToArray();
            var enumerable = error as PointF[] ?? error.ToArray();
            for (int i = 0; i < pointFs.Count(); i++) {
                var p = pointFs[i];
                var e = enumerable[i];

                float width = 10;
                float height = 10;
                if (p.X > 0 && p.X < bitmap.Width && p.Y > 0 && p.Y < bitmap.Height) {

                    pen.Brush = new SolidBrush(pen.Color);
                    gfx.DrawEllipse(pen, p.X - width/2, p.Y - height/2, width, height);
                    pen.Width = 3;
                    gfx.DrawLine(pen, p.X, p.Y, p.X + e.X, p.Y + e.Y);
                }
                else {
                    
                }

                //gfx.DrawLine(pen, p.X, p.Y, p.X - e.X, p.Y - e.Y);
                //gfx.DrawLine(pen, p.X, p.Y, p.X + e.X, p.Y - e.Y);
                //gfx.DrawLine(pen, p.X, p.Y, p.X - e.X, p.Y + e.Y);
            }
            bitmap.Save(file+"reprojection.jpg",ImageFormat.Bmp);
            gfx.Dispose();
            bitmap.Dispose();
            File.Delete(file);
        }
        
        static void testStereoCamera() {

            Scene scene1 = new Scene();
            Scene scene2 = new Scene();
            Scene scene = scene1;

            var markers1 = IO.MarkersFromFile(@"C:\Users\jens\Desktop\calibratie\calibratieruimte_opname1.txt");
            var markers2 = IO.MarkersFromFile(@"C:\Users\jens\Desktop\calibratie\calibratieruimte_opname2.txt");
            markers1 = IO.MarkersFromFile(@"C:\Users\jens\Desktop\calibratie\calibratieruimte_opname_123.txt");
            scene1.AddRange(markers1);
            scene2.AddRange(markers2);


            var detectedAruco = getLRFramesAruco(
                @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\links\2\",
                @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\rechts\2\",
                533, 689, 2.016214371053080730500085338795, 90,533, 2500).ToTupleList();


            var markersscene1 = scene1.getIE<Marker>().ToArray();
            var markersscene2 = scene2.getIE<Marker>().ToArray();
            

            var points = new List<Tuple<
                    Tuple<string, List<Tuple<MCvPoint3D32f, PointF>>>,
                    Tuple<string, List<Tuple<MCvPoint3D32f, PointF>>>>>();



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
            
            Func<IEnumerable<Marker>, IEnumerable<ArucoMarker>, IEnumerable<Tuple<MCvPoint3D32f, PointF>>> find =
                (markers3d, arucomarkers) => {
                    List<Tuple<MCvPoint3D32f, PointF>> r = new List<Tuple<MCvPoint3D32f, PointF>>();
                    foreach (var arucomarker in arucomarkers) {
                        var scenemarker = markers3d.FirstOrDefault(x => x.ID == arucomarker.ID);
                        if (scenemarker != null) {
                            r.Add(new Tuple<MCvPoint3D32f, PointF>(
                                new MCvPoint3D32f((float) scenemarker.Pos.X, (float) scenemarker.Pos.Y,(float) scenemarker.Pos.Z),
                                new PointF(arucomarker.Corner1.X, arucomarker.Corner1.Y)));
                        }
                    }
                    return r;
                };

            foreach (Tuple<Tuple<string, List<ArucoMarker>>, Tuple<string, List<ArucoMarker>>> detectedStereo in detectedStereos) {
                //per fotopaar
                var minCount = 10;
                var L = find(markersscene1, detectedStereo.Item1.Item2);
                var R = find(markersscene1, detectedStereo.Item2.Item2);

                if (L.Count() >= minCount && R.Count() >= minCount) {
                    points.Add(new Tuple<Tuple<string, List<Tuple<MCvPoint3D32f, PointF>>>, Tuple<string, List<Tuple<MCvPoint3D32f, PointF>>>>(
                        new Tuple<string, List<Tuple<MCvPoint3D32f, PointF>>>(detectedStereo.Item1.Item1,L.ToList()),
                        new Tuple<string, List<Tuple<MCvPoint3D32f, PointF>>>(detectedStereo.Item2.Item1,R.ToList())));
                }
            }



            Matrix<double> camMat_left = new Emgu.CV.Matrix<double>(new[,] {
                {852.18, 0,975.84},
                {0,853.05,525.63},
                {0,0,1}
            });
            Matrix<double> dist_left = new Emgu.CV.Matrix<double>(new[] { -.2807, .124, .0005073, -.03291 });

            Matrix<double> camMat_right = new Emgu.CV.Matrix<double>(new[,] {
                {965.89, 0,918.07},
                {0,966.58,481.03},
                {0,0,1}
            });
            Matrix<double> dist_right = new Emgu.CV.Matrix<double>(new[] { -.29344, .15113, .001330, -.048524 });
            MCvTermCriteria criteria = new MCvTermCriteria();

            

            CeresCameraCollection stereoCameraCeres = new CeresCameraCollection();
            CeresCamera cl = new CeresCamera(Matrix4d.Identity);
            CeresCamera cr = new CeresCamera(Matrix4d.Identity);
            stereoCameraCeres.Cameras = new List<CeresCamera>();
            stereoCameraCeres.Cameras.Add(cl);
            stereoCameraCeres.Cameras.Add(cr);


            List<RotationVector3D> leftrots = new List<RotationVector3D>();
            List<RotationVector3D> rightrots = new List<RotationVector3D>();
            var leftTrans = new List<Matrix<double>>();
            var rightTrans = new List<Matrix<double>>();
            var cameradist = new List<Matrix<double>>();

            List<double> dstanc = new List<Double>();
            for (int i = 0; i < points.Count; i++) {
                var L = points[i].Item1.Item2;
                var R = points[i].Item2.Item2;

                var outr_right = new Emgu.CV.RotationVector3D();
                var outr_left = new Emgu.CV.RotationVector3D();

                var outt_right = new Matrix<double>(3, 1);
                var outt_left = new Matrix<double>(3, 1);

                var point3d = L.Select(x => x.Item1).ToArray();
                var points2d = L.Select(x => x.Item2).ToArray();
                CVI.SolvePnP(point3d,points2d,
                    camMat_left, dist_left, outr_left, outt_left, true);
                var reprojection_left = CVI.ProjectPoints(L.Select(x => x.Item1).ToArray(), outr_left, outt_left, camMat_left, dist_left);
                
                List<PointF> residuals_left = points2d.Select((t, j) => new PointF(t.X - reprojection_left[j].X, t.Y - reprojection_left[j].Y)).ToList();
                drawReprojection(points[i].Item1.Item1, reprojection_left,residuals_left);

                point3d = R.Select(x => x.Item1).ToArray();
                points2d = R.Select(x => x.Item2).ToArray();
                CVI.SolvePnP(point3d, points2d,
                    camMat_right, dist_right, outr_right, outt_right, true);
                var reprojection_right = CVI.ProjectPoints(R.Select(x => x.Item1).ToArray(), outr_right, outt_right, camMat_right, dist_right);
                
                List<PointF> residuals_right = points2d.Select((t, k) => new PointF(t.X - reprojection_right[k].X, t.Y - reprojection_right[k].Y)).ToList();
                drawReprojection(points[i].Item2.Item1, reprojection_right, residuals_right);


                leftrots.Add(outr_left);
                rightrots.Add(outr_right);
                leftTrans.Add(outt_left);
                rightTrans.Add(outt_right);

                var r =new Matrix<double>(3, 3);
                CVI.Transpose(outr_left.RotationMatrix, r);

                var posl = r*outt_left;

                CVI.Transpose(outr_right.RotationMatrix, r);
                var posr = r*outt_right;
                var dst = posl - posr;
                double d = dst[0, 0] * dst[0, 0];
                d += dst[1, 0] * dst[1, 0];
                d += dst[2, 0] * dst[2, 0];

                d = Math.Sqrt(d);

                dstanc.Add(d);

                cameradist.Add(dst);
            }
            for (int i = 0; i < points.Count; i++) {
                var L = points[i].Item1.Item2;
                var R = points[i].Item2.Item2;

                
            }
            var rot = new Matrix<double>(3, 3);
            var trans = new Matrix<double>(3, 1);
            var ess = new Matrix<double>(3, 3);
            var fundamental = new Matrix<double>(3, 3);
            MCvTermCriteria termcriteria = new MCvTermCriteria();

            /*CVI.StereoCalibrate(
                points.Select(x=>x.Item1.Item2.Select(y=>y.Item1).ToArray()).ToArray(),
                points.Select(x => x.Item1.Item2.Select(y => y.Item2).ToArray()).ToArray(),
                points.Select(x=>x.Item2.Item2.Select(y=>y.Item2).ToArray()).ToArray(),
                camMat_left, dist_left, camMat_right, dist_right,
                new System.Drawing.Size(1920, 1080), rot, trans, ess, fundamental,
                CalibType.UserIntrinsicGuess, termcriteria);*/

            
            
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

            var K = new Emgu.CV.Matrix<double>(PinholeCamera.getTestCameraHuawei().CameraMatrix.Mat);

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
