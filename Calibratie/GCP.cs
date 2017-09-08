using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Calibratie {
    /// <summary>
    /// Voorlopig voor Agisoft xml
    /// </summary>
    public class GCP : SPoint, IXmlSerializable {
        public static GCP FromXML(XmlReader reader) {
            var r = new GCP();
            r.ReadXml(reader);
            return r;
        }
        public bool Enabled;
        private string _label;

        /// <summary>
        /// fout in xyz
        /// </summary>
        public double sxyz = -1;

        public string Label {
            get {
                if (string.IsNullOrEmpty(_label)) {
                    _label = string.Format("GCP_{0}", Id);
                } 
                return _label;
            }
        }

        public int Id;
        
        public XmlSchema GetSchema() {
            return null;
        }
        /// <summary>
        /// only reader.Name == "marker"
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(XmlReader reader) {
            if (reader.Name != "marker")
                throw new Exception();

            Id = int.Parse(reader["id"]);
            this._label = reader["label"];

            reader.Read();
            reader.Read();
            if (reader.Name == "reference") {
                X = double.Parse(reader["x"], CultureInfo.InvariantCulture);
                Y = double.Parse(reader["y"], CultureInfo.InvariantCulture);
                Z = double.Parse(reader["z"], CultureInfo.InvariantCulture);
                Enabled = bool.Parse(reader["enabled"]);
            }
            while (reader.Read()) {
                if (reader.Name == "marker" && reader.NodeType == XmlNodeType.EndElement) {
                    return;
                }
            }

        }

        public void WriteXml(XmlWriter writer) {
            writer.WriteName("marker");
            writer.WriteAttributeString("label", Label);

            writer.WriteName("reference");
            writer.WriteAttributeString("x", X.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("y", Y.ToString(CultureInfo.InvariantCulture));
            writer.WriteAttributeString("z", Z.ToString(CultureInfo.InvariantCulture));
            if (sxyz >= 0) {
                writer.WriteAttributeString("sxyz", sxyz.ToString(CultureInfo.InvariantCulture));
            }
            writer.WriteAttributeString("enabled", Enabled.ToString());

            writer.WriteEndElement();
        }
    }
}
