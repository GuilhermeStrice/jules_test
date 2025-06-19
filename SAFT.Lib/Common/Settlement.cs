using System;
using System.Xml.Serialization;

namespace SAFT.Lib
{
    public class Settlement
    {
        [XmlElement]
        public string SettlementDiscount { get; set; }
        [XmlElement]
        public decimal? SettlementAmount { get; set; }
        [XmlElement]
        public DateTime? SettlementDate { get; set; }
        [XmlElement]
        public string PaymentTerms { get; set; }
    }
} 