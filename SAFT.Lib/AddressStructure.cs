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

using System.Xml.Serialization;

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
        public string? BuildingNumber { get; set; }

        /// <summary>
        /// The street name (optional).
        /// </summary>
        [XmlElement("StreetName")]
        public string? StreetName { get; set; }

        /// <summary>
        /// The address detail.
        /// </summary>
        [XmlElement("AddressDetail")]
        public string AddressDetail { get; set; } = string.Empty;

        /// <summary>
        /// The city.
        /// </summary>
        [XmlElement("City")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// The postal code.
        /// </summary>
        [XmlElement("PostalCode")]
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// The region (optional).
        /// </summary>
        [XmlElement("Region")]
        public string? Region { get; set; }

        /// <summary>
        /// The country.
        /// </summary>
        [XmlElement("Country")]
        public string Country { get; set; } = string.Empty;
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
        public string? BuildingNumber { get; set; }

        /// <summary>
        /// The street name (optional).
        /// </summary>
        [XmlElement("StreetName")]
        public string? StreetName { get; set; }

        /// <summary>
        /// The address detail.
        /// </summary>
        [XmlElement("AddressDetail")]
        public string AddressDetail { get; set; } = string.Empty;

        /// <summary>
        /// The city.
        /// </summary>
        [XmlElement("City")]
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// The postal code.
        /// </summary>
        [XmlElement("PostalCode")]
        public string PostalCode { get; set; } = string.Empty;

        /// <summary>
        /// The region (optional).
        /// </summary>
        [XmlElement("Region")]
        public string? Region { get; set; }

        /// <summary>
        /// The customer country.
        /// </summary>
        [XmlElement("Country")]
        public string Country { get; set; } = string.Empty;
    }
} 