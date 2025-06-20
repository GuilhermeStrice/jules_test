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
using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents an invoice line with product and tax information.
    /// </summary>
    public class InvoiceLine
    {
        /// <summary>
        /// The line number.
        /// </summary>
        [XmlElement("LineNumber")]
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
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>
        /// The product description.
        /// </summary>
        [XmlElement("ProductDescription")]
        public string ProductDescription { get; set; } = string.Empty;

        /// <summary>
        /// The quantity.
        /// </summary>
        [XmlElement("Quantity")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// The unit of measure.
        /// </summary>
        [XmlElement("UnitOfMeasure")]
        public string UnitOfMeasure { get; set; } = string.Empty;

        /// <summary>
        /// The unit price.
        /// </summary>
        [XmlElement("UnitPrice")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// The tax base (optional).
        /// </summary>
        [XmlElement("TaxBase")]
        public decimal? TaxBase { get; set; }

        /// <summary>
        /// The tax point date.
        /// </summary>
        [XmlElement("TaxPointDate")]
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
        public decimal? DebitAmount { get; set; }

        /// <summary>
        /// The credit amount (used when DebitAmount is not specified).
        /// </summary>
        [XmlElement("CreditAmount")]
        public decimal? CreditAmount { get; set; }

        /// <summary>
        /// Tax information.
        /// </summary>
        [XmlElement("Tax")]
        public Tax Tax { get; set; } = new Tax();

        /// <summary>
        /// Tax exemption reason (optional).
        /// </summary>
        [XmlElement("TaxExemptionReason")]
        public string? TaxExemptionReason { get; set; }

        /// <summary>
        /// Tax exemption code (optional).
        /// </summary>
        [XmlElement("TaxExemptionCode")]
        public string? TaxExemptionCode { get; set; }

        /// <summary>
        /// Settlement amount (optional).
        /// </summary>
        [XmlElement("SettlementAmount")]
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