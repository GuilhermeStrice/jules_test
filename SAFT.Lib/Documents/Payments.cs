/*
 * Original XSD Schema:
 * <xs:element name="Payments" minOccurs="0">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="NumberOfEntries"/>
 * <xs:element ref="TotalDebit"/>
 * <xs:element ref="TotalCredit"/>
 * <xs:element name="Payment" minOccurs="0" maxOccurs="unbounded">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="PaymentRefNo"/>
 * <xs:element ref="ATCUD"/>
 * <xs:element ref="Period" minOccurs="0"/>
 * <xs:element ref="TransactionID" minOccurs="0"/>
 * <xs:element ref="TransactionDate"/>
 * <xs:element name="PaymentType" type="SAFTPTPaymentType"/>
 * <xs:element ref="Description" minOccurs="0"/>
 * <xs:element ref="SystemID" minOccurs="0"/>
 * <xs:element name="DocumentStatus">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="PaymentStatus"/>
 * <xs:element ref="PaymentStatusDate"/>
 * <xs:element ref="Reason" minOccurs="0"/>
 * <xs:element ref="SourceID"/>
 * <xs:element name="SourcePayment" type="SAFTPTSourcePayment"/>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * <xs:element name="PaymentMethod" type="PaymentMethod" minOccurs="0" maxOccurs="unbounded"/>
 * <xs:element ref="SourceID"/>
 * <xs:element ref="SystemEntryDate"/>
 * <xs:element ref="CustomerID"/>
 * <xs:element name="Line" maxOccurs="unbounded">
 * <!-- Complex Line structure with SourceDocumentID -->
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * 
 * Description: Payments containing payment documents with source document references and totals.
 */

using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents payments with summary totals and individual payment documents.
    /// </summary>
    public class Payments
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
        /// Collection of payments (optional, unbounded).
        /// </summary>
        [XmlElement("Payment")]
        public List<Payment> PaymentDocuments { get; set; } = new List<Payment>();
    }

    /// <summary>
    /// Represents a payment document with all its details and lines.
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// The payment reference number.
        /// </summary>
        [XmlElement("PaymentRefNo")]
        public string PaymentRefNo { get; set; } = string.Empty;

        /// <summary>
        /// The ATCUD (Unique Document Code).
        /// </summary>
        [XmlElement("ATCUD")]
        public string ATCUD { get; set; } = string.Empty;

        /// <summary>
        /// The accounting period (optional).
        /// </summary>
        [XmlElement("Period")]
        public string? Period { get; set; }

        /// <summary>
        /// The transaction identifier (optional).
        /// </summary>
        [XmlElement("TransactionID")]
        public string? TransactionID { get; set; }

        /// <summary>
        /// The transaction date.
        /// </summary>
        [XmlElement("TransactionDate")]
        public string TransactionDate { get; set; } = string.Empty;

        /// <summary>
        /// The type of payment.
        /// </summary>
        [XmlElement("PaymentType")]
        public PaymentType PaymentType { get; set; }

        /// <summary>
        /// Description of the payment (optional).
        /// </summary>
        [XmlElement("Description")]
        public string? Description { get; set; }

        /// <summary>
        /// The system identifier (optional).
        /// </summary>
        [XmlElement("SystemID")]
        public string? SystemID { get; set; }

        /// <summary>
        /// The current status of the document.
        /// </summary>
        [XmlElement("DocumentStatus")]
        public PaymentDocumentStatus DocumentStatus { get; set; } = new PaymentDocumentStatus();

        /// <summary>
        /// Collection of payment methods (optional, unbounded).
        /// </summary>
        [XmlElement("PaymentMethod")]
        public List<PaymentMethod> PaymentMethods { get; set; } = new List<PaymentMethod>();

        /// <summary>
        /// The source identifier.
        /// </summary>
        [XmlElement("SourceID")]
        public string SourceID { get; set; } = string.Empty;

        /// <summary>
        /// The system entry date.
        /// </summary>
        [XmlElement("SystemEntryDate")]
        public string SystemEntryDate { get; set; } = string.Empty;

        /// <summary>
        /// The customer identifier.
        /// </summary>
        [XmlElement("CustomerID")]
        public string CustomerID { get; set; } = string.Empty;

        /// <summary>
        /// Collection of payment lines (unbounded).
        /// </summary>
        [XmlElement("Line")]
        public List<PaymentLine> Lines { get; set; } = new List<PaymentLine>();

        /// <summary>
        /// Document totals.
        /// </summary>
        [XmlElement("DocumentTotals")]
        public PaymentDocumentTotals DocumentTotals { get; set; } = new PaymentDocumentTotals();

        /// <summary>
        /// Collection of withholding taxes (optional, unbounded).
        /// </summary>
        [XmlElement("WithholdingTax")]
        public List<WithholdingTax> WithholdingTaxes { get; set; } = new List<WithholdingTax>();
    }

    /// <summary>
    /// Represents the current status of a payment document.
    /// </summary>
    public class PaymentDocumentStatus
    {
        /// <summary>
        /// The status of the payment.
        /// </summary>
        [XmlElement("PaymentStatus")]
        public PaymentStatus PaymentStatus { get; set; }

        /// <summary>
        /// The status date.
        /// </summary>
        [XmlElement("PaymentStatusDate")]
        public string PaymentStatusDate { get; set; } = string.Empty;

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
        /// The source payment information.
        /// </summary>
        [XmlElement("SourcePayment")]
        public SourcePayment SourcePayment { get; set; }
    }

    /// <summary>
    /// Represents a payment line with source document references and amounts.
    /// </summary>
    public class PaymentLine
    {
        /// <summary>
        /// The line number.
        /// </summary>
        [XmlElement("LineNumber")]
        public int LineNumber { get; set; }

        /// <summary>
        /// Collection of source document identifiers (unbounded).
        /// </summary>
        [XmlElement("SourceDocumentID")]
        public List<SourceDocumentID> SourceDocumentIDs { get; set; } = new List<SourceDocumentID>();

        /// <summary>
        /// Settlement amount (optional).
        /// </summary>
        [XmlElement("SettlementAmount")]
        public decimal? SettlementAmount { get; set; }

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
        public PaymentTax? Tax { get; set; }

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
    /// Represents a source document identifier with reference information.
    /// </summary>
    public class SourceDocumentID
    {
        /// <summary>
        /// The originating document number.
        /// </summary>
        [XmlElement("OriginatingON")]
        public string OriginatingON { get; set; } = string.Empty;

        /// <summary>
        /// The invoice date.
        /// </summary>
        [XmlElement("InvoiceDate")]
        public string InvoiceDate { get; set; } = string.Empty;

        /// <summary>
        /// Description of the source document (optional).
        /// </summary>
        [XmlElement("Description")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Represents document totals for payment documents.
    /// </summary>
    public class PaymentDocumentTotals
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
        /// Settlement information (optional).
        /// </summary>
        [XmlElement("Settlement")]
        public PaymentSettlement? Settlement { get; set; }

        /// <summary>
        /// Currency information (optional).
        /// </summary>
        [XmlElement("Currency")]
        public Currency? Currency { get; set; }
    }

    /// <summary>
    /// Represents settlement information for payments.
    /// </summary>
    public class PaymentSettlement
    {
        /// <summary>
        /// The settlement amount.
        /// </summary>
        [XmlElement("SettlementAmount")]
        public decimal SettlementAmount { get; set; }
    }
} 