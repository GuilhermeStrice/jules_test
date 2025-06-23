/*
 * Original XSD Schema:
 * <!-- Estrutura de Cliente (AuditFile.MasterFiles.Customer) -->
 * <xs:element name="Customer">
 *   <xs:complexType>
 *     <xs:sequence>
 *       <xs:element ref="CustomerID"/>
 *       <xs:element ref="AccountID"/>
 *       <xs:element ref="CustomerTaxID"/>
 *       <xs:element ref="CompanyName"/>
 *       <xs:element ref="Contact" minOccurs="0"/>
 *       <xs:element name="BillingAddress" type="CustomerAddressStructure"/>
 *       <xs:element ref="ShipToAddress" minOccurs="0" maxOccurs="unbounded"/>
 *       <xs:element ref="Telephone" minOccurs="0"/>
 *       <xs:element ref="Fax" minOccurs="0"/>
 *       <xs:element ref="Email" minOccurs="0"/>
 *       <xs:element ref="Website" minOccurs="0"/>
 *       <xs:element ref="SelfBillingIndicator"/>
 *     </xs:sequence>
 *   </xs:complexType>
 * </xs:element>
 *
 * Description: Customer structure for master files, including identification, address, and contact details.
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using SAFT.Lib.Enums;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents a Customer as defined in the SAF-T XSD schema.
    /// </summary>
    [XmlRoot("Customer")]
    public class Customer
    {
        [XmlElement("CustomerID")]
        [Required(ErrorMessage = "CustomerID is required")]
        [StringLength(30, ErrorMessage = "CustomerID cannot exceed 30 characters")]
        public string CustomerID { get; set; } = string.Empty;

        [XmlElement("AccountID")]
        [Required(ErrorMessage = "AccountID is required")]
        public GLAccountID AccountID { get; set; } = new GLAccountID();

        [XmlElement("CustomerTaxID")]
        [Required(ErrorMessage = "CustomerTaxID is required")]
        [StringLength(20, ErrorMessage = "CustomerTaxID cannot exceed 20 characters")]
        public string CustomerTaxID { get; set; } = string.Empty;

        [XmlElement("CompanyName")]
        [Required(ErrorMessage = "CompanyName is required")]
        [StringLength(100, ErrorMessage = "CompanyName cannot exceed 100 characters")]
        public string CompanyName { get; set; } = string.Empty;

        [XmlElement("Contact")]
        [StringLength(50, ErrorMessage = "Contact cannot exceed 50 characters")]
        public string? Contact { get; set; }

        [XmlElement("BillingAddress")]
        [Required(ErrorMessage = "BillingAddress is required")]
        public CustomerAddressStructure BillingAddress { get; set; } = new CustomerAddressStructure();

        [XmlElement("ShipToAddress")]
        public List<CustomerAddressStructure>? ShipToAddress { get; set; }

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