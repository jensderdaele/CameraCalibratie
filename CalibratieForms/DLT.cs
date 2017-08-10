using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;

using Matrix = Emgu.CV.Matrix<float>;

namespace CalibratieForms {
    class DLT {

        public static void dlt(Matrix x, Matrix xp) {
            var n = x.Rows;
            if (n < 4) {
                throw new Exception();
            }
            Matrix A = null;
            for (int i = 0; i < x.Rows; i++) {
                var xip = xp[i, 0];
                var yip = xp[i, 1];
                var wip = xp[i, 2];
                var xi = x.GetRow(i);

                var test = -wip*xi;
                double te = (-wip*xi)[0, 0];
                var xit = xi.Transpose();
                var row1 = new Matrix(new float[,] {{0, 0, 0}});
                var row2 = (wip*xit);
                row1 = row1.ConcateHorizontal((-wip*xit)).ConcateHorizontal((yip*xit));
                row2 = row2.ConcateHorizontal(new Matrix(new float[,] { { 0, 0, 0 } })).ConcateHorizontal((-xip * xit));
                if (A == null) {
                    A = row1.ConcateVertical(row2);
                }
                else {
                    A.ConcateVertical(row1);
                    A.ConcateVertical(row2);
                }
            }

            var svd = new SVD<float>(A);
            Matrix H = svd.V.Reshape(3, 3);
            H = H/H[2, 2];
            
        }

        public static void dlt_norm(Matrix x, Matrix xp) {
            
        }
         
    }
        
}
