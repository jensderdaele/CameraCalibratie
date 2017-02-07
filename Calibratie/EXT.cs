using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenTK;

namespace Calibratie {
    public static class EXT {
        public static double[,] toArray(this Matrix4d obj) {
            return new double[,] {
                {obj.M11, obj.M12, obj.M13, obj.M14},
                {obj.M21,obj.M22,obj.M23,obj.M24},
                {obj.M31,obj.M32,obj.M33,obj.M34},
                //{obj.M41,obj.M42,obj.M43,obj.M44}
            };
        }

        public static Mat tocvmat(this Matrix4d o) {
            return new MatOfDouble(4,4,o.toArray());
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
