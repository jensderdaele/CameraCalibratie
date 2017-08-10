using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using ArUcoNET;
using ceresdotnet;
using Calibratie;
using ComponentOwl.BetterListView;
//using OpenTK;

using Emgu.CV;

using Emgu.CV.Structure;
using Emgu.CV.Util;
using OpenTK;
using Matrix = Emgu.CV.Matrix<double>;
using CVI = Emgu.CV.CvInvoke;
using Point2d = Emgu.CV.Structure.MCvPoint2D64f;
using Vector3d = Emgu.CV.Structure.MCvPoint3D64f;


namespace CalibratieForms {
    public class CeresSimulationCollection : List<CeresSimulation> {
        public void ExcecuteAll(int threads) {
            SemaphoreSlim throttler = new SemaphoreSlim(threads); //max 8 threads
            
            List<Task> allTasks = new List<Task>();
            foreach (CeresSimulation simulation in this) {
                throttler.Wait();
                Task t = new Task(() => {
                    simulation.SolveMultiCollection();
                    throttler.Release();
                });
                allTasks.Add(t);
                t.Start();
            }
            Task.WhenAll(allTasks).Wait();
        }

        public Matrix<double> CameraToMatrix(PinholeCamera c) {
            var r = new Matrix<double>(new[] {
                c.Intrinsics.fx, c.Intrinsics.fy, c.Intrinsics.cx, c.Intrinsics.cy, c.Intrinsics.DistortionR1,
                c.Intrinsics.DistortionR2, c.Intrinsics.DistortionR3, c.Intrinsics.DistortionT1,
                c.Intrinsics.DistortionT2
            });
            return r;
        }
        public void toMatlabGem() {
        }
    }
    
    public class CeresSimulation {
        
        public Scene scene;

        public I2DRuisProvider PixelRuisProvider;
        public I3DRuisProvider WorldRuisProvider;
        public ICameraModifier CameraModifier;

        public HashSet<PinholeCamera> UniqueCameras;
        public HashSet<CameraIntrinsics> UniqueIntr;
        public HashSet<CameraCollection> UniqueCollections;

        public Dictionary<PinholeCamera,PinholeCamera> OriginalValues = new Dictionary<PinholeCamera, PinholeCamera>();

        public void SetUniqueValues() {
            UniqueCameras = new HashSet<PinholeCamera>();
            UniqueCollections = new HashSet<CameraCollection>();
            UniqueIntr = new HashSet<CameraIntrinsics>();

            var collections = scene.get<CameraCollection>();
            var cameras = scene.get<PinholeCamera>();

            foreach (var cameraCollection in collections) {
                UniqueCollections.Add(cameraCollection);
                foreach (var phc in cameraCollection) {
                    UniqueCameras.Add(phc);
                }
            }
            foreach (var phc in cameras) {
                UniqueCameras.Add(phc);
            }

            foreach (var pinholeCamera in UniqueCameras) {
                UniqueIntr.Add(pinholeCamera.Intrinsics);
            }
        }

        public CeresSimulation() { }

        public double endCost = 0;

        private static Object lockme = new Object();

        public class stereoPair {
            public Dictionary<ArucoMarker, ArucoMarker> intersection;
            public string image1;
            public string image2;
        }

        public static List<stereoPair> findImagePairsMinMarkers(Dictionary<string, IEnumerable<ArucoMarker>> markersDict,
            int minIdenticalMarkers) {
            List<string> excludedFiles = new List<string>();

            List<stereoPair> pairs = new List<stereoPair>();

            foreach (KeyValuePair<string, IEnumerable<ArucoMarker>> kvp in markersDict) {
                foreach (KeyValuePair<string, IEnumerable<ArucoMarker>> kvp2 in markersDict) {
                    if (kvp.Key.Equals(kvp2.Key)) continue;
                    stereoPair pair;
                    var intersect = kvp.Value.getIntersection(kvp2.Value, new EqualityComparer<ArucoMarker>((marker, arucoMarker) => marker.ID == arucoMarker.ID));
                    if (intersect.Count() >= minIdenticalMarkers) {
                        pairs.Add(new stereoPair {
                            image1 = kvp.Key,
                            image2 = kvp2.Key,
                            intersection = intersect
                        });
                    }
                }
            }
            return pairs;
        }
        
