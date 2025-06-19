using System;
using System.Xml.Serialization;
using SAFT.Lib;

namespace SAFT.Lib.Documents
{
    public class PaymentMethod
    {
        [XmlElement]
        public PaymentMechanism PaymentMechanism { get; set; }
        [XmlElement]
        public decimal PaymentAmount { get; set; }
        [XmlElement]
        public DateTime PaymentDate { get; set; }
    }
} 