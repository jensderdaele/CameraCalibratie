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
using OpenCvSharp;
using OpenTK;
using SceneManager;
using Size = OpenCvSharp.Size;


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

            Mat K = phc.CameraMatrix.cvmat;
            


            Mat W = new Mat(3, 3, MatType.CV_64FC1, new double[] {
                0.0D, -1.0D, 0.0D,
                1.0D, 0.0D, 0.0D,
                0.0D, 0.0D, 1.0D
            });
            Mat Wt = new Mat(3, 3, MatType.CV_64FC1, new double[] {
                0.0D, 1.0D, 0.0D,
                -1.0D, 0.0D, 0.0D,
                0.0D, 0.0D, 1.0D
            });
            Mat Z = new Mat(3, 3, MatType.CV_64FC1, new double[] {
                0.0D, 1.0D, 0.0D,
                -1.0D, 0.0D, 0.0D,
                0.0D, 0.0D, 0D
            });

            Mat diag = new Mat(3, 3, MatType.CV_64FC1, new double[] {
                1.0D, 0.0D, 0.0D,
                0.0D, 1.0D, 0.0D,
                0.0D, 0.0D, 0.0D
            });


            foreach (var stereoPair in pairs) {
                var points_count = stereoPair.intersection.Count;
                Point2d[] punten1px = new Point2d[points_count];
                Point2d[] punten2px = new Point2d[points_count];

                {
                    int i = 0;
                    foreach (KeyValuePair<ArucoMarker, ArucoMarker> kvp in stereoPair.intersection) {
                        punten1px[i] = (kvp.Key.Corner1.to2d());
                        punten2px[i] = (kvp.Value.Corner2.to2d());
                        i++;
                    }
                }
                
                var F = Cv2.FindFundamentalMat(punten1px,punten2px);



                
                Mat essential = K.T() * F * K;
                SVD decomp = new SVD(essential);
                Mat U = decomp.U;
                Mat Vt = decomp.Vt;

                Mat R1 = U * W * Vt;
                Mat R2 = U * W.T() * Vt;
                Mat T1 = U.Col[2];
                Mat T2 = -U.Col[2];

                Mat[] Ps = new Mat[4];

                for (int i = 0; i < 4; i++)
                    Ps[i] = new Mat(3, 4, MatType.CV_64FC1);

                Cv2.HConcat(R1, T1, Ps[0]);
                Cv2.HConcat(R1, T2, Ps[1]);
                Cv2.HConcat(R2, T1, Ps[2]);
                Cv2.HConcat(R2, T2, Ps[3]);

                var mat0001 = new Mat(1, 4, MatType.CV_64F, new double[] { 0, 0, 0, 1 });

                var KPs = new Mat[4];
                KPs[0] = K * Ps[0];
                KPs[1] = K * Ps[1];
                KPs[2] = K * Ps[2];
                KPs[3] = K * Ps[3];

                
                var KP0 = K*Mat.Eye(3, 4, MatType.CV_64FC1);

                for (int i = 0; i < 4; i++) {

                    var punten1px_cv = new MatOfPoint2d(1, punten1px.Length, punten1px);
                    var punten2px_cv = new MatOfPoint2d(1, punten2px.Length, punten2px);
                    

                    Mat output_hom = new Mat();
                    Cv2.TriangulatePoints(KP0, KPs[i], punten1px_cv, punten2px_cv, output_hom);

                    
                    List<Point3d> testList = new List<Point3d>();
                    Mat output = new Mat();
                    for (int j = 0; j < punten1px.Length; j++) {
                        var phom = output_hom.Get<Vec4d>(j);
                        var pt = new Point3d {
                            X = phom.Item0/phom.Item3,
                            Y = phom.Item1/phom.Item3,
                            Z = phom.Item2/phom.Item3
                        };
                        testList.Add(pt);
                    }
                    
                    
                    testList.writePoints(string.Format(@"C:\Users\jens\Desktop\calibratie\output3d\out{0}.txt", i));
                    if (testList.Where(x => x.Z < 0).Count() == 0) {
                        var puntenin3d = testList;
                    }

                }

                Mat S = U*diag*W*U.T();

                Mat R = U*W*decomp.Vt;


            }
            

            //init calibratie
            ZhangCalibration zcalibration = new ZhangCalibration();
            Size csize = new Size(7, 9);
            //zcalibration.LoadImages(@"C:\Users\jens\Desktop\calibratie\Fotos_gedownload_door_AirDroid (1)\zhang\7x9\", csize);
            //zcalibration.CalibrateCV(new ChessBoard(7, 9, 25), out cameramatcv, out distcoeffscv);
            PinholeCamera cam = PinholeCamera.getTestCameraHuawei();
            //zcalibration.Calibrate(new ChessBoard(7, 9, 25), out cam);
            var l1 = markerDictionary.Values.First();
            var l2 = markerDictionary.Values.Last();

            Mat distcoeffs = new MatOfDouble(1,5,cam.Cv_DistCoeffs5);

            var zelfde = l1.selectSame(l2, (a, b) => a.ID == b.ID);

            IEnumerable<Point2d> punten1, punten2;
            punten1 = zelfde.Keys.Select(x => x.Corner1.to2d());
            punten2 = zelfde.Values.Select(x => x.Corner1.to2d());
            Mat cam0pnts = new Mat(1, punten1.Count(), MatType.CV_32FC2);
            Mat cam1pnts = new Mat(1, punten1.Count(), MatType.CV_32FC2);
            cam0pnts.SetArray(0, 0, punten1.Select(x=>new Point2f((float)x.X,(float)x.Y)).ToArray());
            cam1pnts.SetArray(0, 0, punten2.Select(x => new Point2f((float)x.X, (float)x.Y)).ToArray());
            
            Mat cam0pntsnorm = new Mat();
            Mat cam1pntsnorm = new Mat();
            Cv2.UndistortPoints(cam0pnts, cam0pntsnorm, K, distcoeffs,null,K);
            Cv2.UndistortPoints(cam1pnts, cam1pntsnorm, K, distcoeffs,null,K);


            

            Mat cam0pntshom = new Mat(), cam1pntshom = new Mat();
            Cv2.ConvertPointsToHomogeneous(cam0pnts, cam0pntshom);
            Cv2.ConvertPointsToHomogeneous(cam1pnts, cam1pntshom);


            cam1pntshom = cam1pntshom.Reshape(1);
            cam0pntshom = cam0pntshom.Reshape(1);


            

            var fundamentalMatrix = Cv2.FindFundamentalMat(punten1, punten2);
            var F2 = Cv2.FindFundamentalMat(cam0pnts, cam1pnts);

            
            var check = cam1pnts.T()*F2*cam0pnts;
            var check2 = cam1pnts.T() * fundamentalMatrix * cam0pnts;

            

            


            

            Mat Er = null;// = decomp.U * diag * decomp.Vt;

            SVD svd = new SVD(Er);
            

            Mat Winv = new Mat(3, 3, MatType.CV_64FC1, new double[] {
                0.0D, 1.0D, 0.0D,
                -1.0D, 0.0D, 0.0D,
                0.0D, 0.0D, 1.0D
            });

            


            
            


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
                        var res = ceresdotnet.CeresCameraCollectionBundler.testProjectPoint(cc,
                            new CeresPointOrient() { RT = new[] { 0D, 0, 0, 0, 0, 0 } }, marker);
                        proj.X -= res[0];
                        proj.Y -= res[1];
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
            

            double[] rodr;
            var test = new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
            Cv2.Rodrigues(test, out rodr);

            CeresCameraMultiCollectionBundler.MarkersFromCameraDelegate findObservationsFunc = (camera, coll) => observations[coll][camera];
            bundler.MarkersFromCamera = findObservationsFunc;
            bundler.CollectionList = cerescameracollections;
            

            bundler.bundleCollections(iterationCallbackHandler);

        }

        protected CeresCallbackReturnType iterationCallbackHandler(int iterationNr) {
            return CeresCallbackReturnType.SOLVER_CONTINUE;
        }
        
        public void Solve() {
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

            double[] rodr;
            var test = new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };
            Cv2.Rodrigues(test, out rodr);
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
            
        }
        
        

        
        public static Mat toCvMat(Matrix4d mat) {
            Mat mcv = new Mat(4, 4, MatType.CV_64F);
            for (int r = 0; r < 4; r++) {
                for (int c = 0; c < 4; c++) {
                    mcv.Set(r, c, mat[r,c]);
                }
            }
            return mcv;
        }
        public static Mat toCvMat(int rows,int cols,double[,] mat) {
            Mat mcv = new Mat(rows, cols, MatType.CV_64F);
            for (int r = 0; r < rows; r++) {
                for (int c = 0; c < cols; c++) {
                    mcv.Set(r, c, mat[r,c]);
                }
            }
            return mcv;
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
