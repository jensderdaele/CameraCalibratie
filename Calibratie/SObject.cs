using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using ceresdotnet;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using OpenTK;
using Matrix = Emgu.CV.Matrix<double>; 
namespace Calibratie {

    /// <summary>
    /// Gemaakt voor Agisoft project parsing: Chunk xml -> translatie lokaal naar lambert
    /// </summary>
    public class BaseTransform : SObjectBase,IXmlSerializable {
        private const string XMLNAME = "transform";

        public double Scale;

        public XmlSchema GetSchema() {
            return null;
        }

        public void ReadXml(XmlReader reader) {
            if (reader.Name == XMLNAME && reader.NodeType == XmlNodeType.Element) {
                while (reader.Read()) {
                    if (reader.NodeType == XmlNodeType.EndElement) {
                        if (reader.Name == XMLNAME) return;
                        continue;
                    }
                    switch (reader.Name) {
                        case "rotation":
                            reader.UpdateMatrixBetweenName(this.RotationMat);
                            break;
                        case "translation":
                            reader.UpdateMatrixBetweenName(this.Pos);
                            break;
                        case "scale":
                            Scale = double.Parse(reader.ReadValueBetweenName(), CultureInfo.InvariantCulture);
                            break;
                    }
                }
            }
        }

        public void WriteXml(XmlWriter writer) {
            writer.WriteName("transform");

            writer.WriteName("rotation");
            writer.WriteValue(base.RotationMat.ToXMLValueString());
            writer.WriteEndElement();

            writer.WriteName("translation");
            writer.WriteValue(base.Pos.ToXMLValueString());
            writer.WriteEndElement();

            writer.WriteName("scale");
            writer.WriteValue(Scale.ToString(CultureInfo.InvariantCulture));
            writer.WriteEndElement();

            writer.WriteEndElement();
        }
    }


    public abstract class SObject : SObjectBase, IXmlSerializable{
        /// <summary>
        /// Agisoft xml format
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T readTransformXML<T>(XmlReader reader) where T:SObject,new() {
            var r = new T();
            r.ReadXml(reader);
            return r;
        }

        

        public string Name = "";

        protected SObject()
            : base() {
            Name = String.Format("Unnamed:{0}", this.GetHashCode());
        }

        [Obsolete("niet gebruiken, wordt verwijderd")]
        public Matrix4d worldMat {
            get {
                var ret = new Matrix4d();
                for (int r = 0; r < 4; r++) {
                    for (int c = 0; c < 4; c++) {
                        ret[c, r] = base.WorldMat[r, c];
                    }
                }
                return ret;
            }
        }

        public XmlSchema GetSchema() {
            return null;
        }

        /// <summary>
        /// from agisoft xml (node = "transform")
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(XmlReader reader) {
            if (reader.Name == "transform" && reader.NodeType == XmlNodeType.Element) {
                reader.Read();
                var split = reader.Value.Split(new[] {' '});
                for (int i = 0; i < split.Length; i++) {
                    var d = double.Parse(split[i], CultureInfo.InvariantCulture);
                    this.WorldMat[i/4, i%4] = d;
                }
                reader.Read();
            }
        }


