using System;
using System.Xml.Serialization;

namespace SAFT.Lib
{
    public class OrderReferences
    {
        [XmlElement]
        public string OriginatingON { get; set; }
        [XmlElement]
        public DateTime? OrderDate { get; set; }
    }
} 