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

using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using SAFT.Lib;

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
        [Required(ErrorMessage = "ProductType is required")]
        public ProductType? ProductType { get; set; }

        /// <summary>
        /// The unique code identifying the product.
        /// </summary>
        [XmlElement("ProductCode")]
        [Required(ErrorMessage = "ProductCode is required")]
        [StringLength(30, ErrorMessage = "ProductCode cannot exceed 30 characters")]
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>
        /// The group or category the product belongs to (optional).
        /// </summary>
        [XmlElement("ProductGroup")]
        [StringLength(50, ErrorMessage = "ProductGroup cannot exceed 50 characters")]
        public string? ProductGroup { get; set; }

        /// <summary>
        /// A description of the product.
        /// </summary>
        [XmlElement("ProductDescription")]
        [Required(ErrorMessage = "ProductDescription is required")]
        [StringLength(200, ErrorMessage = "ProductDescription cannot exceed 200 characters")]
        public string ProductDescription { get; set; } = string.Empty;

        /// <summary>
        /// The product number code.
        /// </summary>
        [XmlElement("ProductNumberCode")]
        [Required(ErrorMessage = "ProductNumberCode is required")]
        [StringLength(50, ErrorMessage = "ProductNumberCode cannot exceed 50 characters")]
        public string ProductNumberCode { get; set; } = string.Empty;

        /// <summary>
        /// Customs details for the product (optional).
        /// </summary>
        [XmlElement("CustomsDetails")]
        public CustomsDetails? CustomsDetails { get; set; }
    }
} 