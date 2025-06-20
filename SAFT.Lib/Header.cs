/*
 * Original XSD Schema:
 * <xs:element name="Header">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="AuditFileVersion"/>
 * <xs:element ref="CompanyID"/>
 * <xs:element name="TaxRegistrationNumber" type="SAFPTPortugueseVatNumber"/>
 * <xs:element ref="TaxAccountingBasis"/>
 * <xs:element ref="CompanyName"/>
 * <xs:element ref="BusinessName" minOccurs="0"/>
 * <xs:element ref="CompanyAddress"/>
 * <xs:element ref="FiscalYear"/>
 * <xs:element ref="StartDate"/>
 * <xs:element ref="EndDate"/>
 * <xs:element name="CurrencyCode" type="CurrencyPT"/>
 * <xs:element ref="DateCreated"/>
 * <xs:element ref="TaxEntity"/>
 * <xs:element ref="ProductCompanyTaxID"/>
 * <xs:element ref="SoftwareCertificateNumber"/>
 * <xs:element ref="ProductID"/>
 * <xs:element ref="ProductVersion"/>
 * <xs:element ref="HeaderComment" minOccurs="0"/>
 * <xs:element ref="Telephone" minOccurs="0"/>
 * <xs:element ref="Fax" minOccurs="0"/>
 * <xs:element ref="Email" minOccurs="0"/>
 * <xs:element ref="Website" minOccurs="0"/>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * 
 * Description: SAF-T file header with company and file metadata.
 */

using System.Xml.Serialization;
using SAFT.Lib.Enums;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents the SAF-T file header with company and file metadata.
    /// </summary>
    [XmlRoot("Header")]
    public class Header
    {
        [XmlElement("AuditFileVersion")]
        public string AuditFileVersion { get; set; } = string.Empty;

        [XmlElement("CompanyID")]
        public string CompanyID { get; set; } = string.Empty;

        [XmlElement("TaxRegistrationNumber")]
        public PortugueseVatNumber TaxRegistrationNumber { get; set; } = new PortugueseVatNumber();

        [XmlElement("TaxAccountingBasis")]
        public TaxAccountingBasis TaxAccountingBasis { get; set; }

        [XmlElement("CompanyName")]
        public string CompanyName { get; set; } = string.Empty;

        [XmlElement("BusinessName")]
        public string? BusinessName { get; set; }

        [XmlElement("CompanyAddress")]
        public string CompanyAddress { get; set; } = string.Empty;

        [XmlElement("FiscalYear")]
        public int FiscalYear { get; set; }

        [XmlElement("StartDate")]
        public string StartDate { get; set; } = string.Empty;

        [XmlElement("EndDate")]
        public string EndDate { get; set; } = string.Empty;

        [XmlElement("CurrencyCode")]
        public string CurrencyCode { get; set; } = string.Empty;

        [XmlElement("DateCreated")]
        public string DateCreated { get; set; } = string.Empty;

        [XmlElement("TaxEntity")]
        public string TaxEntity { get; set; } = string.Empty;

        [XmlElement("ProductCompanyTaxID")]
        public string ProductCompanyTaxID { get; set; } = string.Empty;

        [XmlElement("SoftwareCertificateNumber")]
        public int SoftwareCertificateNumber { get; set; }

        [XmlElement("ProductID")]
        public string ProductID { get; set; } = string.Empty;

        [XmlElement("ProductVersion")]
        public string ProductVersion { get; set; } = string.Empty;

        [XmlElement("HeaderComment")]
        public string? HeaderComment { get; set; }

        [XmlElement("Telephone")]
        public string? Telephone { get; set; }

        [XmlElement("Fax")]
        public string? Fax { get; set; }

        [XmlElement("Email")]
        public string? Email { get; set; }

        [XmlElement("Website")]
        public string? Website { get; set; }
    }
} 