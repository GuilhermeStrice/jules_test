/*
 * Original XSD Schema:
 * <!-- Estrutura de Retencao na fonte-->
 * <xs:complexType name="WithholdingTax">
 * <xs:sequence>
 * <xs:element ref="WithholdingTaxType" minOccurs="0"/>
 * <xs:element name="WithholdingTaxDescription" type="SAFPTtextTypeMandatoryMax60Car" minOccurs="0"/>
 * <xs:element name="WithholdingTaxAmount" type="SAFmonetaryType"/>
 * </xs:sequence>
 * </xs:complexType>
 * 
 * Description: Withholding tax structure for documents.
 */

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents withholding tax structure for documents.
    /// </summary>
    public class WithholdingTax
    {
        /// <summary>
        /// The type of withholding tax (optional).
        /// </summary>
        [XmlElement("WithholdingTaxType")]
        public WithholdingTaxType? WithholdingTaxType { get; set; }

        /// <summary>
        /// The withholding tax description (optional, max 60 chars).
        /// </summary>
        [XmlElement("WithholdingTaxDescription")]
        public string? WithholdingTaxDescription { get; set; }

        /// <summary>
        /// The withholding tax amount.
        /// </summary>
        [XmlElement("WithholdingTaxAmount")]
        public decimal WithholdingTaxAmount { get; set; }
    }
} 