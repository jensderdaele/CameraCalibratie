using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Gpu;

namespace CalibratieForms {
    public static class Cv2e {
        public static void ProjectPoints(IEnumerable<Point3d> points, double[] rvec, double[] tvec, double[,] cameramat, double[]distcoeffs, out Point2d[] imagePoints, out double[,] jacob) {
            if (points.Count()%2 == 1) {
                var nlist = points.ToList();
                nlist.Add(new Point3d(0, 0, 0));
                Point2d[] o;
                Cv2.ProjectPoints(nlist, rvec, tvec, cameramat, distcoeffs, out o, out jacob);
                var im = o.ToList();
                im.RemoveAt(im.Count-1);
                imagePoints = im.ToArray();
                return;
            }
            Cv2.ProjectPoints(points, rvec, tvec, cameramat, distcoeffs, out imagePoints, out jacob);
        }
        /// <summary>
        /// not tested
        /// </summary>
        /// <param name="cvMat"></param>
        /// <returns></returns>
        public static Image GetImage(this Mat cvMat) {
            var buffer = cvMat.ImEncode(".png", new ImageEncodingParam(ImwriteFlags.PngCompression, 0));
            Image r;
            using (MemoryStream m = new MemoryStream(buffer)) {
                r = Image.FromStream(m);
            }
            return r;
        }
        /// <summary>
        /// USE Cv.Imread if possible
        /// </summary>
        /// <returns></returns>
        public static Mat GetCvMatColor(this Image image) {
            using (var ms = new MemoryStream()) {
                image.Save(ms, image.RawFormat);
                return Cv2.ImDecode(ms.ToArray(), ImreadModes.Color);
            }
        }/// <summary>
        /// USE Cv.Imread if possible
        /// </summary>
        /// <returns></returns>
        public static Mat GetCvMatGray(this Image image) {
            using (var ms = new MemoryStream()) {
                image.Save(ms, image.RawFormat);
                return Cv2.ImDecode(ms.ToArray(), ImreadModes.GrayScale);
            }
        }
    }
}
