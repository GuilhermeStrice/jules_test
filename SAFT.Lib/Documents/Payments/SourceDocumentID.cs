using System;
using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class SourceDocumentID
    {
        [XmlElement]
        public string OriginatingON { get; set; }
        [XmlElement]
        public DateTime InvoiceDate { get; set; }
        [XmlElement]
        public string Description { get; set; }
    }
} 