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
using SceneManager;

using Emgu.CV;

using Emgu.CV.Structure;
using Emgu.CV.Util;
using OpenTK;
using Matrix = Emgu.CV.Matrix<double>;
using CVI = Emgu.CV.CvInvoke;
using Point2d = Emgu.CV.Structure.MCvPoint2D64f;
using Vector3d = Emgu.CV.Structure.MCvPoint3D64f;


namespace CalibratieForms {
    
    public class CeresSimulation {
        
        public Scene scene;
        public Dictionary<PinholeCamera, PinholeCamera> pinholeCameras;
 
        
        public CeresSimulation() { }

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
        /*
        public List<CeresCameraCollection> toCeresColl(List<CameraCollection> colls) {
            Dictionary<PinholeCamera, CeresCamera> usedCameras = new  Dictionary<PinholeCamera, CeresCamera>();
            Dictionary<String, CeresIntrinsics> usedIntrinsics = new Dictionary<String, CeresIntrinsics>();

            List<CeresCameraCollection> cerescolls = new List<CeresCameraCollection>();
            foreach (var coll in colls) {

                CeresCameraCollection ccol = new CeresCameraCollection();
                ccol.Cameras = new List<CeresCamera>();
                foreach (var pinholecamera in coll) {
                    CeresCamera cc;
                    if (usedCameras.ContainsKey(pinholecamera)) {
                        cc = usedCameras[pinholecamera];
                    }
                    else {
                        CeresIntrinsics intr;
                        if (usedIntrinsics.ContainsKey(pinholecamera.Name)) {
                            intr = usedIntrinsics[pinholecamera.Name];
                        }
                        else {
                            intr = new CeresIntrinsics(pinholecamera.toCeresIntrinsics9());
                            usedIntrinsics.Add(pinholecamera.Name, intr);
                        }
                        cc = new CeresCamera(pinholecamera.worldMat) {Internal = intr};
                        usedCameras.Add(pinholecamera, cc);
                    }

                    
                }
                    
            }

        } */



        public void SolveMultiCollection() {
            
            var collections = scene.get<CameraCollection>();
            var markers3d = scene.get<Marker>();
            var cameras = scene.get<PinholeCamera>();




            var collec = new CameraCollection(cameras);

            collections = new[] {collec};

            var intri = new CeresIntrinsics {
                BundleFlags = BundleIntrinsicsFlags.ALL,
                Intrinsics = cameras.First().toCeresIntrinsics9()
               
            };

            var ccoll = new CeresCameraCollection();
            ccoll.Cameras.AddRange(cameras.Select(x => {
                var cc = new CeresCamera(x.worldMat) {
                    Internal = intri
                };
                return cc;
            }));
            ccoll.CreateSecondPositionCopy();

            var bundler =  new ceresdotnet.CeresCameraMultiCollectionBundler();

            


            Dictionary<CeresCameraCollection,Dictionary<CeresCamera, List<CeresMarker>>> observations = new Dictionary<CeresCameraCollection, Dictionary<CeresCamera, List<CeresMarker>>>();
            


            List<CeresCamera> cerescameras = new List<CeresCamera>();
            List<CeresCameraCollection> cerescameracollections = new List<CeresCameraCollection>(); 

            int cameraID = -1;
            
            
            foreach (var collection in collections) {
                var intr = new CeresIntrinsics {
                    BundleFlags = BundleIntrinsicsFlags.ALL,
                    Intrinsics = collection.First().toCeresIntrinsics9()
                };
                var cerescollection = new CeresCameraCollection();
                var collectionobservations = new Dictionary<CeresCamera, List<CeresMarker>>();

                foreach (var camera in collection) {
                    List<CeresMarker> ceresmarkers = new List<CeresMarker>();
                    cameraID++;
                    var puntenCv = markers3d.ToDictionary(m => new MCvPoint3D32f((float)m.Pos.X, (float)m.Pos.Y, (float)m.Pos.Z));
                    MCvPoint3D32f[] visible3d;
                    var visible_proj = camera.ProjectPointd2D_Manually(puntenCv.Keys.ToArray(), out visible3d);
                    var cc = new CeresCamera(camera.worldMat) {
                        Internal = intr
                    };


                    //in een cerescamera worden interne parameters opgeslaan volgens array v doubles

                    //Per interne parameters kan men bepalen wat dient gebundeld te worden
                    //ook combinatie zijn mogelijk

                    cerescameras.Add(cc);


                    for (int i = 0; i < visible3d.Length; i++) {
                        var proj = visible_proj[i];

                        var marker = new CeresMarker() {
                            id = puntenCv[visible3d[i]].ID,
                            Location = new CeresPoint {
                                BundleFlags = BundleWorldCoordinatesFlags.None,
                                Coordinates_arr = visible3d[i].toArrD()
                            },
                            x = proj.X,
                            y = proj.Y
                        };
                        var res = ceresdotnet.CeresTestFunctions.testProjectPoint(cc,
                            new CeresPointOrient() { RT = new[] { 0D, 0, 0, 0, 0, 0 } }, marker);
                        proj.X -= (float)res[0];
                        proj.Y -= (float)res[1];
                        marker.x = proj.X;
                        marker.y = proj.Y;
                        ceresmarkers.Add(marker);
                    }
                    collectionobservations.Add(cc, ceresmarkers);

                    //gesimuleerde foto weergeven
                    var window2 = new CameraSimulationFrm(string.Format("Camera {0}: {1}", cameraID, camera.Name)) {
                        Camera = camera
                    };
                    window2.Show();
                    window2.drawChessboard(visible_proj.Select(x => new Vector2((float)x.X, (float)x.Y)).ToArray());
                }
                observations.Add(cerescollection,collectionobservations);
            }
            


            CeresCameraMultiCollectionBundler.MarkersFromCameraDelegate findObservationsFunc = (camera, coll) => observations[coll][camera];
            bundler.MarkersFromCamera = findObservationsFunc;
            bundler.CollectionList = cerescameracollections;
            

            bundler.bundleCollections(iterationCallbackHandler);

        }

        protected CeresCallbackReturnType iterationCallbackHandler(int iterationNr) {
            return CeresCallbackReturnType.SOLVER_CONTINUE;
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
