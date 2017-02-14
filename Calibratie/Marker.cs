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
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }
    }
}
