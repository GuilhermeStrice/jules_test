using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class WithholdingTax
    {
        [XmlElement]
        public WithholdingTaxType WithholdingTaxType { get; set; }
        [XmlElement]
        public string WithholdingTaxDescription { get; set; }
        [XmlElement]
        public decimal WithholdingTaxAmount { get; set; }
    }
} 