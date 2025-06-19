using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib
{
    public class ProductSerialNumber
    {
        [XmlElement("SerialNumber")]
        public List<string> SerialNumbers { get; set; }
    }
} 