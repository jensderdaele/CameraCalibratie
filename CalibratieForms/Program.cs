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

using ceresdotnet;
using Calibratie;

using ArUcoNET;
using OpenCvSharp.Extensions;

namespace CalibratieForms {

    static class Program {
        public static object lockme = new Object();
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void  Main() {
            Log.ToConsole = true;



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

            //ArUcoNET.Aruco.CreateMarker(1, 400, "1.400.jpg");
            var dir = @"C:\Users\jens\Desktop\calibratie\canon sigma lens\test aruco platen\";
            var dirDetected = Path.Combine(dir, @"\detected\");
            /*int id = 211;
            Pdfgen.createMarker(ref id);
            Pdfgen.createMarker(ref id);
            Pdfgen.createMarker(ref id);
            Pdfgen.createMarker(ref id);
            Pdfgen.createMarker(ref id);
            return;*/
            Directory.CreateDirectory(dirDetected);

            PhotoProvider prov = new PhotoProvider(dir);
            var files = Directory.GetFiles(dir);
                
            var res = Aruco.findArucoMarkers(files, Path.Combine(Path.GetDirectoryName(files[0]), "detected"), 8);

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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
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
