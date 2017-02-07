using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ceresdotnet;
using OpenCvSharp;
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
