/*
 * Original XSD Schema:
 * <xs:element name="WorkingDocuments" minOccurs="0">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="NumberOfEntries"/>
 * <xs:element ref="TotalDebit"/>
 * <xs:element ref="TotalCredit"/>
 * <xs:element name="WorkDocument" minOccurs="0" maxOccurs="unbounded">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="DocumentNumber"/>
 * <xs:element ref="ATCUD"/>
 * <xs:element name="DocumentStatus">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="WorkStatus"/>
 * <xs:element ref="WorkStatusDate"/>
 * <xs:element ref="Reason" minOccurs="0"/>
 * <xs:element ref="SourceID"/>
 * <xs:element name="SourceBilling" type="SAFTPTSourceBilling"/>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * <xs:element ref="Hash"/>
 * <xs:element ref="HashControl"/>
 * <xs:element ref="Period" minOccurs="0"/>
 * <xs:element ref="WorkDate"/>
 * <xs:element ref="WorkType"/>
 * <xs:element ref="SourceID"/>
 * <xs:element ref="EACCode" minOccurs="0"/>
 * <xs:element ref="SystemEntryDate"/>
 * <xs:element ref="TransactionID" minOccurs="0"/>
 * <xs:element ref="CustomerID"/>
 * <xs:element name="Line" maxOccurs="unbounded">
 * <!-- Complex Line structure similar to Invoice -->
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * 
 * Description: Working documents containing work documents with product lines and totals.
 */

