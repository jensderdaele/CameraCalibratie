using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using ceresdotnet;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Newtonsoft.Json;

using Size = System.Drawing.Size;
using Point2d = Emgu.CV.Structure.MCvPoint2D64f;
using Matrix = Emgu.CV.Matrix<double>;

namespace Calibratie {
    
    

    [JsonObject(ItemConverterType = typeof(PinholeCameraConverter))]
    public class PinholeCamera : SObject, INotifyPropertyChanged, ICeresParameterConvertable<CeresPointOrient>, ICeresParameterConvertable<CeresIntrinsics>, IXmlSerializable {
        public static PinholeCamera FromXML(XmlReader reader, Func<int, CameraIntrinsics> getSensorForID = null) {
            if (reader.Name == "camera" && reader.NodeType == XmlNodeType.Element) {

                var r = new PinholeCamera();
                r.ReadXml(reader);
                if (getSensorForID != null) {
                    r._intrinsics = getSensorForID(r.SensorID);
                }
                return r;
            }
            return null;
        }
        /// <summary>
        /// Transformation is reeds bepaald? (agisoft xml transfrom node aanwezig)
        /// </summary>
        public bool TransformationSet { get; set; }
        
        /// <summary>
        /// Enabled (agisoft xml 'enabled' node)
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// filename (agisoft xml 'label' node)
        /// </summary>
        public string Label { get; set; }
        /// <summary>
        /// for internal use, enkel agisoft xml atm
        /// </summary>
        public int CameraID { get; set; }
        /// <summary>
        /// for internal use, enkel agisoft xml atm
        /// </summary>
        public int SensorID { get; set; }
        
        private CameraIntrinsics _intrinsics;
        public CameraIntrinsics Intrinsics { get { return _intrinsics;} }
        /// <summary>
        /// echt geteste camera Casio EX-Z120
        /// </summary>
        /// <returns></returns>
        public static PinholeCamera getTestCameraHuawei() {
            var mat = new double[,] { { 3441.0667667434618, 0, 2090.8502187520326 }, { 0, 3432.3907417119017, 1561.5316432859202 }, { 0, 0, 1 } };
            var m = new CameraIntrinsics(mat);
            var c = new PinholeCamera(m);
            c.PictureSize = new Size(4160, 3120);
            c.Cv_DistCoeffs5 = new[] { 0.24230691691999853, -0.92897577071991688, -0.00073617836680420852, 0.0015104681489398752, 1.0576231378646423 };
            return c;
        }
        public static PinholeCamera getTestCamera() {
            var mat = new double[,] { { 3479.3332725692153, 0, 1499.9382892470603 }, { 0, 3458.5791417359405, 1142.7454458370041 }, { 0, 0, 1 } };
            var m = new CameraIntrinsics(mat);
            var c = new PinholeCamera(m);
            c.PictureSize = new Size(3072, 2304);
            c.Cv_DistCoeffs5 = new[] { 0.062813787874286389, -3.0485685802388809, -0.0017951735131834098, -0.00040688209299854, 14.91660690214403 };
            return c;
        }

        /// <summary>
        /// fotogrootte in pixels
        /// </summary>
        public Size PictureSize {
            get { return _intrinsics.PictureSize; }
            set { _intrinsics.PictureSize = value; }
        }


        public string PictureSizeST { get { return String.Format("{0}x{1}", PictureSize.Width, PictureSize.Height); } }


        public PinholeCamera()
            : base() {
                _intrinsics = new CameraIntrinsics();
                _intrinsics.PropertyChanged += (o, s) => { OnPropertyChanged("Intrinsics." + s.PropertyName); };
        }
        public PinholeCamera(CameraIntrinsics m) {
            _intrinsics = m;
            _intrinsics.PropertyChanged += (o, s) => { OnPropertyChanged("Intrinsics." + s.PropertyName); };
        }


