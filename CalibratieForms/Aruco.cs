using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArUcoNET;

namespace CalibratieForms {
    public static class Aruco {


        private static object lockme = new Object();
        private static object lockme2 = new Object();
        public static Dictionary<string, IEnumerable<ArUcoNET.ArucoMarker>> findArucoMarkers(IEnumerable<string> files, string outputDir = null, int maxThreads = 8) {
            maxThreads = maxThreads > files.Count() ? files.Count() : maxThreads;
            SemaphoreSlim throttler = new SemaphoreSlim(maxThreads);
            List<Task> alltasks = new List<Task>();
            int count = 0;
            if (!string.IsNullOrEmpty(outputDir)) {
                Directory.CreateDirectory(outputDir);
            }
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
                IEnumerable<ArucoMarker> markers;
                string detectedfile = Path.Combine(outputDir, fname + "detected.jpg");
                markers = string.IsNullOrEmpty(outputDir) ? ArUcoNET.Aruco.FindMarkers(f) : ArUcoNET.Aruco.FindMarkers(f, detectedfile);
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
