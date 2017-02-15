using System;
using OpenTK;
using SceneManager;
using Newtonsoft.Json;

namespace Calibratie {
    public class Marker : SObject{
        
        public Marker(int id, Vector3d pos) {
            ID = id;
            Pos = pos;
        }

        public int ID { get; private set; }
        public double X { get { return Pos.X; } }
        public double Y { get { return Pos.Y; } }
        public double Z { get { return Pos.Z; } }
    }
}
