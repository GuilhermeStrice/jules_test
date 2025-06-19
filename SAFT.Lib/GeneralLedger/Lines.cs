using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib.GeneralLedger
{
    public class Lines
    {
        [XmlElement("DebitLine")]
        public List<Line> DebitLines { get; set; }
        [XmlElement("CreditLine")]
        public List<Line> CreditLines { get; set; }
    }
} 