        public double DistortionR1 { get { return _intrinsics.DistortionR1; } set { _intrinsics.DistortionR1 = value; } }
        public double DistortionR2 { get { return _intrinsics.DistortionR2; } set { _intrinsics.DistortionR2 = value; } }
        public double DistortionR3 { get { return _intrinsics.DistortionR3; } set { _intrinsics.DistortionR3 = value; } }
        public double DistortionT1 { get { return _intrinsics.DistortionT1; } set { _intrinsics.DistortionT1 = value; } }
        public double DistortionT2 { get { return _intrinsics.DistortionT2; } set { _intrinsics.DistortionT2 = value; } }



        public double[] Cv_DistCoeffs5 {
            get {
                return new double[] { DistortionR1, DistortionR2, DistortionT1, DistortionT2, DistortionR3 };
            }
            set {
                if (value.Length != 5) {
                    throw new ArgumentException("wrong size");
                }
                DistortionR1 = value[0];
                DistortionR2 = value[1];
                DistortionT1 = value[2];
                DistortionT2 = value[3];
                DistortionR3 = value[4];
                OnPropertyChanged();
            }
        }
        public Matrix<double> Cv_DistCoeffs4 {
            get {
                return new Matrix<double>(new [] { DistortionR1, DistortionR2, DistortionT1, DistortionT2 });
            }
        }

        /// <summary>
        /// 3x4 projection matrix in huidig assenstelsel
        /// </summary>
        public Matrix<double> ProjectionMatrix {
            get {
                var k = new Matrix(3, 4);
                CvInvoke.HConcat(this.Intrinsics.cvmat, new Matrix(3, 1), k);
                //return k*this.WorldMat.Inverted();
                var rt = new Matrix(3, 4);
                CvInvoke.HConcat(this.Rot_transform, this.Pos_transform, rt);
                return Intrinsics.cvmat*rt;
            }
        }



        public Matrix<double> Rvecs {get { return this.Rodr; }}

        public Matrix<double> Tvecs {get { return Pos; }}

        public PointF[] ProjectBoard_Cv_p2d(ChessBoard b) {
            //Todo: Use InputArray on vector3d[] & pin GC
            Point2d[] imagePoints;
            return CvInvoke.ProjectPoints(b.boardLocalCoordinates_cv, Rvecs, Tvecs, _intrinsics.cvmat, Cv_DistCoeffs4);
        }
        public Vector2d[] ProjectBoard_Cvd(ChessBoard b) {
            return ProjectBoard_Cv_p2d(b).Select(x => new Vector2d(x.X, x.Y)).ToArray();
        }
        public Vector2[] ProjectBoard_Cv(ChessBoard b) {
            return ProjectBoard_Cv_p2d(b).Select(x => new Vector2((float)x.X, (float)x.Y)).ToArray();
        }

