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

using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using SAFT.Lib.Enums;
using SAFT.Lib.Utils;
using SAFT.Lib.Constants;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents the SAF-T file header with company and file metadata.
    /// </summary>
    [XmlRoot("Header")]
    public class Header
    {
        /// <summary>
        /// The version of the audit file format.
        /// </summary>
        [XmlElement("AuditFileVersion")]
        [Required(ErrorMessage = "AuditFileVersion is required")]
        [StringLength(10, ErrorMessage = "AuditFileVersion cannot exceed 10 characters")]
        public string AuditFileVersion { get; set; } = string.Empty;

        /// <summary>
        /// The unique identifier for the company.
        /// </summary>
        [XmlElement("CompanyID")]
        [Required(ErrorMessage = "CompanyID is required")]
        [StringLength(30, ErrorMessage = "CompanyID cannot exceed 30 characters")]
        public string CompanyID { get; set; } = string.Empty;

        /// <summary>
        /// The Portuguese VAT registration number.
        /// </summary>
        [XmlElement("TaxRegistrationNumber")]
        [Required(ErrorMessage = "TaxRegistrationNumber is required")]
        public PortugueseVatNumber TaxRegistrationNumber { get; set; } = new PortugueseVatNumber();

        /// <summary>
        /// The tax accounting basis.
        /// </summary>
        [XmlElement("TaxAccountingBasis")]
        [Required(ErrorMessage = "TaxAccountingBasis is required")]
        public TaxAccountingBasis TaxAccountingBasis { get; set; }

        /// <summary>
        /// The name of the company.
        /// </summary>
        [XmlElement("CompanyName")]
        [Required(ErrorMessage = "CompanyName is required")]
        [StringLength(100, ErrorMessage = "CompanyName cannot exceed 100 characters")]
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// The business name (optional).
        /// </summary>
        [XmlElement("BusinessName")]
        [StringLength(100, ErrorMessage = "BusinessName cannot exceed 100 characters")]
        public string? BusinessName { get; set; }

        /// <summary>
        /// The company address.
        /// </summary>
        [XmlElement("CompanyAddress")]
        [Required(ErrorMessage = "CompanyAddress is required")]
        public AddressStructure CompanyAddress { get; set; } = new AddressStructure();

        /// <summary>
        /// The fiscal year.
        /// </summary>
        [XmlElement("FiscalYear")]
        [Required(ErrorMessage = "FiscalYear is required")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "FiscalYear must be exactly 4 characters")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "FiscalYear must be a 4-digit year")]
        public string FiscalYear { get; set; } = string.Empty;

        /// <summary>
        /// The start date of the fiscal period.
        /// </summary>
        [XmlElement("StartDate")]
        [Required(ErrorMessage = "StartDate is required")]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "StartDate must be in YYYY-MM-DD format")]
        public string StartDate { get; set; } = string.Empty;

        /// <summary>
        /// The end date of the fiscal period.
        /// </summary>
        [XmlElement("EndDate")]
        [Required(ErrorMessage = "EndDate is required")]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "EndDate must be in YYYY-MM-DD format")]
        public string EndDate { get; set; } = string.Empty;

        /// <summary>
        /// The currency code.
        /// </summary>
        [XmlElement("CurrencyCode")]
        [Required(ErrorMessage = "CurrencyCode is required")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "CurrencyCode must be exactly 3 characters")]
        public string CurrencyCode { get; set; } = ConfigurationManager.Current.DefaultCurrencyCode;

        /// <summary>
        /// The date when the file was created.
        /// </summary>
        [XmlElement("DateCreated")]
        [Required(ErrorMessage = "DateCreated is required")]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "DateCreated must be in YYYY-MM-DD format")]
        public string DateCreated { get; set; } = string.Empty;

        /// <summary>
        /// The tax entity.
        /// </summary>
        [XmlElement("TaxEntity")]
        [Required(ErrorMessage = "TaxEntity is required")]
        [StringLength(100, ErrorMessage = "TaxEntity cannot exceed 100 characters")]
        public string TaxEntity { get; set; } = string.Empty;

        /// <summary>
        /// The product company tax ID.
        /// </summary>
        [XmlElement("ProductCompanyTaxID")]
        [Required(ErrorMessage = "ProductCompanyTaxID is required")]
        [StringLength(20, ErrorMessage = "ProductCompanyTaxID cannot exceed 20 characters")]
        public string ProductCompanyTaxID { get; set; } = string.Empty;

        /// <summary>
        /// The software certificate number.
        /// </summary>
        [XmlElement("SoftwareCertificateNumber")]
        [Required(ErrorMessage = "SoftwareCertificateNumber is required")]
        [StringLength(20, ErrorMessage = "SoftwareCertificateNumber cannot exceed 20 characters")]
        public string SoftwareCertificateNumber { get; set; } = string.Empty;

        /// <summary>
        /// The product ID.
        /// </summary>
        [XmlElement("ProductID")]
        [Required(ErrorMessage = "ProductID is required")]
        [StringLength(50, ErrorMessage = "ProductID cannot exceed 50 characters")]
        public string ProductID { get; set; } = string.Empty;

        /// <summary>
        /// The product version.
        /// </summary>
        [XmlElement("ProductVersion")]
        [Required(ErrorMessage = "ProductVersion is required")]
        [StringLength(20, ErrorMessage = "ProductVersion cannot exceed 20 characters")]
        public string ProductVersion { get; set; } = string.Empty;

        /// <summary>
        /// Header comment (optional).
        /// </summary>
        [XmlElement("HeaderComment")]
        [StringLength(255, ErrorMessage = "HeaderComment cannot exceed 255 characters")]
        public string? HeaderComment { get; set; }

        /// <summary>
        /// Telephone number (optional).
        /// </summary>
        [XmlElement("Telephone")]
        [StringLength(20, ErrorMessage = "Telephone cannot exceed 20 characters")]
        public string? Telephone { get; set; }

        /// <summary>
        /// Fax number (optional).
        /// </summary>
        [XmlElement("Fax")]
        [StringLength(20, ErrorMessage = "Fax cannot exceed 20 characters")]
        public string? Fax { get; set; }

        /// <summary>
        /// Email address (optional).
        /// </summary>
        [XmlElement("Email")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [StringLength(60, ErrorMessage = "Email cannot exceed 60 characters")]
        public string? Email { get; set; }

        /// <summary>
        /// Website URL (optional).
        /// </summary>
        [XmlElement("Website")]
        [Url(ErrorMessage = "Invalid website URL format")]
        [StringLength(60, ErrorMessage = "Website cannot exceed 60 characters")]
        public string? Website { get; set; }

        /// <summary>
        /// Creates a default header with configuration values.
        /// </summary>
        /// <returns>A new Header instance with default values from configuration.</returns>
        public static Header CreateDefault()
        {
            var config = ConfigurationManager.Current;
            var currentYear = config.DefaultFiscalYear > 0 ? config.DefaultFiscalYear : DateTime.Now.Year;
            
            return new Header
            {
                AuditFileVersion = SAFTConstants.AuditFileVersion,
                CurrencyCode = config.DefaultCurrencyCode,
                FiscalYear = currentYear.ToString(),
                StartDate = $"{currentYear}-01-01",
                EndDate = $"{currentYear}-12-31",
                DateCreated = DateTimeUtils.FormatDateTimeForSaft(DateTime.Now),
                CompanyAddress = new AddressStructure
                {
                    Country = config.DefaultCountryCode
                }
            };
        }
    }
} 