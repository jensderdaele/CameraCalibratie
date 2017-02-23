using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using ArUcoNET;
using ceresdotnet;
using Calibratie;
using Emgu.CV;
using Emgu.CV.Structure;
using OpenTK;



namespace CalibratieForms {
    
    public static class Util {
        

        public static void SolvePnP(Scene scene, PinholeCamera camera, List<Tuple<int,PointF>> detectedMarkers) {
            var markers = scene.getIE<Marker>().ToList();
            //detectedMarkers.OrderBy(x => x.Item1);
            int markersid = 0;
            List<MCvPoint3D32f> points3d = new List<MCvPoint3D32f>();
            List<PointF> imagepoints = new List<PointF>();


            foreach (var detectedMarker in detectedMarkers) {
                var scenemarker = markers.Find(x => x.ID == detectedMarker.Item1);
                if (scenemarker != null) {
                    points3d.Add(new MCvPoint3D32f((float)scenemarker.X, (float)scenemarker.Y,
                        (float)scenemarker.Z));
                    imagepoints.Add(detectedMarker.Item2);
                }
            }
            Matrix<double> outrot = new Matrix<double>(3, 1);
            Matrix<double> outtrans = new Matrix<double>(3, 1);

            double[] rvec, tvec;

            Emgu.CV.CvInvoke.SolvePnP(
                points3d.ToArray(), 
                imagepoints.ToArray(),
                new Emgu.CV.Matrix<double>(camera.Intrinsics.Mat), 
                new Emgu.CV.Matrix<double>(camera.Cv_DistCoeffs5), outrot,outtrans
            );



        }

        private class MarkerComparer : IEqualityComparer<Marker> {
            public bool Equals(Marker m1, Marker m2) {
                if (m1.X == m2.X &&
                    m1.Y == m2.Y &&
                    m1.Z == m2.Z) {
                    return true;
                }
                return false;
            }

            public int GetHashCode(Marker obj) {
                int hcode = (int)obj.X ^ (int)obj.Y ^ (int)obj.Z;
                return hcode.GetHashCode();
            }
        }
        public static List<Marker> createBox(double xmax, double ymax, double zmax, double step) {
            int count = 0;
            var ptn3d = new List<Marker>();
            for (double x = 0; x <= xmax; x += step) {
                for (double y = 0; y <= ymax; y += step) {
                    ptn3d.Add(new Marker(count++,x ,y ,0));
                    ptn3d.Add(new Marker(count++,x, y, zmax));
                }
            }
            for (double x = 0; x <= xmax; x += step) {
                for (double z = step; z + step < zmax; z += step) {
                    ptn3d.Add(new Marker(count++,x, 0, z));
                    ptn3d.Add(new Marker(count++,x, ymax, z));
                }
            }
            for (double z = step; z + step < zmax; z += step) {
                for (double y = step; y + step < ymax; y += step) {
                    ptn3d.Add(new Marker(count++,step, y, z));
                    ptn3d.Add(new Marker(count++,xmax - step, y, z));
                }
            }
            return ptn3d;
        } 
        public static Scene bundleAdjustScene() {
            //scene bevat SObject. dit kunnen eender welke elementen zijn die hier van overerven
            var s = new Scene(); 

            //calibratieruimte 8x4x10m elke 1m een marker, returns List<CalibratieForms::Marker>
            var ptn3d = createBox(8, 4, 10, 0.2); 
            s.objects.AddRange(ptn3d); //markers toevoegen aan scene

            Random rnd = new Random();
            int index = rnd.Next(ptn3d.Count);
            var cameras = new List<PinholeCamera>();
            for (int i = 0; i < 6; i++) { //7 foto's met Huawei camera
                var c = PinholeCamera.getTestCameraHuawei();//Huawei bepaald via Zhang
                c.Name = "huaweip9";
                var pos = new Matrix<double>(new double[] { rnd.Next(2, 6), 2, rnd.Next(3, 7) });//camera staat op x: 2-6m, y=2m, z=3-7m random gekozen
                var target = ptn3d[rnd.Next(ptn3d.Count)]; //camera richt naar een random gekozen marker
                var worldtocamera = MatrixExt.LookAt(pos, target.Pos, MatrixExt.UnitVectorY<double>()); //berekend de wereld->cameracoordinaten
                c.WorldMat = worldtocamera.Inverted();
                //camera->wereldcoordinaten. 
                //Deze matrix bevat dus op kolom3 de Positie van de camera in wereldcoordinten, 
                //hiernaast de rotatie in 3x3 ([R t])
                cameras.Add(c);
            }
            
            /*for (int i = 0; i < 5; i++) { //5 foto's met Casio camera
                var c = PinholeCamera.getTestCamera();//Casio bepaald via zhang
                c.Name = "casio";
                var Pos = new Vector3d(rnd.Next(2, 6), 2, rnd.Next(3, 7));
                var target = ptn3d[rnd.Next(ptn3d.Count)];
                var worldtocamera = Matrix4d.LookAt(Pos, target.Pos, Vector3d.UnitY);
                c.worldMat = worldtocamera.Inverted();
                cameras.Add(c);
            }*/
            s.objects.AddRange(cameras);
            return s;
        }

