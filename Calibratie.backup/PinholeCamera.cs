using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenTK;
using SceneManager;

namespace Calibratie {
    public class PinholeCamera : SObject, INotifyPropertyChanged {
        /// <summary>
        /// echt geteste camera via foto's
        /// </summary>
        /// <returns></returns>
        public static PinholeCamera getTestCamera() {
            var mat = new double[,] { { 3479.3332725692153, 0, 1499.9382892470603 }, { 0, 3458.5791417359405, 1142.7454458370041 }, { 0, 0, 1 } };
            var m = new CameraMatrix(mat);
            var c = new PinholeCamera(m);
            c.PictureSize = new Size(3072, 2304);
            c.Cv_DistCoeffs5 = new[] { 0.062813787874286389, -3.0485685802388809, -0.0017951735131834098, -0.00040688209299854, 14.91660690214403 };
            return c;
        }
        /// <summary>
        /// fotogrootte in pixels
        /// </summary>
        public Size PictureSize { get; set; }


        public string PictureSizeST { get { return String.Format("{0}x{1}", PictureSize.Width, PictureSize.Height); } }
        public CameraMatrix CameraMatrix {
            get { return _cameraMatrix; }
        }

        private readonly CameraMatrix _cameraMatrix;

        public PinholeCamera()
            : base() {
            _cameraMatrix = new CameraMatrix();
            _cameraMatrix.PropertyChanged += (o, s) => { OnPropertyChanged("CameraMatrix." + s.PropertyName); };
        }
        public PinholeCamera(CameraMatrix m) {
            _cameraMatrix = m;
            _cameraMatrix.PropertyChanged += (o, s) => { OnPropertyChanged("CameraMatrix." + s.PropertyName); };
        }

        private double _distortionR1, _distortionR2, _distortionR3;
        private double _distortionT1, _distortionT2;
        public double DistortionR1 { get { return _distortionR1; } set { _distortionR1 = value; OnPropertyChanged(); } }
        public double DistortionR2 { get { return _distortionR2; } set { _distortionR2 = value; OnPropertyChanged(); } }
        public double DistortionR3 { get { return _distortionR3; } set { _distortionR3 = value; OnPropertyChanged(); } }
        public double DistortionT1 { get { return _distortionT1; } set { _distortionT1 = value; OnPropertyChanged(); } }
        public double DistortionT2 { get { return _distortionT2; } set { _distortionT2 = value; OnPropertyChanged(); } }

        public double[] Ceres_DistCoeffs {
            get {
                return new double[] { CameraMatrix.fx, CameraMatrix.fy, 
                    CameraMatrix.cx, CameraMatrix.cy, 
                    DistortionR1, DistortionR2,DistortionR3 , 
                    DistortionT1,DistortionT2  };
            }
            set {
                if (value.Length != 9) {
                    throw new ArgumentException("wrong size");
                }
                CameraMatrix.fx = value[0];
                CameraMatrix.fy = value[1];
                CameraMatrix.cx = value[2];
                CameraMatrix.cy = value[3];
                _distortionR1 = value[4];
                _distortionR2 = value[5];
                _distortionR3 = value[6];
                _distortionT1 = value[7];
                _distortionT2 = value[8];
                OnPropertyChanged();
            }
        }

        public double[] Cv_DistCoeffs5 {
            get {
                return new double[] { DistortionR1, DistortionR2, DistortionT1, DistortionT2, DistortionR3 };
            }
            set {
                if (value.Length != 5) {
                    throw new ArgumentException("wrong size");
                }
                _distortionR1 = value[0];
                _distortionR2 = value[1];
                _distortionT1 = value[2];
                _distortionT2 = value[3];
                _distortionR3 = value[4];
                OnPropertyChanged();
            }
        }
        public double[] Cv_rvecs { get { return new[] { Dir.X, Dir.Y, Dir.Z }; } }
        public double[] Cv_tvecs { get { return new[] { Pos.X, Pos.Y, Pos.Z }; } }

