using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using cameracallibratie;
using CalibratieForms.Logging;
using OpenCvSharp;
using OpenTK;
using Size = OpenCvSharp.Size;

using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Forms.VisualStyles;
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
using OpenCvSharp.Extensions;
using PdfSharp;
using SceneManager;
using Mat = OpenCvSharp.Mat;


namespace CalibratieForms {

    static class Program {

        static void CalibrateCeresSolver(List<Tuple<Point3f,Point2f>> punten, PinholeCamera camera) {
            var m = camera.CameraMatrix.Mat;

            double[] rvec, tvec;
            Cv2.SolvePnP(punten.Select(x => x.Item1), punten.Select(x => x.Item2), m,null,out rvec,out tvec,false,SolvePnPFlags.UPNP);

            //ceresdotnet.MultiCameraBundleProblem problem;

        }
        public static object lockme = new Object();
        // "C:\Users\jens\Desktop\calibratie\Opnames_thesis\links\2
        public static Dictionary<string, string> GetLRFrames(string dirL,string dirR,int KeyFrameL, int KeyFrameR, double LR, int intervalL,int startL, int stopL) {
            var r = new Dictionary<string, string>();


            int startframeL = KeyFrameL;
            int startframeR = KeyFrameR;
            int fps_links = 30;
            Func<int, int> frameR = frameL => {
                return (int)((frameL - startframeL) * LR + startframeR);
            };



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

        public static Dictionary<Tuple<string, IEnumerable<ArucoMarker>>, Tuple<string, IEnumerable<ArucoMarker>>> getLRFramesAruco(
            string dirL,string dirR,
            int KeyFrameL,
            int KeyFrameR, double LR, int intervalL,int startL, int stopL) {
                var files = GetLRFrames(dirL, dirR,KeyFrameL, KeyFrameR, LR, intervalL, startL, stopL);

            var L = Aruco.findArucoMarkers(files.Keys).Select(x=>new Tuple<string,IEnumerable<ArucoMarker>>(x.Key,x.Value)).ToList();
            var R = Aruco.findArucoMarkers(files.Values).Select(x => new Tuple<string, IEnumerable<ArucoMarker>>(x.Key, x.Value)).ToList();

            var r = new Dictionary<Tuple<string, IEnumerable<ArucoMarker>>, Tuple<string, IEnumerable<ArucoMarker>>>();
            for (int i = 0; i < L.Count; i++) {
                r.Add(L[i], R[i]);
            }
            return r;
        }  

        static void testStereoCamera() {
            StereoCamera sc = new StereoCamera();
            ZhangCalibration calibration = new ZhangCalibration();

            Scene scene1 = new Scene();
            Scene scene2 = new Scene();


            var markers1 = IO.MarkersFromFile(@"C:\Users\jens\Desktop\calibratie\calibratieruimte_opname1.txt");
            var markers2 = IO.MarkersFromFile(@"C:\Users\jens\Desktop\calibratie\calibratieruimte_opname2.txt");
            scene1.AddRange(markers1);
            scene2.AddRange(markers2);


            




            var detectedZhang = GetLRFrames(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\links\1",
                @"C:\Users\jens\Desktop\calibratie\Opnames_thesis\rechts\1"
                ,342, 1740, 2.016214371053080730500085338795, 20, 500, 1300);


            ZhangCalibration stereotest = new ZhangCalibration();
            List<string> badl = new List<string>();
            List<string> badr = new List<string>();
            stereotest.LoadImages(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\links\1\", new Size(6, 9), detectedZhang.Keys.ToList(), badl);
            stereotest.LoadImages2(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\rechts\1\", new Size(6, 9), detectedZhang.Values.ToList(), badr);

            List<int> badindices = new List<int>();

            for (int i = 0; i < stereotest.images.Count; i++) {
                if (stereotest.images[i] == null || stereotest.images2[i] == null) {
                    stereotest.images.RemoveAt(i);
                    stereotest.images2.RemoveAt(i);
                    i--;
                }

            }

            stereotest.StereoCalibrateCV(new ChessBoard(6, 9, 55.03333333333));


            ZhangCalibration zhang = new ZhangCalibration();
            zhang.LoadImages(detectedZhang.Keys.ToList(),new Size(6,9));

            double[,] cameramat;
            double[] dist5;
            zhang.Calibrate(new ChessBoard(9, 6, 55.03333333333333333), out cameramat,out dist5);



            
            var detectedAruco = getLRFramesAruco("","",533, 689, 2.016214371053080730500085338795, 30,533, 2500);

            List<List<MCvPoint3D32f>> points3d = new List<List<MCvPoint3D32f>>();
            List<List<PointF>> imagepoints = new List<List<PointF>>();

            var markersscene1 = scene1.getIE<Marker>();
            var markersscene2 = scene2.getIE<Marker>();

            foreach (Tuple<string, IEnumerable<ArucoMarker>> kvp in detectedAruco.Keys) {
                var current3d =new List<MCvPoint3D32f>();
                var current2d = new List<PointF>();

                foreach (var arucoMarker in kvp.Item2) {
                    var scenemarker = markersscene1.FirstOrDefault(x => x.ID == arucoMarker.ID);
                    if (scenemarker != null) {
                        current2d.Add(new PointF(arucoMarker.Corner1.X, arucoMarker.Corner1.Y));
                        current3d.Add(new MCvPoint3D32f((float)scenemarker.Pos.X, (float)scenemarker.Pos.Y, (float)scenemarker.Pos.Z));
                    }

                    scenemarker = markersscene2.FirstOrDefault(x => x.ID == arucoMarker.ID);
                    if (scenemarker != null) {
                        //current2d.Add(new PointF(arucoMarker.Corner1.X, arucoMarker.Corner1.Y));
                        //current3d.Add(new MCvPoint3D32f((float)scenemarker.Pos.X, (float)scenemarker.Pos.Y, (float)scenemarker.Pos.Z));
                    }
                }
                if (current3d.Count > 7) {
                    points3d.Add(current3d);
                    imagepoints.Add(current2d);
                }
            }
            
            Matrix<double> camMat = new Emgu.CV.Matrix<double>(3,3);
            Matrix<double> dist = new Emgu.CV.Matrix<double>(4,1);
            MCvTermCriteria criteria = new MCvTermCriteria();
            Emgu.CV.Mat[] out1, out2;

            Emgu.CV.CvInvoke.CalibrateCamera(points3d.Select(x => x.ToArray()).ToArray(),
                imagepoints.Select(x => x.ToArray()).ToArray(),
                new System.Drawing.Size(1920, 1080), camMat, dist, CalibType.Default,
                criteria, out out1, out out2);



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

            stereotest.StereoCalibrateCV(new ChessBoard(6, 9, 55.03333333333));

            

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
            /*var res2 = Aruco.findArucoMarkersAsync(files, Path.Combine(Path.GetDirectoryName(files[0])), 8,
                (file, outputDir, markers) => {
                    Console.WriteLine("{0} markers found ({1})", file, markers.Count());
                });
            

            var result = res2.Result;*/
            //var res = t.Result;
            //t.Wait();
            //res = t.Result;
            var testfile = @"C:\Users\jens\Desktop\calibratie\fotos paneel\testh\im.jpg";
            var immat = Cv2.ImRead(testfile);
            Mat m = new Mat();
            var ph = PinholeCamera.getTestCameraHuawei();

            Cv2.Undistort(immat, m, ph.CameraMatrix.cvmat,ph.Cv_DistCoeffs5_cv);


            var undistf = @"C:\Users\jens\Desktop\calibratie\fotos paneel\testh\im.undistorted.jpg";
            m.SaveImage(undistf);
            var pwereld = new List<Point2d> {
                new Point2d(0, 0),
                new Point2d(2440, 0)*2,
                new Point2d(2440, 1220)*2,
                new Point2d(0,1220)*2,
            };
            var ppixel = new List<Point2d> {
                new Point2d(77, 2163),
                new Point2d(3940, 2559),
                new Point2d(3892, 391),
                new Point2d(797,898),
            };
            var pwereldf = new List<Point2f> {
                new Point2f(0, 0)*2,
                new Point2f(2440, 0)*2,
                new Point2f(2440, 1220)*2,
                new Point2f(0,1220)*2,
            };
            var ppixelf = new List<Point2f> {
                new Point2f(77, 2163),
                new Point2f(3940, 2559),
                new Point2f(3892, 391),
                new Point2f(797,898),
            };
            var H = Cv2.FindHomography(ppixel, pwereld, HomographyMethods.LMedS);
            Mat outp = new Mat();
            Cv2.WarpPerspective(m, outp,H,new Size(m.Size().Width*2,m.Size().Height*2));

            var outputfile = @"C:\Users\jens\Desktop\calibratie\fotos paneel\testh\im.outp.jpg";
            outp.SaveImage(@"C:\Users\jens\Desktop\calibratie\fotos paneel\testh\im.outp.jpg");

            var markersRectified = ArUcoNET.Aruco.FindMarkers(outputfile, outputfile + "detected.jpg");
            

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
                    pixel.Add(m.Corner1.to2d());
                }
            }
            var H = Cv2.FindHomography(pixel, wereld, HomographyMethods.LMedS);
        }


    }
}
