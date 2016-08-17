using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ArUcoNET;
using ceresdotnet;
using Calibratie;
using ComponentOwl.BetterListView;
using OpenCvSharp;
using OpenTK;
using SceneManager;



namespace CalibratieForms {
    
    public class CeresSimulation {
        
        public Scene scene;
        public Dictionary<PinholeCamera, PinholeCamera> pinholeCameras;
 
        
        public CeresSimulation() { }

        private static Object lockme = new Object();


        public static unsafe void ceresSolveAruco() {
            string dir = @"C:\Users\jens\Desktop\calibratie\Fotos_gedownload_door_AirDroid (1)\aruco\stereo test\";
            List<CeresMarker> ceresmarkers = new List<CeresMarker>();
            List<CeresCamera> cerescameras = new List<CeresCamera>();
            var files = Directory.GetFiles(dir).ToList();

            
            
            List<Task> allTasks = new List<Task>();
            SemaphoreSlim throttler = new SemaphoreSlim(4); //max 4 threads

            Dictionary<string, IEnumerable<ArucoMarker>> markerDictionary = new Dictionary<string, IEnumerable<ArucoMarker>>();

            Action<Object> arucomarkerAction = o => {
                String file = (string) o;
                int id = files.IndexOf(file);
                CeresCamera camera = CeresCamera.From(PinholeCamera.getTestCamera());
                var markers = ArUcoNET.Aruco.FindMarkers(file);
                markerDictionary.Add(file, markers);
                if (markers.Any()) {
                    foreach (var arucoMarker in markers) {
                        ceresmarkers.AddRange(arucoMarker.getCeresMarkers(id, camera));
                    }
                }
                cerescameras.Add(camera);
                throttler.Release();
            };

            foreach (var file in files) {
                throttler.Wait();
                var t = new Task(arucomarkerAction, file);
                t.Start();
                allTasks.Add(t);
            }
            Task.WhenAll(allTasks).Wait();

            //init calibratie
            ZhangCalibration zcalibration = new ZhangCalibration();
            Size csize = new Size(7, 9);
            //zcalibration.LoadImages(@"C:\Users\jens\Desktop\calibratie\Fotos_gedownload_door_AirDroid (1)\zhang\7x9\", csize);
            //zcalibration.CalibrateCV(new ChessBoard(7, 9, 25), out cameramatcv, out distcoeffscv);
            PinholeCamera cam = PinholeCamera.getTestCameraHuawei();
            //zcalibration.Calibrate(new ChessBoard(7, 9, 25), out cam);
            var l1 = markerDictionary.Values.First();
            var l2 = markerDictionary.Values.Last();

            Mat K = cam.CameraMatrix.cvmat;
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

            ArUcoNET.CV_Native.TestFmat(cam0pnts,cam1pnts,K);

            Mat cam0pntshom = new Mat(), cam1pntshom = new Mat();
            Cv2.ConvertPointsToHomogeneous(cam0pnts, cam0pntshom);
            Cv2.ConvertPointsToHomogeneous(cam1pnts, cam1pntshom);


            cam1pntshom = cam1pntshom.Reshape(1);
            cam0pntshom = cam0pntshom.Reshape(1);


            

            var fundamentalMatrix = Cv2.FindFundamentalMat(punten1, punten2);
            var F2 = Cv2.FindFundamentalMat(cam0pnts, cam1pnts);

            
            var check = cam1pnts.T()*F2*cam0pnts;
            var check2 = cam1pnts.T() * fundamentalMatrix * cam0pnts;

            

            


            

            Mat essential = K.T() * fundamentalMatrix * K;

            
            

            SVD decomp = new SVD(essential);

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

            Mat diag = new Mat(3, 3, MatType.CV_64FC1, new double[] {
                1.0D, 0.0D, 0.0D,
                0.0D, 1.0D, 0.0D,
                0.0D, 0.0D, 0.0D
            });

            Mat Er = decomp.U * diag * decomp.Vt;

            SVD svd = new SVD(Er);


            Mat Winv = new Mat(3, 3, MatType.CV_64FC1, new double[] {
                0.0D, 1.0D, 0.0D,
                -1.0D, 0.0D, 0.0D,
                0.0D, 0.0D, 1.0D
            });

            Mat R1 = svd.U * W * svd.Vt;
            Mat T1 = svd.U.Col[2];
            Mat R2 = svd.U * Winv * svd.Vt;
            Mat T2 = -svd.U.Col[2];

            Mat[] Ps = new Mat[4];

            for (int i = 0; i < 4; i++)
                Ps[i] = new Mat(3, 4, MatType.CV_64FC1);

            Cv2.HConcat(R1, T1, Ps[0]);
            Cv2.HConcat(R1, T2, Ps[1]);
            Cv2.HConcat(R2, T1, Ps[2]);
            Cv2.HConcat(R2, T2, Ps[3]);


            Mat P0 = new Mat(3, 4, MatType.CV_64FC1, new double[] {
                1D, 0D, 0.0D , 0D,
                0D, 1D, 0.0D , 0D,
                0D, 0D, 1.0D , 0D,
            });
            
            List<Point3d> list3d = new List<Point3d>();
            Mat outputMat = new Mat();
            for (int i = 0; i < 4; i++) {
                Mat cam0pntsf = new Mat();
                cam0pnts.ConvertTo(cam0pntsf,MatType.CV_32FC1);
                Mat cam1pntsf = new Mat();
                cam1pnts.ConvertTo(cam1pntsf, MatType.CV_32FC1);
                Cv2.TriangulatePoints(P0, Ps[i], cam0pntsf, cam1pntsf, outputMat);
                Mat hom = outputMat.Reshape(4);
                Mat pnts3D = new Mat();
                Cv2.ConvertPointsFromHomogeneous(hom, pnts3D);

                List<Point3d> testList = new List<Point3d>();
                for (int j = 0; j < punten1.Count(); j++) {
                    var p = pnts3D.Get<Point3d>(j);
                    testList.Add(p);
                }
                testList.writePoints(string.Format(@"C:\Users\jens\Desktop\calibratie\output3d\out{0}.txt", i));
                if (testList.Where(x => x.Z < 0).Count() == 0) {
                    list3d = testList;
                }
                
            }


        }