        public Point2d[] ProjectBoard_Cv_p2d(ChessBoard b, out double[,] jacobian) {
            //Todo: Use InputArray on vector3d[] & pin GC
            Point2d[] imagePoints;
            Cv2.ProjectPoints(b.boardWorldCoordinated_Cv, Cv_rvecs, Cv_tvecs, CameraMatrix.Mat, Cv_DistCoeffs5, out imagePoints, out jacobian);
            return imagePoints;
        }
        public Point2d[] ProjectBoard_Cv_p2d(ChessBoard b) {
            //Todo: Use InputArray on vector3d[] & pin GC
            Point2d[] imagePoints;
            double[,] jacobian;
            Cv2.ProjectPoints(b.boardWorldCoordinated_Cv, Cv_rvecs, Cv_tvecs, CameraMatrix.Mat, Cv_DistCoeffs5, out imagePoints, out jacobian);
            return imagePoints;
        }
        public Vector2d[] ProjectBoard_Cvd(ChessBoard b) {
            return ProjectBoard_Cv_p2d(b).Select(x => new Vector2d(x.X, x.Y)).ToArray();
        }
        public Vector2d[] ProjectBoard_Cvd(ChessBoard b, out double[,] jacobian) {
            return ProjectBoard_Cv_p2d(b, out jacobian).Select(x => new Vector2d(x.X, x.Y)).ToArray();
        }
        public Vector2[] ProjectBoard_Cv(ChessBoard b) {
            return ProjectBoard_Cv_p2d(b).Select(x => new Vector2((float)x.X, (float)x.Y)).ToArray();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            var handler = PropertyChanged;
            if (handler != null) { handler(this, new PropertyChangedEventArgs(propertyName)); }
        }

        /// <summary>
        /// projects entire scene, 
        /// output returns list of double x,y coordinates & each corresponding scenepoint
        /// </summary>
        /// <param name="c"></param>
        public Dictionary<SObject,Point2d> projectScene(Scene scene) {
            Point2d[] outpoints;
            double[,] jac;
            Point3d[] inpoints = scene.objects.Select(o => new Point3d(o.Pos.X, o.Pos.Y, o.Pos.Z)).ToArray();
            
            Cv2.ProjectPoints(inpoints,this.Cv_rvecs,this.Cv_tvecs,CameraMatrix.Mat,Cv_DistCoeffs5,out outpoints,out jac);

            Dictionary<SObject, Point2d> r = new Dictionary<SObject, Point2d>();
            

            for (int i = 0; i < outpoints.Length; i++) {
                var p = outpoints[i];
                if (p.X > 0 && p.Y > 0 && p.X <= this.PictureSize.Width && p.Y <= this.PictureSize.Height) {
                    r.Add(scene.objects[i], p);
                }
            }

            return r;
        }

