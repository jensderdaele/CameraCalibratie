using System;
using System.Collections.Generic;
using ceresdotnet;
using Emgu.CV;
using OpenTK;
using Newtonsoft.Json;

namespace Calibratie {
    public class MarkerIDComparer : IComparer<IMarker3d> {
        public int Compare(IMarker3d x, IMarker3d y) {
            if (x.ID > y.ID)
                return 1;
            if (x.ID < y.ID)
                return -1;
            else
                return 0;
        }
    }
    public class Marker : SPoint, ICeresParameterConvertable<CeresPoint>, IMarker3d {

        public Marker() : this(-1,0,0,0) {

        }

        public Marker(int id, Vector3d pos) {
            ID = id;
            Pos[0, 0] = pos.X;
            Pos[1, 0] = pos.Y;
            Pos[2, 0] = pos.Z;
        }
        public Marker(int id, double x, double y, double z) {
            ID = id;
            Pos[0, 0] = x;
            Pos[1, 0] = y;
            Pos[2, 0] = z;
        }

        public int ID { get; private set; }
        public double X { get { return Pos[0, 0]; } }
        public double Y { get { return Pos[1, 0]; } }
        public double Z { get { return Pos[2, 0]; } }

        private CeresPoint _cerespoint = null;
        public CeresPoint toCeresParameter() {
            if (_cerespoint == null) {
                _cerespoint = new CeresPoint() {X = X, Y = Y, Z = Z};
            }
            else {
                _cerespoint.X = X;
                _cerespoint.Y = Y;
                _cerespoint.Z = Z;

            }
            return _cerespoint;
        }
        public CeresPoint toCeresParameter(Enum BundleSettings) {
            var r = toCeresParameter();
            r.BundleFlags = (BundleWorldCoordinatesFlags)BundleSettings;
            return r;
        }
        public void updateFromCeres(CeresPoint paramblock) {
            Pos[0, 0] = paramblock.X;
            Pos[1, 0] = paramblock.Y;
            Pos[2, 0] = paramblock.Z;
        }
    }
}
