using System.Xml.Serialization;
using System.Collections.Generic;

namespace SAFT.Lib.Documents
{
    [XmlType("DocumentTotals")]
    public class InvoiceDocumentTotals
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
        // Currency, Settlement, Payment, WithholdingTax omitted for brevity
    }
} 