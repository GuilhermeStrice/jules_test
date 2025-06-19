using System;
using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class DocumentStatus
    {
        [XmlElement]
        public string InvoiceStatus { get; set; }
        [XmlElement]
        public DateTime InvoiceStatusDate { get; set; }
        [XmlElement]
        public string Reason { get; set; }
        [XmlElement]
        public string SourceID { get; set; }
        [XmlElement]
        public string SourceBilling { get; set; }
    }
} 