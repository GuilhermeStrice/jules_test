/*
 * Original XSD Schema:
 * <!--
 * Este elemento apenas deve ser gerado quando a moeda original do documento for diferente de euro 
 * -->
 * <xs:complexType name="Currency">
 * <xs:sequence>
 * <xs:element ref="CurrencyCode"/>
 * <xs:element ref="CurrencyAmount"/>
 * <xs:element ref="ExchangeRate"/>
 * </xs:sequence>
 * </xs:complexType>
 * 
 * Description: Currency information for document totals when the original document currency is different from euro.
 */

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents currency information for document totals when the original document currency is different from euro.
    /// </summary>
    public class Currency
    {
        /// <summary>
        /// The currency code (ISO 4217).
        /// </summary>
        [XmlElement("CurrencyCode")]
        public string CurrencyCode { get; set; } = string.Empty;

        /// <summary>
        /// The amount in the original currency.
        /// </summary>
        [XmlElement("CurrencyAmount")]
        public decimal CurrencyAmount { get; set; }

        /// <summary>
        /// The exchange rate used for conversion.
        /// </summary>
        [XmlElement("ExchangeRate")]
        public decimal ExchangeRate { get; set; }
    }
} 