        public Matrix CalcFMat(PinholeCamera camera2) {
            throw new NotImplementedException();
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


        /// <summary>
        /// Project points similar to ceredotnet cost function
        /// </summary>
        /// <param name="vector3D"></param>
        /// <returns></returns>
        public Point2d ProjectPointd2D_Manually(Vector3d vector3D) {
            var transf = this.worldMat.Inverted();
            var camcoord = Vector3d.TransformPerspective(vector3D, transf);

            if (camcoord.Z < 0) {
                return new Point2d();
            }

            var testx = camcoord.X/camcoord.Z*this._intrinsics.fx + this._intrinsics.cx;
            var testy = camcoord.Y/camcoord.Z*this._intrinsics.fy + this._intrinsics.cy;


            var x = camcoord.X/camcoord.Z;
            var y = camcoord.Y/camcoord.Z;

            var r2 = x*x + y*y;
            var r4 = r2*r2;
            var r6 = r4*r2;
            var r_coeff = ((1) + this.DistortionR1*r2 + this.DistortionR2*r4 + this.DistortionR3*r6);
            var tdistx = 2*this.DistortionT1*x*y + this.DistortionT2*(r2 + 2*x*x);
            var tdisty = 2*this.DistortionT2*x*y + this.DistortionT1*(r2 + 2*y*y);
            var xd = x*r_coeff + tdistx;
            var yd = y*r_coeff + tdisty;

            var im_x = this._intrinsics.fx*xd + this._intrinsics.cx;
            var im_y = this._intrinsics.fy*yd + this._intrinsics.cy;

            if (im_x >= 0 && im_x <= this.PictureSize.Width && im_y >= 0 && im_y <= PictureSize.Height) {
                return new Point2d(im_x, im_y);
            }
            return new Point2d();
        }

        public PointF ProjectPoint_ceresdotnetAgisoftmodel(IMarker3d point) {
            throw new NotImplementedException();
        }


        CeresIntrinsics ICeresParameterConvertable<CeresIntrinsics>.toCeresParameter(Enum BundleSettings) {
            return Intrinsics.toCeresParameter(BundleSettings);
        }

        public void updateFromCeres() {
            Intrinsics.updateFromCeres(this.Intrinsics._ceresintr);
            this.updateFromCeres(this._ceresparam);
        }

        public void updateFromCeres(CeresIntrinsics paramblock) {
            Intrinsics.updateFromCeres(paramblock);
        }

        CeresIntrinsics ICeresParameterConvertable<CeresIntrinsics>.toCeresParameter() {
            return Intrinsics.toCeresParameter();
        }

        private CeresPointOrient _cerespointorient ;
        CeresPointOrient ICeresParameterConvertable<CeresPointOrient>.toCeresParameter() {
            if (_cerespointorient == null) {
                _cerespointorient = new CeresPointOrient();
            }
            var invert = this.worldMat.Inverted();
            var trans = invert.ExtractTranslation().toArr();
            Vector3d axis;
            double angle;
            invert.ExtractRotation().ToAxisAngle(out axis, out angle);
            axis.Normalize();
            axis *= angle;

            _cerespointorient.t = trans;
            _cerespointorient.R_rod = axis.toArr();

            return _cerespointorient;
        }
        CeresPointOrient ICeresParameterConvertable<CeresPointOrient>.toCeresParameter(Enum BundleSettings) {
            var r = ((ICeresParameterConvertable<CeresPointOrient>)this).toCeresParameter();
            r.BundleFlags = (BundlePointOrientFlags) BundleSettings;
            return r;
        }

        public void updateFromCeres(CeresPointOrient paramblock) {
            var rot3d = new RotationVector3D(paramblock.R_rod);
            var mat3 = new Matrix<double>(3, 3, rot3d.RotationMatrix.DataPointer);

            for (int r = 0; r < 3; r++) {
                for (int c = 0; c < 3; c++) {
                    RotationMat[r, c] = mat3[r, c];
                }
            }
            setPos(paramblock.t);

            WorldMat = WorldMat.Inverted();

            OnPropertyChanged();
        }



        /// <summary>
        /// from agisoft xml (node = "camera")
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(XmlReader reader) {

            if (reader.Name == "camera" && reader.NodeType == XmlNodeType.Element) {
                this.CameraID = int.Parse(reader["id"]);
                this.SensorID = int.Parse(reader["sensor_id"]);
                this.Label = reader["label"];
                this.Enabled = bool.Parse(reader["enabled"]);
                this.TransformationSet = false;
                while (reader.Read()) {
                    switch (reader.Name) {
                        case "camera": //end of node
                            return;
                            break;
                        case "transform":
                            base.ReadXml(reader);
                            this.TransformationSet = true;
                            break;
                        case "orientation": //geen idee ( = 1)
                            break;
                    }

                }
            }
        }


        /// <summary>
        /// not implemented
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer) {
            writer.WriteName("camera");
            writer.WriteAttributeString("id", this.CameraID.ToString());
            writer.WriteAttributeString("label", this.Label);
            writer.WriteAttributeString("sensor_id", this.SensorID.ToString());
            writer.WriteAttributeString("enabled", this.Enabled.ToString());
            if (this.Enabled) {
                base.WriteXml(writer);
            }
            writer.WriteEndElement();
        }
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
