using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    [XmlType("DocumentTotals")]
    public class MovementDocumentTotals
    {
        [XmlElement]
        public decimal TaxPayable { get; set; }
        [XmlElement]
        public decimal NetTotal { get; set; }
        [XmlElement]
        public decimal GrossTotal { get; set; }
        [XmlElement]
        public Currency Currency { get; set; }
        [XmlElement("WithholdingTax")]
        public List<WithholdingTax> WithholdingTaxes { get; set; }
    }
} 