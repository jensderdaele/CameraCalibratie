using System;
using ceresdotnet;
using Emgu.CV;
using OpenTK;
using SceneManager;
using Newtonsoft.Json;

namespace Calibratie {
    public class Marker : SObject, ICeresParameterConvertable<CeresPoint>, IMarker3d {
        
        public Marker(int id, Vector3d pos) {
            ID = id;
            Pos[0, 0] = pos.X;
            Pos[1, 0] = pos.Y;
            Pos[2, 0] = pos.Z;
        }

        public int ID { get; private set; }
        public double X { get { return Pos[0,0]; } }
        public double Y { get { return Pos[1, 0]; } }
        public double Z { get { return Pos[2, 0]; } }


        public CeresPoint toCeresParameter() {
            return new CeresPoint() {X = X,Y = Y,Z = Z};
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
