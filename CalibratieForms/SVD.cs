using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;

namespace CalibratieForms {
    public class SVD<TDepth> where TDepth : new() {
        public Matrix<TDepth> U = new Matrix<TDepth>(3, 3);
        public Matrix<TDepth> W = new Matrix<TDepth>(3, 3);
        public Matrix<TDepth> Vt = new Matrix<TDepth>(3, 3);
        public Matrix<TDepth> V = new Matrix<TDepth>(3, 3);

        public SVD(Matrix<TDepth> mat, SvdFlag flag = SvdFlag.Default) {
            CvInvoke.SVDecomp(mat, W, U, V, flag);
            Vt = V.Transpose();
        }
    }
}
