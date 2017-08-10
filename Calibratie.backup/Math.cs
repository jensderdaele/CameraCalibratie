using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace Calibratie {
    public static class Math {
        public static void  projectPoints(IEnumerable<Point3d> objectPoints, double[] rvec_rodrigues, double[] tvec, double[] cameraIntrinsics) {
            Cv2.ProjectPoints();
        }
    }
}
