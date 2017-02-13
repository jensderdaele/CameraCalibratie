using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using SceneManager;

using ceresdotnet;
using System.Runtime.InteropServices;
using Emgu.CV;
using Newtonsoft.Json;

using Size = System.Drawing.Size;
using Point2d = Emgu.CV.Structure.MCvPoint2D64f;

namespace Calibratie {
    [JsonObject(ItemConverterType = typeof(PinholeCameraConverter))]
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
        public Matrix<double> Cv_DistCoeffs4 {
            get {
                return new Matrix<double>(new [] { DistortionR1, DistortionR2, DistortionT1, DistortionT2 });
            }
        }
        public Matrix<double> Rvecs { get { return new Matrix<double>(new[] { Dir.X, Dir.Y, Dir.Z }); } }
        public Matrix<double> Tvecs { get { return new Matrix<double>(new[] { Pos.X, Pos.Y, Pos.Z }); } }

        public PointF[] ProjectBoard_Cv_p2d(ChessBoard b) {
            //Todo: Use InputArray on vector3d[] & pin GC
            Point2d[] imagePoints;
            return CvInvoke.ProjectPoints(b.boardLocalCoordinates_cv, Rvecs, Tvecs, CameraMatrix.cvmat, Cv_DistCoeffs4);
        }
        public Vector2d[] ProjectBoard_Cvd(ChessBoard b) {
            return ProjectBoard_Cv_p2d(b).Select(x => new Vector2d(x.X, x.Y)).ToArray();
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

    public class PinholeCameraConverter : JsonConverter {
        public override bool CanConvert(Type objectType) {
            return objectType == typeof(PinholeCamera);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            var camera = value as PinholeCamera;
            /*
            writer.WriteStartObject();
            writer.WritePropertyName("id");
            writer.WriteValue(camera.id);
            writer.WritePropertyName("name");
            writer.WriteValue(camera.name);

            foreach (var item in camera.fields) {
                writer.WritePropertyName(item.Key);
                writer.WriteValue(item.Value);
            }*/
            writer.WriteEndObject();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var product = existingValue as PinholeCamera ?? new PinholeCamera();
            /*
            while (reader.Read()) {
                if (reader.TokenType == JsonToken.EndObject)
                    continue;

                var value = reader.Value.ToString();
                switch (value) {
                    case "id":
                        product.id = reader.ReadAsString();
                        break;
                    case "name":
                        product.name = reader.ReadAsString();
                        break;
                    default:
                        product.fields.Add(value, reader.ReadAsString());
                        break;
                }

            }*/

            return product;
        }
    }
}
