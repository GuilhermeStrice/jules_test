using System.Xml.Serialization;

namespace SAFT.Lib
{
    public class PaymentTax
    {
        [XmlElement]
        public TaxType TaxType { get; set; }
        [XmlElement]
        public string TaxCountryRegion { get; set; }
        [XmlElement]
        public string TaxCode { get; set; }
        [XmlElement]
        public decimal TaxAmount { get; set; }
    }
} 