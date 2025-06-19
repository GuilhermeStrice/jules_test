using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class Payment
    {
        [XmlElement]
        public string PaymentRefNo { get; set; }
        [XmlElement]
        public string ATCUD { get; set; }
        [XmlElement]
        public int? Period { get; set; }
        [XmlElement]
        public string TransactionID { get; set; }
        [XmlElement]
        public DateTime TransactionDate { get; set; }
        [XmlElement]
        public string PaymentType { get; set; }
        [XmlElement]
        public string Description { get; set; }
        [XmlElement]
        public string SystemID { get; set; }
        [XmlElement]
        public PaymentDocumentStatus DocumentStatus { get; set; }
        [XmlElement("PaymentMethod")]
        public List<PaymentMethod> PaymentMethods { get; set; }
        [XmlElement]
        public string SourceID { get; set; }
        [XmlElement]
        public DateTime SystemEntryDate { get; set; }
        [XmlElement]
        public string CustomerID { get; set; }
        [XmlElement("Line")]
        public List<PaymentLine> Lines { get; set; }
        [XmlElement]
        public PaymentDocumentTotals DocumentTotals { get; set; }
    }
} 