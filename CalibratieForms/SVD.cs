using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace CalibratieForms {
    class SVD {
        public Matrix<double> U = new Matrix<double>(3,3);
        public Matrix<double> W = new Matrix<double>(3, 3);
        public Matrix<double> Vt = new Matrix<double>(3, 3);


        public SVD(Matrix<double> mat,SvdFlag flag = SvdFlag.Default) {
            CvInvoke.SVDecomp(mat, W, U, Vt, flag);
            Vt = Vt.Transpose();
        }
    }
}
