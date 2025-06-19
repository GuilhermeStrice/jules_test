using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class SourceDocuments
    {
        [XmlElement("SalesInvoices")]
        public SalesInvoices SalesInvoices { get; set; }
        [XmlElement("MovementOfGoods")]
        public MovementOfGoods MovementOfGoods { get; set; }
        [XmlElement("WorkingDocuments")]
        public WorkingDocuments WorkingDocuments { get; set; }
        [XmlElement("Payments")]
        public Payments Payments { get; set; }
    }
} 