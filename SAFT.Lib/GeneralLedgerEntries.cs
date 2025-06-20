/*
 * Original XSD Schema:
 * <xs:element name="GeneralLedgerEntries">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="NumberOfEntries"/>
 * <xs:element ref="TotalDebit"/>
 * <xs:element ref="TotalCredit"/>
 * <xs:element name="Journal" minOccurs="0" maxOccurs="unbounded">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="JournalID"/>
 * <xs:element ref="Description"/>
 * <xs:element name="Transaction" minOccurs="0" maxOccurs="unbounded">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="TransactionID"/>
 * <xs:element name="Period" type="SAFPTAccountingPeriod"/>
 * <xs:element ref="TransactionDate"/>
 * <xs:element ref="SourceID"/>
 * <xs:element ref="Description"/>
 * <xs:element ref="DocArchivalNumber"/>
 * <xs:element ref="TransactionType"/>
 * <xs:element ref="GLPostingDate"/>
 * <xs:choice>
 * <xs:element ref="CustomerID" minOccurs="0"/>
 * <xs:element ref="SupplierID" minOccurs="0"/>
 * </xs:choice>
 * <xs:element name="Lines">
 * <xs:complexType>
 * <xs:all>
 * <xs:element name="DebitLine" maxOccurs="unbounded">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="RecordID"/>
 * <xs:element name="AccountID" type="SAFPTGLAccountID"/>
 * <xs:element ref="SourceDocumentID" minOccurs="0"/>
 * <xs:element ref="SystemEntryDate"/>
 * <xs:element ref="Description"/>
 * <xs:element name="DebitAmount" type="SAFmonetaryType"/>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * <xs:element name="CreditLine" maxOccurs="unbounded">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="RecordID"/>
 * <xs:element ref="AccountID" type="SAFPTGLAccountID"/>
 * <xs:element ref="SourceDocumentID" minOccurs="0"/>
 * <xs:element ref="SystemEntryDate"/>
 * <xs:element ref="Description"/>
 * <xs:element name="CreditAmount" type="SAFmonetaryType"/>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * </xs:all>
 * </xs:complexType>
 * </xs:element>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * 
 * Description: General ledger entries containing journals with transactions and their debit/credit lines.
 */

