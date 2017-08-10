using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ArUcoNET;
using Calibratie;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace CalibratieForms {
    public class ArucoMarkerDetector : IMarkerDetector {
        public IEnumerable<ArucoMarker> detectMarkers(Mat image) {
            return ArUcoNET.Aruco.FindMarkers(image, "");
            
        }
        IEnumerable<Marker2d> IMarkerDetector.detectMarkers(Mat image) {
            return detectMarkers(image);
        }
    }
    public static class Aruco {
        private static object lockme = new Object();
        private static object lockme2 = new Object();
        public static Dictionary<string, IEnumerable<ArUcoNET.ArucoMarker>> findArucoMarkers(IEnumerable<string> files, string outputDir = "detected", int maxThreads = 8,Func<string,Emgu.CV.Mat> fileReader = null) {

            

            maxThreads = maxThreads > files.Count() ? files.Count() : maxThreads;
            SemaphoreSlim throttler = new SemaphoreSlim(maxThreads);
            List<Task> alltasks = new List<Task>();
            int count = 0;
            
            if (!string.IsNullOrEmpty(outputDir)) {
                Directory.CreateDirectory(outputDir);
            }
            Dictionary<string, IEnumerable<ArucoMarker>> dict = new Dictionary<string, IEnumerable<ArucoMarker>>(files.Count());
            
            Action<Object> findarucomarkersaction = o => {

                XmlSerializer ser = new XmlSerializer(typeof(List<ArucoMarker>));
                String f = (String)o;
                var xmlfile = f + ".aruco.xml";

                IEnumerable<ArucoMarker> markers;

                
                var fname = Path.GetFileName(f);
                Console.WriteLine("searching file {0} for aruco markers", fname);
                int test = count;
                lock (lockme) {
                    test = count;
                    count++;
                }
                //var picture = Bitmap.FromFile(f);

                string detectedfile = null;

                if (outputDir == "detected") {
                    detectedfile = Path.Combine(f, "/detected/", "detected.jpg");
                }else if (!string.IsNullOrEmpty(outputDir)) {
                    detectedfile = Path.Combine(outputDir, fname + "detected.jpg");
                }
                if (File.Exists(xmlfile)) {
                    var reader = new XmlTextReader(xmlfile);
                    markers = (List<ArucoMarker>)ser.Deserialize(reader);
                    if (detectedfile != null && !File.Exists(detectedfile)) {
                        ArUcoNET.Aruco.DrawMarkers(CvInvoke.Imread(f), markers, detectedfile);
                    }
                    reader.Close();
                    reader.Dispose();
                }
                else if(fileReader != null) {
                    markers = ArUcoNET.Aruco.FindMarkers(fileReader(f), detectedfile);
                }
                else {
                    markers = ArUcoNET.Aruco.FindMarkers(f, detectedfile);
                }
                if (!File.Exists(xmlfile)) {
                    var writer = new XmlTextWriter(xmlfile,Encoding.ASCII);
                    ser.Serialize(writer,markers.ToList());
                    writer.Flush();
                    writer.Close();
                }


                
                lock (lockme2) {
                    dict.Add(detectedfile, markers);
                }
                throttler.Release();
            };

            foreach (var f in files) {
                throttler.Wait();
                Task t = new Task(findarucomarkersaction, f);
                t.Start();
                alltasks.Add(t);
            }
            Task.WhenAll(alltasks).Wait();
            return dict;
        }

        public delegate void markerDetectCallback(string file, string outputDir, IEnumerable<ArucoMarker> markers);
        public static async Task<Dictionary<string, IEnumerable<ArUcoNET.ArucoMarker>>> findArucoMarkersAsync(IEnumerable<string> files, string outputDir, int maxThreads = 8,markerDetectCallback callback = null) {

            maxThreads = maxThreads > files.Count() ? files.Count() : maxThreads;
            SemaphoreSlim throttler = new SemaphoreSlim(maxThreads);
            List<Task> alltasks = new List<Task>();
            int count = 0;
            Directory.CreateDirectory(outputDir);
            Dictionary<string, IEnumerable<ArucoMarker>> dict = new Dictionary<string, IEnumerable<ArucoMarker>>();
            Action<Object> findarucomarkersaction = o => {
                String f = (String)o;
                var fname = Path.GetFileName(f);
                Console.WriteLine("searching file {0} for aruco markers", fname);
                int test = count;
                lock (lockme) {
                    test = count;
                    count++;
                }
                var picture = Bitmap.FromFile(f);
                var detectedFile = Path.Combine(outputDir, fname + "detected.jpg");
                var markers = ArUcoNET.Aruco.FindMarkers(f, detectedFile);
                if (callback != null) { callback(f, detectedFile, markers); }
                dict.Add(f, markers);
                throttler.Release();
            };
            var tsk = new Task(() => {
                foreach (var f in files) {
                    throttler.WaitAsync();
                    Task t = new Task(findarucomarkersaction, f);
                    t.Start();
                    alltasks.Add(t);
                }
            });
            tsk.Start();
            //await tsk;
            await Task.WhenAll(alltasks);
            //Task.WhenAll(alltasks).Wait();
            return dict;
        }
    }
}
