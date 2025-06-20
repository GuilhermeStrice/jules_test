/*
 * Original XSD Schema:
 * <xs:element name="DocumentStatus">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="InvoiceStatus"/>
 * <xs:element ref="InvoiceStatusDate"/>
 * <xs:element ref="Reason" minOccurs="0"/>
 * <xs:element ref="SourceID"/>
 * <xs:element name="SourceBilling" type="SAFTPTSourceBilling"/>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * 
 * Description: Represents the current status of a document with status information and source billing details.
 */

using System.Xml.Serialization;
using SAFT.Lib.Enums;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents the current status of a document.
    /// </summary>
    public class DocumentStatus
    {
        /// <summary>
        /// The status of the document.
        /// </summary>
        [XmlElement("InvoiceStatus")]
        public InvoiceStatus InvoiceStatus { get; set; }

        /// <summary>
        /// The status date.
        /// </summary>
        [XmlElement("InvoiceStatusDate")]
        public string InvoiceStatusDate { get; set; } = string.Empty;

        /// <summary>
        /// The reason for the status (optional).
        /// </summary>
        [XmlElement("Reason")]
        public string? Reason { get; set; }

        /// <summary>
        /// The source identifier.
        /// </summary>
        [XmlElement("SourceID")]
        public string SourceID { get; set; } = string.Empty;

        /// <summary>
        /// The source billing information.
        /// </summary>
        [XmlElement("SourceBilling")]
        public SourceBilling SourceBilling { get; set; } = new SourceBilling();
    }
} 