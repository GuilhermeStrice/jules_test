/*
 * Original XSD Schema:
 * <!-- Estrutura de Taxa -->
 * <xs:complexType name="Tax">
 * <xs:sequence>
 * <xs:element ref="TaxType"/>
 * <xs:element ref="TaxCountryRegion"/>
 * <xs:element ref="TaxCode"/>
 * <xs:choice>
 * <xs:element ref="TaxPercentage"/>
 * <xs:element ref="TaxAmount"/>
 * </xs:choice>
 * </xs:sequence>
 * </xs:complexType>
 * 
 * Description: Tax structure for document lines.
 */

using System.Xml.Serialization;
using SAFT.Lib;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents tax structure for document lines.
    /// </summary>
    public class Tax
    {
        /// <summary>
        /// The type of tax.
        /// </summary>
        [XmlElement("TaxType")]
        public TaxType TaxType { get; set; }

        /// <summary>
        /// The country or region for which this tax applies.
        /// </summary>
        [XmlElement("TaxCountryRegion")]
        public string TaxCountryRegion { get; set; } = string.Empty;

        /// <summary>
        /// The tax code.
        /// </summary>
        [XmlElement("TaxCode")]
        public string TaxCode { get; set; } = string.Empty;

        /// <summary>
        /// The tax percentage (used when TaxAmount is not specified).
        /// </summary>
        [XmlElement("TaxPercentage")]
        public decimal? TaxPercentage { get; set; }

        /// <summary>
        /// The tax amount (used when TaxPercentage is not specified).
        /// </summary>
        [XmlElement("TaxAmount")]
        public decimal? TaxAmount { get; set; }

        /// <summary>
        /// Indicates whether TaxPercentage is set (for XML serialization choice).
        /// </summary>
        [XmlIgnore]
        public bool TaxPercentageSpecified => TaxPercentage.HasValue;

        /// <summary>
        /// Indicates whether TaxAmount is set (for XML serialization choice).
        /// </summary>
        [XmlIgnore]
        public bool TaxAmountSpecified => TaxAmount.HasValue;
    }
} 