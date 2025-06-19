using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class DocumentTotals
    {
        [XmlElement]
        public decimal TaxPayable { get; set; }
        [XmlElement]
        public decimal NetTotal { get; set; }
        [XmlElement]
        public decimal GrossTotal { get; set; }
        // Currency, Settlement, Payment, WithholdingTax omitted for brevity
    }
} 