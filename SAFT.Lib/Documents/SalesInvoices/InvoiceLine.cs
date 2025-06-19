using System;
using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class InvoiceLine
    {
        [XmlElement]
        public int LineNumber { get; set; }
        [XmlElement]
        public string ProductCode { get; set; }
        [XmlElement]
        public string ProductDescription { get; set; }
        [XmlElement]
        public decimal Quantity { get; set; }
        [XmlElement]
        public string UnitOfMeasure { get; set; }
        [XmlElement]
        public decimal UnitPrice { get; set; }
        [XmlElement]
        public DateTime TaxPointDate { get; set; }
        [XmlElement]
        public string Description { get; set; }
        [XmlElement]
        public decimal? DebitAmount { get; set; }
        [XmlElement]
        public decimal? CreditAmount { get; set; }
        [XmlElement]
        public Tax Tax { get; set; }
        [XmlElement]
        public string TaxExemptionReason { get; set; }
        [XmlElement]
        public string TaxExemptionCode { get; set; }
        [XmlElement]
        public decimal? SettlementAmount { get; set; }
        [XmlElement]
        public decimal? TaxBase { get; set; }
    }
} 