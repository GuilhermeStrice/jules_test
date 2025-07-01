/*
 * Original XSD Schema:
 * <!-- Estrutura de informacao aduaneira-->
 * <xs:complexType name="CustomsInformation">
 * <xs:sequence>
 * <xs:element ref="ARCNo" minOccurs="0" maxOccurs="unbounded"/>
 * <xs:element ref="IECAmount" minOccurs="0"/>
 * </xs:sequence>
 * </xs:complexType>
 * 
 * Description: Customs information structure for document lines.
 */

using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents customs information structure for document lines.
    /// </summary>
    public class CustomsInformation
    {
        /// <summary>
        /// Collection of ARC numbers (optional, unbounded).
        /// </summary>
        [XmlElement("ARCNo")]
        public List<string> ARCNo { get; set; } = new List<string>();

        /// <summary>
        /// The IEC amount (optional).
        /// </summary>
        [XmlElement("IECAmount")]
        public decimal? IECAmount { get; set; }
    }
} 