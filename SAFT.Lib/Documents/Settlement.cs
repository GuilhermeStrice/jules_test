/*
 * Original XSD Schema:
 * <!-- Estrutura de Acordos entre cliente e fornecedor-->
 * <xs:complexType name="Settlement">
 * <xs:sequence>
 * <xs:element name="SettlementDiscount" type="SAFPTtextTypeMandatoryMax30Car" minOccurs="0"/>
 * <xs:element name="SettlementAmount" type="SAFmonetaryType" minOccurs="0"/>
 * <xs:element name="SettlementDate" type="SAFdateType" minOccurs="0"/>
 * <xs:element name="PaymentTerms" type="SAFPTtextTypeMandatoryMax100Car" minOccurs="0"/>
 * </xs:sequence>
 * </xs:complexType>
 * 
 * Description: Settlement structure for agreements between customer and supplier.
 */

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents settlement structure for agreements between customer and supplier.
    /// </summary>
    public class Settlement
    {
        /// <summary>
        /// The settlement discount (optional, max 30 chars).
        /// </summary>
        [XmlElement("SettlementDiscount")]
        public string? SettlementDiscount { get; set; }

        /// <summary>
        /// The settlement amount (optional).
        /// </summary>
        [XmlElement("SettlementAmount")]
        public decimal? SettlementAmount { get; set; }

        /// <summary>
        /// The settlement date (optional).
        /// </summary>
        [XmlElement("SettlementDate")]
        public string? SettlementDate { get; set; }

        /// <summary>
        /// The payment terms (optional, max 100 chars).
        /// </summary>
        [XmlElement("PaymentTerms")]
        public string? PaymentTerms { get; set; }
    }
} 