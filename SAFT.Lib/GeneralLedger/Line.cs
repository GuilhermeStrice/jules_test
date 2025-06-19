using System;
using System.Xml.Serialization;

namespace SAFT.Lib.GeneralLedger
{
    public class Line
    {
        [XmlElement]
        public string RecordID { get; set; }
        [XmlElement]
        public string AccountID { get; set; }
        [XmlElement]
        public string SourceDocumentID { get; set; }
        [XmlElement]
        public DateTime SystemEntryDate { get; set; }
        [XmlElement]
        public string Description { get; set; }
        [XmlElement]
        public decimal? DebitAmount { get; set; }
        [XmlElement]
        public decimal? CreditAmount { get; set; }
    }
} 