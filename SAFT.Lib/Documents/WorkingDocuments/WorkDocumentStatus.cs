using System;
using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class WorkDocumentStatus
    {
        [XmlElement]
        public WorkStatus WorkStatus { get; set; }
        [XmlElement]
        public DateTime WorkStatusDate { get; set; }
        [XmlElement]
        public string Reason { get; set; }
        [XmlElement]
        public string SourceID { get; set; }
        [XmlElement]
        public string SourceBilling { get; set; }
    }
} 