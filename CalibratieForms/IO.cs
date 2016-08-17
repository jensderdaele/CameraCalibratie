using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenTK;

namespace CalibratieForms {
    public static class IO {
        /// <summary>
        /// filters out all {x,y,z}
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Point3d[] readPoints(string s) {
            var matches = Regex.Matches(s, @"{[-+]?([0-9]*\.[0-9]+|[0-9]+),[-+]?([0-9]*\.[0-9]+|[0-9]+),[-+]?([0-9]*\.[0-9]+|[0-9]+)}");
            var r = new List<Point3d>();
            foreach (Match match in matches) {
                var split = match.Value.Split(new[] {'{', '}', ','}, StringSplitOptions.RemoveEmptyEntries);
                double x, y, z;
                x = Double.Parse(split[0], NumberStyles.Any, NumberFormatInfo.InvariantInfo);
                y = Double.Parse(split[1], NumberStyles.Any, NumberFormatInfo.InvariantInfo);
                z = Double.Parse(split[2], NumberStyles.Any, NumberFormatInfo.InvariantInfo);
                r.Add(new Point3d(x,y,z));
            }
            return r.ToArray();
        }

        public static Vector3d[] readVectors(string s) {
            var matches = Regex.Matches(s, @"{[-+]?([0-9]*\.[0-9]+|[0-9]+),[-+]?([0-9]*\.[0-9]+|[0-9]+),[-+]?([0-9]*\.[0-9]+|[0-9]+)}");
            var r = new List<Vector3d>();
            foreach (Match match in matches) {
                var split = match.Value.Split(new[] { '{', '}', ',' }, StringSplitOptions.RemoveEmptyEntries);
                double x, y, z;
                x = Double.Parse(split[0], NumberStyles.Any, NumberFormatInfo.InvariantInfo);
                y = Double.Parse(split[1], NumberStyles.Any, NumberFormatInfo.InvariantInfo);
                z = Double.Parse(split[2], NumberStyles.Any, NumberFormatInfo.InvariantInfo);
                r.Add(new Vector3d(x, y, z));
            }
            return r.ToArray();
        }


        public static void writePoints(this List<Point3d> pts, string file) {
            try { File.Delete(file); }
            catch { }
            var sw = File.CreateText(file);
            foreach (var point3D in pts) {
                var s = String.Format("{0} {1} {2}", point3D.X, point3D.Y, point3D.Z);
                sw.WriteLine(s.Replace(',','.'));
            }
            sw.Flush();
            sw.Close();
        }
    }
}
