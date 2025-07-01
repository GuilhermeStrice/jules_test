/*
 * Original XSD Schema:
 * <!-- Estrutura de referencias a outros documentos em documentos retificativos de faturas-->
 * <xs:complexType name="References">
 * <xs:sequence>
 * <xs:element ref="Reference" minOccurs="0"/>
 * <xs:element ref="Reason" minOccurs="0"/>
 * </xs:sequence>
 * </xs:complexType>
 * 
 * Description: References structure for rectifying invoice documents.
 */

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents references structure for rectifying invoice documents.
    /// </summary>
    public class References
    {
        /// <summary>
        /// The reference to another document (optional).
        /// </summary>
        [XmlElement("Reference")]
        public string? Reference { get; set; }

        /// <summary>
        /// The reason for the reference (optional).
        /// </summary>
        [XmlElement("Reason")]
        public string? Reason { get; set; }
    }
} 