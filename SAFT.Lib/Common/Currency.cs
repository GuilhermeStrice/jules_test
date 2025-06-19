using System.Xml.Serialization;

namespace SAFT.Lib
{
    public class Currency
    {
        [XmlElement]
        public string CurrencyCode { get; set; }
        [XmlElement]
        public decimal CurrencyAmount { get; set; }
        [XmlElement]
        public decimal ExchangeRate { get; set; }
    }
} 