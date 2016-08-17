using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace Calibratie
{
    public class CameraMatrix : INotifyPropertyChanged {

        public CameraMatrix() {
        }
        public CameraMatrix(double[,] mat) {
            _mat = mat;
        }

        private double[,] _mat = new double[3, 3];
        public double[,] Mat { get { return _mat; } set { _mat = value; OnPropertyChanged(); } }

        public double ac { get { return Mat[0, 1] / fx; } set { Mat[0, 1] = value * fx; OnPropertyChanged(); } }

        /// <summary>
        /// copy cv mat
        /// </summary>
        public Mat cvmat {
            get {
                Mat m = new MatOfDouble(3,3);
                for (int r = 0; r < 3; r++) {
                    for (int c = 0; c < 3; c++) {
                        m.Set(r,c,Mat[r,c]);
                    }
                }
                return m;
            } }

        public double fx {
            get { return Mat[0, 0]; }
            set { Mat[0, 0] = value; OnPropertyChanged(); }
        }
        public double fy {
            get { return Mat[1, 1]; }
            set { Mat[1, 1] = value; OnPropertyChanged(); }
        }
        public double cx {
            get { return Mat[0, 2]; }
            set { Mat[0, 2] = value; OnPropertyChanged(); }
        }
        public double cy {
            get { return Mat[1, 2]; }
            set { Mat[1, 2] = value; OnPropertyChanged(); }
        }
        /*
        public double this[int c, int r] {
            get { return CvMat.Get<double>(c, r); }
            set { CvMat.Set(c, r, value); OnPropertyChanged(); }
        }*/
        /*public double[,] Mat {
            get {
                double[,] r = new double[3, 3];
                CvMat.GetArray(0, 0, r);
                return r;
            }
        }*/
        public static CameraMatrix FromProjectionMatrix(double[,] pm) {
            double[,] cameraMat, rotMat;
            double[] transVect;
            Cv2.DecomposeProjectionMatrix(pm, out cameraMat, out rotMat, out transVect);
            return new CameraMatrix(cameraMat);
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanged;
            if (handler != null) { handler(this, new PropertyChangedEventArgs(propertyName)); }
        }
    }
}