        /// <summary>
        /// returns null if not in view
        /// </summary>
        /// <param name="c"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public Point2d projectPoint(Point3d point) {
            Point2d[] outpoints;
            double[,] jac;
            Point3d[] inpoints = {point};
            Cv2.ProjectPoints(inpoints, this.Cv_rvecs, this.Cv_tvecs, CameraMatrix.Mat, Cv_DistCoeffs5, out outpoints, out jac);
            return outpoints[0];
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        public Point2d[] projectPoints(IEnumerable<Point3d> pts) {
            Point2d[] outpoints;
            double[,] jac;
            Cv2.ProjectPoints(pts, this.Cv_rvecs, this.Cv_tvecs, CameraMatrix.Mat, Cv_DistCoeffs5, out outpoints, out jac);
            return outpoints;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        /// <returns></returns>
        public Point2d[] projectPoints(IEnumerable<Vector3d> pts) {
            Point2d[] outpoints;
            double[,] jac;
            var worldtocamera = this.worldMat.Inverted();
            var t = worldtocamera.ExtractTranslation();
            double[] tvecs = {t.X,t.Y,t.Z};
            var r = ToEulerAngles(worldtocamera.ExtractRotation());
            double[] rvecs = (new double[] { r.X, r.Y, r.Z }).Select(x=>x/Math.PI*360).ToArray();


            worldtocamera = this.worldMat;
            t = worldtocamera.ExtractTranslation();
            tvecs = new[]{ t.X, t.Y, t.Z };
            r = ToEulerAngles(worldtocamera.ExtractRotation());
            rvecs = (new double[] { r.X, r.Y, r.Z }).Select(x => x / Math.PI * 360).ToArray();

            var proj = Matrix4dtoproj(worldtocamera);
            double[,] outcamera, outrot, outrotmx, outrotmy, outrotmz;
            double[] outtrans, eulerangles;
            //Cv2.DecomposeProjectionMatrix(proj, out outcamera, out outrot, out outtrans, out outrotmx, out outrotmy, out outrotmz,out eulerangles);

            Cv2.ProjectPoints(pts.Select(x => new Point3d(x.X, x.Y, x.Z)), rvecs, this.Cv_tvecs, CameraMatrix.Mat, Cv_DistCoeffs5, out outpoints, out jac);
            return outpoints;
        }

        public static double[,] Matrix4dtoproj(Matrix4d m) {
            var r = new double[4, 4];
            for (int x = 0; x < 4; x++) {
                for (int y = 0; y < 4; y++) {
                    r[x, y] = m[x, y];
                }
            }
            return r;
        }
        public static Vector3d ToEulerAngles(Quaterniond q) {
            // Store the Euler angles in radians
            Vector3d pitchYawRoll = new Vector3d();

            double sqw = q.W * q.W;
            double sqx = q.X * q.X;
            double sqy = q.Y * q.Y;
            double sqz = q.Z * q.Z;

            // If quaternion is normalised the unit is one, otherwise it is the correction factor
            double unit = sqx + sqy + sqz + sqw;
            double test = q.X * q.Y + q.Z * q.W;

            if (test > 0.499f * unit) {
                // Singularity at north pole
                pitchYawRoll.Y = 2f * (float)Math.Atan2(q.X, q.W);  // Yaw
                pitchYawRoll.X = Math.PI * 0.5f;                         // Pitch
                pitchYawRoll.Z = 0f;                                // Roll
                return pitchYawRoll;
            }
            else if (test < -0.499f * unit) {
                // Singularity at south pole
                pitchYawRoll.Y = -2f * (float)Math.Atan2(q.X, q.W); // Yaw
                pitchYawRoll.X = -Math.PI * 0.5f;                        // Pitch
                pitchYawRoll.Z = 0f;                                // Roll
                return pitchYawRoll;
            }

            pitchYawRoll.Y = (float)Math.Atan2(2 * q.Y * q.W - 2 * q.X * q.Z, sqx - sqy - sqz + sqw);       // Yaw
            pitchYawRoll.X = (float)Math.Asin(2 * test / unit);                                             // Pitch
            pitchYawRoll.Z = (float)Math.Atan2(2 * q.X * q.W - 2 * q.Y * q.Z, -sqx + sqy - sqz + sqw);      // Roll
            
            return pitchYawRoll;
        }

        /// <summary>
        /// Project via OpenCV
        /// </summary>
        /// <param name="points3d"></param>
        /// <returns></returns>
        public Point2d[] ProjectPoints2D_VisibleOnly(Point3d[] points3d,out Point3d[] visible ) {
            var worldtocamera = worldMat.Inverted();
            var mat4 = worldtocamera;
            var trans = mat4.ExtractTranslation();
            var rmat = mat4.ExtractRotation();
            double[] outv;
            Cv2.Rodrigues(Matrix3d.CreateFromQuaternion(rmat).Normalized().toArr(), out outv);

            var tvecs = trans.toArr();
            double[,] jacob;
            Point2d[] proje;

            Cv2.ProjectPoints(points3d, outv, tvecs,this.CameraMatrix.Mat, this.Cv_DistCoeffs5, out proje, out jacob);

            List<int> correctIndex = new List<int>();
            for (int i = 0; i < proje.Length; i++) {
                if (IsinBounds(proje[i])) {
                    correctIndex.Add(i);
                }
            }
            var r = new Point2d[correctIndex.Count];
            visible = new Point3d[correctIndex.Count];
            for (int i = 0; i < correctIndex.Count; i++) {
                var index = correctIndex[i];
                r[i] = proje[index];
                visible[i] = points3d[index];
            }
            return r;
        }

        public bool IsinBounds(Point2d p) {
            return (p.X >= 0 && p.X <= this.PictureSize.Width && p.Y >= 0 && p.Y <= PictureSize.Height);
        }
    }
}
