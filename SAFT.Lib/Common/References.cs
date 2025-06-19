using System.Xml.Serialization;

namespace SAFT.Lib
{
    public class References
    {
        [XmlElement]
        public string Reference { get; set; }
        [XmlElement]
        public string Reason { get; set; }
    }
} 