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

using ceresdotnet;
using System.Runtime.InteropServices;

namespace Calibratie {
    public class PinholeCamera : SObject, INotifyPropertyChanged {
        /// <summary>
        /// echt geteste camera Casio EX-Z120
        /// </summary>
        /// <returns></returns>
        public static PinholeCamera getTestCameraHuawei() {
            var mat = new double[,] { { 3441.0667667434618, 0, 2090.8502187520326 }, { 0, 3432.3907417119017, 1561.5316432859202 }, { 0, 0, 1 } };
            var m = new CameraMatrix(mat);
            var c = new PinholeCamera(m);
            c.PictureSize = new Size(4160, 3120);
            c.Cv_DistCoeffs5 = new[] { 0.24230691691999853, -0.92897577071991688, -0.00073617836680420852, 0.0015104681489398752, 1.0576231378646423 };
            return c;
        }
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
        public MatOfDouble Cv_DistCoeffs5_cv {
            get {
                var data = new double[] { DistortionR1, DistortionR2, DistortionT1, DistortionT2, DistortionR3 };
                return new MatOfDouble(5,1,data);
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

        public static double[,] Matrix4dtoproj(Matrix4d m) {
            var r = new double[4, 4];
            for (int x = 0; x < 4; x++) {
                for (int y = 0; y < 4; y++) {
                    r[x, y] = m[x, y];
                }
            }
            return r;
        }

        /// <summary>
        /// van internet
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public static Vector3d ToEulerAngles(Quaterniond q) {
            Vector3d pitchYawRoll = new Vector3d();

            double sqw = q.W * q.W;
            double sqx = q.X * q.X;
            double sqy = q.Y * q.Y;
            double sqz = q.Z * q.Z;

            double unit = sqx + sqy + sqz + sqw;
            double test = q.X * q.Y + q.Z * q.W;

            if (test > 0.499f * unit) {
                pitchYawRoll.Y = 2f * (float)Math.Atan2(q.X, q.W);  
                pitchYawRoll.X = Math.PI * 0.5f;                      
                pitchYawRoll.Z = 0f;                                
                return pitchYawRoll;
            }
            else if (test < -0.499f * unit) {
                pitchYawRoll.Y = -2f * (float)Math.Atan2(q.X, q.W); 
                pitchYawRoll.X = -Math.PI * 0.5f;                 
                pitchYawRoll.Z = 0f;                            
                return pitchYawRoll;
            }

            pitchYawRoll.Y = (float)Math.Atan2(2 * q.Y * q.W - 2 * q.X * q.Z, sqx - sqy - sqz + sqw);  
            pitchYawRoll.X = (float)Math.Asin(2 * test / unit);                                         
            pitchYawRoll.Z = (float)Math.Atan2(2 * q.X * q.W - 2 * q.Y * q.Z, -sqx + sqy - sqz + sqw);   
            
            return pitchYawRoll;
        }

        public Point2d ProjectPointd2D_Manually(Vector3d vector3D) {
            var axisangle = this.worldMat.ExtractRotation().ToAxisAngle();



                var transf = this.worldMat.Inverted();
                var camcoord = Vector3d.TransformPerspective(vector3D, transf);

                if (camcoord.Z < 0) {
                    return new Point2d();
                }

                var testx = camcoord.X / camcoord.Z * this.CameraMatrix.fx + this.CameraMatrix.cx;
                var testy = camcoord.Y / camcoord.Z * this.CameraMatrix.fy + this.CameraMatrix.cy;


                var x = camcoord.X / camcoord.Z;
                var y = camcoord.Y / camcoord.Z;

                var r2 = x * x + y * y;
                var r4 = r2 * r2;
                var r6 = r4 * r2;
                var r_coeff = ((1) + this.DistortionR1 * r2 + this.DistortionR2 * r4 + this.DistortionR3 * r6);
                var tdistx = 2 * this.DistortionT1 * x * y + this.DistortionT2 * (r2 + 2 * x * x);
                var tdisty = 2 * this.DistortionT2 * x * y + this.DistortionT1 * (r2 + 2 * y * y);
                var xd = x * r_coeff + tdistx;
                var yd = y * r_coeff + tdisty;

                var im_x = this.CameraMatrix.fx * xd + this.CameraMatrix.cx;
                var im_y = this.CameraMatrix.fy * yd + this.CameraMatrix.cy;

                if (im_x >= 0 && im_x <= this.PictureSize.Width && im_y >= 0 && im_y <= PictureSize.Height) {
                    return new Point2d(im_x, im_y);
                }
                return new Point2d();
        }
        public Point2d[] ProjectPointd2D_Manually(Vector3d[] points3d, out Vector3d[] visible) {
            var r = new List<Point2d>();
            var vis = new List<Vector3d>();

           var axisangle =  this.worldMat.ExtractRotation().ToAxisAngle();
            
            
           
            foreach (var vector3D in points3d) {
                var transf = this.worldMat.Inverted();
                var camcoord = Vector3d.TransformPerspective(vector3D, transf);

                if (camcoord.Z < 0) {
                    continue;
                }

                var testx = camcoord.X / camcoord.Z * this.CameraMatrix.fx + this.CameraMatrix.cx;
                var testy = camcoord.Y / camcoord.Z * this.CameraMatrix.fy + this.CameraMatrix.cy;


                var x =camcoord.X/camcoord.Z;
                var y = camcoord.Y/camcoord.Z;

                var r2 = x*x + y*y;
                var r4 = r2 * r2;
                var r6 = r4 * r2;
                var r_coeff = ((1) + this.DistortionR1 * r2 + this.DistortionR2 * r4 + this.DistortionR3 * r6);
                var tdistx = 2*this.DistortionT1*x*y + this.DistortionT2*(r2 + 2*x*x);
                var tdisty = 2*this.DistortionT2*x*y + this.DistortionT1*(r2 + 2*y*y);
                var xd = x * r_coeff + tdistx;
                var yd = y * r_coeff + tdisty;

                var im_x = this.CameraMatrix.fx * xd + this.CameraMatrix.cx;
                var im_y = this.CameraMatrix.fy * yd + this.CameraMatrix.cy;

                if (im_x >= 0 && im_x <= this.PictureSize.Width && im_y >= 0 && im_y <= PictureSize.Height) {
                    vis.Add(vector3D);
                    r.Add(new Point2d(im_x,im_y));
                }
            }
            visible = vis.ToArray();
            return r.ToArray();
        }

        /// <summary>
        /// Project via OpenCV punten in cameracoordinaten
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

            double[] outvmin = new[] {outv[0], outv[1], outv[2]};

            var tvecs = trans.toArr();
            double[,] jacob;
            Point2d[] proje;

            Point3d[] old = null;
            if (points3d.Length%2 == 1) { //bij oneven aantal punten werkt Cv2.projectpoints niet (omdat deze ook een jacobiaan output) snelle, onefficiente, fix
                old = points3d;
                var points3dnew = new Point3d[points3d.Length+1];
                for (int i = 0; i < points3d.Length; i++) {
                    points3dnew[i] = points3d[i];
                }
                points3dnew[points3dnew.Length-1] = new Point3d(0,0,0);
                points3d = points3dnew;
            }

            Cv2.ProjectPoints(points3d, outvmin, tvecs, this.CameraMatrix.Mat, this.Cv_DistCoeffs5, out proje, out jacob);

            if (old != null) {
                points3d = old;
                proje = proje.Take(proje.Length - 1).ToArray();
            }
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

        private bool IsinBounds(Point2d p) {
            return (p.X >= 0 && p.X <= this.PictureSize.Width && p.Y >= 0 && p.Y <= PictureSize.Height);
        }

        /*
        ~PinholeCamera() {
            if (_Rt != IntPtr.Zero) Marshal.FreeHGlobal(_Rt);
            if (_Intrinsics != IntPtr.Zero) Marshal.FreeHGlobal(_Intrinsics);
        }

        
        #region ICERESCAMERA
        private IntPtr _Rt;
        private IntPtr _Intrinsics;

        private void updateCeresRtValues() {
            var projMat = worldMat.Inverted();
            Vector3d axis;
            double angle;
            projMat.ExtractRotation().ToAxisAngle(out axis, out angle);
            axis.Normalize();
            axis = axis * angle;
            Marshal.Copy(axis.toArr(), 0, _Rt, 3);
        }

        private unsafe void updateCeresIntrinsics() {
            var ptr = (double*)_Intrinsics.ToPointer();
            ptr[(int)IntrinsicsOffsets.OFFSET_FOCAL_LENGTH_X] = this.CameraMatrix.fx;
            ptr[(int)IntrinsicsOffsets.OFFSET_FOCAL_LENGTH_Y] = this.CameraMatrix.fy;
            ptr[(int)IntrinsicsOffsets.OFFSET_PRINCIPAL_POINT_X] = this.CameraMatrix.cx;
            ptr[(int)IntrinsicsOffsets.OFFSET_PRINCIPAL_POINT_Y] = this.CameraMatrix.cy;
            ptr[(int)IntrinsicsOffsets.OFFSET_K1] = this.DistortionR1;
            ptr[(int)IntrinsicsOffsets.OFFSET_K2] = this.DistortionR2;
            ptr[(int)IntrinsicsOffsets.OFFSET_P1] = this.DistortionT1;
            ptr[(int)IntrinsicsOffsets.OFFSET_P2] = this.DistortionT2;
            ptr[(int)IntrinsicsOffsets.OFFSET_K3] = this.DistortionR3;
        }


        unsafe double* ICeresCamera.Rt {
            get {
                if (_Rt == IntPtr.Zero) {
                    _Rt = Marshal.AllocHGlobal(sizeof(double) * 6);
                    updateCeresRtValues();
                }
                return (double*)_Rt;
            }
        }

        BundleIntrinsicsFlags ICeresCamera.BundleIntrinsics { get; set; }

        unsafe double* ICeresCamera.Intrinsics {
            get {
                if (_Intrinsics == IntPtr.Zero) {
                    _Intrinsics = Marshal.AllocHGlobal(sizeof(double) * 9);
                    updateCeresIntrinsics();
                }
                return (double*)_Intrinsics;
            }
        }

        string ICeresCamera.Name {
            get { return this.Name; }
        }
        #endregion
         * */
    }
}
