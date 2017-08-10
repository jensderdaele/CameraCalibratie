using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using ceresdotnet;
using Emgu.CV;

namespace Calibratie {

    /// <summary>
    /// statics
    /// </summary>
    public partial class CameraIntrinsics {
        /// <summary>
        /// Linkse Camera
        /// </summary>
        public static CameraIntrinsics EOS5DMARKII {
            get {
                return new CameraIntrinsics(new double[,] {
                    {3842, 0, 2822},
                    {0, 3841, 1842},
                    {0, 0, 1}
                }) {
                    CVDIST = new Matrix<double>(new[] { -.1249, .0943 }),
                    PictureSize = new Size(5616, 3744)
                };
            }
        }

        public static CameraIntrinsics GOPROHERO3_BROWNR3_AFGEROND_EXTRADIST {
            get {
                return new CameraIntrinsics(new double[,] {
                    {1600, 0, 3680/2},
                    {0, 1600, 2760/2},
                    {0, 0, 1}
                }) {
                    //CVDIST = new Matrix<double>(new[] { -.22887, .0570365, -.0000811723, -.000047540, -.00651444 }),
                    CVDIST = new Matrix<double>(new[] { -.25, .06, -.0005, -.0001, -.01 }),
                    PictureSize = new Size(3680, 2760)
                };
            }
        }

        /// <summary>
        /// rechtse camera
        /// </summary>
        public static CameraIntrinsics EOS1000D {
            get {
                return new CameraIntrinsics(new double[,] {
                    {4019, 0, 1989},
                    {0, 4018, 1287},
                    {0, 0, 1}
                }) {
                    CVDIST = new Matrix<double>(new[] { -.1389, .15689 }),
                    PictureSize = new Size(3888, 2592)
                };
            }
        }

        /// <summary>
        /// Linkse gopro camera
        /// </summary>
        public static CameraIntrinsics GOPROLEFT {
            get {
                return new CameraIntrinsics(new double[,] {
                    {954, 0, 923},
                    {0, 960, 484},
                    {0, 0, 1}
                }) {
                    CVDIST = new Matrix<double>(new[] { -.2172, .04159, 0, 0 }),
                    PictureSize = new Size(1920, 1080)
                };
            }
        }

        /// <summary>
        /// Rechtse gopro camera
        /// </summary>
        public static CameraIntrinsics GOPRORIGHT {
            get {
                return new CameraIntrinsics(new double[,] {
                    {845, 0, 954},
                    {0, 848, 522},
                    {0, 0, 1}
                }) {
                    CVDIST = new Matrix<double>(new[] { -.1886, .02519, 0, 0 }),
                    PictureSize = new Size(1920, 1080)
                };
            }
        }

        public static CameraIntrinsics GOPROFOTOWIDE {
            get {
                return new CameraIntrinsics(new double[,] {
                    {1599.930940149525, 0, 1844.0057832637037},
                    {0, 1598.3202905837152, 1372.0600999998783},
                    {0, 0, 1}
                }) {
                    CVDIST = new Matrix<double>(new double[] { -0.20706923286921078, 0.041005407838082646, -0.00037614709418833662, -0.00063068237132738857, -0.0033808250802939443 }),
                    PictureSize = new Size(3680, 2760)
                };
            }
        }
    }

    public partial class CameraIntrinsics : INotifyPropertyChanged, ICeresParameterConvertable<CeresIntrinsics>, IXmlSerializable {
        public enum Agisoftclass {
            adjusted,
            initial,
            none
        }

        #region statics
        /// <summary>
        /// return new intr baseon img
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public static CameraIntrinsics FromImage(Bitmap img) {
            var r = new CameraIntrinsics();

            r.PictureSize = new Size(img.Width, img.Height);

            return r;
        }
        public static CameraIntrinsics FromAgisoftXML(XmlReader reader) {
            var r = new CameraIntrinsics();
            r.ReadXml(reader);
            return r;
        }

        public void SetDistortionsZero() {
            _distortionR1 = 0;
            _distortionR2 = 0;
            _distortionR3 = 0;
            _distortionR4 = 0;

            _distortionS1 = 0;
            _distortionS2 = 0;

            _distortionT1 = 0;
            _distortionT2 = 0;
            _distortionT3 = 0;
            _distortionT4 = 0;
            
        }


        #endregion

        #region fields & propfields

        public Agisoftclass agisoftclass = Agisoftclass.adjusted;
        /// <summary>
        /// AGISOFT XML VALUE
        /// </summary>
        public bool Fixed;
        /// <summary>
        /// naam vd camera
        /// </summary>
        public string Label;
        private double _distortionR1, _distortionR2, _distortionR3, _distortionR4, _distortionR5, _distortionR6;
        private double _distortionT1, _distortionT2, _distortionT3, _distortionT4;
        private double _distortionS1, _distortionS2;

