using System.Xml.Serialization;
using SAFT.Lib.Documents;
using SAFT.Lib.Files;
using SAFT.Lib.GeneralLedger;

namespace SAFT.Lib
{
    [XmlRoot("AuditFile", Namespace = "urn:OECD:StandardAuditFile-Tax:PT_1.04_01")]
    public class AuditFile
    {
        [XmlElement("Header")]
        public Header Header { get; set; }

        [XmlElement("MasterFiles")]
        public MasterFiles MasterFiles { get; set; }

        [XmlElement("GeneralLedgerEntries")]
        public GeneralLedgerEntries GeneralLedgerEntries { get; set; }

        [XmlElement("SourceDocuments")]
        public SourceDocuments SourceDocuments { get; set; }
    }
} 