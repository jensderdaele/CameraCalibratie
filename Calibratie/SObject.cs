using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using ceresdotnet;
using Calibratie.Annotations;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using OpenTK;
using Matrix = Emgu.CV.Matrix<double>; 
namespace Calibratie {

    /// <summary>
    /// Gemaakt voor Agisoft project parsing: Chunk xml -> translatie lokaal naar lambert
    /// </summary>
    public class BaseTransform : SObjectBase,IXmlSerializable,ICeresScaleTransform {
        public new double[] Rodr {
            get {
                var r = new RotationVector3D();
                CvInvoke.Rodrigues(RotationMat, r);
                return new []{r[0,0],r[1,0],r[2,0]};
            }
            set {
                var r = new RotationVector3D();
                r[0, 0] = value[0];
                r[1, 0] = value[1];
                r[2, 0] = value[2];
                CvInvoke.Rodrigues(r, this.RotationMat);
            }
        }
        
        
        BundleWorldCoordinatesFlags BundleFlags { get; }

        private ICeresScaleTransform _ceresScaleTransformImplementation;
        private const string XMLNAME = "transform";
        

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

        public double Scale { get; set; }
        public double scale {
            get => Scale;
            set => Scale = value;
        }

        double[] ICeresScaleTransform.Rot_rodr {
            get => Rodr;
            set => Rodr = value;
        }
         double ICeresScaleTransform.ZOffset {
             get => Z;
             set => Z = value;
         }
         double ICeresScaleTransform.YOffset {
             get => Y;
             set => Y = value;
         }
        double ICeresScaleTransform.XOffset {
            get => X;
            set => X = value;
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
    public abstract class SObjectBase : SPoint, ICeresPointOrient {

        private Matrix _worldMat = new Matrix(new double[,] {
            {1,0,0,0},
            {0,1,0,0},
            {0,0,1,0},
            {0,0,0,1}
        });
        private Matrix _rot;

        public SObjectBase() {
            base._pos = new Matrix<double>(3, 1, _worldMat.Mat.DataPointer + 3 * Matrix<double>.SizeOfElement, _worldMat.Mat.Step);
            _rot = new Matrix<double>(3, 3, _worldMat.Mat.DataPointer, _worldMat.Mat.Step);
        }

        /// <summary>
        /// 3,3 niet originele data
        /// </summary>
        public Matrix Rot_transform {
            get => RotationMat.Transpose();
            set => RotationMat = value.Transpose();
        }

        /// <summary>
        /// 3,1 niet originele data, set: ROT eerst setten!!
        /// </summary>
        public Matrix Pos_transform {
            get => -1 * Rot_transform * Pos;
            set {
                base.setPos((-1 * Rot_transform).Inverted() * value);
                OnPropertyChanged();/*
                OnPropertyChanged(nameof(X));
                OnPropertyChanged(nameof(Y));
                OnPropertyChanged(nameof(Z));*/
            }
        }

        public Matrix WorldMat {
            get => _worldMat;
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
            get => _rot;
            set {
                for (int r = 0; r < 3; r++) {
                    for (int c = 0; c < 3; c++) {
                        _rot[r, c] = value[r, c];
                    }
                }
                OnPropertyChanged();
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




        public BundleWorldCoordinatesFlags BundleFlags => throw new NotImplementedException();

        Enum ICeresParameterblock.BundleFlags { get => BundleFlags; set => throw new NotImplementedException(); }


        double[] ICeresPointOrient.Pos_paramblock {
            get {
                var p = Pos_transform;
                return new[] { p[0, 0], p[1, 0], p[2, 0] };
            }
            set {
                Pos_transform = new Matrix(value);
            }
        }

        double[] ICeresPointOrient.Rodr {
            get {
                var rodr = Rodr;
                return new[] {rodr[0, 0], rodr[1, 0], rodr[2, 0]};
            }
            set {
                Matrix m = new Matrix(3,3);
                CvInvoke.Rodrigues(new Matrix(value), m);
                this.Rot_transform = m;
                OnPropertyChanged();
            }
        }

        double ICeresPointOrient.X {
            get => this.Pos_transform[0, 0];
            set {
                var p = Pos_transform;
                p[0, 0] = value;
                Pos_transform = p;
                OnPropertyChanged();
            }
        }
        double ICeresPointOrient.Y {
            get => this.Pos_transform[1, 0];
            set {
                var p = Pos_transform;
                p[1, 0] = value;
                Pos_transform = p;
                OnPropertyChanged();
            }
        }
        double ICeresPointOrient.Z {
            get => this.Pos_transform[2, 0];
            set {
                var p = Pos_transform;
                p[2, 0] = value;
                Pos_transform = p;
                OnPropertyChanged();
            }
        }
        
    }

    /// <summary>
    /// alle data interne in 3x1 matrix
    /// </summary>
    public abstract class SPoint : ICeresPoint, INotifyPropertyChanged{
        protected Matrix _pos;

        public double X {
            get {return _pos[0, 0];}
            set { _pos[0, 0] = value;
                OnPropertyChanged();
            }
        }

        public double Y {
            get { return _pos[1, 0]; }
            set { _pos[1, 0] = value;
                OnPropertyChanged();
            }
        }
        public double Z {
            get { return _pos[2, 0]; }
            set {
                _pos[2, 0] = value;
                OnPropertyChanged();
            }
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

        public BundleWorldCoordinatesFlags BundleFlags => throw new NotImplementedException();

        Enum ICeresParameterblock.BundleFlags { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
            OnPropertyChanged(nameof(X));
            OnPropertyChanged(nameof(Y));
            OnPropertyChanged(nameof(Z));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
