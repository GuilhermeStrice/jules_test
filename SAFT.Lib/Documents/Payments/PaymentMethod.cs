using System;
using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class PaymentMethod
    {
        [XmlElement]
        public string PaymentMechanism { get; set; }
        [XmlElement]
        public decimal PaymentAmount { get; set; }
        [XmlElement]
        public DateTime PaymentDate { get; set; }
    }
} 