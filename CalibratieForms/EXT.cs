using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ArUcoNET;
using ceresdotnet;
using Calibratie;
using CalibratieForms.Properties;
using OpenCvSharp;
using OpenTK;

namespace CalibratieForms {
    public static partial class EXT {
        public static Dictionary<string, IEnumerable<ArucoMarker>> getArucoMarkers(IEnumerable<string> files) {
            return files.ToDictionary(f => f, ArUcoNET.Aruco.FindMarkers);
        } 
        public static Point2d to2d(this Point2f p) {
            return new Point2d(p.X,p.Y);
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

        /// <summary>
        /// CERESMARKERID = ARUCOID + ArucoMarkerFlags!
        /// GEBRUIK .first() voor 1 corner (LB)
        /// </summary>
        /// <param name="armarker"></param>
        /// <param name="imageID"></param>
        /// <returns></returns>
        public static IEnumerable<CeresMarker> getCeresMarkers(this ArUcoNET.ArucoMarker armarker,int imageID,CeresCamera parentCamera = null) {
            throw new NotImplementedException();
            /*
            yield return new CeresMarker(imageID, armarker.ID, armarker.Corner1.X, armarker.Corner1.Y) { parentCamera = parentCamera };
            yield return new CeresMarker(imageID, armarker.ID | (int)ArucoMarkerFlags.Corner2, armarker.Corner2.X, armarker.Corner2.Y) { parentCamera = parentCamera };
            yield return new CeresMarker(imageID, armarker.ID | (int)ArucoMarkerFlags.Corner3, armarker.Corner3.X, armarker.Corner3.Y) { parentCamera = parentCamera };
        */} 

        public static double[] toArr(this Vector4d v) {
            return new[] { v.X, v.Y, v.Z, v.W };
        }
        public static double[] toArr(this Vector3d v) {
            return new[] { v.X, v.Y, v.Z };
        }
        public static double[] toCeresIntrinsics9(this PinholeCamera c) {
            double[] intrinsics = {
                c.CameraMatrix.fx,
                c.CameraMatrix.fy,
                c.CameraMatrix.cx,
                c.CameraMatrix.cy,
                c.DistortionR1,
                c.DistortionR2,
                c.DistortionR3,
                c.DistortionT1,
                c.DistortionT2,
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

        public static unsafe double[,] toArr(this Matrix3d mat) {
            double[,] ret = new double[3, 3];
            fixed (double* retp = ret) {
                CopyMemory(retp, &mat, 9*8);
            }
            return ret;
        }

        public static unsafe double[,] toArr(this Matrix3x4d mat) {
            double[,] ret = new double[3, 4];
            fixed (double* retp = ret) {
                CopyMemory(retp, &mat, 3*4 * 8);
            }
            return ret;
        }
        
    }
}