        public static void findImagePairsMinMarkersBestFit(Dictionary<string, IEnumerable<ArucoMarker>> markersDict,
            int minIdenticalMarkers) {
            List<string> excludedFiles = new List<string>();
            foreach (KeyValuePair<string, IEnumerable<ArucoMarker>> kvp in markersDict) {
                foreach (KeyValuePair<string, IEnumerable<ArucoMarker>> kvp2 in markersDict) {
                    if (!kvp.Key.Equals(kvp2.Key)) {
                        var intersect = kvp.Value.Intersect(kvp2.Value, new EqualityComparer<ArucoMarker>((marker, arucoMarker) => marker.ID == arucoMarker.ID));
                    }
                }
            }
        }
        
        public static unsafe void ceresSolveAruco() {
            var phc = PinholeCamera.getTestCameraHuawei();
            string dir = @"C:\Users\jens\Desktop\calibratie\Huawei p9\aruco\stereo test\";
            List<CeresMarker> ceresmarkers = new List<CeresMarker>();
            List<CeresCamera> cerescameras = new List<CeresCamera>();
            var files = Directory.GetFiles(dir).ToList();


            //8 punten nodig
            var markerDictionary = Aruco.findArucoMarkers(files, Path.Combine(dir, "aruco_detected\\"),1);
            var pairs = findImagePairsMinMarkers(markerDictionary, 8);

            Matrix K = new Matrix(phc.Intrinsics.Mat);

            var W = new Matrix(new double[] {
                0.0D, -1.0D, 0.0D,
                1.0D, 0.0D, 0.0D,
                0.0D, 0.0D, 1.0D
            });


            var Wt = new Matrix(new double[] {
                0.0D, 1.0D, 0.0D,
                -1.0D, 0.0D, 0.0D,
                0.0D, 0.0D, 1.0D
            });
            var Z = new Matrix(new double[] {
                0.0D, 1.0D, 0.0D,
                -1.0D, 0.0D, 0.0D,
                0.0D, 0.0D, 0D
            });

            var diag = new Matrix(new double[] {
                1.0D, 0.0D, 0.0D,
                0.0D, 1.0D, 0.0D,
                0.0D, 0.0D, 0.0D
            });


            foreach (var stereoPair in pairs) {

                var points_count = stereoPair.intersection.Count;
                VectorOfPointF punten1px, punten2px;
                {
                    int i = 0;
                    List<PointF> p1 = new List<PointF>();
                    List<PointF> p2 = new List<PointF>();
                    foreach (KeyValuePair<ArucoMarker, ArucoMarker> kvp in stereoPair.intersection) {
                        p1.Add(kvp.Key.Corner1);
                        p2.Add(kvp.Value.Corner1);
                        i++;
                    }
                    punten1px = new VectorOfPointF(p1.ToArray());
                    punten2px = new VectorOfPointF(p2.ToArray());
                }


                Matrix F = new Matrix(3, 3);
                CVI.FindFundamentalMat(punten1px,punten2px,F);




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
                    Ps[i] = new Matrix(3,4);

                CVI.HConcat(R1, T1, Ps[0]);
                CVI.HConcat(R1, T2, Ps[1]);
                CVI.HConcat(R2, T1, Ps[2]);
                CVI.HConcat(R2, T2, Ps[3]);

                var KPs = new Matrix[4];
                KPs[0] = K * Ps[0];
                KPs[1] = K * Ps[1];
                KPs[2] = K * Ps[2];
                KPs[3] = K * Ps[3];
                
                
                var KP0 = K * new Matrix(new double [,]{{1,0,0,0},{0,1,0,0},{0,0,1,0}});

                for (int i = 0; i < 4; i++) {


                    Matrix<float> output_hom = new Matrix<float>(4,punten1px.Size);
                    VectorOfPoint3D32F output_3d = new VectorOfPoint3D32F();
                    
                    CVI.TriangulatePoints(KP0, KPs[i], punten1px, punten2px, output_hom);
                    CVI.ConvertPointsFromHomogeneous(output_hom, output_3d);
                }

                Matrix S = U*diag*W*U.Transpose();

                Matrix R = U*W*decomp.Vt;


            }
        }

        public double GemFeaturesPerFoto;



