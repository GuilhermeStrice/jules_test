using System.Xml.Serialization;

namespace SAFT.Lib
{
    public class Header
    {
        [XmlElement]
        public string AuditFileVersion { get; set; }
        [XmlElement]
        public string CompanyID { get; set; }
        [XmlElement]
        public int TaxRegistrationNumber { get; set; }
        [XmlElement]
        public TaxAccountingBasis TaxAccountingBasis { get; set; }
        [XmlElement]
        public string CompanyName { get; set; }
        [XmlElement]
        public string BusinessName { get; set; }
        [XmlElement]
        public AddressStructure CompanyAddress { get; set; }
        [XmlElement]
        public int FiscalYear { get; set; }
        [XmlElement]
        public DateTime StartDate { get; set; }
        [XmlElement]
        public DateTime EndDate { get; set; }
        [XmlElement]
        public string CurrencyCode { get; set; }
        [XmlElement]
        public DateTime DateCreated { get; set; }
        [XmlElement]
        public string TaxEntity { get; set; }
        [XmlElement]
        public string ProductCompanyTaxID { get; set; }
        [XmlElement]
        public int SoftwareCertificateNumber { get; set; }
        [XmlElement]
        public string ProductID { get; set; }
        [XmlElement]
        public string ProductVersion { get; set; }
        [XmlElement]
        public string HeaderComment { get; set; }
        [XmlElement]
        public string Telephone { get; set; }
        [XmlElement]
        public string Fax { get; set; }
        [XmlElement]
        public string Email { get; set; }
        [XmlElement]
        public string Website { get; set; }
    }
}