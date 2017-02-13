using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;

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
        public Matrix<double> cvmat {
            get {
                return new Matrix<double>(_mat);
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



        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanged;
            if (handler != null) { handler(this, new PropertyChangedEventArgs(propertyName)); }
        }
    }
}