using System.Collections.Generic;
using System.Xml.Serialization;
using SAFT.Lib.Enums;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents general ledger entries with summary totals and journal details.
    /// </summary>
    [XmlRoot("GeneralLedgerEntries")]
    public class GeneralLedgerEntries
    {
        /// <summary>
        /// The number of entries in the general ledger.
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
        /// Collection of journals (optional, unbounded).
        /// </summary>
        [XmlElement("Journal")]
        public List<Journal> Journals { get; set; } = new List<Journal>();
    }

    /// <summary>
    /// Represents a journal containing multiple transactions.
    /// </summary>
    public class Journal
    {
        /// <summary>
        /// The unique identifier for the journal.
        /// </summary>
        [XmlElement("JournalID")]
        public string JournalID { get; set; } = string.Empty;

        /// <summary>
        /// Description of the journal.
        /// </summary>
        [XmlElement("Description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Collection of transactions in this journal (optional, unbounded).
        /// </summary>
        [XmlElement("Transaction")]
        public List<Transaction> Transactions { get; set; } = new List<Transaction>();
    }

    /// <summary>
    /// Represents a transaction with its details and debit/credit lines.
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// The unique identifier for the transaction.
        /// </summary>
        [XmlElement("TransactionID")]
        public string TransactionID { get; set; } = string.Empty;

        /// <summary>
        /// The accounting period for this transaction.
        /// </summary>
        [XmlElement("Period")]
        public int Period { get; set; }

        /// <summary>
        /// The date of the transaction.
        /// </summary>
        [XmlElement("TransactionDate")]
        public string TransactionDate { get; set; } = string.Empty;

        /// <summary>
        /// The source identifier.
        /// </summary>
        [XmlElement("SourceID")]
        public string SourceID { get; set; } = string.Empty;

        /// <summary>
        /// Description of the transaction.
        /// </summary>
        [XmlElement("Description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The document archival number.
        /// </summary>
        [XmlElement("DocArchivalNumber")]
        public string DocArchivalNumber { get; set; } = string.Empty;

        /// <summary>
        /// The type of transaction.
        /// </summary>
        [XmlElement("TransactionType")]
        public TransactionType TransactionType { get; set; }

        /// <summary>
        /// The general ledger posting date.
        /// </summary>
        [XmlElement("GLPostingDate")]
        public string GLPostingDate { get; set; } = string.Empty;

        /// <summary>
        /// The customer identifier (optional, used when SupplierID is not specified).
        /// </summary>
        [XmlElement("CustomerID")]
        public string? CustomerID { get; set; }

        /// <summary>
        /// The supplier identifier (optional, used when CustomerID is not specified).
        /// </summary>
        [XmlElement("SupplierID")]
        public string? SupplierID { get; set; }

        /// <summary>
        /// The lines containing debit and credit entries.
        /// </summary>
        [XmlElement("Lines")]
        public TransactionLines Lines { get; set; } = new TransactionLines();

        /// <summary>
        /// Indicates whether CustomerID is set (for XML serialization choice).
        /// </summary>
        [XmlIgnore]
        public bool CustomerIDSpecified => !string.IsNullOrEmpty(CustomerID);

        /// <summary>
        /// Indicates whether SupplierID is set (for XML serialization choice).
        /// </summary>
        [XmlIgnore]
        public bool SupplierIDSpecified => !string.IsNullOrEmpty(SupplierID);
    }

    /// <summary>
    /// Represents the debit and credit lines of a transaction.
    /// </summary>
    public class TransactionLines
    {
        /// <summary>
        /// Collection of debit lines (unbounded).
        /// </summary>
        [XmlElement("DebitLine")]
        public List<DebitLine> DebitLines { get; set; } = new List<DebitLine>();

        /// <summary>
        /// Collection of credit lines (unbounded).
        /// </summary>
        [XmlElement("CreditLine")]
        public List<CreditLine> CreditLines { get; set; } = new List<CreditLine>();
    }

    /// <summary>
    /// Represents a debit line in a transaction.
    /// </summary>
    public class DebitLine
    {
        /// <summary>
        /// The record identifier.
        /// </summary>
        [XmlElement("RecordID")]
        public string RecordID { get; set; } = string.Empty;

        /// <summary>
        /// The general ledger account identifier.
        /// </summary>
        [XmlElement("AccountID")]
        public GLAccountID AccountID { get; set; } = new GLAccountID();

        /// <summary>
        /// The source document identifier (optional).
        /// </summary>
        [XmlElement("SourceDocumentID")]
        public string? SourceDocumentID { get; set; }

        /// <summary>
        /// The system entry date.
        /// </summary>
        [XmlElement("SystemEntryDate")]
        public string SystemEntryDate { get; set; } = string.Empty;

        /// <summary>
        /// Description of the debit line.
        /// </summary>
        [XmlElement("Description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The debit amount.
        /// </summary>
        [XmlElement("DebitAmount")]
        public decimal DebitAmount { get; set; }
    }

    /// <summary>
    /// Represents a credit line in a transaction.
    /// </summary>
    public class CreditLine
    {
        /// <summary>
        /// The record identifier.
        /// </summary>
        [XmlElement("RecordID")]
        public string RecordID { get; set; } = string.Empty;

        /// <summary>
        /// The general ledger account identifier.
        /// </summary>
        [XmlElement("AccountID")]
        public GLAccountID AccountID { get; set; } = new GLAccountID();

        /// <summary>
        /// The source document identifier (optional).
        /// </summary>
        [XmlElement("SourceDocumentID")]
        public string? SourceDocumentID { get; set; }

        /// <summary>
        /// The system entry date.
        /// </summary>
        [XmlElement("SystemEntryDate")]
        public string SystemEntryDate { get; set; } = string.Empty;

        /// <summary>
        /// Description of the credit line.
        /// </summary>
        [XmlElement("Description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The credit amount.
        /// </summary>
        [XmlElement("CreditAmount")]
        public decimal CreditAmount { get; set; }
    }
} 