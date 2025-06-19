using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class Invoice
    {
        [XmlElement]
        public string InvoiceNo { get; set; }
        [XmlElement]
        public string ATCUD { get; set; }
        [XmlElement]
        public InvoiceDocumentStatus DocumentStatus { get; set; }
        [XmlElement]
        public string Hash { get; set; }
        [XmlElement]
        public string HashControl { get; set; }
        [XmlElement]
        public int? Period { get; set; }
        [XmlElement]
        public DateTime InvoiceDate { get; set; }
        [XmlElement]
        public InvoiceType InvoiceType { get; set; }
        [XmlElement]
        public SpecialRegimes SpecialRegimes { get; set; }
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
        public List<InvoiceLine> Lines { get; set; }
        [XmlElement]
        public InvoiceDocumentTotals DocumentTotals { get; set; }
    }
} 