        /// <summary>
        /// wordt bij voorkeur Matrix<double>
        /// </summary>
        private double[,] _mat = new double[3, 3];

        /// <summary>
        /// ID, van toepassing intern voor linken van externe-interne params, niet aankomen
        /// </summary>
        public int SensorID { get; set; }

        /// <summary>
        /// fotogrootte in pixels/resolutie
        /// </summary>
        public Size PictureSize { get; set; }

        /// <summary>
        /// pixel breedte in mm
        /// </summary>
        public double Pixel_width { get; set; }

        /// <summary>
        /// pixel hoogte in mm
        /// </summary>
        public double Pixel_height { get; set; }

        #endregion

        #region properties

        #region getset

        public double DistortionR1 {
            get { return _distortionR1; }
            set {
                _distortionR1 = value;
                OnPropertyChanged();
            }
        }

        public double DistortionR2 {
            get { return _distortionR2; }
            set {
                _distortionR2 = value;
                OnPropertyChanged();
            }
        }

        public double DistortionR3 {
            get { return _distortionR3; }
            set {
                _distortionR3 = value;
                OnPropertyChanged();
            }
        }

        public double DistortionR4 {
            get { return _distortionR4; }
            set {
                _distortionR4 = value;
                OnPropertyChanged();
            }
        }
        public double DistortionR5 {
            get { return _distortionR5; }
            set {
                _distortionR5 = value;
                OnPropertyChanged();
            }
        }
        public double DistortionR6 {
            get { return _distortionR6; }
            set {
                _distortionR6 = value;
                OnPropertyChanged();
            }
        }

        public double DistortionT1 {
            get { return _distortionT1; }
            set {
                _distortionT1 = value;
                OnPropertyChanged();
            }
        }

