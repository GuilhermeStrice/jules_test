using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class PaymentLine
    {
        [XmlElement]
        public int LineNumber { get; set; }
        [XmlElement("SourceDocumentID")]
        public List<SourceDocumentID> SourceDocumentIDs { get; set; }
        [XmlElement]
        public decimal? SettlementAmount { get; set; }
        [XmlElement]
        public decimal? DebitAmount { get; set; }
        [XmlElement]
        public decimal? CreditAmount { get; set; }
        [XmlElement]
        public PaymentTax Tax { get; set; }
        [XmlElement]
        public string TaxExemptionReason { get; set; }
        [XmlElement]
        public string TaxExemptionCode { get; set; }
    }
} 