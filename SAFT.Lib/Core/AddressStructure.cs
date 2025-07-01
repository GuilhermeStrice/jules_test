/*
 * Original XSD Schema:
 * <!-- Estrutura de Moradas-->
 * <xs:complexType name="AddressStructure">
 * <xs:sequence>
 * <xs:element ref="BuildingNumber" minOccurs="0"/>
 * <xs:element ref="StreetName" minOccurs="0"/>
 * <xs:element ref="AddressDetail"/>
 * <xs:element ref="City"/>
 * <xs:element ref="PostalCode"/>
 * <xs:element ref="Region" minOccurs="0"/>
 * <xs:element ref="Country"/>
 * </xs:sequence>
 * </xs:complexType>
 * <!-- Estrutura de Moradas de clientes -->
 * <xs:complexType name="CustomerAddressStructure">
 * <xs:sequence>
 * <xs:element ref="BuildingNumber" minOccurs="0"/>
 * <xs:element ref="StreetName" minOccurs="0"/>
 * <xs:element ref="AddressDetail"/>
 * <xs:element ref="City"/>
 * <xs:element ref="PostalCode"/>
 * <xs:element ref="Region" minOccurs="0"/>
 * <xs:element name="Country" type="CustomerCountry"/>
 * </xs:sequence>
 * </xs:complexType>
 * 
 * Description: Address structures for general addresses and customer-specific addresses.
 */

using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using SAFT.Lib.Utils;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents a general address structure with building number, street, city, postal code, region, and country.
    /// </summary>
    public class AddressStructure
    {
        /// <summary>
        /// The building number (optional).
        /// </summary>
        [XmlElement("BuildingNumber")]
        [StringLength(10, ErrorMessage = "BuildingNumber cannot exceed 10 characters")]
        public string? BuildingNumber { get; set; }

        /// <summary>
        /// The street name (optional).
        /// </summary>
        [XmlElement("StreetName")]
        [StringLength(90, ErrorMessage = "StreetName cannot exceed 90 characters")]
        public string? StreetName { get; set; }

        /// <summary>
        /// The address detail.
        /// </summary>
        [XmlElement("AddressDetail")]
        [Required(ErrorMessage = "AddressDetail is required")]
        [StringLength(210, ErrorMessage = "AddressDetail cannot exceed 210 characters")]
        public string AddressDetail { get; set; } = string.Empty;

        /// <summary>
        /// The city.
        /// </summary>
        [XmlElement("City")]
        [Required(ErrorMessage = "City is required")]
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// The postal code.
        /// </summary>
        [XmlElement("PostalCode")]
        [Required(ErrorMessage = "PostalCode is required")]
        [StringLength(10, ErrorMessage = "PostalCode cannot exceed 10 characters")]
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// The region (optional).
        /// </summary>
        [XmlElement("Region")]
        [StringLength(50, ErrorMessage = "Region cannot exceed 50 characters")]
        public string? Region { get; set; }

        /// <summary>
        /// The country code (ISO 3166 1-alpha-2).
        /// </summary>
        [XmlElement("Country")]
        [Required(ErrorMessage = "Country is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country must be exactly 2 characters")]
        [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Country must be a valid ISO 3166 1-alpha-2 country code")]
        public string Country { get; set; } = ConfigurationManager.Current.DefaultCountryCode;
    }

    /// <summary>
    /// Represents a customer-specific address structure with building number, street, city, postal code, region, and customer country.
    /// </summary>
    public class CustomerAddressStructure
    {
        /// <summary>
        /// The building number (optional).
        /// </summary>
        [XmlElement("BuildingNumber")]
        [StringLength(10, ErrorMessage = "BuildingNumber cannot exceed 10 characters")]
        public string? BuildingNumber { get; set; }

        /// <summary>
        /// The street name (optional).
        /// </summary>
        [XmlElement("StreetName")]
        [StringLength(90, ErrorMessage = "StreetName cannot exceed 90 characters")]
        public string? StreetName { get; set; }

        /// <summary>
        /// The address detail.
        /// </summary>
        [XmlElement("AddressDetail")]
        [Required(ErrorMessage = "AddressDetail is required")]
        [StringLength(210, ErrorMessage = "AddressDetail cannot exceed 210 characters")]
        public string AddressDetail { get; set; } = string.Empty;

        /// <summary>
        /// The city.
        /// </summary>
        [XmlElement("City")]
        [Required(ErrorMessage = "City is required")]
        [StringLength(50, ErrorMessage = "City cannot exceed 50 characters")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// The postal code.
        /// </summary>
        [XmlElement("PostalCode")]
        [Required(ErrorMessage = "PostalCode is required")]
        [StringLength(10, ErrorMessage = "PostalCode cannot exceed 10 characters")]
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// The region (optional).
        /// </summary>
        [XmlElement("Region")]
        [StringLength(50, ErrorMessage = "Region cannot exceed 50 characters")]
        public string? Region { get; set; }

        /// <summary>
        /// The customer country (ISO 3166 1-alpha-2 country code).
        /// </summary>
        [XmlElement("Country")]
        [Required(ErrorMessage = "Country is required")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Country must be exactly 2 characters")]
        [RegularExpression(@"^[A-Z]{2}$", ErrorMessage = "Country must be a valid ISO 3166 1-alpha-2 country code")]
        public string Country { get; set; } = string.Empty;
    }
} 