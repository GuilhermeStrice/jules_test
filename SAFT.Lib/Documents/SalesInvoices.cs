/*
 * Original XSD Schema:
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
 * 
 * Description: Sales invoices collection with summary totals and individual invoice details.
 */

using System.Collections.Generic;
using System.Xml.Serialization;
using SAFT.Lib;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents a collection of sales invoices with summary totals.
    /// </summary>
    public class SalesInvoices
    {
        /// <summary>
        /// The number of invoice entries.
        /// </summary>
        [XmlElement("NumberOfEntries")]
        public int NumberOfEntries { get; set; }

        /// <summary>
        /// The total debit amount.
        /// </summary>
        [XmlElement("TotalDebit")]
        public decimal TotalDebit { get; set; }

        /// <summary>
        /// The total credit amount.
        /// </summary>
        [XmlElement("TotalCredit")]
        public decimal TotalCredit { get; set; }

        /// <summary>
        /// Collection of invoices (optional, unbounded).
        /// </summary>
        [XmlElement("Invoice")]
        public List<Invoice> Invoices { get; set; } = new List<Invoice>();
    }

    /// <summary>
    /// Represents a sales invoice with all its details and lines.
    /// </summary>
    public class Invoice
    {
        /// <summary>
        /// The invoice number.
        /// </summary>
        [XmlElement("InvoiceNo")]
        public string InvoiceNo { get; set; } = string.Empty;

        /// <summary>
        /// The ATCUD (Unique Document Code).
        /// </summary>
        [XmlElement("ATCUD")]
        public string ATCUD { get; set; } = string.Empty;

        /// <summary>
        /// The current status of the document.
        /// </summary>
        [XmlElement("DocumentStatus")]
        public DocumentStatus DocumentStatus { get; set; } = new DocumentStatus();

        /// <summary>
        /// The document hash.
        /// </summary>
        [XmlElement("Hash")]
        public string Hash { get; set; } = string.Empty;

        /// <summary>
        /// The hash control.
        /// </summary>
        [XmlElement("HashControl")]
        public string HashControl { get; set; } = string.Empty;

        /// <summary>
        /// The accounting period (optional).
        /// </summary>
        [XmlElement("Period")]
        public string? Period { get; set; }

        /// <summary>
        /// The invoice date.
        /// </summary>
        [XmlElement("InvoiceDate")]
        public string InvoiceDate { get; set; } = string.Empty;

        /// <summary>
        /// The type of invoice.
        /// </summary>
        [XmlElement("InvoiceType")]
        public InvoiceType InvoiceType { get; set; }

        /// <summary>
        /// Special regimes information.
        /// </summary>
        [XmlElement("SpecialRegimes")]
        public SpecialRegimes SpecialRegimes { get; set; } = new SpecialRegimes();

        /// <summary>
        /// The source identifier.
        /// </summary>
        [XmlElement("SourceID")]
        public string SourceID { get; set; } = string.Empty;

        /// <summary>
        /// The EAC code (optional).
        /// </summary>
        [XmlElement("EACCode")]
        public string? EACCode { get; set; }

        /// <summary>
        /// The system entry date.
        /// </summary>
        [XmlElement("SystemEntryDate")]
        public string SystemEntryDate { get; set; } = string.Empty;

        /// <summary>
        /// The transaction identifier (optional).
        /// </summary>
        [XmlElement("TransactionID")]
        public string? TransactionID { get; set; }

        /// <summary>
        /// The customer identifier.
        /// </summary>
        [XmlElement("CustomerID")]
        public string CustomerID { get; set; } = string.Empty;

        /// <summary>
        /// Ship to information (optional).
        /// </summary>
        [XmlElement("ShipTo")]
        public ShippingPointStructure? ShipTo { get; set; }

        /// <summary>
        /// Ship from information (optional).
        /// </summary>
        [XmlElement("ShipFrom")]
        public ShippingPointStructure? ShipFrom { get; set; }

        /// <summary>
        /// Movement end time (optional).
        /// </summary>
        [XmlElement("MovementEndTime")]
        public string? MovementEndTime { get; set; }

        /// <summary>
        /// Movement start time (optional).
        /// </summary>
        [XmlElement("MovementStartTime")]
        public string? MovementStartTime { get; set; }

        /// <summary>
        /// Collection of invoice lines (unbounded).
        /// </summary>
        [XmlElement("Line")]
        public List<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();

        /// <summary>
        /// Document totals.
        /// </summary>
        [XmlElement("DocumentTotals")]
        public DocumentTotals DocumentTotals { get; set; } = new DocumentTotals();

        /// <summary>
        /// Collection of withholding taxes (optional, unbounded).
        /// </summary>
        [XmlElement("WithholdingTax")]
        public List<WithholdingTax> WithholdingTaxes { get; set; } = new List<WithholdingTax>();
    }
} 