        public void SolveMultiCollection() {
            SetUniqueValues();
            var collections = scene.get<CameraCollection>();
            var markers3d = scene.get<Marker>();
            var cameras = scene.get<PinholeCamera>();




            var collec = new CameraCollection(cameras);

            collections = new[] {collec};


            var ccoll = new CeresCameraCollection();
            ccoll.Cameras = new List<CeresCamera>();
            ccoll.Cameras.AddRange(cameras.Select(x => {
                var cc = x.toCeresCamera();
                return cc;
            }));
            // ccoll.CreateSecondPositionCopy();

            var bundler = new ceresdotnet.CeresCameraMultiCollectionBundler();




            Dictionary<CeresCameraCollection, Dictionary<CeresCamera, List<CeresMarker>>> observations =
                new Dictionary<CeresCameraCollection, Dictionary<CeresCamera, List<CeresMarker>>>();



            List<CeresCamera> cerescameras = new List<CeresCamera>();
            List<CeresCameraCollection> cerescameracollections = new List<CeresCameraCollection>();

            int cameraID = -1;

            var collectionobservations2 = new Dictionary<CeresCamera, List<CeresMarker>>();

            foreach (var camera in UniqueCameras) {
                //voor ruis kopie maken
                var cameracopy = new PinholeCamera();
                var cc = camera.toCeresCamera();
                cameracopy.toCeresCamera();
                cameracopy.updateFromCeres(cc.Internal);
                cameracopy.updateFromCeres(cc.External);
                this.OriginalValues.Add(camera, cameracopy);
            }

            

            int totaalfotos = 0;
            foreach (var collection in collections) {
                var cerescollection = new CeresCameraCollection();
                var collectionobservations = new Dictionary<CeresCamera, List<CeresMarker>>();

                foreach (var camera in collection) {
                    totaalfotos++;
                    List<CeresMarker> ceresmarkers = new List<CeresMarker>();
                    cameraID++;

                    var puntenCv =
                        markers3d.ToDictionary(m => new MCvPoint3D32f((float) m.X, (float) m.Y, (float) m.Z));

                    var cc = camera.toCeresCamera();


                    ceresdotnet.CeresTestFunctions.ProjectPoint(cc.Internal, cc.External, markers3d[0].Pos);

                    
                    
                    var visible_proj = camera.ProjectPointd2D_Manually(markers3d,out Marker[] visible3d);
                    GemFeaturesPerFoto += visible_proj.Length;

                    //Pixel ruis
                    for (int i = 0; i < visible_proj.Length; i++) {
                        PixelRuisProvider?.Apply(ref visible_proj[i]);
                    }


                    //in een cerescamera worden interne parameters opgeslaan volgens array v doubles

                    //Per interne parameters kan men bepalen wat dient gebundeld te worden
                    //ook combinatie zijn mogelijk
                    //3D ruis
                    foreach (var marker in markers3d) {
                        //WorldRuisProvider?.Apply(marker);
                    }

                    for (int i = 0; i < visible3d.Length; i++) {
                        var proj = visible_proj[i];
                        ceresmarkers.Add(new CeresMarker() {
                            id = visible3d[i].ID,
                            Location = visible3d[i].toCeresParameter(),
                            x = proj.X,
                            y = proj.Y
                        });
                    }

                    cerescameras.Add(cc);
                    collectionobservations.Add(cc, ceresmarkers);

                    //gesimuleerde foto weergeven
                    /*var window2 = new CameraSimulationFrm(string.Format("Camera {0}: {1}", cameraID, camera.Name)) {
                        Camera = camera
                    };
                    window2.Show();
                    window2.drawChessboard(visible_proj);*/
                }
                observations.Add(cerescollection, collectionobservations);
                collectionobservations2 = collectionobservations;
            }
            GemFeaturesPerFoto /= totaalfotos;
            foreach (var intr in UniqueIntr) {
                //Camera ruis/modifier
                CameraModifier?.Apply(intr);
                intr.toCeresParameter();
            }
            foreach (var marker in markers3d) {
                WorldRuisProvider?.Apply(marker);
                marker.toCeresParameter();
            }


            CeresCameraMultiCollectionBundler.MarkersFromCameraDelegate findObservationsFunc = (camera, coll) => {
                var r = collectionobservations2[camera];
                double Allreproj = 0;
                double totalmrkrcout = 0;
                //foreach (var ceresCamera in bundler.StandaloneCameraList) {
                double reprojections = 0;
                foreach (var ceresMarker in r) {
                    var reproj = CeresTestFunctions.ReprojectPoint(camera.Internal, camera.External,
                        ceresMarker.toPointF(), ceresMarker.Location.toMatrix());
                    reprojections += Math.Sqrt(reproj.X * reproj.X + reproj.Y * reproj.Y);
                }
                Allreproj += reprojections;
                totalmrkrcout += r.Count;
                reprojections /= r.Count;
                //}

                return r;

            };

            bundler.MarkersFromCamera = findObservationsFunc;
            bundler.CollectionList = cerescameracollections;
            bundler.StandaloneCameraList = cerescameras;

            bundler.bundleCollections(iterationCallbackHandler);

            CeresCameraMultiCollectionBundler b = bundler;

            double AllReprojections = 0;
            double totalmarkercount = 0;

            double meanPosX = 0;
            double meanPosY = 0;
            double meanPosZ = 0;

            foreach (var ceresCamera in b.StandaloneCameraList) {
                double reprojections = 0;
                var markerlist = b.MarkersFromCamera(ceresCamera, null);
                foreach (var ceresMarker in markerlist) {
                    var reproj = CeresTestFunctions.ReprojectPoint(ceresCamera.Internal, ceresCamera.External, ceresMarker.toPointF(), ceresMarker.Location.toMatrix());
                    reprojections += Math.Sqrt(reproj.X * reproj.X + reproj.Y * reproj.Y);
                }
                AllReprojections += reprojections;
                totalmarkercount += markerlist.Count;
                reprojections /= markerlist.Count;

                //mean cam pos;
                var pos = ceresCamera.External.t;
                meanPosX += pos[0];
                meanPosY += pos[1];
                meanPosZ += pos[2];
            }
            meanPosX /= b.StandaloneCameraList.Count;
            meanPosY /= b.StandaloneCameraList.Count;
            meanPosZ /= b.StandaloneCameraList.Count;

            AllReprojections /= totalmarkercount;
            reporjectionsstring += String.Format("({0}) Error: {1}", "final", AllReprojections) + Environment.NewLine;
            meancamereapos += String.Format("({0}) pos: {1}  {2}  {3}", "final", AllReprojections, meanPosX, meanPosY, meanPosZ) + Environment.NewLine;
            Console.WriteLine("({0}) reprojerror: {1}   mean cam pos: x({2}) y({3}) z({4})", "final", AllReprojections, meanPosX, meanPosY, meanPosZ);

            lastReproj = AllReprojections;

            foreach (var collection in collections) {
                foreach (var camera in collection) {
                    camera.updateFromCeres();
                }
            }
        }