        /// <summary>
        /// not implemented
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer) {
            writer.WriteName("transform");
            var s = WorldMat.ToXMLValueString();
            
            writer.WriteValue(s);
            writer.WriteEndElement();
        }
    }

    /// <summary>
    /// alle data intern in 4x4 matrix
    /// </summary>
    public abstract class SObjectBase : SPoint, ICeresParameterConvertable<CeresPointOrient> {

        private Matrix _worldMat = new Matrix(new double[,] {
            {1,0,0,0},
            {0,1,0,0},
            {0,0,1,0},
            {0,0,0,1}
        });
        private Matrix _rot;

        public SObjectBase() {
            //_worldMat = new Matrix(4,4);
            base._pos = new Matrix<double>(3, 1, _worldMat.Mat.DataPointer + 3 * Matrix<double>.SizeOfElement, _worldMat.Mat.Step);
            _rot = new Matrix<double>(3, 3, _worldMat.Mat.DataPointer, _worldMat.Mat.Step);
        }

        /// <summary>
        /// 3,3 niet originele data
        /// </summary>
        public Matrix Rot_transform {
            get { return RotationMat.Transpose(); }
            set { RotationMat = value.Transpose(); }
        }

        /// <summary>
        /// 3,1 niet originele data, set: ROT eerst setten!!
        /// </summary>
        public Matrix Pos_transform {
            get { return -1 * Rot_transform * Pos; }
            set { base.setPos((-1 * Rot_transform).Inverted() * value);}
        }

        public Matrix WorldMat {
            get { return _worldMat; }
            set {
                _worldMat = value;
                _pos = new Matrix<double>(3, 1, value.Mat.DataPointer + 3 * 8, _worldMat.Mat.Step);
                _rot = new Matrix<double>(3, 3, _worldMat.Mat.DataPointer, _worldMat.Mat.Step);
            }
        }


        

        /// <summary>
        /// 3,3 Gebruikt zelfde data als worldmat
        /// set: copies data
        /// </summary>
        public Matrix RotationMat {
            get { return _rot; }
            set {
                for (int r = 0; r < 3; r++) {
                    for (int c = 0; c < 3; c++) {
                        _rot[r, c] = value[r, c];
                    }
                }
            }
        }

        /// <summary>
        /// 3,1 rodr vector van rot_transpose
        /// </summary>
        public RotationVector3D Rodr {
            get {
                var r = new RotationVector3D();
                CvInvoke.Rodrigues(Rot_transform, r);
                return r;
            }
        }



        internal CeresPointOrient _ceresparam;

        /// <summary>
        /// returns the unique ceresparam for this element
        /// </summary>
        /// <returns></returns>
        public CeresPointOrient toCeresParameter() {
            if (_ceresparam == null) {
                _ceresparam = new CeresPointOrient();
                _ceresparam.BundleFlags = BundlePointOrientFlags.ALL;
            }
            var rodr = Rodr;
            var t = Pos_transform;
            _ceresparam.R_rod = new double[] {rodr[0, 0], rodr[1, 0], rodr[2, 0]};
            _ceresparam.t = new double[]{t[0,0],t[1,0],t[2,0]};
            return _ceresparam;
        }

        public CeresPointOrient toCeresParameter(Enum BundleSettings) {
            var r = toCeresParameter();
            r.BundleFlags = (BundlePointOrientFlags) BundleSettings;
            return r;
        }

        public void updateFromCeres(CeresPointOrient paramblock) {
            _pos[0, 0] = paramblock.t[0];
            _pos[1, 0] = paramblock.t[1];
            _pos[2, 0] = paramblock.t[2];

            CvInvoke.Rodrigues(new Matrix(paramblock.R_rod), _rot);

            CvInvoke.Invert(_worldMat, _worldMat, DecompMethod.Cholesky);
        }
    }

    /// <summary>
    /// alle data interne in 3x1 matrix
    /// </summary>
    public abstract class SPoint : ICeresParameterConvertable<CeresPoint> {
        protected Matrix _pos;

        public double X {
            get {return _pos[0, 0];}
            set { _pos[0, 0] = value; }
        }

        public double Y {
            get { return _pos[1, 0]; }
            set { _pos[1, 0] = value; }
        }
        public double Z {
            get { return _pos[2, 0]; }
            set { _pos[2, 0] = value; }
        }

        public MCvPoint3D32f toMCvPoint3D32f() {
            return new MCvPoint3D32f((float)X, (float)Y, (float)Z);
        }

        /// <summary>
        /// 3,1 Gebruikt zelfde data als worldmat
        /// set: copies data
        /// </summary>
        public Matrix Pos {
            get { return _pos; }
            set { setPos(value); }
        }

        public SPoint() {
            _pos = new Matrix(3, 1);
        }

        public void setPos(double[] arr)
        {
            _pos[0, 0] = arr[0];
            _pos[1, 0] = arr[1];
            _pos[2, 0] = arr[2];
        }
        /// <summary>
        /// copies data into pos
        /// </summary>
        /// <param name="arr"></param>
        public void setPos(Matrix pos)
        {
            _pos[0, 0] = pos[0, 0];
            _pos[1, 0] = pos[1, 0];
            _pos[2, 0] = pos[2, 0];
        }

        private CeresPoint _cerespoint;
        public CeresPoint toCeresParameter() {
            if (_cerespoint == null) {
                _cerespoint = new CeresPoint();
            }
            _cerespoint.X = _pos[0, 0];
            _cerespoint.Y = _pos[1, 0];
            _cerespoint.Z = _pos[2, 0];
            return _cerespoint;
        }

        public CeresPoint toCeresParameter(Enum BundleSettings) {
            var r = toCeresParameter();
            r.BundleFlags = (BundleWorldCoordinatesFlags) BundleSettings;
            return r;
        }

        public void updateFromCeres(CeresPoint paramblock) {
            _pos[0, 0] = paramblock.X;
            _pos[1, 0] = paramblock.Y;
            _pos[2, 0] = paramblock.Z;
        }
    }
}
