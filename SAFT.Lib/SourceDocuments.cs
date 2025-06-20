/*
 * Original XSD Schema:
 * <xs:element name="SourceDocuments">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element name="SalesInvoices" minOccurs="0">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="NumberOfEntries"/>
 * <xs:element ref="TotalDebit"/>
 * <xs:element ref="TotalCredit"/>
 * <xs:element name="Invoice" minOccurs="0" maxOccurs="unbounded">
 * <!-- Complex Invoice structure with DocumentStatus, Line, DocumentTotals, etc. -->
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * <xs:element name="MovementOfGoods" minOccurs="0">
 * <!-- Similar structure for stock movements -->
 * </xs:element>
 * <xs:element name="WorkingDocuments" minOccurs="0">
 * <!-- Similar structure for work documents -->
 * </xs:element>
 * <xs:element name="Payments" minOccurs="0">
 * <!-- Similar structure for payments -->
 * </xs:element>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * 
 * Description: Source documents containing sales invoices, movements of goods, working documents, and payments.
 */

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents source documents including sales invoices, movements, working documents, and payments.
    /// </summary>
    [XmlRoot("SourceDocuments")]
    public class SourceDocuments
    {
        /// <summary>
        /// Sales invoices section (optional).
        /// </summary>
        [XmlElement("SalesInvoices")]
        public SalesInvoices? SalesInvoices { get; set; }

        /// <summary>
        /// Movement of goods section (optional).
        /// </summary>
        [XmlElement("MovementOfGoods")]
        public MovementOfGoods? MovementOfGoods { get; set; }

        /// <summary>
        /// Working documents section (optional).
        /// </summary>
        [XmlElement("WorkingDocuments")]
        public WorkingDocuments? WorkingDocuments { get; set; }

        /// <summary>
        /// Payments section (optional).
        /// </summary>
        [XmlElement("Payments")]
        public Payments? Payments { get; set; }
    }
} 