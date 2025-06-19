using System;
using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class PaymentDocumentStatus
    {
        [XmlElement]
        public PaymentStatus PaymentStatus { get; set; }
        [XmlElement]
        public DateTime PaymentStatusDate { get; set; }
        [XmlElement]
        public string Reason { get; set; }
        [XmlElement]
        public string SourceID { get; set; }
        [XmlElement]
        public string SourcePayment { get; set; }
    }
} 