using System.Collections.Generic;
using System.Xml.Serialization;
using SAFT.Lib.Enums;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents working documents with summary totals and individual work documents.
    /// </summary>
    public class WorkingDocuments
    {
        /// <summary>
        /// The number of entries.
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
        /// Collection of work documents (optional, unbounded).
        /// </summary>
        [XmlElement("WorkDocument")]
        public List<WorkDocument> WorkDocuments { get; set; } = new List<WorkDocument>();
    }

    /// <summary>
    /// Represents a work document with all its details and lines.
    /// </summary>
    public class WorkDocument
    {
        /// <summary>
        /// The document number.
        /// </summary>
        [XmlElement("DocumentNumber")]
        public string DocumentNumber { get; set; } = string.Empty;

        /// <summary>
        /// The ATCUD (Unique Document Code).
        /// </summary>
        [XmlElement("ATCUD")]
        public string ATCUD { get; set; } = string.Empty;

        /// <summary>
        /// The current status of the document.
        /// </summary>
        [XmlElement("DocumentStatus")]
        public WorkDocumentStatus DocumentStatus { get; set; } = new WorkDocumentStatus();

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
        /// The work date.
        /// </summary>
        [XmlElement("WorkDate")]
        public string WorkDate { get; set; } = string.Empty;

        /// <summary>
        /// The type of work.
        /// </summary>
        [XmlElement("WorkType")]
        public WorkType WorkType { get; set; }

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
        /// Collection of work document lines (unbounded).
        /// </summary>
        [XmlElement("Line")]
        public List<WorkDocumentLine> Lines { get; set; } = new List<WorkDocumentLine>();

        /// <summary>
        /// Document totals.
        /// </summary>
        [XmlElement("DocumentTotals")]
        public WorkDocumentTotals DocumentTotals { get; set; } = new WorkDocumentTotals();
    }

    /// <summary>
    /// Represents the current status of a work document.
    /// </summary>
    public class WorkDocumentStatus
    {
        /// <summary>
        /// The status of the work.
        /// </summary>
        [XmlElement("WorkStatus")]
        public WorkStatus WorkStatus { get; set; }

        /// <summary>
        /// The status date.
        /// </summary>
        [XmlElement("WorkStatusDate")]
        public string WorkStatusDate { get; set; } = string.Empty;

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

    /// <summary>
    /// Represents a work document line with product and tax information.
    /// </summary>
    public class WorkDocumentLine
    {
        /// <summary>
        /// The line number.
        /// </summary>
        [XmlElement("LineNumber")]
        public int LineNumber { get; set; }

        /// <summary>
        /// Collection of order references (optional, unbounded).
        /// </summary>
        [XmlElement("OrderReferences")]
        public List<OrderReferences> OrderReferences { get; set; } = new List<OrderReferences>();

        /// <summary>
        /// The product code.
        /// </summary>
        [XmlElement("ProductCode")]
        public string ProductCode { get; set; } = string.Empty;

        /// <summary>
        /// The product description.
        /// </summary>
        [XmlElement("ProductDescription")]
        public string ProductDescription { get; set; } = string.Empty;

        /// <summary>
        /// The quantity.
        /// </summary>
        [XmlElement("Quantity")]
        public decimal Quantity { get; set; }

        /// <summary>
        /// The unit of measure.
        /// </summary>
        [XmlElement("UnitOfMeasure")]
        public string UnitOfMeasure { get; set; } = string.Empty;

        /// <summary>
        /// The unit price.
        /// </summary>
        [XmlElement("UnitPrice")]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// The tax base (optional).
        /// </summary>
        [XmlElement("TaxBase")]
        public decimal? TaxBase { get; set; }

        /// <summary>
        /// The tax point date.
        /// </summary>
        [XmlElement("TaxPointDate")]
        public string TaxPointDate { get; set; } = string.Empty;

        /// <summary>
        /// Collection of references (optional, unbounded).
        /// </summary>
        [XmlElement("References")]
        public List<References> References { get; set; } = new List<References>();

        /// <summary>
        /// The line description.
        /// </summary>
        [XmlElement("Description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Product serial number information (optional).
        /// </summary>
        [XmlElement("ProductSerialNumber")]
        public ProductSerialNumber? ProductSerialNumber { get; set; }

        /// <summary>
        /// The debit amount (used when CreditAmount is not specified).
        /// </summary>
        [XmlElement("DebitAmount")]
        public decimal? DebitAmount { get; set; }

        /// <summary>
        /// The credit amount (used when DebitAmount is not specified).
        /// </summary>
        [XmlElement("CreditAmount")]
        public decimal? CreditAmount { get; set; }

        /// <summary>
        /// Tax information (optional).
        /// </summary>
        [XmlElement("Tax")]
        public Tax? Tax { get; set; }

        /// <summary>
        /// Tax exemption reason (optional).
        /// </summary>
        [XmlElement("TaxExemptionReason")]
        public string? TaxExemptionReason { get; set; }

        /// <summary>
        /// Tax exemption code (optional).
        /// </summary>
        [XmlElement("TaxExemptionCode")]
        public string? TaxExemptionCode { get; set; }

        /// <summary>
        /// Settlement amount (optional).
        /// </summary>
        [XmlElement("SettlementAmount")]
        public decimal? SettlementAmount { get; set; }

        /// <summary>
        /// Customs information (optional).
        /// </summary>
        [XmlElement("CustomsInformation")]
        public CustomsInformation? CustomsInformation { get; set; }

        /// <summary>
        /// Indicates whether DebitAmount is set (for XML serialization choice).
        /// </summary>
        [XmlIgnore]
        public bool DebitAmountSpecified => DebitAmount.HasValue;

        /// <summary>
        /// Indicates whether CreditAmount is set (for XML serialization choice).
        /// </summary>
        [XmlIgnore]
        public bool CreditAmountSpecified => CreditAmount.HasValue;
    }

    /// <summary>
    /// Represents document totals for work documents.
    /// </summary>
    public class WorkDocumentTotals
    {
        /// <summary>
        /// The tax payable amount.
        /// </summary>
        [XmlElement("TaxPayable")]
        public decimal TaxPayable { get; set; }

        /// <summary>
        /// The net total amount.
        /// </summary>
        [XmlElement("NetTotal")]
        public decimal NetTotal { get; set; }

        /// <summary>
        /// The gross total amount.
        /// </summary>
        [XmlElement("GrossTotal")]
        public decimal GrossTotal { get; set; }

        /// <summary>
        /// Currency information (optional).
        /// </summary>
        [XmlElement("Currency")]
        public Currency? Currency { get; set; }
    }
} 