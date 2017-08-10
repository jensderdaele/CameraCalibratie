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
        public static CeresCamera toCeresCamera(this PinholeCamera camera) {
            var cc = new CeresCamera(camera.Intrinsics.toCeresParameter(), camera.toCeresParameter());
            return cc;
        }

        public static PointF toPointF(this CeresMarker marker) {
            return new PointF((float)marker.x, (float)marker.y);
        }
        public static Matrix<double> toMatrix(this CeresPoint marker) {
            return new Matrix<double>(new double[]{ marker.X, marker .Y, marker .Z});
        }
        public static CeresMarker toCeresMarker(this Marker marker, double x_proj, double y_proj) {
            var cm = new CeresMarker {
                id = marker.ID,
                Location = new CeresPoint {
                    BundleFlags = BundleWorldCoordinatesFlags.None,
                    X = marker.X,
                    Y = marker.Y,
                    Z = marker.Z
                },
                x = x_proj,
                y = y_proj
            };
            return cm;
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

        [Obsolete("Projectie enkel volgens standaard distortiemodel")]
        public static PointF[] ProjectPointd2D_Manually(this PinholeCamera cam, MCvPoint3D32f[] points3d, out MCvPoint3D32f[] visible) {
            var r = new List<PointF>();
            var vis = new List<MCvPoint3D32f>();
            
            
            var transftk = cam.worldMat.Inverted();
            var transf = transftk.tocv();
            

            VectorOfPoint3D32F points3dvec = new VectorOfPoint3D32F(points3d);
            VectorOfPoint3D32F points3dvectransf = new VectorOfPoint3D32F(points3dvec.Size);
            CvInvoke.Transform(points3dvec, points3dvectransf, transf);
            

            for (int i = 0; i < points3dvec.Size; i++) {
                var camcoord = points3dvectransf[i];

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
                    vis.Add(points3dvec[i]);
                    r.Add(new PointF((float)im_x, (float)im_y));
                }

            }
            visible = vis.ToArray();
            return r.ToArray();
        }

        public static PointF[] ProjectPointd2D_Manually<T>(this PinholeCamera cam, T[] points3d, out T[] visible) where T:SPoint {
            var r = new List<PointF>();
            var vis = new List<T>();

            //var tt = cam.worldMat.Inverted();
            //var transf = tt.tocv();
            var transf = cam.WorldMat.Inverted();
            var worldmatinv = cam.worldMat.Inverted();
            
            var mcvpoints3d = points3d.Select(x => x.toMCvPoint3D32f());

            VectorOfPoint3D32F points3dvec = new VectorOfPoint3D32F(mcvpoints3d.ToArray());
            //VectorOfPoint3D32F points3dvectransf = new VectorOfPoint3D32F(points3dvec.Size);


            var camcoords = new Matrix<double>(3, points3d.Length);
            
            var phm_mat = new Mat();
            var camcoords_mat = new Mat();
            var camcoords_mat2 = new VectorOfPoint3D32F();
            CvInvoke.ConvertPointsToHomogeneous(points3dvec, phm_mat);

            //var phm = new Matrix<float>(4, points3d.Length, phm_mat.DataPointer);
            //var camcoord = cam.WorldMat.Inverted() * phm;
            CvInvoke.Transform(phm_mat, camcoords_mat, transf);
            CvInvoke.ConvertPointsFromHomogeneous(camcoords_mat, camcoords_mat2);
            

            var cc = cam.toCeresCamera();
            for (int i = 0; i < camcoords_mat2.Size; i++) {
                var phmm = new Matrix<double>(3, 1);

                var camcoord = Vector3d.TransformPerspective(new Vector3d(points3d[i].X, points3d[i].Y, points3d[i].Z), worldmatinv);
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

                var im_x2 = cam.Intrinsics.fx * xd + cam.Intrinsics.cx;
                var im_y2 = cam.Intrinsics.fy * yd + cam.Intrinsics.cy;

                if (camcoord.Z < 0){//camcoords_mat2[i].Z < 0) {
                    continue;
                }

                var pointf = ceresdotnet.CeresTestFunctions.ProjectPoint(cc.Internal, cc.External, points3d[i].Pos);

                var im_x = pointf.X;
                var im_y = pointf.Y;

                if (im_x >= 0 && im_x <= cam.PictureSize.Width && im_y >= 0 && im_y <= cam.PictureSize.Height) {
                    vis.Add(points3d[i]);
                    r.Add(pointf);
                }
                if (im_x2 >= 0 && im_x2 <= cam.PictureSize.Width && im_y2 >= 0 && im_y2 <= cam.PictureSize.Height) {
                    //vis.Add(points3d[i]);
                    //pointf.X = (float)im_x2;
                    //pointf.Y = (float)im_y2;
                    //r.Add(pointf);
                }

            }
            visible = vis.ToArray();
            return r.ToArray();
        }
    }
}
