using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Calibratie;

namespace CalibratieForms {
    public abstract class Calibration {
        public Scene Scene;
        public List<CameraIntrinsics> AllIntrinsics;

        public void readObservations(ICameraImageProvider provider) {
            
        }

        public void loadMarkersToScene(string file) {
            Scene.AddRange(IO.MarkersFromFile(file));
        }

        public void AddObservation(IObservation obs) {
            if (AllIntrinsics.Contains(obs.Camera.Intrinsics)) {
                
            }
        }
    }
}
