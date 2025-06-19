using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib
{
    public class CustomsDetails
    {
        [XmlElement("CNCode")]
        public List<string> CNCodes { get; set; }
        [XmlElement("UNNumber")]
        public List<string> UNNumbers { get; set; }
    }
} 