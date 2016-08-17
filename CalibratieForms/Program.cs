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

namespace CalibratieForms {

    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void  Main() {
            Log.ToConsole = true;


            
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
            
            
            //var bla = ArUcoNET.Aruco.FindMarkers(@"C:\Users\jens\Desktop\calibratie\canon 60d\IMG_3181.JPG");

            ArucoMarker m = new ArucoMarker();

            //var t = c.LoadImages(@"C:\Users\jens\Desktop\canon 60d\patterns\9x11", csize);
            //t.Wait();
            //c.Calibrate(new ChessBoard(9,11,20));
            //Console.ReadLine();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

    }
}
