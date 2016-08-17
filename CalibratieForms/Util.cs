using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Calibratie;
using OpenCvSharp;
using OpenTK;
using SceneManager;



namespace CalibratieForms {
    public static class Util {
        private class MarkerComparer : IEqualityComparer<Marker> {
            public bool Equals(Marker m1, Marker m2) {
                if (m1.Pos.X == m2.Pos.X &&
                    m1.Pos.Y == m2.Pos.Y &&
                    m1.Pos.Z == m2.Pos.Z) {
                    return true;
                }
                return false;
            }

            public int GetHashCode(Marker obj) {
                int hcode = (int)obj.Pos.X ^ (int)obj.Pos.Y ^ (int)obj.Pos.Z;
                return hcode.GetHashCode();
            }
        }
        public static List<Marker> createBox(double xmax, double ymax, double zmax, double step) {
            int count = 0;
            var ptn3d = new List<Marker>();
            for (double x = 0; x <= xmax; x += step) {
                for (double y = 0; y <= ymax; y += step) {
                    ptn3d.Add(new Marker(count++,new Vector3d(x ,y ,0)));
                    ptn3d.Add(new Marker(count++, new Vector3d(x, y, zmax)));
                }
            }
            for (double x = 0; x <= xmax; x += step) {
                for (double z = step; z + step < zmax; z += step) {
                    ptn3d.Add(new Marker(count++, new Vector3d(x, 0, z)));
                    ptn3d.Add(new Marker(count++, new Vector3d(x, ymax, z)));
                }
            }
            for (double z = step; z + step < zmax; z += step) {
                for (double y = step; y + step < ymax; y += step) {
                    ptn3d.Add(new Marker(count++, new Vector3d(step, y, z)));
                    ptn3d.Add(new Marker(count++, new Vector3d(xmax - step, y, z)));
                }
            }
            /*
            var ptn3dnodupe = new List<Marker>();
            foreach (var marker in ptn3d) {
                if (!ptn3dnodupe.Contains(marker, new MarkerComparer())) {
                    ptn3dnodupe.Add(marker);
                }
            }*/
            return ptn3d;
        } 
        public static Scene bundleAdjustScene() {
            //scene bevat SObject. dit kunnen eender welke elementen zijn die hier van overerven
            var s = new Scene(); 

            //calibratieruimte 8x4x10m elke 1m een marker, returns List<CalibratieForms::Marker>
            var ptn3d = createBox(8, 4, 10, 1); 
            s.objects.AddRange(ptn3d); //markers toevoegen aan scene

            Random rnd = new Random();
            int index = rnd.Next(ptn3d.Count);
            var cameras = new List<PinholeCamera>();
            for (int i = 0; i < 6; i++) { //7 foto's met Huawei camera
                var c = PinholeCamera.getTestCameraHuawei();//Huawei bepaald via Zhang
                c.Name = "huaweip9";
                var Pos = new Vector3d(rnd.Next(2,6),2,rnd.Next(3,7));//camera staat op x: 2-6m, y=2m, z=3-7m random gekozen
                var target = ptn3d[rnd.Next(ptn3d.Count)]; //camera richt naar een random gekozen marker
                var worldtocamera = Matrix4d.LookAt(Pos, target.Pos, Vector3d.UnitY); //berekend de wereld->cameracoordinaten
                c.worldMat = worldtocamera.Inverted(); 
                                //camera->wereldcoordinaten. 
                                //Deze matrix bevat dus op kolom3 de Positie van de camera in wereldcoordinten, 
                                //hiernaast de rotatie in 3x3 ([R t])
                cameras.Add(c);
            }
            
            for (int i = 0; i < 5; i++) { //5 foto's met Casio camera
                var c = PinholeCamera.getTestCameraHuawei();//Casio bepaald via zhang
                c.Name = "casio";
                var Pos = new Vector3d(rnd.Next(2, 6), 2, rnd.Next(3, 7));
                var target = ptn3d[rnd.Next(ptn3d.Count)];
                var worldtocamera = Matrix4d.LookAt(Pos, target.Pos, Vector3d.UnitY);
                c.worldMat = worldtocamera.Inverted();
                cameras.Add(c);
            }
            s.objects.AddRange(cameras);
            return s;
        }
        public static Scene GetTestScene() {
            var  s  = new Scene();
            double[] euler;
            var c = PinholeCamera.getTestCamera();
            c.Cv_DistCoeffs5 = new double[] {0, 0, 0, 0, 0};
            var Pos = new Vector3d(2.9,2.4,-1.8);
            c.worldMat = Matrix4d.LookAt(Pos, Vector3d.Zero, Vector3d.UnitY).Inverted();
            
            var l = c.Pos.Length;
            
            
            string markersst = @"
                    {0,0,1}
                    {0,0,0}
                    {0,1,0}
                    {0.5,1,0}
                    {0.2,0.5,0.6}
                    {-0.4,0.35,-0.1}
                    {-0.4,.35,-1}
                    {0.7,1.5,-0.8}
                    {0.9,-0.2,-0.4}
                    {1.7,0.75,-1.7}
                    {1.6,1.36,-0.3}
                    {1.2,0,0.8}
                    {1.1,0.8,0.8}";

            int t = 0;
            for (double z = 0; z < 5; z += .1) {
                s.objects.Add(new Marker(t++,new Vector3d(z,0,0)));
                s.objects.Add(new Marker(t++, new Vector3d(0, z, 0)));
                s.objects.Add(new Marker(t++, new Vector3d(0, 0, z)));
            }

            var markers = IO.readVectors(markersst);
            s.objects.AddRange(markers.Select((x,i)=> new Marker(i,x)));


            s.objects.Add(c);
            return s;
        }
        public static Scene GetTestSceneRandom() {
            Random r = new Random();
            
            var s = new Scene();
            double[] euler;
            var c = PinholeCamera.getTestCamera();
            c.Pos = new Vector3d(2.9, 2.4, -1.8);



            string markersst = @"
                    {0,0,1}
                    {0,0,0}
                    {0,1,0}
                    {0.5,1,0}
                    {0.2,0.5,0.6}
                    {-0.4,0.35,-0.1}
                    {-0.4,.35,-.1}
                    {0.7,1.5,-0.8}
                    {0.9,-0.2,-0.4}
                    {1.7,0.75,-1.7}
                    {1.6,1.36,-0.3}
                    {1.2,0,0.8}
                    {1.1,0.8,0.8}";
            int t = 0;
            for (int i = 0; i < 50; i++) {
                s.objects.Add(new Marker(i, new Vector3d(r.NextDouble() * 4, r.NextDouble() * 4, r.NextDouble() * 4)));
            }

            var markers = IO.readVectors(markersst);
            //s.objects.AddRange(markers.Select((x,i)=> new Marker(i,x)));
            s.objects.Add(c);
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

        public static double reprojectionError(double[] data1, double[] data2, out double[] diff) {
            throw new NotImplementedException();
        }

        public static double[] toArray(this Vec3d v) {
            return new[] {v.Item0, v.Item1, v.Item2};
        }


        #endregion
    }
}

