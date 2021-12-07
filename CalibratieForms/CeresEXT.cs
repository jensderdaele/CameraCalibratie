using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ceresdotnet;
using Calibratie;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using OpenTK;
using Point2d = Emgu.CV.Structure.MCvPoint2D64f;

namespace CalibratieForms {
    public static class CeresEXT {


        public static PointF toPointF(this CeresMarker marker) {
            return new PointF((float)marker.x, (float)marker.y);
        }
        public static Matrix<double> toMatrix(this CeresPoint marker) {
            return new Matrix<double>(new double[]{ marker.X, marker .Y, marker .Z});
        }


        public static Point2d[] ProjectPointd2D_Manually(this PinholeCamera cam, Vector3d[] points3d, out Vector3d[] visible) {

            var r = new List<Point2d>();
            var vis = new List<Vector3d>();



            var transf = cam.worldMat.Inverted();

            foreach (var vector3D in points3d) {
                var camcoord = Vector3d.TransformPerspective(vector3D, transf);

                if (camcoord.Z < 0) {
                    continue;
                }


                var x = camcoord.X / camcoord.Z;
                var y = camcoord.Y / camcoord.Z;

                var r2 = x * x + y * y;
                var r4 = r2 * r2;
                var r6 = r4 * r2;
                var r_coeff = ((1) + cam.DistortionR1 * r2 + cam.DistortionR2 * r4 + cam.DistortionR3 * r6);
                var tdistx = 2 * cam.DistortionT1 * x * y + cam.DistortionT2 * (r2 + 2 * x * x);
                var tdisty = 2 * cam.DistortionT2 * x * y + cam.DistortionT1 * (r2 + 2 * y * y);
                var xd = x * r_coeff + tdistx;
                var yd = y * r_coeff + tdisty;

                var im_x = cam.Intrinsics.fx * xd + cam.Intrinsics.cx;
                var im_y = cam.Intrinsics.fy * yd + cam.Intrinsics.cy;

                if (im_x >= 0 && im_x <= cam.PictureSize.Width && im_y >= 0 && im_y <= cam.PictureSize.Height) {
                    vis.Add(vector3D);
                    r.Add(new Point2d(im_x, im_y));
                }
                /*
               var test = ceresdotnet.CeresCameraCollectionBundler.testProjectPoint(
                   cam.toCeresCamera(), 
                   new CeresPointOrient() {
                       RT = new[] { axis.X, axis.Y, axis.Z, trans.X, trans.Y, trans .Z}
                   },
                    new CeresMarker2() {
                        Location = new CeresPoint2() {
                            X = vector3D.X,
                            Y = vector3D.Y,
                            Z = vector3D.Z
                        },
                        x = im_x,
                        y = im_y
                    });*/
                

            }
            visible = vis.ToArray();
            return r.ToArray();
        }
        
       
    }
}
