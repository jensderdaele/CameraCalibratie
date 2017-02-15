using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArUcoNET;
using ceresdotnet;
using Calibratie;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using OpenTK;
using SceneManager;

namespace CalibratieForms {
    class test {
        private static void testStereoCamera() {

            Scene scene = new Scene();

            StereoImageFileProvider multiimageprovider = new StereoImageFileProvider(533, 689, 30, 533, 2500,
                2.016214371053080730500085338795,
                (i => string.Format(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\links\2\{0:00000000}.jpg", i)),
                (i => string.Format(@"C:\Users\jens\Desktop\calibratie\Opnames_thesis\rechts\2\{0:00000000}.jpg", i)));

            IMarkerDetector arucodetector = new ArucoMarkerDetector();

            IMultiCameraImageProvider prov = multiimageprovider;



            var markersscene = scene.getIE<Marker>().ToArray();


            var points = new List<Tuple<
                Tuple<string, List<Tuple<MCvPoint3D32f, PointF>>>,
                Tuple<string, List<Tuple<MCvPoint3D32f, PointF>>>>>();



            Action<IEnumerable<Tuple<Tuple<string, List<ArucoMarker>>,
                Tuple<string, List<ArucoMarker>>>>> intersect = list => {
                    foreach (var stereoPair in list) {
                        var L = stereoPair.Item1;
                        var R = stereoPair.Item2;


                        L.Item2.IntersectLists(R.Item2, ((marker, arucoMarker) => marker.ID == arucoMarker.ID));

                    }
                };
            
        }
    }
}
