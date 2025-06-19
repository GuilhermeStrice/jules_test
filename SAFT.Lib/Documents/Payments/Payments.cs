using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class Payments
    {
        [XmlElement]
        public int NumberOfEntries { get; set; }
        [XmlElement]
        public decimal TotalDebit { get; set; }
        [XmlElement]
        public decimal TotalCredit { get; set; }
        [XmlElement("Payment")]
        public List<Payment> PaymentList { get; set; }
    }
} 