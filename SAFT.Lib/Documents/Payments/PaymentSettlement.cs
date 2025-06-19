using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class PaymentSettlement
    {
        [XmlElement]
        public decimal SettlementAmount { get; set; }
    }
} 