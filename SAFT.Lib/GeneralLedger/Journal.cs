using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib.GeneralLedger
{
    public class Journal
    {
        [XmlElement]
        public string JournalID { get; set; }
        [XmlElement]
        public string Description { get; set; }
        [XmlElement("Transaction")]
        public List<Transaction> Transactions { get; set; }
    }
} 