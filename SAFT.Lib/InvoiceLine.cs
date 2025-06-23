/*
 * Original XSD Schema:
 * <xs:element name="Line" maxOccurs="unbounded">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="LineNumber"/>
 * <xs:element name="OrderReferences" type="OrderReferences" minOccurs="0" maxOccurs="unbounded"/>
 * <xs:element ref="ProductCode"/>
 * <xs:element ref="ProductDescription"/>
 * <xs:element ref="Quantity"/>
 * <xs:element ref="UnitOfMeasure"/>
 * <xs:element ref="UnitPrice"/>
 * <xs:element ref="TaxBase" minOccurs="0"/>
 * <xs:element ref="TaxPointDate"/>
 * <xs:element name="References" type="References" minOccurs="0" maxOccurs="unbounded"/>
 * <xs:element ref="Description"/>
 * <xs:element name="ProductSerialNumber" type="ProductSerialNumber" minOccurs="0"/>
 * <xs:choice>
 * <xs:element ref="DebitAmount"/>
 * <xs:element ref="CreditAmount"/>
 * </xs:choice>
 * <xs:element name="Tax" type="Tax"/>
 * <xs:element ref="TaxExemptionReason" minOccurs="0"/>
 * <xs:element ref="TaxExemptionCode" minOccurs="0"/>
 * <xs:element ref="SettlementAmount" minOccurs="0"/>
 * <xs:element name="CustomsInformation" type="CustomsInformation" minOccurs="0"/>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * 
 * Description: Represents an invoice line with product details, quantities, pricing, and tax information.
 */

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents an invoice line with product details, quantities, pricing, and tax information.
    /// </summary>
    public class InvoiceLine
    {
        /// <summary>
        /// The line number.
        /// </summary>
        [XmlElement("LineNumber")]
        [Required(ErrorMessage = "LineNumber is required")]
        [Range(1, int.MaxValue, ErrorMessage = "LineNumber must be greater than 0")]
        public int LineNumber { get; set; }

        /// <summary>
        /// Collection of order references (optional, unbounded).
        /// </summary>
        [XmlElement("OrderReferences")]
        public List<OrderReferences> OrderReferences { get; set; } = new List<OrderReferences>();

        /// <summary>
        /// The product code.
        /// </summary>
        [XmlElement("ProductCode")]
        [Required(ErrorMessage = "ProductCode is required")]
        [StringLength(30, ErrorMessage = "ProductCode cannot exceed 30 characters")]
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>
        /// The product description.
        /// </summary>
        [XmlElement("ProductDescription")]
        [Required(ErrorMessage = "ProductDescription is required")]
        [StringLength(200, ErrorMessage = "ProductDescription cannot exceed 200 characters")]
        public string ProductDescription { get; set; } = string.Empty;

        /// <summary>
        /// The quantity.
        /// </summary>
        [XmlElement("Quantity")]
        [Required(ErrorMessage = "Quantity is required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// The unit of measure.
        /// </summary>
        [XmlElement("UnitOfMeasure")]
        [Required(ErrorMessage = "UnitOfMeasure is required")]
        [StringLength(20, ErrorMessage = "UnitOfMeasure cannot exceed 20 characters")]
        public string UnitOfMeasure { get; set; } = string.Empty;

        /// <summary>
        /// The unit price.
        /// </summary>
        [XmlElement("UnitPrice")]
        [Required(ErrorMessage = "UnitPrice is required")]
        [Range(0, double.MaxValue, ErrorMessage = "UnitPrice must be non-negative")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// The tax base amount (optional).
        /// </summary>
        [XmlElement("TaxBase")]
        [Range(0, double.MaxValue, ErrorMessage = "TaxBase must be non-negative")]
        public decimal? TaxBase { get; set; }

        /// <summary>
        /// The tax point date.
        /// </summary>
        [XmlElement("TaxPointDate")]
        [Required(ErrorMessage = "TaxPointDate is required")]
        [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "TaxPointDate must be in YYYY-MM-DD format")]
        public string TaxPointDate { get; set; } = string.Empty;

        /// <summary>
        /// Collection of references (optional, unbounded).
        /// </summary>
        [XmlElement("References")]
        public List<References> References { get; set; } = new List<References>();

        /// <summary>
        /// The line description.
        /// </summary>
        [XmlElement("Description")]
        [Required(ErrorMessage = "Description is required")]
        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Product serial number information (optional).
        /// </summary>
        [XmlElement("ProductSerialNumber")]
        public ProductSerialNumber? ProductSerialNumber { get; set; }

        /// <summary>
        /// The debit amount (used when CreditAmount is not specified).
        /// </summary>
        [XmlElement("DebitAmount")]
        [Range(0, double.MaxValue, ErrorMessage = "DebitAmount must be non-negative")]
        public decimal? DebitAmount { get; set; }

        /// <summary>
        /// The credit amount (used when DebitAmount is not specified).
        /// </summary>
        [XmlElement("CreditAmount")]
        [Range(0, double.MaxValue, ErrorMessage = "CreditAmount must be non-negative")]
        public decimal? CreditAmount { get; set; }

        /// <summary>
        /// Tax information.
        /// </summary>
        [XmlElement("Tax")]
        [Required(ErrorMessage = "Tax is required")]
        public Tax Tax { get; set; } = new Tax();

        /// <summary>
        /// Tax exemption reason (optional).
        /// </summary>
        [XmlElement("TaxExemptionReason")]
        [StringLength(60, ErrorMessage = "TaxExemptionReason cannot exceed 60 characters")]
        public string? TaxExemptionReason { get; set; }

        /// <summary>
        /// Tax exemption code (optional).
        /// </summary>
        [XmlElement("TaxExemptionCode")]
        [StringLength(10, ErrorMessage = "TaxExemptionCode cannot exceed 10 characters")]
        public string? TaxExemptionCode { get; set; }

        /// <summary>
        /// Settlement amount (optional).
        /// </summary>
        [XmlElement("SettlementAmount")]
        [Range(0, double.MaxValue, ErrorMessage = "SettlementAmount must be non-negative")]
        public decimal? SettlementAmount { get; set; }

        /// <summary>
        /// Customs information (optional).
        /// </summary>
        [XmlElement("CustomsInformation")]
        public CustomsInformation? CustomsInformation { get; set; }

        /// <summary>
        /// Indicates whether DebitAmount is set (for XML serialization choice).
        /// </summary>
        [XmlIgnore]
        public bool DebitAmountSpecified => DebitAmount.HasValue;

        /// <summary>
        /// Indicates whether CreditAmount is set (for XML serialization choice).
        /// </summary>
        [XmlIgnore]
        public bool CreditAmountSpecified => CreditAmount.HasValue;
    }
} 