        public void Solve() {
            Random rand = new Random();
            var markers3d = scene.get<Marker>();
            var cameras = scene.get<PinholeCamera>();

            Dictionary<string, CeresCamera> uniekeIntrinsics = new Dictionary<string, CeresCamera>();

            List<CeresCamera> cerescameras = new List<CeresCamera>();
            List<CeresMarker> ceresmarkers = new List<CeresMarker>();
            //x = marker
            List<CeresPoint> cerespoints = markers3d.Select(x => new CeresPoint(x.Pos, x.ID)).ToList();
            int cameraID = -1;
            foreach (var camera in cameras) {
                cameraID++;
                Dictionary<Vector3d, Marker> puntenCv = markers3d.ToDictionary(m => m.Pos);
                //bevat zichtbare punten
                Vector3d[] visible3d;
                //alle zichtbare punten worden geprojecteerd. andere worden verwijderd:
                //  punten die in cameracoordinaten een negatieve Z-waarde hebben staan achter de camera
                //  punten die buiten het sensorbereik vallen 
                var visible_proj = camera.ProjectPointd2D_Manually(puntenCv.Keys.ToArray(), out visible3d);
                var cc = CeresCamera.From(camera);
                //in een cerescamera worden interne parameters opgeslaan volgens array v doubles
                cc.Intrinsics = camera.toCeresIntrinsics9();
                cc.id = cameraID;

                //Per interne parameters kan men bepalen wat dient gebundeld te worden
                //ook combinatie zijn mogelijk
                cc.bundle_intrinsics = (int) BundleIntrinsics.BUNDLE_ALL; 
                if (uniekeIntrinsics.ContainsKey(camera.Name)) {
                    //De software zal voor alle zelfde camera 1 paar 
                    //intrinsieke parameters (9 waarden) hanteren & optimaliseren
                    cc.LinkIntrinsicsToCamera(uniekeIntrinsics[camera.Name]); 
                }
                cerescameras.Add(cc);

                for (int i = 0; i < visible3d.Length; i++) {
                    var proj = visible_proj[i];
                    ceresmarkers.Add(new CeresMarker(cameraID, puntenCv[visible3d[i]].ID,
                        proj.X,
                        proj.Y) {
                        parentCamera = cc //elke marker bevat de bijhorende camera & 3Dpunt
                    }); 
                }
                
                //gesimuleerde foto weergeven
                var window2 = new CameraSimulationFrm(string.Format("Camera {0}: {1}", cameraID, camera.Name)) {
                    Camera = camera
                };
                window2.Show();
                window2.drawChessboard(visible_proj.Select(x => new Vector2((float)x.X, (float)x.Y)).ToArray());
            }
            var problem = new ceresdotnet.MultiCameraBundleProblem();
            problem.all_points_managed.AddRange(cerespoints);
            problem.cameras.AddRange(cerescameras);
            problem.markers.AddRange(ceresmarkers);

            //output wordt in Console geschreven door ceresdotnet
            problem.SolveProblem();
        }
        public void test() {
            var markers = scene.get<Marker>();
            var cameras = scene.get<PinholeCamera>();
            var camera = cameras[0];
            
            //TEST1
            //var worldtocamera = Matrix4d.LookAt(camera.Pos, Vector3d.Zero, Vector3d.UnitY);
            var worldtocamera = camera.worldMat.Inverted();

            var markerstransp = new List<Marker>();
            foreach (var m in markers) {
                var pos = m.Pos;
                var pos1 = Vector3d.TransformPerspective(pos, worldtocamera);
                var pos2 = Vector3d.Transform(pos, worldtocamera);

                //m.Pos = pos1;
            }

            var trans = worldtocamera.ExtractTranslation();
            var rmat = worldtocamera.ExtractRotation();
            double[] outv;
            Cv2.Rodrigues(Matrix3d.CreateFromQuaternion(rmat).Normalized().toArr(), out outv);
            Point3d[] visible3d;
            /*
            var persp = Matrix4d.CreatePerspectiveFieldOfView(((double)70 / 180) * Math.PI, (double)4 / (double)3, 0.1, 100);
            var mvp = persp * worldtocamera;

            var mat4 = mvp;
            var bla = new double[,] {
                {mat4.M11, mat4.M21, mat4.M31, mat4.M41},
                {mat4.M12, mat4.M22, mat4.M32, mat4.M42},
                {mat4.M13, mat4.M23, mat4.M33, mat4.M43},
            };
            double[,] cm, rot,cm2,rot2;
            double[] t,t2;
            Cv2.DecomposeProjectionMatrix(bla, out cm, out rot, out t);
            MatOfDouble projmat = new MatOfDouble(3, 4, bla);
            Mat cameram = new Mat();
            Mat rotm = new Mat();
            Mat transm = new Mat();
            Cv2.DecomposeProjectionMatrix(projmat,cameram,rotm,transm);
            Mat rodrm = new Mat();
            Cv2.Rodrigues(rotm, rodrm);
            //Cv2.ProjectPoints();


            mat4 = camera.worldMat;
            bla = new double[,] {
                {mat4.M11, mat4.M21, mat4.M31, mat4.M41},
                {mat4.M12, mat4.M22, mat4.M32, mat4.M42},
                {mat4.M13, mat4.M23, mat4.M33, mat4.M43},
            };
            
            
            Cv2.DecomposeProjectionMatrix(bla, out cm, out rot2, out t2);
            double[] rodr,rodr2;
            Cv2.Rodrigues(rot, out rodr);
            Cv2.Rodrigues(rot2, out rodr2);
            double[] tv = new[] { t[0] / t[3], t[1] / t[3], t[2] / t[3] };
            double[] tvmin = new[] { -t[0] / t[3], -t[1] / t[3], -t[2] / t[3] };
            double[] tv2 = new[] { t2[0] / t2[3], t2[1] / t2[3], t2[2] / t2[3] };
            
            var visible_proj =
                camera.ProjectPoints2D_VisibleOnly2(
                    markers.Select(x => new Point3d(x.Pos.X, x.Pos.Y, x.Pos.Z)).ToArray(), out visible3d, rodr, tv);

            var window2 = new CameraSimulationFrm();
            window2.Camera = camera;
            window2.Show();
            window2.drawChessboard(visible_proj.Select(x => new Vector2((float)x.X, (float)x.Y)).ToArray());
            */
            var visible_proj =
                camera.ProjectPoints2D_VisibleOnly(
                    markers.Select(x => new Point3d(x.Pos.X, x.Pos.Y, x.Pos.Z)).ToArray(), out visible3d);//, rodr, tvmin);

            var window2 = new CameraSimulationFrm();
            window2 = new CameraSimulationFrm();
            window2.Camera = camera;
            window2.Show();
            window2.drawChessboard(visible_proj.Select(x => new Vector2((float)x.X, (float)x.Y)).ToArray());
            

            var ceresmarkers = visible_proj.Select((x, i) => new CeresMarker(0, i, x.X, x.Y));
            var intrinsics = camera.toCeresIntrinsics9();

            List<CeresCamera> cerescameras = new List<CeresCamera>();
            var ccamera = new CeresCamera(Matrix3d.CreateFromQuaternion(worldtocamera.ExtractRotation()), trans, 0);
            var ccamera2 = new CeresCamera(Matrix3d.CreateFromAxisAngle(Vector3d.UnitY, 0.1), Vector3d.One, 0);
            cerescameras.Add(ccamera);
            var cerespoints = visible3d.Select((x, i) => new CeresPoint(new Vector3d(x.X,x.Y,x.Z), i));

            var correct = (double[]) intrinsics.Clone();
            var before = (double[])correct.Clone();
            var r = new Random();
            for (int i = 0; i < intrinsics.Length; i++) {
                intrinsics[i] *= 1 + (r.NextDouble() - .5)/10;
            }


            var c = PinholeCamera.getTestCamera();
            var cmat = c.CameraMatrix.Mat;
            double[] intr = c.Cv_DistCoeffs5;
            double[] rvecGuess = new double[3], tvecGuess = new double[3];
            var vis3dfloat = visible3d.Select(x => new Point3f((float) x.X, (float) x.Y, (float) x.Z)).ToList();//.GetRange(0,4).ToList();
            var vis2dfloat = visible_proj.Select(x => new Point2f((float)x.X, (float)x.Y)).ToList();//.GetRange(0, 4).ToList();
            //Cv2.SolvePnP(vis3dfloat,vis2dfloat,cmat,intr,out rvecGuess,out tvecGuess,false,SolvePnPFlags.P3P);
            
            //ArUcoNET.CV_Native.SolvePnP(vis3dfloat, vis2dfloat,c.CameraMatrix.cvmat,c.CameraMatrix.Mat,intr,SolvePnPFlags.P3P);
            

            ceresdotnet.BundleProblem.EuclideanBundleCommonIntrinsics(ceresmarkers, (int)(BundleIntrinsics.BUNDLE_ALL), 0, intrinsics, cerescameras,cerespoints);
            
            
            ceresdotnet.MultiCameraBundleProblem mp = new MultiCameraBundleProblem();
            intrinsics = camera.Ceres_DistCoeffs;

            correct = (double[])correct.Clone();
            before = (double[])correct.Clone();
            r = new Random();
            for (int i = 0; i < intrinsics.Length; i++) {
                intrinsics[i] *= 1 + (r.NextDouble() - .5) / 10;
            }
            camera.Ceres_DistCoeffs = intrinsics;

            
            
            var cercam = CeresCamera.From(camera);

            mp.markers = ceresmarkers.ToList();
            mp.markers.ForEach(x => { x.parentCamera = cercam; });
            mp.all_points_managed = cerespoints.ToList();
            cerescameras = new List<CeresCamera>();
            cerescameras.Add(cercam);

            //cerescameras.Add(new CeresCamera(Matrix3d.CreateFromQuaternion(worldtocamera.ExtractRotation()), trans, 0));
            mp.cameras = cerescameras;
            var diff = before.Select((x, i) => x - intrinsics[i]).ToArray();

            var in1 = cercam.Intrinsics;
            

            mp.Solve();
            var in2 = cercam.Intrinsics;
            var cercam2 = CeresCamera.From(camera);
            var in3 = cercam2.Intrinsics;

            cercam2.LinkIntrinsicsToCamera(cercam);
            var in4 = cercam2.Intrinsics;

        }
        public void test3() {
            var markers = scene.get<Marker>();
            var cameras = scene.get<PinholeCamera>();
            var camera = cameras[0];
            Vector3d[] visible3d;

            var visible_proj =
                camera.ProjectPointd2D_Manually(markers.Select(x=>x.Pos).ToArray(), out visible3d);//, rodr, tvmin);

            var window2 = new CameraSimulationFrm();
            window2 = new CameraSimulationFrm();
            window2.Camera = camera;
            window2.Show();
            window2.drawChessboard(visible_proj.Select(x => new Vector2((float)x.X, (float)x.Y)).ToArray());


            var ceresmarkers = visible_proj.Select((x, i) => new CeresMarker(0, i, x.X, x.Y));
            var intrinsics = camera.toCeresIntrinsics9();

            List<CeresCamera> cerescameras = new List<CeresCamera>();
            var ccamera = new CeresCamera(Matrix3d.CreateFromQuaternion(camera.worldMat.ExtractRotation()), camera.Pos, 0);
            var ccamera2 = new CeresCamera(Matrix3d.CreateFromAxisAngle(Vector3d.UnitY, 0.1), Vector3d.One, 0);
            cerescameras.Add(ccamera);
            var cerespoints = visible3d.Select((x, i) => new CeresPoint(new Vector3d(x.X, x.Y, x.Z), i));

            var correct = (double[])intrinsics.Clone();
            var before = (double[])correct.Clone();
            var r = new Random();
            for (int i = 0; i < intrinsics.Length; i++) {
                intrinsics[i] *= 1 + (r.NextDouble() - .5) / 10;
            }


            var c = PinholeCamera.getTestCamera();
            var cmat = c.CameraMatrix.Mat;
            double[] intr = c.Cv_DistCoeffs5;
            double[] rvecGuess = new double[3], tvecGuess = new double[3];
            var vis3dfloat = visible3d.Select(x => new Point3f((float)x.X, (float)x.Y, (float)x.Z)).ToList();//.GetRange(0,4).ToList();
            var vis2dfloat = visible_proj.Select(x => new Point2f((float)x.X, (float)x.Y)).ToList();//.GetRange(0, 4).ToList();
            //Cv2.SolvePnP(vis3dfloat,vis2dfloat,cmat,intr,out rvecGuess,out tvecGuess,false,SolvePnPFlags.P3P);

            //ArUcoNET.CV_Native.SolvePnP(vis3dfloat, vis2dfloat,c.CameraMatrix.cvmat,c.CameraMatrix.Mat,intr,SolvePnPFlags.P3P);


            ceresdotnet.BundleProblem.EuclideanBundleCommonIntrinsics(ceresmarkers, (int)(BundleIntrinsics.BUNDLE_ALL), 0, intrinsics, cerescameras, cerespoints);


            ceresdotnet.MultiCameraBundleProblem mp = new MultiCameraBundleProblem();
            intrinsics = camera.Ceres_DistCoeffs;

            correct = (double[])correct.Clone();
            before = (double[])correct.Clone();
            r = new Random();
            for (int i = 0; i < intrinsics.Length; i++) {
                intrinsics[i] *= 1 + (r.NextDouble() - .5) / 10;
            }
            camera.Ceres_DistCoeffs = intrinsics;



            var cercam = CeresCamera.From(camera);

            mp.markers = ceresmarkers.ToList();
            mp.markers.ForEach(x => { x.parentCamera = cercam; });
            mp.all_points_managed = cerespoints.ToList();
            cerescameras = new List<CeresCamera>();
            cerescameras.Add(cercam);

            //cerescameras.Add(new CeresCamera(Matrix3d.CreateFromQuaternion(worldtocamera.ExtractRotation()), trans, 0));
            mp.cameras = cerescameras;
            var diff = before.Select((x, i) => x - intrinsics[i]).ToArray();

            var in1 = cercam.Intrinsics;


            mp.Solve();
            var in2 = cercam.Intrinsics;
            var cercam2 = CeresCamera.From(camera);
            var in3 = cercam2.Intrinsics;

            cercam2.LinkIntrinsicsToCamera(cercam);
            var in4 = cercam2.Intrinsics;

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
