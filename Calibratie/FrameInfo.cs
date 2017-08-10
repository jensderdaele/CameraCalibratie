using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Calibratie {

    /// <summary>
    /// gebruikt voor Agisoft Project
    /// </summary>
    public class FrameInfo : IXmlSerializable {
        public string PhotoPath;
        public int CameraID;

        private Dictionary<string, string> Properties = new Dictionary<string, string>(); 
        public XmlSchema GetSchema() {
            return null;
        }

        public void ReadXml(XmlReader reader) {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer) {
            throw new NotImplementedException();
        }
    }
}
