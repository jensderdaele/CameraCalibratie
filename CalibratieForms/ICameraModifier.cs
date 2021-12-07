using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ceresdotnet;
using Calibratie;

namespace CalibratieForms {
    public interface ICameraModifier {
        void Apply(PinholeCamera camera,CeresCamera cc);
        void Apply(CameraIntrinsics intr);
    }

    /// <summary>
    /// zet dist = 0 | ruis op positie camera | pp midden sensor
    /// </summary>
    public class CameraModifier : ICameraModifier {
        public I3DRuisProvider PosRuis;
        public void Apply(PinholeCamera camera, CeresCamera cc) {
            PosRuis?.Apply(ref cc.External.Pos_paramblock[0], ref cc.External.Pos_paramblock[1], ref cc.External.Pos_paramblock[2]);
            camera.Intrinsics.fx *= .95;
            camera.Intrinsics.fy *= .95;

            camera.Intrinsics.cx = camera.Intrinsics.PictureSize.Width / 2;
            camera.Intrinsics.cy = camera.Intrinsics.PictureSize.Height / 2;

            camera.Intrinsics.SetDistortionsZero();
        }

        public void Apply(CameraIntrinsics intr) {
            intr.fx *= .95;
            intr.fy *= .95;

            intr.cx = intr.PictureSize.Width / 2;
            intr.cy = intr.PictureSize.Height / 2;

            intr.SetDistortionsZero();
        }
    }
}
