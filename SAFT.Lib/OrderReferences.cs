/*
 * Original XSD Schema:
 * <!-- Estrutura de Referencias ao documento de origem-->
 * <xs:complexType name="OrderReferences">
 * <xs:sequence>
 * <xs:element ref="OriginatingON" minOccurs="0"/>
 * <xs:element ref="OrderDate" minOccurs="0"/>
 * </xs:sequence>
 * </xs:complexType>
 * 
 * Description: Order references structure for document lines.
 */

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents order references structure for document lines.
    /// </summary>
    public class OrderReferences
    {
        /// <summary>
        /// The originating order number (optional).
        /// </summary>
        [XmlElement("OriginatingON")]
        public string? OriginatingON { get; set; }

        /// <summary>
        /// The order date (optional).
        /// </summary>
        [XmlElement("OrderDate")]
        public string? OrderDate { get; set; }
    }
} 