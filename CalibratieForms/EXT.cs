using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ArUcoNET;
using ceresdotnet;
using Calibratie;
using CalibratieForms.Properties;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using OpenTK;

namespace CalibratieForms {
    public class EqualityComparer<T> : IEqualityComparer<T> {
        public EqualityComparer(Func<T, T, bool> cmp) {
            this.cmp = cmp;
        }
        public bool Equals(T x, T y) {
            return cmp(x, y);
        }

        public int GetHashCode(T obj) {
            return obj.GetHashCode();
        }

        public Func<T, T, bool> cmp { get; set; }
    }
    public static partial class EXT {


        public static Dictionary<T, T> getIntersection<T>(this IEnumerable<T> list1, IEnumerable<T> list2,
            IEqualityComparer<T> comparer,int maxItems = 0) {
            var r = new Dictionary<T,T>();
            var enumerable = list2 as T[] ?? list2.ToArray();
            int c = 0;
            foreach (var obj1 in list1) {
                foreach (var obj2 in enumerable) {
                    if (comparer.Equals(obj1, obj2)) {
                        r.Add(obj1, obj2);
                        if (c++ >= maxItems && maxItems != 0) { return r; }
                    }
                }
            }
            return r;
        }


        public static IEnumerable<Tuple<T, T2>> ToTupleList<T, T2>(this Dictionary<T, T2> dict) {
            return dict.Select(kvp => new Tuple<T, T2>(kvp.Key,kvp.Value));
        }

        /// <summary>
        /// Selecteert overeenkomstige data in lists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <param name="selector"></param>
        /// <returns></returns>
        public static Dictionary<T,T> selectSame<T>(this IEnumerable<T> list1, IEnumerable<T> list2, Func<T,T,bool> selector ) {
            var r = new Dictionary<T,T>();

            var l1enum = list1.GetEnumerator();
            while (l1enum.MoveNext()) {
                var l2enum = list2.GetEnumerator();
                l2enum.Reset();
                while (l2enum.MoveNext()) {
                    if (selector(l1enum.Current, l2enum.Current)) {
                        r.Add(l1enum.Current, l2enum.Current);
                    }
                }
            }

            return r;
        }

        /// <summary>
        /// 
        /// </summary>
        [Flags]
        public enum ArucoMarkerFlags : int{
            Corner2 = 0x4000,
            Corner3 = 0x8000
        }

        public static void IntersectLists<T>(this List<T> list1, List<T> list2,
           Func<T, T, bool> compare) {
            List<T> remove1 = new List<T>(), remove2 = new List<T>();
            foreach (T t1 in list1) {
                foreach (T t2 in list2) {
                    if (!compare(t1, t2)) {
                        remove1.Add(t1);
                        remove2.Add(t2);
                    }
                }
            }
            foreach (var V in remove1) {
                list1.Remove(V);
            }
            foreach (var V in remove2) {
                list2.Remove(V);
            }
        }

        public static Matrix<float> toMat(this PointF[] arr) {
            VectorOfPoint3D32F t;
            var r = new Matrix<float>(new[] { arr[0].X, arr[0].Y }).Transpose();
            for (int i = 1; i < arr.Length; i++) {
                r = r.ConcateVertical(new Matrix<float>(new[] { arr[i].X, arr[i].Y }).Transpose());
            }
            return r;
        }
        public static Matrix<float> toMat(this MCvPoint3D32f[] arr) {
            VectorOfPoint3D32F t;
            var r = new Matrix<float>(new[] { arr[0].X, arr[0].Y, arr[0].Z }).Transpose();
            for (int i = 1; i < arr.Length; i++) {
                r = r.ConcateVertical(new Matrix<float>(new[] { arr[i].X, arr[i].Y, arr[i].Y }).Transpose());
            }
            return r;
        } 

        public static Matrix<double> tocv(this Matrix4d tk) {
            return new Matrix<double>(tk.toArray());
        }

        public static float[] toArr(this MCvPoint3D32f v) {
            return new[] { v.X, v.Y, v.Z };
        }
        public static double[] toArrD(this MCvPoint3D32f v) {
            return new double [] { v.X, v.Y, v.Z };
        }
        public static double[] toArr(this Vector4d v) {
            return new[] { v.X, v.Y, v.Z, v.W };
        }
        public static double[] toArr(this Vector3d v) {
            return new[] { v.X, v.Y, v.Z };
        }
        public static double[] toArr(this MCvPoint3D64f v) {
            return new[] { v.X, v.Y, v.Z };
        }

        public static Matrix<double> toMatrixD(this PointF p) {
            return new Matrix<double>(p.toArrD());
        }
        public static Matrix<float> toMatrixF(this PointF p) {
            return new Matrix<float>(p.toArrF());
        }
        public static double[] toArrD(this PointF p) {
            return new double[] { p.X, p.Y };
        }
        public static float[] toArrF(this PointF p) {
            return new [] { p.X, p.Y };
        } 
        public static double[] toCeresIntrinsics9(this PinholeCamera c) {
            double[] intrinsics = {
                c.Intrinsics.fx,
                c.Intrinsics.fy,
                c.Intrinsics.cx,
                c.Intrinsics.cy,
                c.DistortionR1,
                c.DistortionR2,
                c.DistortionT1,
                c.DistortionT2,
                c.DistortionR3,
            };
            return intrinsics;
            
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

        public static double[] toEuler(this Quaterniond q) {
            var heading = Math.Atan2(2 * q.Y * q.W - 2 * q.X * q.Z, 1 - 2 * q.Y * q.Y - 2 * q.Z * q.Z);
            var attitude = Math.Asin(2*q.X*q.Y + 2*q.Z*q.W);
            var bank = Math.Atan2(2 * q.X * q.W - 2 * q.Y * q.Z, 1 - 2 * q.X*q.X - 2 * q.Z*q.Z);
            return new[] {attitude,heading, bank};
        }

    }
}