        public static Scene bundleAdjustSceneMultiCollection() {
            //scene bevat SObject. dit kunnen eender welke elementen zijn die hier van overerven
            var s = new Scene();

            //calibratieruimte 8x4x10m elke 1m een marker, returns List<CalibratieForms::Marker>
            var ptn3d = createBox(8, 4, 10, 0.2);
            s.objects.AddRange(ptn3d); //markers toevoegen aan scene

            Random rnd = new Random();
            int index = rnd.Next(ptn3d.Count);
            var cameras = new List<PinholeCamera>();
            for (int i = 0; i < 6; i++) { //7 foto's met Huawei camera
                var c = PinholeCamera.getTestCameraHuawei();//Huawei bepaald via Zhang
                c.Name = "huaweip9";
                var pos = new Matrix<double>(new double []{rnd.Next(2, 6), 2, rnd.Next(3, 7)});//camera staat op x: 2-6m, y=2m, z=3-7m random gekozen
                var target = ptn3d[rnd.Next(ptn3d.Count)]; //camera richt naar een random gekozen marker
                var worldtocamera = MatrixExt.LookAt(pos, target.Pos, MatrixExt.UnitVectorY<double>()); //berekend de wereld->cameracoordinaten
                c.WorldMat = worldtocamera.Inverted();
                //camera->wereldcoordinaten. 
                //Deze matrix bevat dus op kolom3 de Positie van de camera in wereldcoordinten, 
                //hiernaast de rotatie in 3x3 ([R t])
                cameras.Add(c);
            }
            CameraCollection coll = new CameraCollection(cameras);
            CameraCollection coll2 = new CameraCollection(cameras);
            
            coll.SetCollectionCenter_MidCameras();

            /*for (int i = 0; i < 5; i++) { //5 foto's met Casio camera
                var c = PinholeCamera.getTestCamera();//Casio bepaald via zhang
                c.Name = "casio";
                var Pos = new Vector3d(rnd.Next(2, 6), 2, rnd.Next(3, 7));
                var target = ptn3d[rnd.Next(ptn3d.Count)];
                var worldtocamera = Matrix4d.LookAt(Pos, target.Pos, Vector3d.UnitY);
                c.worldMat = worldtocamera.Inverted();
                cameras.Add(c);
            }*/
            //s.objects.AddRange(cameras);
            s.objects.Add(coll);
            s.objects.Add(coll2);
            return s;
        }


        public static void InvokeIfRequired(this ISynchronizeInvoke obj,
                                         MethodInvoker action) {
            if (obj.InvokeRequired) {
                var args = new object[0];
                obj.Invoke(action, args);
            }
            else {
                action();
            }
        }

        #region math
        private static Random random = new Random();


        //http://www.alanzucconi.com/2015/09/16/how-to-sample-from-a-gaussian-distribution/
        public static double NextGaussian() {
            double v1, v2, s;
            do {
                v1 = 2.0*random.NextDouble() - 1.0;
                v2 = 2.0*random.NextDouble() - 1.0;
                s = v1*v1 + v2*v2;
            } while (s >= 1.0f || s == 0f);

            s = Math.Sqrt((-2.0f*Math.Log(s))/s);

            return v1*s;
        }
        public static double NextGaussian(double mean, double standard_deviation) {
            return mean + NextGaussian() * standard_deviation;
        }
        public static double NextGaussian(double mean, double standard_deviation, double min, double max) {
            double x;
            do {
                x = NextGaussian(mean, standard_deviation);
            } while (x < min || x > max);
            return x;
        }
        public static double[] gaussDistr(int count,double mean, double standard_deviation, double min, double max) {
            var r = new double[count];
            for (int i = 0; i < count; i++) {
                r[i] = NextGaussian(mean, standard_deviation, min, max);
            }
            return r;
        }


        #endregion
    }
}

