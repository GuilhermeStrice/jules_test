using System;
using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class MovementDocumentStatus
    {
        [XmlElement]
        public string MovementStatus { get; set; }
        [XmlElement]
        public DateTime MovementStatusDate { get; set; }
        [XmlElement]
        public string Reason { get; set; }
        [XmlElement]
        public string SourceID { get; set; }
        [XmlElement]
        public string SourceBilling { get; set; }
    }
} 