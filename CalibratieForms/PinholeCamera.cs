using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using cameracallibratie;
using CalibratieForms.Annotations;
using OpenCvSharp;
using SceneManager;

namespace CalibratieForms {
    public class PinholeCamera : SObject, INotifyPropertyChanged {
        /// <summary>
        /// echt geteste camera via foto's
        /// </summary>
        /// <returns></returns>
        public static PinholeCamera getTestCamera() {
            var mat = new double[,]{{3479.3332725692153,0,1499.9382892470603},{0,3458.5791417359405,1142.7454458370041},{0,0,1}};
            var m = new CameraMatrix(mat);
            var c = new PinholeCamera(m);
            c.PictureSize = new Size(3072, 2304);
            c.Cv_DistCoeffs5 = new[] {0.062813787874286389, -3.0485685802388809, -0.0017951735131834098, -0.00040688209299854, 14.91660690214403};
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

        public PinholeCamera() : base() {
            _cameraMatrix = new CameraMatrix();
            _cameraMatrix.PropertyChanged += (o, s) => { OnPropertyChanged("CameraMatrix."+s.PropertyName); };
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

        public double[] Cv_DistCoeffs5 { get {
            return new double[] {DistortionR1, DistortionR2, DistortionT1, DistortionT2, DistortionR3};
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
            }}
        public double[] Cv_rvecs { get { return new [] {Dir.X, Dir.Y, Dir.Z}; } }
        public double[] Cv_tvecs { get { return new[] { Pos.X, Pos.Y, Pos.Z }; } }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            var handler = PropertyChanged;
            if (handler != null) { handler(this, new PropertyChangedEventArgs(propertyName)); }
        }
    }
}
