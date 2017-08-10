using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Emgu.CV;
using Emgu.CV.Flann;

namespace Calibratie {
    /// <summary>
    /// bounding box (Agisoft xml)
    /// </summary>
    public class Bbox : SObject,IXmlSerializable {
        public Matrix<double> Size;

        public new void ReadXml(XmlReader reader) {
            if (reader.Name == "region" & reader.NodeType == XmlNodeType.Element) {
                while (reader.Read()) {
                    switch (reader.Name) {
                        case "region":
                            return;
                            break;
                        case "center":
                            if (reader.NodeType == XmlNodeType.Element) {
                                reader.Read();
                                base.Pos.ReadString(reader.Value);
                            }
                            break;
                        case "size":
                            if (reader.NodeType == XmlNodeType.Element) {
                                reader.Read();
                                Size.ReadString(reader.Value);
                            }
                            break;
                        case "R":
                            if (reader.NodeType == XmlNodeType.Element) {
                                reader.Read();
                                base.RotationMat.ReadString(reader.Value);
                            }
                            break;
                    }
                }
            }
        }

        public new void WriteXml(XmlWriter writer) {
            writer.WriteName("region");

            writer.WriteName("center");
            writer.WriteValue(base.Pos.ToXMLValueString());
            writer.WriteEndElement();

            writer.WriteName("size");
            writer.WriteValue(Size.ToXMLValueString());
            writer.WriteEndElement();

            writer.WriteName("R");
            writer.WriteValue(base.RotationMat.ToXMLValueString());
            writer.WriteEndElement();

            writer.WriteEndElement();
        }

    }
}