        public string cameraparamsstring = "";
        public string reporjectionsstring = "";
        public string meancamereapos = "";
        public double lastReproj = 0;


        protected CeresCallbackReturnType iterationCallbackHandler(object sender, ceresdotnet.IterationSummary summary) {
            //return CeresCallbackReturnType.SOLVER_CONTINUE;
            int iterationNr = summary.iteration;
            
            CeresCameraMultiCollectionBundler b = sender as CeresCameraMultiCollectionBundler;

            double AllReprojections = 0;
            double totalmarkercount = 0;

            double meanPosX = 0;
            double meanPosY = 0;
            double meanPosZ = 0;

            foreach (var ceresCamera in b.StandaloneCameraList) {
                double reprojections = 0;
                var markerlist = b.MarkersFromCamera(ceresCamera, null);
                foreach (var ceresMarker in markerlist) {
                    var reproj = CeresTestFunctions.ReprojectPoint(ceresCamera.Internal, ceresCamera.External, ceresMarker.toPointF(), ceresMarker.Location.toMatrix());
                    reprojections += Math.Sqrt(reproj.X * reproj.X + reproj.Y * reproj.Y);
                }
                AllReprojections += reprojections;
                totalmarkercount += markerlist.Count;
                reprojections /= markerlist.Count;

                //mean cam pos;
                var pos = ceresCamera.External.t;
                meanPosX += pos[0];
                meanPosY += pos[1];
                meanPosZ += pos[2];
            }
            meanPosX /= b.StandaloneCameraList.Count;
            meanPosY /= b.StandaloneCameraList.Count;
            meanPosZ /= b.StandaloneCameraList.Count;

            AllReprojections /= totalmarkercount;
            reporjectionsstring += String.Format("({0}) Error: {1}", iterationNr, AllReprojections) + Environment.NewLine;
            meancamereapos += String.Format("({0}) pos: {1}  {2}  {3}", iterationNr, AllReprojections, meanPosX, meanPosY, meanPosZ) + Environment.NewLine;
            //Console.WriteLine("({0}) reprojerror: {1}   mean cam pos: x({2}) y({3}) z({4})", iterationNr, AllReprojections, meanPosX, meanPosY, meanPosZ);

            lastReproj = AllReprojections;
            endCost = summary.cost;
            var r = CeresCallbackReturnType.SOLVER_CONTINUE;

            return r;
        }

