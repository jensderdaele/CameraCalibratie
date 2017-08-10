using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Emgu.CV;
using Emgu.CV.CvEnum;
using OpenTK;

namespace Calibratie {
    public static class EXT {
        public static void writeProperty(this XmlWriter writer, string name, string value) {
            writer.WriteName("property");
            writer.WriteAttributeString("name", name);
            writer.WriteAttributeString("value", value);
            writer.WriteEndElement();
        }
        public static void UpdateMatrixBetweenName(this XmlReader r, Matrix<double> mat) {
            mat.ReadString(r.ReadValueBetweenName());
        }
        public static Matrix<double> ReadMatrixBetweenName(this XmlReader r, int rows, int cols) {
            var mat = new Matrix<double>(rows, cols);
            mat.ReadString(r.ReadValueBetweenName());
            return mat;
        } 
         
        public static string ReadValueBetweenName(this XmlReader r) {
            r.Read();
            var ret = r.Value;
            //r.Read();
            return ret;
        }
        public static Matrix<T> Inverted<T>(this Matrix<T> mat) where T : new() {
            return mat.Inverted(DecompMethod.LU);
        }
        public static Matrix<T> Inverted<T>(this Matrix<T> mat,DecompMethod decompmeth) where T : new() {
            Matrix<T> r = new Matrix<T>(mat.Rows, mat.Cols);
            CvInvoke.Invert(mat, r, decompmeth);
            return r;
        }
        public static double[,] toArray(this Matrix4d obj) {
            return new double[,] {
                {obj.M11, obj.M12, obj.M13, obj.M14},
                {obj.M21,obj.M22,obj.M23,obj.M24},
                {obj.M31,obj.M32,obj.M33,obj.M34},
                {obj.M41,obj.M42,obj.M43,obj.M44}
            };
        }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static unsafe extern void CopyMemory(void* dest, void* src, uint count);

        public static unsafe double[,] toArr(this Matrix3d mat) {
            double[,] ret = new double[3, 3];
            fixed (double* retp = ret) {
                CopyMemory(retp, &mat, 9 * 8);
            }
            return ret;
        }

        public static unsafe double[,] toArr(this Matrix3x4d mat) {
            double[,] ret = new double[3, 4];
            fixed (double* retp = ret) {
                CopyMemory(retp, &mat, 3 * 4 * 8);
            }
            return ret;
        }
        public static double[] toArr(this Vector4d v) {
            return new[] { v.X, v.Y, v.Z, v.W };
        }
        public static double[] toArr(this Vector3d v) {
            return new[] { v.X, v.Y, v.Z };
        }

        public static TResult AddMember<TResult, T>(this IEnumerable<T> ie,  Func<T, TResult> selector,Func<TResult, TResult, TResult> add) {
            var r = default(TResult);
            foreach (T item in ie) {
                r = add(r, selector(item));
            }
            return r;
        }
    }
}
