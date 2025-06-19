using System.Xml.Serialization;
using SAFT.Lib;

namespace SAFT.Lib.Files
{
    public class Account
    {
        [XmlElement]
        public string AccountID { get; set; }
        [XmlElement]
        public string AccountDescription { get; set; }
        [XmlElement]
        public AccountType AccountType { get; set; }
        [XmlElement]
        public AccountCategory AccountCategory { get; set; }
        [XmlElement]
        public string GroupingCategory { get; set; }
        [XmlElement]
        public string GroupingCode { get; set; }
        [XmlElement]
        public decimal OpeningDebitBalance { get; set; }
        [XmlElement]
        public decimal OpeningCreditBalance { get; set; }
        [XmlElement]
        public decimal ClosingDebitBalance { get; set; }
        [XmlElement]
        public decimal ClosingCreditBalance { get; set; }
        [XmlElement]
        public int? TaxonomyCode { get; set; }
    }
} 