        public double DistortionT2 {
            get { return _distortionT2; }
            set {
                _distortionT2 = value;
                OnPropertyChanged();
            }
        }
        public double DistortionT3 {
            get { return _distortionT3; }
            set {
                _distortionT3 = value;
                OnPropertyChanged();
            }
        }
        public double DistortionT4 {
            get { return _distortionT4; }
            set {
                _distortionT4 = value;
                OnPropertyChanged();
            }
        }
        public double DistortionS1 {
            get { return _distortionS1; }
            set {
                _distortionS1 = value;
                OnPropertyChanged();
            }
        }
        public double DistortionS2 {
            get { return _distortionS2; }
            set {
                _distortionS2 = value;
                OnPropertyChanged();
            }
        }
        public double[,] Mat {
            get { return _mat; }
            set {
                _mat = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// skew/fx
        /// </summary>
        public double ac {
            get { return Mat[0, 1] / fx; }
        }

        /// <summary>
        /// copy cv mat
        /// </summary>
        public Matrix<double> cvmat {
            get { return new Matrix<double>(_mat); }
            set { _mat = value.Data; }
        }

        public double fx {
            get { return Mat[0, 0]; }
            set {
                Mat[0, 0] = value;
                OnPropertyChanged();
            }
        }

        public double skew {
            get { return Mat[0, 1]; }
            set {
                Mat[0, 1] = value;
                OnPropertyChanged();
            }
        }

        public double fy {
            get { return Mat[1, 1]; }
            set {
                Mat[1, 1] = value;
                OnPropertyChanged();
            }
        }

        public double cx {
            get { return Mat[0, 2]; }
            set {
                Mat[0, 2] = value;
                OnPropertyChanged();
            }
        }

        public double cy {
            get { return Mat[1, 2]; }
            set {
                Mat[1, 2] = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region get
        
        public Matrix<double> Cv_DistCoeffs4 {
            get { return new Matrix<double>(new[] { DistortionR1, DistortionR2, DistortionT1, DistortionT2 }); }
        }
        #endregion

        #region set
        /// <summary>
        /// sets the distortion, 2,3 (zonder tang) of 4, 5(met tang) waarden
        /// gets dist5
        /// </summary>
        public Matrix<double> CVDIST {
            set {
                _distortionR1 = value[0, 0];
                _distortionR2 = value[1, 0];

                if (value.Rows == 3) {
                    _distortionR3 = value[2, 0];
                }
                if (value.Rows == 4) {
                    _distortionT1 = value[2, 0];
                    _distortionT2 = value[3, 0];
                }
                if (value.Rows == 5) {
                    _distortionR3 = value[4, 0];
                    _distortionT1 = value[2, 0];
                    _distortionT2 = value[3, 0];
                }
            }
            get => new Matrix<double>(new[] { DistortionR1, DistortionR2, DistortionT1, DistortionT2, DistortionR3 });
        }
        #endregion

        #endregion

        #region constr

        public CameraIntrinsics() {
            _mat[2, 2] = 1;
        }
        public CameraIntrinsics(double[,] mat) {
            Mat = mat;
            _mat[2, 2] = 1;
        }
        public CameraIntrinsics(Matrix<double> mat) {
            cvmat = mat;
            _mat[2, 2] = 1;
        }

        #endregion






        public double[] dist5 {
            get { return new[] {DistortionR1, DistortionR2, DistortionT1, DistortionT2, DistortionR3}; }
        }





        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public DistortionModel Distortionmodel = DistortionModel.Standard;
        /// <summary>
        /// returns a unique ceresparameter class
        /// </summary>
        /// <returns></returns>
        public CeresIntrinsics toCeresParameter() {
            if (_ceresintr == null) {
                _ceresintr = new CeresIntrinsics(this.Distortionmodel);
                _ceresintr.BundleFlags = (BundleIntrinsicsFlags.ALL);// = BundleIntrinsicsFlags.ALL;
            }
            var paramblock = _ceresintr;
            paramblock.fx = fx;
            paramblock.fy = fy;

            paramblock.ppx = cx;
            paramblock.ppy = cy;

            paramblock.skew = skew;

            paramblock.k1 = _distortionR1;
            paramblock.k2 = _distortionR2;
            paramblock.k3 = _distortionR3;

            paramblock.p1 = _distortionT1;
            paramblock.p2 = _distortionT2;

            if (_ceresintr.Distortionmodel == DistortionModel.AgisoftPhotoscan) {
                paramblock.k4 = _distortionR4;

                paramblock.p3 = _distortionT3;
                paramblock.p4 = _distortionT4;
            }
            if (_ceresintr.Distortionmodel == DistortionModel.OpenCVAdvanced) {
                paramblock.k4 = _distortionR4;
                paramblock.k5 = _distortionR5;
                paramblock.k6 = _distortionR6;

                paramblock.s1 = _distortionS1;
                paramblock.s2 = _distortionS2;
            }

            paramblock._imageWidth = PictureSize.Width;
            paramblock._imageHeight = PictureSize.Height;

            return paramblock;
        }
        

        public void updateFromCeres(CeresIntrinsics paramblock) {
            fx = paramblock.fx;
            fy = paramblock.fy;

            cx = paramblock.ppx;
            cy = paramblock.ppy;

            skew = paramblock.skew;

            _distortionR1 = paramblock.k1;
            _distortionR2 = paramblock.k2;
            _distortionR3 = paramblock.k3;

            _distortionT1 = paramblock.p1;
            _distortionT2 = paramblock.p2;

            if (_ceresintr.Distortionmodel == DistortionModel.OpenCVAdvanced) {
                _distortionR4 = paramblock.k4;
                _distortionR5 = paramblock.k5;
                _distortionR6 = paramblock.k6;

                _distortionS1 = paramblock.s1;
                _distortionS2 = paramblock.s2;
            }
            if (_ceresintr.Distortionmodel == DistortionModel.AgisoftPhotoscan) {
                _distortionR4 = paramblock.k4;

                _distortionT3 = paramblock.p3;
                _distortionT4 = paramblock.p4;
            }

            OnPropertyChanged();
        }

        public CeresIntrinsics _ceresintr;
        
        

        public CeresIntrinsics toCeresParameter(Enum BundleSettings) {
            var r = toCeresParameter();
            r.BundleFlags = ((BundleIntrinsicsFlags)BundleSettings);
            return r;
        }
        

        /// <summary>
        /// null
        /// </summary>
        /// <returns></returns>
        public XmlSchema GetSchema() {
            return null;
        }

        private static double readInnerXmlDouble(XmlReader reader) {
            reader.Read();
            var r = Double.Parse(reader.Value, CultureInfo.InvariantCulture);
            reader.Read();
            return r;
        }

        /// <summary>
        /// Agisoft xml format (camera calibration output)
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(XmlReader reader) {

            if (reader.Name != "sensor") {
                return;
            }
            this.SensorID = int.Parse(reader["id"]); //int
            this.Label = reader["label"];
            
            //var type = reader["type"]; //"frame"

            do {
                switch (reader.Name) {
                    case "sensor":
                        if (reader.NodeType == XmlNodeType.EndElement) return;
                        break;
                    case "resolution":
                        this.PictureSize = new Size(int.Parse(reader["width"]), int.Parse(reader["height"]));
                        break;
                    case "property":
                        switch (reader["name"]) {
                            case "pixel_width": //EXIF in mm
                                this.Pixel_width = double.Parse(reader["value"],CultureInfo.InvariantCulture);
                                break;
                            case "pixel_height": //EXIF in mm
                                this.Pixel_height = double.Parse(reader["value"], CultureInfo.InvariantCulture);
                                break;
                            case "focal_length"://EXIF in mm
                                //this.Pixel_width = double.Parse(reader["value"]);
                                break;
                            case "fixed": //fixed for BA?
                                var fixd = bool.Parse(reader["value"]);
                                break;
                        }
                        break;

                    case "calibration":
                        if (reader["class"] == "initial") {//nog niet gecalibreerd
                            //Initial calibration
                        }if (reader["class"] == "adjusted") {//gecalibreerd via tiepoints
                            //Initial calibration
                        }
                        break;
                    case "fx":
                        this.fx = readInnerXmlDouble(reader);
                        break;
                    case "fy":
                        this.fy = readInnerXmlDouble(reader);
                        break;
                    case "cx":
                        this.cx = readInnerXmlDouble(reader);
                        break;
                    case "cy":
                        this.cy = readInnerXmlDouble(reader);
                        break;
                    case "p1":
                        this._distortionT1 = readInnerXmlDouble(reader);
                        break;
                    case "p2":
                        this._distortionT2 = readInnerXmlDouble(reader);
                        break;
                    case "p3":
                        this._distortionT3 = readInnerXmlDouble(reader);
                        break;
                    case "p4":
                        this._distortionT4 = readInnerXmlDouble(reader);
                        break;
                    case "k1":
                        this._distortionR1 = readInnerXmlDouble(reader);
                        break;
                    case "k2":
                        this._distortionR2 = readInnerXmlDouble(reader);
                        break;
                    case "k3":
                        this._distortionR3 = readInnerXmlDouble(reader);
                        break;
                    case "k4":
                        this._distortionR4 = readInnerXmlDouble(reader);
                        break;
                    case "skew":
                        this.skew = readInnerXmlDouble(reader);
                        break;
                    default:
                        break;
                }
            } while (reader.Read());


        }

        public void WriteXml(XmlWriter writer) {
            writer.WriteName("sensor");
            writer.WriteAttributeString("id", SensorID.ToString());
            writer.WriteAttributeString("label",Label);
            writer.WriteAttributeString("type", "frame");

            writer.WriteName("resolution");
            writer.WriteAttributeString("width", PictureSize.Width.ToString());
            writer.WriteAttributeString("height", PictureSize.Height.ToString());
            writer.WriteEndElement();

            writer.writeProperty("pixel_width", this.Pixel_width.ToString(CultureInfo.InvariantCulture));
            writer.writeProperty("pixel_height", this.Pixel_width.ToString(CultureInfo.InvariantCulture));

            writer.writeProperty("focal_length", (this.fx * Pixel_width).ToString(CultureInfo.InvariantCulture));
            writer.writeProperty("fixed",Fixed.ToString());

            WriteCalibration(writer);
            writer.WriteEndElement();

        }

        public void WriteCalibration(XmlWriter writer) {
            writer.WriteName("calibration");
            writer.WriteAttributeString("type", "frame");
            writer.WriteAttributeString("class", agisoftclass.ToString());

            writer.WriteName("resolution");
            writer.WriteAttributeString("width", PictureSize.Width.ToString());
            writer.WriteAttributeString("height", PictureSize.Height.ToString());
            writer.WriteEndElement();

            writer.WriteName("fx");
            writer.WriteValue(this.fx);
            writer.WriteEndElement();

            writer.WriteName("fy");
            writer.WriteValue(this.fy);
            writer.WriteEndElement();

            writer.WriteName("cx");
            writer.WriteValue(this.cx);
            writer.WriteEndElement();

            writer.WriteName("cy");
            writer.WriteValue(this.cy);
            writer.WriteEndElement();

            writer.WriteName("skew");
            writer.WriteValue(this.skew);
            writer.WriteEndElement();

            writer.WriteName("k1");
            writer.WriteValue(this.DistortionR1);
            writer.WriteEndElement();

            writer.WriteName("k2");
            writer.WriteValue(this.DistortionR2);
            writer.WriteEndElement();

            writer.WriteName("k3");
            writer.WriteValue(this.DistortionR3);
            writer.WriteEndElement();

            writer.WriteName("k4");
            writer.WriteValue(this.DistortionR4);
            writer.WriteEndElement();

            /*
            writer.WriteName("k4");
            writer.WriteValue(this.DistortionR4);
            writer.WriteEndElement();*/

            writer.WriteName("p1");
            writer.WriteValue(this.DistortionT1);
            writer.WriteEndElement();

            writer.WriteName("p2");
            writer.WriteValue(this.DistortionT2);
            writer.WriteEndElement();

            writer.WriteName("p3");
            writer.WriteValue(this.DistortionT3);
            writer.WriteEndElement();

            writer.WriteName("p4");
            writer.WriteValue(this.DistortionT4);
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

    }
}