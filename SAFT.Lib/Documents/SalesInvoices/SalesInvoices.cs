using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class SalesInvoices
    {
        [XmlElement]
        public int NumberOfEntries { get; set; }
        [XmlElement]
        public decimal TotalDebit { get; set; }
        [XmlElement]
        public decimal TotalCredit { get; set; }
        [XmlElement("Invoice")]
        public List<Invoice> Invoices { get; set; }
    }
} 