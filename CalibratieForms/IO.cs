using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ArUcoNET;
using Calibratie;
using OpenTK;

using Point3d = Emgu.CV.Structure.MCvPoint3D64f;

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

        public static void MarkersToFile(IEnumerable<Marker> markers, string file) {
            var stream = File.Create(file);
            StreamWriter writer = new StreamWriter(stream);
            foreach (var marker in markers) {
                writer.WriteLine("{0},{1},{2},{3}", marker.ID, marker.Pos.X, marker.Pos.Y, marker.Pos.Z);
            }
        }
        public static void MarkersToFile(IEnumerable<ArucoMarker> markers, string file) {
            var stream = File.Create(file);
            StreamWriter writer = new StreamWriter(stream);
            foreach (var marker in markers) {
                writer.WriteLine("{0},{1},{2}", marker.ID, marker.Corner1.X, marker.Corner1.Y);
            }
            writer.Flush();
            stream.Flush();
            writer.Close();
            stream.Close();
        }
        public static void MarkersToFile(PointF[] markers,int[] ids, string file) {
            var stream = File.Create(file);
            StreamWriter writer = new StreamWriter(stream);
            for (int i = 0; i < markers.Length; i++) {
                writer.WriteLine("{0},{1},{2}", ids[i], markers[i].X, markers[i].Y);
                
            }
            writer.Flush();
            stream.Flush();
            writer.Close();
            stream.Close();
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

        public static List<Marker> MarkersFromFile(string file) {
            var r = new List<Marker>();
            var stream = new FileStream(file, FileMode.Open);
            var reader = new StreamReader(stream, Encoding.UTF8);
            var txt = reader.ReadToEnd();
            var split = txt.Split(new[] {',','\n','\t','\r'}, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < split.Length; i++) {
                int id;
                if (!int.TryParse(split[i++], out id)) {
                    i += 2;
                    continue;
                }
                double x, y, z;
                if (double.TryParse(split[i], NumberStyles.AllowDecimalPoint,NumberFormatInfo.InvariantInfo, out x)
                    && double.TryParse(split[i + 1], NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out y) &&
                    double.TryParse(split[i + 2], NumberStyles.AllowDecimalPoint, NumberFormatInfo.InvariantInfo, out z)) {
                    r.Add(new Marker(id, new Vector3d(x, y, z)));
                    i += 2;
                }
                else {
                    i += 2;
                    continue;
                }
            }
            return r;
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
