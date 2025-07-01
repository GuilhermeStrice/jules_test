/*
 * Original XSD Schema:
 * <xs:element name="AuditFile">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="Header" minOccurs="1"/>
 * <xs:element name="MasterFiles">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="GeneralLedgerAccounts" minOccurs="0"/>
 * <xs:element ref="Customer" minOccurs="0" maxOccurs="unbounded"/>
 * <xs:element ref="Supplier" minOccurs="0" maxOccurs="unbounded"/>
 * <xs:element ref="Product" minOccurs="0" maxOccurs="unbounded"/>
 * <xs:element ref="TaxTable" minOccurs="0"/>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * <xs:element ref="GeneralLedgerEntries" minOccurs="0"/>
 * <xs:element ref="SourceDocuments" minOccurs="0"/>
 * </xs:sequence>
 * </xs:complexType>
 * <!-- Constraint-->
 * <xs:unique name="AccountIDConstraint">
 * <xs:selector xpath="ns:MasterFiles/ns:GeneralLedgerAccounts/ns:Account"/>
 * <xs:field xpath="ns:AccountID"/>
 * </xs:unique>
 * <xs:keyref name="GroupingCodeConstraint" refer="AccountIDConstraint">
 * <xs:selector xpath="ns:MasterFiles/ns:GeneralLedgerAccounts/ns:Account"/>
 * <xs:field xpath="ns:GroupingCode"/>
 * </xs:keyref>
 * <xs:unique name="CustomerIDConstraint">
 * <xs:selector xpath="ns:MasterFiles/ns:Customer"/>
 * <xs:field xpath="ns:CustomerID"/>
 * </xs:unique>
 * <xs:unique name="SupplierIDConstraint">
 * <xs:selector xpath="ns:MasterFiles/ns:Supplier"/>
 * <xs:field xpath="ns:SupplierID"/>
 * </xs:unique>
 * <xs:unique name="ProductCodeConstraint">
 * <xs:selector xpath="ns:MasterFiles/ns:Product"/>
 * <xs:field xpath="ns:ProductCode"/>
 * </xs:unique>
 * <xs:keyref name="GeneralLedgerEntriesDebitLineAccountIDConstraint" refer="AccountIDConstraint">
 * <xs:selector xpath="ns:GeneralLedgerEntries/ns:Journal/ns:Transaction/ns:Lines/ns:DebitLine"/>
 * <xs:field xpath="ns:AccountID"/>
 * </xs:keyref>
 * <xs:keyref name="GeneralLedgerEntriesCreditLineAccountIDConstraint" refer="AccountIDConstraint">
 * <xs:selector xpath="ns:GeneralLedgerEntries/ns:Journal/ns:Transaction/ns:Lines/ns:CreditLine"/>
 * <xs:field xpath="ns:AccountID"/>
 * </xs:keyref>
 * <xs:keyref name="GeneralLedgerEntriesCustomerIDConstraint" refer="CustomerIDConstraint">
 * <xs:selector xpath="ns:GeneralLedgerEntries/ns:Journal/ns:Transaction"/>
 * <xs:field xpath="ns:CustomerID"/>
 * </xs:keyref>
 * <xs:unique name="GeneralLedgerEntriesJournalIdConstraint">
 * <xs:selector xpath="ns:GeneralLedgerEntries/ns:Journal"/>
 * <xs:field xpath="ns:JournalID"/>
 * </xs:unique>
 * <xs:keyref name="GeneralLedgerEntriesSupplierIDConstraint" refer="SupplierIDConstraint">
 * <xs:selector xpath="ns:GeneralLedgerEntries/ns:Journal/ns:Transaction"/>
 * <xs:field xpath="ns:SupplierID"/>
 * </xs:keyref>
 * <xs:unique name="GeneralLedgerEntriesTransactionIdConstraint">
 * <xs:selector xpath="ns:GeneralLedgerEntries/ns:Journal/ns:Transaction"/>
 * <xs:field xpath="ns:TransactionID"/>
 * </xs:unique>
 * <xs:unique name="InvoiceNoConstraint">
 * <xs:selector xpath="ns:SourceDocuments/ns:SalesInvoices/ns:Invoice"/>
 * <xs:field xpath="ns:InvoiceNo"/>
 * </xs:unique>
 * <xs:keyref name="InvoiceCustomerIDConstraint" refer="CustomerIDConstraint">
 * <xs:selector xpath="ns:SourceDocuments/ns:SalesInvoices/ns:Invoice"/>
 * <xs:field xpath="ns:CustomerID"/>
 * </xs:keyref>
 * <xs:keyref name="InvoiceProductCodeConstraint" refer="ProductCodeConstraint">
 * <xs:selector xpath="ns:SourceDocuments/ns:SalesInvoices/ns:Invoice/ns:Line"/>
 * <xs:field xpath="ns:ProductCode"/>
 * </xs:keyref>
 * <xs:unique name="DocumentNumberConstraint">
 * <xs:selector xpath="ns:SourceDocuments/ns:MovementOfGoods/ns:StockMovement"/>
 * <xs:field xpath="ns:DocumentNumber"/>
 * </xs:unique>
 * <xs:keyref name="StockMovementCustomerIDConstraint" refer="CustomerIDConstraint">
 * <xs:selector xpath="ns:SourceDocuments/ns:MovementOfGoods/ns:StockMovement"/>
 * <xs:field xpath="ns:CustomerID"/>
 * </xs:keyref>
 * <xs:keyref name="StockMovementSupplierIDConstraint" refer="SupplierIDConstraint">
 * <xs:selector xpath="ns:SourceDocuments/ns:MovementOfGoods/ns:StockMovement"/>
 * <xs:field xpath="ns:SupplierID"/>
 * </xs:keyref>
 * <xs:keyref name="StockMovementProductCodeConstraint" refer="ProductCodeConstraint">
 * <xs:selector xpath="ns:SourceDocuments/ns:MovementOfGoods/ns:StockMovement/ns:Line"/>
 * <xs:field xpath="ns:ProductCode"/>
 * </xs:keyref>
 * <xs:unique name="WorkDocumentDocumentNumberConstraint">
 * <xs:selector xpath="ns:SourceDocuments/ns:WorkingDocuments/ns:WorkDocument"/>
 * <xs:field xpath="ns:DocumentNumber"/>
 * </xs:unique>
 * <xs:keyref name="WorkDocumentDocumentCustomerIDConstraint" refer="CustomerIDConstraint">
 * <xs:selector xpath="ns:SourceDocuments/ns:WorkingDocuments/ns:WorkDocument"/>
 * <xs:field xpath="ns:CustomerID"/>
 * </xs:keyref>
 * <xs:keyref name="WorkDocumentDocumentProductCodeConstraint" refer="ProductCodeConstraint">
 * <xs:selector xpath="ns:SourceDocuments/ns:WorkingDocuments/ns:WorkDocument/ns:Line"/>
 * <xs:field xpath="ns:ProductCode"/>
 * </xs:keyref>
 * <xs:unique name="PaymentPaymentRefNoConstraint">
 * <xs:selector xpath="ns:SourceDocuments/ns:Payments/ns:Payment"/>
 * <xs:field xpath="ns:PaymentRefNo"/>
 * </xs:unique>
 * <xs:keyref name="PaymentPaymentRefNoCustomerIDConstraint" refer="CustomerIDConstraint">
 * <xs:selector xpath="ns:SourceDocuments/ns:Payments/ns:Payment"/>
 * <xs:field xpath="ns:CustomerID"/>
 * </xs:keyref>
 * </xs:element>
 * 
 * Description: Main SAF-T audit file structure with header, master files, general ledger entries, and source documents.
 */

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SAFT.Lib.Utils;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents the main SAF-T audit file structure.
    /// </summary>
    [XmlRoot("AuditFile", Namespace = "urn:OECD:StandardAuditFile-Tax:PT_1.04_01")]
    public class AuditFile
    {
        /// <summary>
        /// The file header with company and file metadata.
        /// </summary>
        [XmlElement("Header")]
        public Header Header { get; set; } = new Header();

        /// <summary>
        /// The master files containing reference data.
        /// </summary>
        [XmlElement("MasterFiles")]
        public MasterFiles MasterFiles { get; set; } = new MasterFiles();

        /// <summary>
        /// The general ledger entries (optional).
        /// </summary>
        [XmlElement("GeneralLedgerEntries")]
        public GeneralLedgerEntries? GeneralLedgerEntries { get; set; }

        /// <summary>
        /// The source documents (optional).
        /// </summary>
        [XmlElement("SourceDocuments")]
        public SourceDocuments? SourceDocuments { get; set; }

        /// <summary>
        /// Saves the audit file to the configured output directory.
        /// </summary>
        /// <param name="fileName">Optional filename (defaults to "saft_{fiscalYear}.xml")</param>
        public void SaveToOutputDirectory(string? fileName = null)
        {
            fileName ??= $"saft_{Header.FiscalYear}.xml";
            XmlUtils.SerializeToFile(this, fileName);
        }
    }

    /// <summary>
    /// Represents the master files containing reference data.
    /// </summary>
    public class MasterFiles
    {
        /// <summary>
        /// The general ledger accounts (optional).
        /// </summary>
        [XmlElement("GeneralLedgerAccounts")]
        public GeneralLedgerAccounts? GeneralLedgerAccounts { get; set; }

        /// <summary>
        /// The list of customers (optional, unbounded).
        /// </summary>
        [XmlElement("Customer")]
        public List<Customer> Customers { get; set; } = new List<Customer>();

        /// <summary>
        /// The list of suppliers (optional, unbounded).
        /// </summary>
        [XmlElement("Supplier")]
        public List<Supplier> Suppliers { get; set; } = new List<Supplier>();

        /// <summary>
        /// The list of products (optional, unbounded).
        /// </summary>
        [XmlElement("Product")]
        public List<Product> Products { get; set; } = new List<Product>();

        /// <summary>
        /// The tax table (optional).
        /// </summary>
        [XmlElement("TaxTable")]
        public TaxTable? TaxTable { get; set; }
    }
} 