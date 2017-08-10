using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Calibratie {
    public static class EXT {

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
    }
}
