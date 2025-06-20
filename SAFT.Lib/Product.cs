/*
 * Original XSD Schema:
 * <xs:element name="Product">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="ProductType"/>
 * <xs:element ref="ProductCode"/>
 * <xs:element ref="ProductGroup" minOccurs="0"/>
 * <xs:element ref="ProductDescription"/>
 * <xs:element ref="ProductNumberCode"/>
 * <xs:element name="CustomsDetails" type="CustomsDetails" minOccurs="0"/>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * 
 * Description: Product information including type, code, description, and optional customs details.
 */

using System.Xml.Serialization;
using SAFT.Lib.Enums;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents a product with its type, code, description, and optional customs details.
    /// </summary>
    [XmlRoot("Product")]
    public class Product
    {
        /// <summary>
        /// The type of the product.
        /// </summary>
        [XmlElement("ProductType")]
        public ProductType ProductType { get; set; }

        /// <summary>
        /// The unique code identifying the product.
        /// </summary>
        [XmlElement("ProductCode")]
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>
        /// The group or category the product belongs to (optional).
        /// </summary>
        [XmlElement("ProductGroup")]
        public string? ProductGroup { get; set; }

        /// <summary>
        /// A description of the product.
        /// </summary>
        [XmlElement("ProductDescription")]
        public string ProductDescription { get; set; } = string.Empty;

        /// <summary>
        /// The product number code.
        /// </summary>
        [XmlElement("ProductNumberCode")]
        public string ProductNumberCode { get; set; } = string.Empty;

        /// <summary>
        /// Customs details for the product (optional).
        /// </summary>
        [XmlElement("CustomsDetails")]
        public CustomsDetails? CustomsDetails { get; set; }
    }
} 