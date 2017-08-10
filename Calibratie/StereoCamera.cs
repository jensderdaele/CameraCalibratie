using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calibratie {
    public class StereoCamera : CameraCollection {
        public StereoCamera() : base(2) {}

        public PinholeCamera LeftCamera {
            get {
                return base[0];
            }
            set { _cameras[0] = value; }
        }
        public PinholeCamera RightCamera {
            get {
                return base[1];
            }
            set { _cameras[1] = value; }
        }

    }
}