        [Obsolete()]
        public void Solve() {
            /*
            var markers3d = scene.get<Marker>();
            var cameras = scene.get<PinholeCamera>();

            var cameraCollecion = new CameraCollection(cameras);

            //cameraCollecion.SetCollectionCenter_MidCameras();
            
            List<CeresCamera> cerescameras = new List<CeresCamera>();
            Dictionary<CeresCamera, List<CeresMarker>> observations = new Dictionary<CeresCamera, List<CeresMarker>>();
            
            int cameraID = -1;

            var intr = new CeresIntrinsics {
                BundleFlags = BundleIntrinsicsFlags.ALL,
                Intrinsics = cameras.First().toCeresIntrinsics9()
            };
            
            foreach (var camera in cameras) {
                List<CeresMarker> ceresmarkers = new List<CeresMarker>();
                cameraID++;
                Dictionary<Vector3d, Marker> puntenCv = markers3d.ToDictionary(m => m.Pos);
                Vector3d[] visible3d;
                var visible_proj = camera.ProjectPointd2D_Manually(puntenCv.Keys.ToArray(), out visible3d);
                var cc = new CeresCamera(camera.worldMat) {
                    Internal = intr
                };

                
                //in een cerescamera worden interne parameters opgeslaan volgens array v doubles
                
                //Per interne parameters kan men bepalen wat dient gebundeld te worden
                //ook combinatie zijn mogelijk
 
                cerescameras.Add(cc);

                var obs2 = new List<Tuple<int, System.Drawing.PointF>>();
                for (int i = 0; i < visible3d.Length; i++) {
                    var proj = visible_proj[i];

                    var marker = new CeresMarker() {
                        id = puntenCv[visible3d[i]].ID,
                        Location = new CeresPoint {
                            BundleFlags = BundleWorldCoordinatesFlags.None,
                            Coordinates_arr = visible3d[i].toArr()
                        },
                        x = proj.X,
                        y = proj.Y
                    };
                    obs2.Add(new Tuple<int, PointF>(marker.id, new PointF((float)marker.x, (float)marker.y)));
                    var res = ceresdotnet.CeresCameraCollectionBundler.testProjectPoint(cc,
                        new CeresPointOrient() { RT = new[] { 0D, 0, 0, 0, 0, 0 } }, marker);
                    proj.X -= res[0];
                    proj.Y -= res[1];
                    marker.x = proj.X;
                    marker.y = proj.Y;
                    ceresmarkers.Add(marker); 
                }
                
                Util.SolvePnP(scene,camera,obs2);
                observations.Add(cc, ceresmarkers);
                
                //gesimuleerde foto weergeven
                var window2 = new CameraSimulationFrm(string.Format("Camera {0}: {1}", cameraID, camera.Name)) {
                    Camera = camera
                };
                window2.Show();
                window2.drawChessboard(visible_proj.Select(x => new Vector2((float)x.X, (float)x.Y)).ToArray());
            }
            var bundler = new ceresdotnet.CeresCameraCollectionBundler();

            double[] rodr = new double[]{0, 0, 0};
            bundler.Collection = new CeresCameraCollection() {
                Cameras = cerescameras,
                Position = new CeresPointOrient() {
                    Bundle = false,
                    R_rod = rodr,
                    t = new[] { 0D, 0, 0 }
                }
            };
            bundler.Observations = observations;

            bundler.bundleCollection(iterationCallbackHandler);
            */
        }
        

        [Flags]
        public enum BundleIntrinsics : int {
            BUNDLE_NO_INTRINSICS = 0,
            BUNDLE_FOCAL_LENGTH = 1,
            BUNDLE_PRINCIPAL_POINT = 2,
            BUNDLE_RADIAL_K1 = 4,
            BUNDLE_RADIAL_K2 = 8,
            BUNDLE_RADIAL = 12,
            BUNDLE_TANGENTIAL_P1 = 16,
            BUNDLE_TANGENTIAL_P2 = 32,
            BUNDLE_TANGENTIAL = 48,
            BUNDLE_ALL = 1 | 2 | 12 | 48,
        };
    }
}
