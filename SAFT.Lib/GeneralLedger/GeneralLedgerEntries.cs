using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib.GeneralLedger
{
    public class GeneralLedgerEntries
    {
        [XmlElement]
        public int NumberOfEntries { get; set; }
        [XmlElement]
        public decimal TotalDebit { get; set; }
        [XmlElement]
        public decimal TotalCredit { get; set; }
        [XmlElement("Journal")]
        public List<Journal> Journals { get; set; }
    }
} 