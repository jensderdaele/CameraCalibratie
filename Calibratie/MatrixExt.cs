using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using OpenTK;


namespace Calibratie {
    public static class MatrixExt {
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
        public static Matrix<T> LookAt<T>(Matrix<T> eye, Matrix<T> target, Matrix<T> up) where T:new() {
            
            Matrix<T> z = new Matrix<T>(3, 1);
            Matrix<T> x = new Matrix<T>(3, 1);
            Matrix<T> y = new Matrix<T>(3, 1);
            CvInvoke.Normalize(eye - target, z);
            CvInvoke.Normalize(up * z, x);
            CvInvoke.Normalize(z*x, z);

            var rot = new Matrix<T>(new T[,]{
                {x[0,0],y[0,0],z[0,0],default(T)},
                {x[1,0],y[1,0],z[1,0],default(T)},
                {x[2,0],y[2,0],z[2,0],default(T)},
                {default(T),default(T),default(T),(T)Convert.ChangeType(1,typeof(T))},
            });
            var eyemin = - 1*eye;
            var trans = new Matrix<T>(new T[,]{
                {default(T),default(T),default(T),default(T)},
                {default(T),default(T),default(T),default(T)},
                {default(T),default(T),default(T),default(T)},
                {eyemin[0,0],eyemin[1,0],eyemin[2,0],(T)Convert.ChangeType(1,typeof(T))}
            });



            return trans * rot;
        }

    }
}
