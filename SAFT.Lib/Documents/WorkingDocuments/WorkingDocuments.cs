using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class WorkingDocuments
    {
        [XmlElement]
        public int NumberOfEntries { get; set; }
        [XmlElement]
        public decimal TotalDebit { get; set; }
        [XmlElement]
        public decimal TotalCredit { get; set; }
        [XmlElement("WorkDocument")]
        public List<WorkDocument> WorkDocuments { get; set; }
    }
} 