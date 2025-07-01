/*
 * Original XSD Schema:
 * <!-- Estrutura de caraterizacao aduaneira de produtos-->
 * <xs:complexType name="CustomsDetails">
 * <xs:sequence>
 * <xs:element ref="CNCode" minOccurs="0" maxOccurs="unbounded"/>
 * <xs:element ref="UNNumber" minOccurs="0" maxOccurs="unbounded"/>
 * </xs:sequence>
 * </xs:complexType>
 * 
 * Description: Customs characterization structure for products.
 */

using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents customs characterization structure for products.
    /// </summary>
    public class CustomsDetails
    {
        /// <summary>
        /// Collection of CN codes (optional, unbounded).
        /// </summary>
        [XmlElement("CNCode")]
        public List<string> CNCode { get; set; } = new List<string>();

        /// <summary>
        /// Collection of UN numbers (optional, unbounded).
        /// </summary>
        [XmlElement("UNNumber")]
        public List<string> UNNumber { get; set; } = new List<string>();
    }
} 