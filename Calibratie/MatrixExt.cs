using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ceresdotnet;
using Emgu.CV;
using Emgu.CV.CvEnum;
using OpenTK;


namespace Calibratie {
    public static class MatrixExt {

        public static void ReadString(this Matrix<double> mat, string s) {
            var split = s.Split(new[] { ' ' });
            for (int i = 0; i < split.Length; i++) {
                var d = double.Parse(split[i], CultureInfo.InvariantCulture);
                mat[i / mat.Cols, i % mat.Cols] = d;
            }
        }
        public static string ToXMLValueString(this Matrix<double> mat) {
            string s = "";
            for (int r = 0; r < mat.Rows; r++) {
                for (int c = 0; c < mat.Cols; c++) {
                    s += mat[r, c].ToString(CultureInfo.InvariantCulture);
                }
            }
            return s;
        } 
        public static RotationVector3D toRotationVector(this Matrix<double> mat) {
            var r = new RotationVector3D();
            CvInvoke.Rodrigues(mat, r);
            return r;
        }
        public static Matrix<T> UnitVectorZ<T>() where T : new() {
            return new Matrix<T>(new T[] { default(T), default(T), (T)Convert.ChangeType(1, typeof(T)) });
        }
        public static Matrix<T> UnitVectorY<T>() where T : new() {
            return new Matrix<T>(new T[] { default(T), (T)Convert.ChangeType(1, typeof(T)), default(T)});
        }
        /// <summary>
        /// Build a world space to camera space matrix
        /// </summary>
        /// <param name="eye">Eye (camera) position in world space</param>
        /// <param name="target">Target position in world space</param>
        /// <param name="up">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <returns>A Matrix that transforms world space to camera space</returns>
        public static Matrix<T> LookAt<T>(Matrix<T> eye, Matrix<T> target, Matrix<T> up) where T : new() {

            Matrix<T> z = new Matrix<T>(3, 1);
            Matrix<T> x = new Matrix<T>(3, 1);
            Matrix<T> y = new Matrix<T>(3, 1);



            CvInvoke.Normalize(eye - target, z);
            CvInvoke.Normalize(up.Mat.Cross(z), x);
            CvInvoke.Normalize(z.Mat.Cross(x), y);

            var rot = new Matrix<T>(new T[,]{
                {x[0,0],y[0,0],z[0,0],default(T)},
                {x[1,0],y[1,0],z[1,0],default(T)},
                {x[2,0],y[2,0],z[2,0],default(T)},
                {default(T),default(T),default(T),(T)Convert.ChangeType(1,typeof(T))},
            });
            var eyemin = -1 * eye;
            var trans = new Matrix<T>(new T[,]{
                {(T)Convert.ChangeType(1,typeof(T)),default(T),default(T),default(T)},
                {default(T),(T)Convert.ChangeType(1,typeof(T)),default(T),default(T)},
                {default(T),default(T),(T)Convert.ChangeType(1,typeof(T)),default(T)},
                {eyemin[0,0],eyemin[1,0],eyemin[2,0],(T)Convert.ChangeType(1,typeof(T))}
            });

            var r = new Matrix<T>(new T[,]{
                {x[0,0],y[0,0],z[0,0],default(T)},
                {x[1,0],y[1,0],z[1,0],default(T)},
                {x[2,0],y[2,0],z[2,0],default(T)},
                {(T)Convert.ChangeType(-x.DotProduct(eye),typeof(T)),default(T),default(T),(T)Convert.ChangeType(1,typeof(T))},
            });

            return trans * rot;
        }

        /// <summary>
        /// Build a world space to camera space matrix
        /// </summary>
        /// <param name="eye">Eye (camera) position in world space</param>
        /// <param name="target">Target position in world space</param>
        /// <param name="up">Up vector in world space (should not be parallel to the camera direction, that is target - eye)</param>
        /// <returns>A Matrix that transforms world space to camera space</returns>
        public static Matrix<double> LookAt(Matrix<double> eye, Matrix<double> target, Matrix<double> up) {
            var test = Matrix4d.LookAt(
                eye[0, 0], eye[1, 0], eye[2, 0],
                target[0, 0], target[1, 0], target[2, 0],
                up[0, 0], up[1, 0], up[2, 0]);
            var ret = new Matrix<double>(test.toArray()).Transpose();
            //var wrld = ret.Inverted(DecompMethod.LU);
            //var wrldd = test.Inverted();
            return ret;
        }

    }
}
