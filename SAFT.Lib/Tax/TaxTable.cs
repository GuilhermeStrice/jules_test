/*
 * Original XSD Schema:
 * <xs:element name="TaxTable">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="TaxTableEntry" minOccurs="1" maxOccurs="unbounded"/>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * 
 * Description: A table containing tax entries with at least one entry required.
 */

using System.Collections.Generic;
using System.Xml.Serialization;
using SAFT.Lib;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents a table of tax entries.
    /// </summary>
    [XmlRoot("TaxTable")]
    public class TaxTable
    {
        /// <summary>
        /// Collection of tax table entries. At least one entry is required.
        /// </summary>
        [XmlElement("TaxTableEntry")]
        public List<TaxTableEntry> TaxTableEntries { get; set; } = new List<TaxTableEntry>();
    }

    /// <summary>
    /// Represents a single tax table entry with tax type, country/region, code, and rate/amount.
    /// </summary>
    public class TaxTableEntry
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
        /// The tax code for this entry.
        /// </summary>
        [XmlElement("TaxCode")]
        public string TaxCode { get; set; } = string.Empty;

        /// <summary>
        /// Description of the tax entry (mandatory, max 255 characters).
        /// </summary>
        [XmlElement("Description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The expiration date for this tax entry (optional).
        /// </summary>
        [XmlElement("TaxExpirationDate")]
        public string? TaxExpirationDate { get; set; }

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