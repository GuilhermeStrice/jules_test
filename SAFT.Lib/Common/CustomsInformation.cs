using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib
{
    public class CustomsInformation
    {
        [XmlElement("ARCNo")]
        public List<string> ARCNo { get; set; }
        [XmlElement]
        public decimal? IECAmount { get; set; }
    }
} 