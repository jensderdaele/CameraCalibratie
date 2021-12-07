using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ceresdotnet;
using Emgu.CV;
namespace Calibratie {

    
    
    public interface IMultiCameraImageProvider {
        CameraIntrinsics[] CameraIntrinsics { get; }
        int Length { get; }
        Mat this[int index, CameraIntrinsics intrinsics] { get; }
        ICameraImageProvider GetProviderForCamera(CameraIntrinsics index);
    }
    public interface ICameraImageProvider {
        CameraIntrinsics CameraIntrinsics { get; }
        Mat this[int index] { get; }
    }

    public interface IMarkerDetector {
        IEnumerable<Marker2d> detectMarkers(Emgu.CV.Mat image);
    }


    public abstract class MultiCameraObservation {
        private CameraCollection collection;


        private Tuple<Marker2d, IMarker3d> this[int index, PinholeCamera cam] {
            get {

                throw new NotImplementedException();
            }
            
        }

        public IObservation getObservation(PinholeCamera cam) {
            throw new NotImplementedException();
        }


    }

    public interface IObservation {
        IMarkerDetector detector { get; }
        PinholeCamera Camera { get; }

        Tuple<Marker2d, IMarker3d> this[int index] { get; }
        IEnumerable<Marker2d> AllMarkers2 { get; }
        IEnumerable<Marker2d> KnownMarkers2 { get; }
        IEnumerable<Marker2d> Markers { get; }

    }

    public class CameraObservation : IObservation {

        public IMarkerDetector detector { get; private set; }
        public PinholeCamera Camera { get; set; }

        public Tuple<Marker2d, IMarker3d> this[int index] {
            get { throw new NotImplementedException(); }
        }

        public IEnumerable<Marker2d> AllMarkers2 { get; private set; }
        public IEnumerable<Marker2d> KnownMarkers2 { get; private set; }
        public IEnumerable<Marker2d> Markers { get; private set; }
    }

    public abstract class Marker2d  {
        public abstract int ID { get; }
        public abstract float X { get; }
        public abstract float Y { get; }

        public PointF PointF { get {return new PointF(X,Y);} }

        public Emgu.CV.Matrix<float> CV {
            get {
                return new Matrix<float>(new []{X,Y});
            }
        }
    }

    public class Marker2dTest : Marker2d {
        private int id;
        private float x, y;
        public override int ID { get { return id; } }
        public override float X { get { return x; } }
        public override float Y { get { return y; } }

        public Marker2dTest(int id, float x, float y) {
            this.id = id;
            this.x = x;
            this.y = y;
        }
    }
    

    public interface IMarker3d {
        int ID { get; }
        double X { get; }
        double Y { get; }
        double Z { get; }
    }
}
