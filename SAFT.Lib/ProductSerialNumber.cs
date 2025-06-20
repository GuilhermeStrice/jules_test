/*
 * Original XSD Schema:
 * <!-- Estrutura de numero de serie do produto-->
 * <xs:complexType name="ProductSerialNumber">
 * <xs:sequence>
 * <xs:element ref="SerialNumber" maxOccurs="unbounded"/>
 * </xs:sequence>
 * </xs:complexType>
 * 
 * Description: Product serial number structure for document lines.
 */

using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents product serial number structure for document lines.
    /// </summary>
    public class ProductSerialNumber
    {
        /// <summary>
        /// Collection of serial numbers (unbounded).
        /// </summary>
        [XmlElement("SerialNumber")]
        public List<string> SerialNumbers { get; set; } = new List<string>();
    }
} 