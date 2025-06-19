using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class WorkDocument
    {
        [XmlElement]
        public string DocumentNumber { get; set; }
        [XmlElement]
        public string ATCUD { get; set; }
        [XmlElement]
        public WorkDocumentStatus DocumentStatus { get; set; }
        [XmlElement]
        public string Hash { get; set; }
        [XmlElement]
        public string HashControl { get; set; }
        [XmlElement]
        public int? Period { get; set; }
        [XmlElement]
        public DateTime WorkDate { get; set; }
        [XmlElement]
        public string WorkType { get; set; }
        [XmlElement]
        public string SourceID { get; set; }
        [XmlElement]
        public string EACCode { get; set; }
        [XmlElement]
        public DateTime SystemEntryDate { get; set; }
        [XmlElement]
        public string TransactionID { get; set; }
        [XmlElement]
        public string CustomerID { get; set; }
        [XmlElement("Line")]
        public List<WorkDocumentLine> Lines { get; set; }
        [XmlElement]
        public WorkDocumentTotals DocumentTotals { get; set; }
    }
} 