using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using BundleAdjuster.GCPView.ViewModels;
using BundleAdjuster.IntrinsicsView.ViewModels;
using ceresdotnet;
using Calibratie;
using Caliburn.Micro;
using Emgu.CV;
using Emgu.CV.Util;


//using BundleAdjuster.CameraView.ViewModels;

namespace BundleAdjuster
{


    public static class BP
    {
        public static ceresdotnet.SolverOptions SolverOptions = new SolverOptions();
        public static ceresdotnet.MultiCameraBundler Bundler; //= new ceresdotnet.MultiCameraBundler();

        public static ceresdotnet.BundlerIterationTracker Tracker;

        public static HashSet<Calibratie.IMarker3d> Markers = new HashSet<Calibratie.IMarker3d>();
        public static ObservableCollection<GCP> GCPS = new ObservableCollection<GCP>();
        public static ObservableCollection<PinholeCamera> Cameras = new ObservableCollection<PinholeCamera>();

        public static double TotalCost => Bundler.GetTotalCost();

        /// <summary>
        /// DON'T ADD/REMOVE ITEMS (or bad things will happen)
        /// </summary>
        public static ObservableCollection<CameraIntrinsics> Intrinsics = new ObservableCollection<CameraIntrinsics>();


        static BP() {
            /*Bundler = new MultiCameraBundler();
            Bundler.Iteration += (sender, summary) => CeresCallbackReturnType.SOLVER_CONTINUE;

            Cameras.CollectionChanged += Cameras_CollectionChanged;
            var cvm = IoC.Get<CameraViewModel>();
            cvm.Cameras = Cameras;

            var gcpvm = IoC.Get<GCPViewModel>();
            gcpvm.Gcps = GCPS;
            gcpvm.SetBundler(Bundler);

            var ivm = IoC.Get<IntrinsicsViewModel>();
            ivm.Intrinsics = Intrinsics;*/
        }

        public static void Solve() {
            Bundler.SolveProblem(SolverOptions);
        }
        private static void Cameras_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            var col = sender as ObservableCollection<PinholeCamera>;
            if (e.Action == NotifyCollectionChangedAction.Remove) {
                foreach (var eOldItem in e.OldItems) {
                    if (!col.Contains(eOldItem) && eOldItem is PinholeCamera c) { //removed
                        //check if intr is lost
                        if (col.FirstOrDefault(x => x.Intrinsics == c.Intrinsics) == null) {
                            Intrinsics.Remove(c.Intrinsics);
                        }
                    }
                }
            }
            if (e.Action == NotifyCollectionChangedAction.Add) {
                foreach (var eNewItem in e.NewItems) {
                    var intr = (eNewItem as PinholeCamera).Intrinsics;
                    if (!Intrinsics.Contains(intr)) {
                        Intrinsics.Add(intr);
                    }
                }
            }
        }

        public static void LoadPhotoscanChunk(PhotoscanIO.Chunk chunk)
        {
            GCPS.Clear();
            Markers.Clear();
            Intrinsics.Clear();
            Cameras.Clear();
            


            //Bundler = new ceresdotnet.MultiCameraBundler();
            foreach (var cam in chunk.Cameras.Where(x=>x.Enabled)) {
                    Cameras.Add(cam);
                    Bundler.AddCamera(cam);
                
            }

            var pcloud = chunk.readPointCloud();
            var pts = pcloud.Pts3d;
            var comparer = new MarkerIDComparer();
            pts.Sort(comparer);

            foreach (var marker in pts) {
                Markers.Add(marker);
                
            }



            //ADD Observations
            var ptsid = pts.Select(x => x.ID).ToList();
            foreach (var pinholeCamera in chunk.Cameras.Where(x => x.Enabled)) {
                var bundlerobs = Bundler.GetObservationList(pinholeCamera);
                var obs2d = pcloud.ReadMarkersForCameraID(pinholeCamera.CameraID);

                bundlerobs.AddRange(from marker2D in obs2d
                    let found = ptsid.BinarySearch(marker2D.ID)
                    where found >= 0
                    select new CeresMarker{
                        id = pts[found].ID,
                        x = marker2D.X,
                        y = marker2D.Y,
                        Location = pts[found]
                    });
            }

            //ADD GCP
            foreach (var gcp in chunk.GCPs)
            {
                var obs = chunk.Frame.markerid_observations.FirstOrDefault(x => x.Key == gcp.Id);
                if (obs.Value != null && obs.Value.Count >= 2)
                {
                    var pts2d = obs.Value.Select(x => new PointF(x.x, x.y)).ToArray();
                    var pts2dvec = new VectorOfPointF(pts2d);
                    var pts2dvec_undst = new VectorOfPointF();
                    var projectionmats = obs.Value.Select(x => chunk.Cameras.Where(ca => ca.Enabled).First(cam => cam.CameraID == x.camera_id).ProjectionMatrix).ToArray();
                    var camera = chunk.Cameras.Where(ca => ca.Enabled).First(x => x.CameraID == obs.Value.First().camera_id);
                    var marker3d = new Marker(0x40000000 + obs.Key, 0, 0, 0);

                    CvInvoke.UndistortPoints(pts2dvec, pts2dvec_undst, camera.Intrinsics.cvmat, camera.Intrinsics.CVDIST, null, camera.Intrinsics.cvmat);
                    CVNative.CVNative.triangulateNViews(pts2dvec_undst.ToArray(), projectionmats, marker3d.Pos);
                    
                    gcp.AdjustedPosition = marker3d;
                    Bundler.AddGCP(gcp);
                    GCPS.Add(gcp);
                }
            }
            Bundler.BuildProblem();
        }

    }
}
