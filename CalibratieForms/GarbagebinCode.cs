using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalibratieForms {
    static class GarbagebinCode {
        ///
        ///PROGRAM.CS - READING OUTPUT CAMERAS PHOTOSCAN
        /// 
        /*
         * var file = @"D:\3dtrenchview\IO formats\photoscan\garage.xml";


            List<CameraIntrinsics> intrinsics = new List<CameraIntrinsics>();
            List<PinholeCamera> cameras = new List<PinholeCamera>();

            var r = XmlReader.Create(new XmlTextReader(file),new XmlReaderSettings());

            while (r.Read()) {
                if (r.Name == "document") {

                }
                if (r.Name == "sensors") {
                    //XmlDocument sensors = new XmlDocument();
                    //sensors.LoadXml(r.ReadInnerXml());

                }
                //verschillende sensoren
                if (r.Name == "sensor" && r.NodeType == XmlNodeType.Element) {
                    intrinsics.Add(CameraIntrinsics.FromAgisoftXML(r));
                }
                if (r.Name == "camera" && r.NodeType == XmlNodeType.Element) {
                    int cameraid = int.Parse(r["id"]);
                    int sensor = int.Parse(r["sensor_id"]);
                    string label = r["label"];
                    bool enabled = bool.Parse(r["enabled"]);

                    var cam = PinholeCamera.FromXML(r, id => intrinsics.FirstOrDefault(x => x.SensorID == id));
                    cameras.Add(cam);
                }
                if (r.Name == "transform") {
                    var coll = CameraCollection.readTransfromXML(r);
                }
            }

            Dictionary<PinholeCamera, List<Marker2d>> camerasDict = new Dictionary<PinholeCamera, List<Marker2d>>();

            var matchesOrimaFile = @"D:\3dtrenchview\IO formats\photoscan\matches orima.txt";
            var f = new StreamReader(matchesOrimaFile);
            string line;
            while ((line = f.ReadLine()) != null) {
                var split = line.Split(new[] {'\n', '\t', ' '},StringSplitOptions.RemoveEmptyEntries);
                string filename = split[0];

                int markerid = int.Parse(split[1]);
                double x_mm = double.Parse(split[2], CultureInfo.InvariantCulture);
                double y_mm = double.Parse(split[3], CultureInfo.InvariantCulture);

                var camera = cameras.FirstOrDefault(x => x.Label == filename);
                if (camera == null) continue;

                if (!camerasDict.ContainsKey(camera)) {
                    camerasDict.Add(camera, new List<Marker2d>());
                }
                var list = camerasDict[camera];

                float x_px = (float)(x_mm / camera.Intrinsics.Pixel_width   + (double)camera.Intrinsics.PictureSize.Width/2);
                float y_px = (float)(-1 * y_mm / camera.Intrinsics.Pixel_height + (double)camera.Intrinsics.PictureSize.Height / 2);

                list.Add(new Marker2dTest(markerid, x_px,y_px));
            }

            foreach (KeyValuePair<PinholeCamera, List<Marker2d>> kvp in camerasDict) {
                var infile = String.Format(@"D:\thesis\opnames\Huawei p9\aruco\{0}", kvp.Key.Label);
                var outfile = infile + "agisoftmarkers.jpg";
                if (File.Exists(infile)) {
                    drawMarkers(infile, outfile,kvp.Value);
                }
            }*/
    }
}
