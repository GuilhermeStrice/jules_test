/*
 * Original XSD Schema:
 * <!--
 * Estrutura de Taxa de documentos de movimentacao de mercadorias
 * -->
 * <xs:complexType name="MovementTax">
 * <xs:sequence>
 * <xs:element name="TaxType" type="SAFTPTMovementTaxType"/>
 * <xs:element ref="TaxCountryRegion"/>
 * <xs:element name="TaxCode" type="SAFTPTMovementTaxCode"/>
 * <xs:element ref="TaxPercentage"/>
 * </xs:sequence>
 * </xs:complexType>
 * 
 * Description: Tax structure for movement of goods documents.
 */

using System.Xml.Serialization;
using SAFT.Lib.Enums;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents tax structure for movement of goods documents.
    /// </summary>
    public class MovementTax
    {
        /// <summary>
        /// The type of movement tax.
        /// </summary>
        [XmlElement("TaxType")]
        public MovementTaxType TaxType { get; set; } = new MovementTaxType();

        /// <summary>
        /// The country or region for which this tax applies.
        /// </summary>
        [XmlElement("TaxCountryRegion")]
        public string TaxCountryRegion { get; set; } = string.Empty;

        /// <summary>
        /// The movement tax code.
        /// </summary>
        [XmlElement("TaxCode")]
        public MovementTaxCode TaxCode { get; set; } = new MovementTaxCode();

        /// <summary>
        /// The tax percentage.
        /// </summary>
        [XmlElement("TaxPercentage")]
        public decimal TaxPercentage { get; set; }
    }
} 