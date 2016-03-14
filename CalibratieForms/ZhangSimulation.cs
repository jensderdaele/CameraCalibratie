﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using cameracallibratie;
using OpenCvSharp;
using OpenTK;
using SceneManager;

namespace CalibratieForms {
    public class ZhangSimulation {

        //todo: remove projection funcs & add in PinholeCamera

        #region ctors
        public static ZhangSimulation CreateSimulation(PinholeCamera c,ChessBoard board,int pictureCount, 
            Func<int,double[]> distanceGenerator ,
            Func<int,double[]> Angles) {

            ZhangSimulation simulation = new ZhangSimulation();

            var distances = distanceGenerator(pictureCount);
            var angles = Angles(pictureCount);
            Random r = new Random();
            var randomvec = new Vector3d(r.NextDouble(),r.NextDouble(),r.NextDouble()).Normalized();
            for (int i = 0; i < pictureCount; i++) {
                var pos = c.Pos + c.Dir*distances[i];
                var q1 = Quaterniond.FromAxisAngle(Vector3d.Cross(c.Dir, randomvec), angles[i]);
                var q2 = Quaterniond.FromAxisAngle(c.Dir, r.NextDouble()*Math.PI*2);
                var q = q1 + q2; //MAG DIT?
                var b = new ChessBoard();
                b.ChessboardSize = board.ChessboardSize;
                b.SquareSizemm = board.SquareSizemm;
                b.Pos = pos;
                b.Orient(q);
                simulation.Chessboards.Add(b);
            }


            
            simulation.Camera = c;
            
            return simulation;
        }
#endregion

        #region props
        //Before calc
        protected List<ChessBoard> _chessboards = new List<ChessBoard>();
        public List<ChessBoard> Chessboards { get { return _chessboards; } }
        public PinholeCamera Camera { get; set; }
        //After calc
        public double AvgReprojectionError { get; private set; }
        public PinholeCamera CalibratedCamera { get; private set; }
        public Vec3d[] Calibratedrvecs { get; private set; }
        public Vec3d[] Calibratedtvecs { get; private set; }

        public bool Solved { get { return CalibratedCamera != null; } }

        #endregion

        public Vector2[] Calc2DProjection(ChessBoard b) {

            CreateSimulation(Camera, Chessboards[0], 12,
                length => MathNet.Numerics.Generate.UniformMap(length, dist => 30 + dist*5),
                length => MathNet.Numerics.Generate.UniformMap(length, angle => (angle-.5) * Math.PI));

            Vector2[] corners;
            get2DProjection_OpenCv(b, out corners);
            return corners;

            
        }
        
        public double calcMeanDist() {
            return Chessboards.Sum(chessboard => (chessboard.Pos - Camera.Pos).Length/Chessboards.Count);
        }

        private IEnumerable<IEnumerable<Point3f>> CvWorldChessPointsf {
            get {
                return _chessboards.Select(chessboard => chessboard.boardWorldCoordinates.Select(x => new Point3f((float)x.X, (float)x.Y, (float)x.Z)));
            }
        }

        public void calculateCv2() {
            Log.WriteLine("zhang simulatie berekenen start");
            List<List<Point2f>> imagePoints = new List<List<Point2f>>();
            foreach (var chessboard in Chessboards) {
                Point2f[] projected;
                get2DProjection_OpenCv(chessboard, out projected);
                imagePoints.Add(projected.ToList());
            }
            Vec3d[] rvecs,tvecs;

            CalibratedCamera = new PinholeCamera();
            CalibratedCamera.PictureSize = Camera.PictureSize;
            Log.WriteLine("Cv2.CalibrateCamera");
            Cv2.CalibrateCamera(CvWorldChessPointsf, imagePoints, Camera.PictureSize, CalibratedCamera.CameraMatrix.Mat,
                CalibratedCamera.Cv_DistCoeffs5, out rvecs, out tvecs);
            Calibratedrvecs = rvecs;
            Calibratedtvecs = tvecs;
            Log.WriteLine("zhang simulatie berekenen einde");
        }

        #region 2dprojection
        private void get2DProjection_OpenCv(ChessBoard b, out Point2f[] projected) {
            Point2d[] r;
            get2DProjection_OpenCv(b, out r);
            projected = r.Select(x => new Point2f((float)x.X, (float)x.Y)).ToArray();
        }
        private void get2DProjection_OpenCv(ChessBoard b, out Point2d[] projected) {
            //Todo: Use InputArray on vector3d[] & pin GC
            double[,] jabobian;
            Cv2.ProjectPoints(b.boardWorldCoordinated_Cv, Camera.Cv_rvecs, Camera.Cv_tvecs, Camera.CameraMatrix.Mat, Camera.Cv_DistCoeffs5, out projected, out jabobian);
        }
        private void get2DProjection_OpenCv(ChessBoard b, out Vector2[] projected) {
            Point2d[] r;
            get2DProjection_OpenCv(b, out r);
            projected = r.Select(x => new Vector2((float)x.X, (float)x.Y)).ToArray();
        }
        #endregion

        #region unused

        private IEnumerable<IEnumerable<Vector3d>> worldChessPoints {
            get {
                return _chessboards.Select(chessboard => chessboard.boardWorldCoordinates);
            }
        }
        private IEnumerable<IEnumerable<Point3d>> CvWorldChessPoints {
            get {
                return _chessboards.Select(chessboard => chessboard.boardWorldCoordinates.Select(x => new Point3d(x.X, x.Y, x.Z)));
            }
        }
        #endregion

    }
}