using System;
using OpenTK;
using SceneManager;
using Newtonsoft.Json;

namespace Calibratie {
    public class Marker : SObject {
        public int ID;

        
        public Marker(int id, Vector3d pos) {
            ID = id;
            Pos = pos;
        }
    }
}
