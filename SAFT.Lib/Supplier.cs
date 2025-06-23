using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using SAFT.Lib.Enums;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents a Supplier as defined in the SAF-T XSD schema.
    /// </summary>
    [XmlRoot("Supplier")]
    public class Supplier
    {
        [XmlElement("SupplierID")]
        [Required(ErrorMessage = "SupplierID is required")]
        [StringLength(30, ErrorMessage = "SupplierID cannot exceed 30 characters")]
        public string SupplierID { get; set; } = string.Empty;

        [XmlElement("AccountID")]
        [Required(ErrorMessage = "AccountID is required")]
        public GLAccountID AccountID { get; set; } = new GLAccountID();

        [XmlElement("SupplierTaxID")]
        [Required(ErrorMessage = "SupplierTaxID is required")]
        [StringLength(20, ErrorMessage = "SupplierTaxID cannot exceed 20 characters")]
        public string SupplierTaxID { get; set; } = string.Empty;

        [XmlElement("CompanyName")]
        [Required(ErrorMessage = "CompanyName is required")]
        [StringLength(100, ErrorMessage = "CompanyName cannot exceed 100 characters")]
        public string CompanyName { get; set; } = string.Empty;

        [XmlElement("Contact")]
        [StringLength(50, ErrorMessage = "Contact cannot exceed 50 characters")]
        public string? Contact { get; set; }

        [XmlElement("BillingAddress")]
        [Required(ErrorMessage = "BillingAddress is required")]
        public AddressStructure BillingAddress { get; set; } = new AddressStructure();

        [XmlElement("ShipFromAddress")]
        public List<AddressStructure>? ShipFromAddress { get; set; }

        [XmlElement("Telephone")]
        [StringLength(20, ErrorMessage = "Telephone cannot exceed 20 characters")]
        public string? Telephone { get; set; }

        [XmlElement("Fax")]
        [StringLength(20, ErrorMessage = "Fax cannot exceed 20 characters")]
        public string? Fax { get; set; }

        [XmlElement("Email")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        [StringLength(60, ErrorMessage = "Email cannot exceed 60 characters")]
        public string? Email { get; set; }

        [XmlElement("Website")]
        [Url(ErrorMessage = "Invalid website URL format")]
        [StringLength(60, ErrorMessage = "Website cannot exceed 60 characters")]
        public string? Website { get; set; }

        [XmlElement("SelfBillingIndicator")]
        [Range(0, 1, ErrorMessage = "SelfBillingIndicator must be 0 or 1")]
        public int SelfBillingIndicator { get; set; }
    }
} 