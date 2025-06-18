using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SaftPtGenerator
{
    /// <summary>
    /// Complete SAFT-PT Generator for Portuguese Tax Audit Files
    /// Based on SAFT-PT schema version 1.04_01
    /// </summary>
    public class SaftPtGenerator
    {
        private readonly AuditFile _auditFile;
        private readonly string _privateKeyPath;
        private readonly string _publicKeyPath;

        public SaftPtGenerator(string privateKeyPath = null, string publicKeyPath = null)
        {
            _auditFile = new AuditFile();
            _privateKeyPath = privateKeyPath;
            _publicKeyPath = publicKeyPath;
        }

        /// <summary>
        /// Initialize the audit file header
        /// </summary>
        public void InitializeHeader(
            string companyId,
            int taxRegistrationNumber,
            string companyName,
            string businessName,
            Address companyAddress,
            int fiscalYear,
            DateTime startDate,
            DateTime endDate,
            DateTime dateCreated,
            string taxEntity,
            string productCompanyTaxId,
            int softwareCertificateNumber,
            string productId,
            string productVersion)
        {
            _auditFile.Header = new Header
            {
                AuditFileVersion = "1.04_01",
                CompanyID = companyId,
                TaxRegistrationNumber = taxRegistrationNumber,
                TaxAccountingBasis = "F", // Faturação
                CompanyName = companyName,
                BusinessName = businessName,
                CompanyAddress = companyAddress,
                FiscalYear = fiscalYear,
                StartDate = startDate,
                EndDate = endDate,
                CurrencyCode = "EUR",
                DateCreated = dateCreated,
                TaxEntity = taxEntity,
                ProductCompanyTaxID = productCompanyTaxId,
                SoftwareCertificateNumber = softwareCertificateNumber,
                ProductID = productId,
                ProductVersion = productVersion
            };
        }

        /// <summary>
        /// Add a customer
        /// </summary>
        public void AddCustomer(Customer customer)
        {
            if (_auditFile.MasterFiles == null)
                _auditFile.MasterFiles = new MasterFiles();

            if (_auditFile.MasterFiles.Customer == null)
                _auditFile.MasterFiles.Customer = new List<Customer>();

            _auditFile.MasterFiles.Customer.Add(customer);
        }

        /// <summary>
        /// Add a supplier
        /// </summary>
        public void AddSupplier(Supplier supplier)
        {
            if (_auditFile.MasterFiles == null)
                _auditFile.MasterFiles = new MasterFiles();

            if (_auditFile.MasterFiles.Supplier == null)
                _auditFile.MasterFiles.Supplier = new List<Supplier>();

            _auditFile.MasterFiles.Supplier.Add(supplier);
        }

        /// <summary>
        /// Add a product
        /// </summary>
        public void AddProduct(Product product)
        {
            if (_auditFile.MasterFiles == null)
                _auditFile.MasterFiles = new MasterFiles();

            if (_auditFile.MasterFiles.Product == null)
                _auditFile.MasterFiles.Product = new List<Product>();

            _auditFile.MasterFiles.Product.Add(product);
        }

        /// <summary>
        /// Add general ledger accounts
        /// </summary>
        public void SetGeneralLedgerAccounts(GeneralLedgerAccounts accounts)
        {
            if (_auditFile.MasterFiles == null)
                _auditFile.MasterFiles = new MasterFiles();

            _auditFile.MasterFiles.GeneralLedgerAccounts = accounts;
        }

        /// <summary>
        /// Add tax table
        /// </summary>
        public void SetTaxTable(TaxTable taxTable)
        {
            if (_auditFile.MasterFiles == null)
                _auditFile.MasterFiles = new MasterFiles();

            _auditFile.MasterFiles.TaxTable = taxTable;
        }

        /// <summary>
        /// Add general ledger entries
        /// </summary>
        public void SetGeneralLedgerEntries(GeneralLedgerEntries entries)
        {
            _auditFile.GeneralLedgerEntries = entries;
        }

        /// <summary>
        /// Add source documents
        /// </summary>
        public void SetSourceDocuments(SourceDocuments documents)
        {
            _auditFile.SourceDocuments = documents;
        }

        /// <summary>
        /// Generate the SAFT-PT XML file
        /// </summary>
        public void GenerateFile(string filePath)
        {
            var serializer = new XmlSerializer(typeof(AuditFile));
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8
            };

            using (var writer = XmlWriter.Create(filePath, settings))
            {
                var ns = new XmlSerializerNamespaces();
                ns.Add("", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
                serializer.Serialize(writer, _auditFile, ns);
            }
        }

        /// <summary>
        /// Create digital signature hash
        /// </summary>
        public string CreateSignature(DateTime docDate, DateTime systemEntryDate, string docNumber, decimal grossTotal, string lastHash = "")
        {
            if (string.IsNullOrEmpty(_privateKeyPath))
                throw new InvalidOperationException("Private key path is required for signature creation");

            var data = $"{docDate:yyyy-MM-dd}{systemEntryDate:yyyy-MM-dd HH:mm:ss}{docNumber}{grossTotal:F2}{lastHash}";
            
            using (var rsa = RSA.Create())
            {
                var privateKey = File.ReadAllText(_privateKeyPath);
                rsa.ImportFromPem(privateKey);
                
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                
                return Convert.ToBase64String(signature);
            }
        }

        /// <summary>
        /// Verify digital signature
        /// </summary>
        public bool VerifySignature(string signature, DateTime docDate, DateTime systemEntryDate, string docNumber, decimal grossTotal, string lastHash = "")
        {
            if (string.IsNullOrEmpty(_publicKeyPath))
                throw new InvalidOperationException("Public key path is required for signature verification");

            var data = $"{docDate:yyyy-MM-dd}{systemEntryDate:yyyy-MM-dd HH:mm:ss}{docNumber}{grossTotal:F2}{lastHash}";
            
            using (var rsa = RSA.Create())
            {
                var publicKey = File.ReadAllText(_publicKeyPath);
                rsa.ImportFromPem(publicKey);
                
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var signatureBytes = Convert.FromBase64String(signature);
                
                return rsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
        }

        /// <summary>
        /// Get the current audit file for inspection
        /// </summary>
        public AuditFile GetAuditFile() => _auditFile;
    }

    #region Data Classes

    [XmlRoot("AuditFile", Namespace = "urn:OECD:StandardAuditFile-Tax:PT_1.04_01")]
    public class AuditFile
    {
        [XmlElement("Header")]
        public Header Header { get; set; }

        [XmlElement("MasterFiles")]
        public MasterFiles MasterFiles { get; set; }

        [XmlElement("GeneralLedgerEntries")]
        public GeneralLedgerEntries GeneralLedgerEntries { get; set; }

        [XmlElement("SourceDocuments")]
        public SourceDocuments SourceDocuments { get; set; }
    }

    public class Header
    {
        [XmlElement("AuditFileVersion")]
        public string AuditFileVersion { get; set; }

        [XmlElement("CompanyID")]
        public string CompanyID { get; set; }

        [XmlElement("TaxRegistrationNumber")]
        public int TaxRegistrationNumber { get; set; }

        [XmlElement("TaxAccountingBasis")]
        public string TaxAccountingBasis { get; set; }

        [XmlElement("CompanyName")]
        public string CompanyName { get; set; }

        [XmlElement("BusinessName")]
        public string BusinessName { get; set; }

        [XmlElement("CompanyAddress")]
        public Address CompanyAddress { get; set; }

        [XmlElement("FiscalYear")]
        public int FiscalYear { get; set; }

        [XmlElement("StartDate")]
        public DateTime StartDate { get; set; }

        [XmlElement("EndDate")]
        public DateTime EndDate { get; set; }

        [XmlElement("CurrencyCode")]
        public string CurrencyCode { get; set; }

        [XmlElement("DateCreated")]
        public DateTime DateCreated { get; set; }

        [XmlElement("TaxEntity")]
        public string TaxEntity { get; set; }

        [XmlElement("ProductCompanyTaxID")]
        public string ProductCompanyTaxID { get; set; }

        [XmlElement("SoftwareCertificateNumber")]
        public int SoftwareCertificateNumber { get; set; }

        [XmlElement("ProductID")]
        public string ProductID { get; set; }

        [XmlElement("ProductVersion")]
        public string ProductVersion { get; set; }

        [XmlElement("HeaderComment")]
        public string HeaderComment { get; set; }

        [XmlElement("Telephone")]
        public string Telephone { get; set; }

        [XmlElement("Fax")]
        public string Fax { get; set; }

        [XmlElement("Email")]
        public string Email { get; set; }

        [XmlElement("Website")]
        public string Website { get; set; }
    }

    public class MasterFiles
    {
        [XmlElement("GeneralLedgerAccounts")]
        public GeneralLedgerAccounts GeneralLedgerAccounts { get; set; }

        [XmlElement("Customer")]
        public List<Customer> Customer { get; set; }

        [XmlElement("Supplier")]
        public List<Supplier> Supplier { get; set; }

        [XmlElement("Product")]
        public List<Product> Product { get; set; }

        [XmlElement("TaxTable")]
        public TaxTable TaxTable { get; set; }
    }

    public class GeneralLedgerAccounts
    {
        [XmlElement("TaxonomyReference")]
        public string TaxonomyReference { get; set; }

        [XmlElement("Account")]
        public List<Account> Account { get; set; }
    }

    public class Account
    {
        [XmlElement("AccountID")]
        public string AccountID { get; set; }

        [XmlElement("AccountDescription")]
        public string AccountDescription { get; set; }

        [XmlElement("OpeningDebitBalance")]
        public decimal OpeningDebitBalance { get; set; }

        [XmlElement("OpeningCreditBalance")]
        public decimal OpeningCreditBalance { get; set; }

        [XmlElement("ClosingDebitBalance")]
        public decimal ClosingDebitBalance { get; set; }

        [XmlElement("ClosingCreditBalance")]
        public decimal ClosingCreditBalance { get; set; }

        [XmlElement("GroupingCategory")]
        public string GroupingCategory { get; set; }

        [XmlElement("GroupingCode")]
        public string GroupingCode { get; set; }

        [XmlElement("TaxonomyCode")]
        public int? TaxonomyCode { get; set; }
    }

    public class Customer
    {
        [XmlElement("CustomerID")]
        public string CustomerID { get; set; }

        [XmlElement("AccountID")]
        public string AccountID { get; set; }

        [XmlElement("CustomerTaxID")]
        public string CustomerTaxID { get; set; }

        [XmlElement("CompanyName")]
        public string CompanyName { get; set; }

        [XmlElement("Contact")]
        public string Contact { get; set; }

        [XmlElement("BillingAddress")]
        public CustomerAddress BillingAddress { get; set; }

        [XmlElement("ShipToAddress")]
        public List<CustomerAddress> ShipToAddress { get; set; }

        [XmlElement("Telephone")]
        public string Telephone { get; set; }

        [XmlElement("Fax")]
        public string Fax { get; set; }

        [XmlElement("Email")]
        public string Email { get; set; }

        [XmlElement("Website")]
        public string Website { get; set; }

        [XmlElement("SelfBillingIndicator")]
        public int SelfBillingIndicator { get; set; }
    }

    public class Supplier
    {
        [XmlElement("SupplierID")]
        public string SupplierID { get; set; }

        [XmlElement("AccountID")]
        public string AccountID { get; set; }

        [XmlElement("SupplierTaxID")]
        public string SupplierTaxID { get; set; }

        [XmlElement("CompanyName")]
        public string CompanyName { get; set; }

        [XmlElement("Contact")]
        public string Contact { get; set; }

        [XmlElement("BillingAddress")]
        public Address BillingAddress { get; set; }

        [XmlElement("ShipFromAddress")]
        public List<Address> ShipFromAddress { get; set; }

        [XmlElement("Telephone")]
        public string Telephone { get; set; }

        [XmlElement("Fax")]
        public string Fax { get; set; }

        [XmlElement("Email")]
        public string Email { get; set; }

        [XmlElement("Website")]
        public string Website { get; set; }

        [XmlElement("SelfBillingIndicator")]
        public int SelfBillingIndicator { get; set; }
    }

    public class Product
    {
        [XmlElement("ProductType")]
        public string ProductType { get; set; }

        [XmlElement("ProductCode")]
        public string ProductCode { get; set; }

        [XmlElement("ProductGroup")]
        public string ProductGroup { get; set; }

        [XmlElement("ProductDescription")]
        public string ProductDescription { get; set; }

        [XmlElement("ProductNumberCode")]
        public string ProductNumberCode { get; set; }

        [XmlElement("CustomsDetails")]
        public CustomsDetails CustomsDetails { get; set; }
    }

    public class TaxTable
    {
        [XmlElement("TaxTableEntry")]
        public List<TaxTableEntry> TaxTableEntry { get; set; }
    }

    public class TaxTableEntry
    {
        [XmlElement("TaxType")]
        public string TaxType { get; set; }

        [XmlElement("TaxCountryRegion")]
        public string TaxCountryRegion { get; set; }

        [XmlElement("TaxCode")]
        public string TaxCode { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("TaxExpirationDate")]
        public DateTime? TaxExpirationDate { get; set; }

        [XmlElement("TaxPercentage")]
        public decimal? TaxPercentage { get; set; }

        [XmlElement("TaxAmount")]
        public decimal? TaxAmount { get; set; }
    }

    public class GeneralLedgerEntries
    {
        [XmlElement("NumberOfEntries")]
        public int NumberOfEntries { get; set; }

        [XmlElement("TotalDebit")]
        public decimal TotalDebit { get; set; }

        [XmlElement("TotalCredit")]
        public decimal TotalCredit { get; set; }

        [XmlElement("Journal")]
        public List<Journal> Journal { get; set; }
    }

    public class Journal
    {
        [XmlElement("JournalID")]
        public string JournalID { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("Transaction")]
        public List<Transaction> Transaction { get; set; }
    }

    public class Transaction
    {
        [XmlElement("TransactionID")]
        public string TransactionID { get; set; }

        [XmlElement("Period")]
        public int Period { get; set; }

        [XmlElement("TransactionDate")]
        public DateTime TransactionDate { get; set; }

        [XmlElement("SourceID")]
        public string SourceID { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("DocArchivalNumber")]
        public string DocArchivalNumber { get; set; }

        [XmlElement("TransactionType")]
        public string TransactionType { get; set; }

        [XmlElement("GLPostingDate")]
        public DateTime GLPostingDate { get; set; }

        [XmlElement("CustomerID")]
        public string CustomerID { get; set; }

        [XmlElement("SupplierID")]
        public string SupplierID { get; set; }

        [XmlElement("Lines")]
        public TransactionLines Lines { get; set; }
    }

    public class TransactionLines
    {
        [XmlElement("DebitLine")]
        public List<DebitLine> DebitLine { get; set; }

        [XmlElement("CreditLine")]
        public List<CreditLine> CreditLine { get; set; }
    }

    public class DebitLine
    {
        [XmlElement("RecordID")]
        public string RecordID { get; set; }

        [XmlElement("AccountID")]
        public string AccountID { get; set; }

        [XmlElement("SourceDocumentID")]
        public string SourceDocumentID { get; set; }

        [XmlElement("SystemEntryDate")]
        public DateTime SystemEntryDate { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("DebitAmount")]
        public decimal DebitAmount { get; set; }
    }

    public class CreditLine
    {
        [XmlElement("RecordID")]
        public string RecordID { get; set; }

        [XmlElement("AccountID")]
        public string AccountID { get; set; }

        [XmlElement("SourceDocumentID")]
        public string SourceDocumentID { get; set; }

        [XmlElement("SystemEntryDate")]
        public DateTime SystemEntryDate { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("CreditAmount")]
        public decimal CreditAmount { get; set; }
    }

    public class SourceDocuments
    {
        [XmlElement("SalesInvoices")]
        public SalesInvoices SalesInvoices { get; set; }

        [XmlElement("MovementOfGoods")]
        public MovementOfGoods MovementOfGoods { get; set; }

        [XmlElement("WorkingDocuments")]
        public WorkingDocuments WorkingDocuments { get; set; }

        [XmlElement("Payments")]
        public Payments Payments { get; set; }
    }

    public class SalesInvoices
    {
        [XmlElement("NumberOfEntries")]
        public int NumberOfEntries { get; set; }

        [XmlElement("TotalDebit")]
        public decimal TotalDebit { get; set; }

        [XmlElement("TotalCredit")]
        public decimal TotalCredit { get; set; }

        [XmlElement("Invoice")]
        public List<Invoice> Invoice { get; set; }
    }

    public class Invoice
    {
        [XmlElement("InvoiceNo")]
        public string InvoiceNo { get; set; }

        [XmlElement("ATCUD")]
        public string ATCUD { get; set; }

        [XmlElement("DocumentStatus")]
        public InvoiceDocumentStatus DocumentStatus { get; set; }

        [XmlElement("Hash")]
        public string Hash { get; set; }

        [XmlElement("HashControl")]
        public string HashControl { get; set; }

        [XmlElement("Period")]
        public int? Period { get; set; }

        [XmlElement("InvoiceDate")]
        public DateTime InvoiceDate { get; set; }

        [XmlElement("InvoiceType")]
        public string InvoiceType { get; set; }

        [XmlElement("SpecialRegimes")]
        public SpecialRegimes SpecialRegimes { get; set; }

        [XmlElement("SourceID")]
        public string SourceID { get; set; }

        [XmlElement("EACCode")]
        public string EACCode { get; set; }

        [XmlElement("SystemEntryDate")]
        public DateTime SystemEntryDate { get; set; }

        [XmlElement("TransactionID")]
        public string TransactionID { get; set; }

        [XmlElement("CustomerID")]
        public string CustomerID { get; set; }

        [XmlElement("ShipTo")]
        public ShippingPoint ShipTo { get; set; }

        [XmlElement("ShipFrom")]
        public ShippingPoint ShipFrom { get; set; }

        [XmlElement("MovementEndTime")]
        public DateTime? MovementEndTime { get; set; }

        [XmlElement("MovementStartTime")]
        public DateTime? MovementStartTime { get; set; }

        [XmlElement("Line")]
        public List<InvoiceLine> Line { get; set; }

        [XmlElement("DocumentTotals")]
        public InvoiceDocumentTotals DocumentTotals { get; set; }

        [XmlElement("WithholdingTax")]
        public List<WithholdingTax> WithholdingTax { get; set; }
    }

    public class InvoiceDocumentStatus
    {
        [XmlElement("InvoiceStatus")]
        public string InvoiceStatus { get; set; }

        [XmlElement("InvoiceStatusDate")]
        public DateTime InvoiceStatusDate { get; set; }

        [XmlElement("Reason")]
        public string Reason { get; set; }

        [XmlElement("SourceID")]
        public string SourceID { get; set; }

        [XmlElement("SourceBilling")]
        public string SourceBilling { get; set; }
    }

    public class InvoiceLine
    {
        [XmlElement("LineNumber")]
        public int LineNumber { get; set; }

        [XmlElement("OrderReferences")]
        public List<OrderReferences> OrderReferences { get; set; }

        [XmlElement("ProductCode")]
        public string ProductCode { get; set; }

        [XmlElement("ProductDescription")]
        public string ProductDescription { get; set; }

        [XmlElement("Quantity")]
        public decimal Quantity { get; set; }

        [XmlElement("UnitOfMeasure")]
        public string UnitOfMeasure { get; set; }

        [XmlElement("UnitPrice")]
        public decimal UnitPrice { get; set; }

        [XmlElement("TaxBase")]
        public decimal? TaxBase { get; set; }

        [XmlElement("TaxPointDate")]
        public DateTime TaxPointDate { get; set; }

        [XmlElement("References")]
        public List<References> References { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("ProductSerialNumber")]
        public ProductSerialNumber ProductSerialNumber { get; set; }

        [XmlElement("DebitAmount")]
        public decimal? DebitAmount { get; set; }

        [XmlElement("CreditAmount")]
        public decimal? CreditAmount { get; set; }

        [XmlElement("Tax")]
        public Tax Tax { get; set; }

        [XmlElement("TaxExemptionReason")]
        public string TaxExemptionReason { get; set; }

        [XmlElement("TaxExemptionCode")]
        public string TaxExemptionCode { get; set; }

        [XmlElement("SettlementAmount")]
        public decimal? SettlementAmount { get; set; }

        [XmlElement("CustomsInformation")]
        public CustomsInformation CustomsInformation { get; set; }
    }

    public class InvoiceDocumentTotals
    {
        [XmlElement("TaxPayable")]
        public decimal TaxPayable { get; set; }

        [XmlElement("NetTotal")]
        public decimal NetTotal { get; set; }

        [XmlElement("GrossTotal")]
        public decimal GrossTotal { get; set; }

        [XmlElement("Currency")]
        public Currency Currency { get; set; }

        [XmlElement("Settlement")]
        public List<Settlement> Settlement { get; set; }

        [XmlElement("Payment")]
        public List<PaymentMethod> Payment { get; set; }
    }

    public class MovementOfGoods
    {
        [XmlElement("NumberOfMovementLines")]
        public int NumberOfMovementLines { get; set; }

        [XmlElement("TotalQuantityIssued")]
        public decimal TotalQuantityIssued { get; set; }

        [XmlElement("StockMovement")]
        public List<StockMovement> StockMovement { get; set; }
    }

    public class StockMovement
    {
        [XmlElement("DocumentNumber")]
        public string DocumentNumber { get; set; }

        [XmlElement("ATCUD")]
        public string ATCUD { get; set; }

        [XmlElement("DocumentStatus")]
        public MovementDocumentStatus DocumentStatus { get; set; }

        [XmlElement("Hash")]
        public string Hash { get; set; }

        [XmlElement("HashControl")]
        public string HashControl { get; set; }

        [XmlElement("Period")]
        public int? Period { get; set; }

        [XmlElement("MovementDate")]
        public DateTime MovementDate { get; set; }

        [XmlElement("MovementType")]
        public string MovementType { get; set; }

        [XmlElement("SystemEntryDate")]
        public DateTime SystemEntryDate { get; set; }

        [XmlElement("TransactionID")]
        public string TransactionID { get; set; }

        [XmlElement("CustomerID")]
        public string CustomerID { get; set; }

        [XmlElement("SupplierID")]
        public string SupplierID { get; set; }

        [XmlElement("SourceID")]
        public string SourceID { get; set; }

        [XmlElement("EACCode")]
        public string EACCode { get; set; }

        [XmlElement("MovementComments")]
        public string MovementComments { get; set; }

        [XmlElement("ShipTo")]
        public ShippingPoint ShipTo { get; set; }

        [XmlElement("ShipFrom")]
        public ShippingPoint ShipFrom { get; set; }

        [XmlElement("MovementEndTime")]
        public DateTime? MovementEndTime { get; set; }

        [XmlElement("MovementStartTime")]
        public DateTime? MovementStartTime { get; set; }

        [XmlElement("ATDocCodeID")]
        public string ATDocCodeID { get; set; }

        [XmlElement("Line")]
        public List<MovementLine> Line { get; set; }

        [XmlElement("DocumentTotals")]
        public MovementDocumentTotals DocumentTotals { get; set; }
    }

    public class MovementDocumentStatus
    {
        [XmlElement("MovementStatus")]
        public string MovementStatus { get; set; }

        [XmlElement("MovementStatusDate")]
        public DateTime MovementStatusDate { get; set; }

        [XmlElement("Reason")]
        public string Reason { get; set; }

        [XmlElement("SourceID")]
        public string SourceID { get; set; }

        [XmlElement("SourceBilling")]
        public string SourceBilling { get; set; }
    }

    public class MovementLine
    {
        [XmlElement("LineNumber")]
        public int LineNumber { get; set; }

        [XmlElement("OrderReferences")]
        public List<OrderReferences> OrderReferences { get; set; }

        [XmlElement("ProductCode")]
        public string ProductCode { get; set; }

        [XmlElement("ProductDescription")]
        public string ProductDescription { get; set; }

        [XmlElement("Quantity")]
        public decimal Quantity { get; set; }

        [XmlElement("UnitOfMeasure")]
        public string UnitOfMeasure { get; set; }

        [XmlElement("UnitPrice")]
        public decimal UnitPrice { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("ProductSerialNumber")]
        public ProductSerialNumber ProductSerialNumber { get; set; }

        [XmlElement("DebitAmount")]
        public decimal? DebitAmount { get; set; }

        [XmlElement("CreditAmount")]
        public decimal? CreditAmount { get; set; }

        [XmlElement("Tax")]
        public MovementTax Tax { get; set; }

        [XmlElement("TaxExemptionReason")]
        public string TaxExemptionReason { get; set; }

        [XmlElement("TaxExemptionCode")]
        public string TaxExemptionCode { get; set; }

        [XmlElement("SettlementAmount")]
        public decimal? SettlementAmount { get; set; }

        [XmlElement("CustomsInformation")]
        public CustomsInformation CustomsInformation { get; set; }
    }

    public class MovementDocumentTotals
    {
        [XmlElement("TaxPayable")]
        public decimal TaxPayable { get; set; }

        [XmlElement("NetTotal")]
        public decimal NetTotal { get; set; }

        [XmlElement("GrossTotal")]
        public decimal GrossTotal { get; set; }

        [XmlElement("Currency")]
        public Currency Currency { get; set; }
    }

    public class WorkingDocuments
    {
        [XmlElement("NumberOfEntries")]
        public int NumberOfEntries { get; set; }

        [XmlElement("TotalDebit")]
        public decimal TotalDebit { get; set; }

        [XmlElement("TotalCredit")]
        public decimal TotalCredit { get; set; }

        [XmlElement("WorkDocument")]
        public List<WorkDocument> WorkDocument { get; set; }
    }

    public class WorkDocument
    {
        [XmlElement("DocumentNumber")]
        public string DocumentNumber { get; set; }

        [XmlElement("ATCUD")]
        public string ATCUD { get; set; }

        [XmlElement("DocumentStatus")]
        public WorkDocumentStatus DocumentStatus { get; set; }

        [XmlElement("Hash")]
        public string Hash { get; set; }

        [XmlElement("HashControl")]
        public string HashControl { get; set; }

        [XmlElement("Period")]
        public int? Period { get; set; }

        [XmlElement("WorkDate")]
        public DateTime WorkDate { get; set; }

        [XmlElement("WorkType")]
        public string WorkType { get; set; }

        [XmlElement("SourceID")]
        public string SourceID { get; set; }

        [XmlElement("EACCode")]
        public string EACCode { get; set; }

        [XmlElement("SystemEntryDate")]
        public DateTime SystemEntryDate { get; set; }

        [XmlElement("TransactionID")]
        public string TransactionID { get; set; }

        [XmlElement("CustomerID")]
        public string CustomerID { get; set; }

        [XmlElement("Line")]
        public List<WorkDocumentLine> Line { get; set; }

        [XmlElement("DocumentTotals")]
        public WorkDocumentTotals DocumentTotals { get; set; }
    }

    public class WorkDocumentStatus
    {
        [XmlElement("WorkStatus")]
        public string WorkStatus { get; set; }

        [XmlElement("WorkStatusDate")]
        public DateTime WorkStatusDate { get; set; }

        [XmlElement("Reason")]
        public string Reason { get; set; }

        [XmlElement("SourceID")]
        public string SourceID { get; set; }

        [XmlElement("SourceBilling")]
        public string SourceBilling { get; set; }
    }

    public class WorkDocumentLine
    {
        [XmlElement("LineNumber")]
        public int LineNumber { get; set; }

        [XmlElement("OrderReferences")]
        public List<OrderReferences> OrderReferences { get; set; }

        [XmlElement("ProductCode")]
        public string ProductCode { get; set; }

        [XmlElement("ProductDescription")]
        public string ProductDescription { get; set; }

        [XmlElement("Quantity")]
        public decimal Quantity { get; set; }

        [XmlElement("UnitOfMeasure")]
        public string UnitOfMeasure { get; set; }

        [XmlElement("UnitPrice")]
        public decimal UnitPrice { get; set; }

        [XmlElement("TaxBase")]
        public decimal? TaxBase { get; set; }

        [XmlElement("TaxPointDate")]
        public DateTime TaxPointDate { get; set; }

        [XmlElement("References")]
        public List<References> References { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("ProductSerialNumber")]
        public ProductSerialNumber ProductSerialNumber { get; set; }

        [XmlElement("DebitAmount")]
        public decimal? DebitAmount { get; set; }

        [XmlElement("CreditAmount")]
        public decimal? CreditAmount { get; set; }

        [XmlElement("Tax")]
        public Tax Tax { get; set; }

        [XmlElement("TaxExemptionReason")]
        public string TaxExemptionReason { get; set; }

        [XmlElement("TaxExemptionCode")]
        public string TaxExemptionCode { get; set; }

        [XmlElement("SettlementAmount")]
        public decimal? SettlementAmount { get; set; }

        [XmlElement("CustomsInformation")]
        public CustomsInformation CustomsInformation { get; set; }
    }

    public class WorkDocumentTotals
    {
        [XmlElement("TaxPayable")]
        public decimal TaxPayable { get; set; }

        [XmlElement("NetTotal")]
        public decimal NetTotal { get; set; }

        [XmlElement("GrossTotal")]
        public decimal GrossTotal { get; set; }

        [XmlElement("Currency")]
        public Currency Currency { get; set; }
    }

    public class Payments
    {
        [XmlElement("NumberOfEntries")]
        public int NumberOfEntries { get; set; }

        [XmlElement("TotalDebit")]
        public decimal TotalDebit { get; set; }

        [XmlElement("TotalCredit")]
        public decimal TotalCredit { get; set; }

        [XmlElement("Payment")]
        public List<Payment> Payment { get; set; }
    }

    public class Payment
    {
        [XmlElement("PaymentRefNo")]
        public string PaymentRefNo { get; set; }

        [XmlElement("ATCUD")]
        public string ATCUD { get; set; }

        [XmlElement("Period")]
        public int? Period { get; set; }

        [XmlElement("TransactionID")]
        public string TransactionID { get; set; }

        [XmlElement("TransactionDate")]
        public DateTime TransactionDate { get; set; }

        [XmlElement("PaymentType")]
        public string PaymentType { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("SystemID")]
        public string SystemID { get; set; }

        [XmlElement("DocumentStatus")]
        public PaymentDocumentStatus DocumentStatus { get; set; }

        [XmlElement("PaymentMethod")]
        public List<PaymentMethod> PaymentMethod { get; set; }

        [XmlElement("SourceID")]
        public string SourceID { get; set; }

        [XmlElement("SystemEntryDate")]
        public DateTime SystemEntryDate { get; set; }

        [XmlElement("CustomerID")]
        public string CustomerID { get; set; }

        [XmlElement("Line")]
        public List<PaymentLine> Line { get; set; }

        [XmlElement("DocumentTotals")]
        public PaymentDocumentTotals DocumentTotals { get; set; }

        [XmlElement("WithholdingTax")]
        public List<WithholdingTax> WithholdingTax { get; set; }
    }

    public class PaymentDocumentStatus
    {
        [XmlElement("PaymentStatus")]
        public string PaymentStatus { get; set; }

        [XmlElement("PaymentStatusDate")]
        public DateTime PaymentStatusDate { get; set; }

        [XmlElement("Reason")]
        public string Reason { get; set; }

        [XmlElement("SourceID")]
        public string SourceID { get; set; }

        [XmlElement("SourcePayment")]
        public string SourcePayment { get; set; }
    }

    public class PaymentLine
    {
        [XmlElement("LineNumber")]
        public int LineNumber { get; set; }

        [XmlElement("SourceDocumentID")]
        public List<SourceDocumentID> SourceDocumentID { get; set; }

        [XmlElement("SettlementAmount")]
        public decimal? SettlementAmount { get; set; }

        [XmlElement("DebitAmount")]
        public decimal? DebitAmount { get; set; }

        [XmlElement("CreditAmount")]
        public decimal? CreditAmount { get; set; }

        [XmlElement("Tax")]
        public PaymentTax Tax { get; set; }

        [XmlElement("TaxExemptionReason")]
        public string TaxExemptionReason { get; set; }

        [XmlElement("TaxExemptionCode")]
        public string TaxExemptionCode { get; set; }
    }

    public class PaymentDocumentTotals
    {
        [XmlElement("TaxPayable")]
        public decimal TaxPayable { get; set; }

        [XmlElement("NetTotal")]
        public decimal NetTotal { get; set; }

        [XmlElement("GrossTotal")]
        public decimal GrossTotal { get; set; }

        [XmlElement("Settlement")]
        public PaymentSettlement Settlement { get; set; }

        [XmlElement("Currency")]
        public Currency Currency { get; set; }
    }

    #endregion

    #region Supporting Classes

    public class Address
    {
        [XmlElement("BuildingNumber")]
        public string BuildingNumber { get; set; }

        [XmlElement("StreetName")]
        public string StreetName { get; set; }

        [XmlElement("AddressDetail")]
        public string AddressDetail { get; set; }

        [XmlElement("City")]
        public string City { get; set; }

        [XmlElement("PostalCode")]
        public string PostalCode { get; set; }

        [XmlElement("Region")]
        public string Region { get; set; }

        [XmlElement("Country")]
        public string Country { get; set; }
    }

    public class CustomerAddress
    {
        [XmlElement("BuildingNumber")]
        public string BuildingNumber { get; set; }

        [XmlElement("StreetName")]
        public string StreetName { get; set; }

        [XmlElement("AddressDetail")]
        public string AddressDetail { get; set; }

        [XmlElement("City")]
        public string City { get; set; }

        [XmlElement("PostalCode")]
        public string PostalCode { get; set; }

        [XmlElement("Region")]
        public string Region { get; set; }

        [XmlElement("Country")]
        public string Country { get; set; }
    }

    public class Currency
    {
        [XmlElement("CurrencyCode")]
        public string CurrencyCode { get; set; }

        [XmlElement("CurrencyAmount")]
        public decimal CurrencyAmount { get; set; }

        [XmlElement("ExchangeRate")]
        public decimal ExchangeRate { get; set; }
    }

    public class CustomsDetails
    {
        [XmlElement("CNCode")]
        public List<string> CNCode { get; set; }

        [XmlElement("UNNumber")]
        public List<string> UNNumber { get; set; }
    }

    public class CustomsInformation
    {
        [XmlElement("ARCNo")]
        public List<string> ARCNo { get; set; }

        [XmlElement("IECAmount")]
        public decimal? IECAmount { get; set; }
    }

    public class MovementTax
    {
        [XmlElement("TaxType")]
        public string TaxType { get; set; }

        [XmlElement("TaxCountryRegion")]
        public string TaxCountryRegion { get; set; }

        [XmlElement("TaxCode")]
        public string TaxCode { get; set; }

        [XmlElement("TaxPercentage")]
        public decimal TaxPercentage { get; set; }
    }

    public class Tax
    {
        [XmlElement("TaxType")]
        public string TaxType { get; set; }

        [XmlElement("TaxCountryRegion")]
        public string TaxCountryRegion { get; set; }

        [XmlElement("TaxCode")]
        public string TaxCode { get; set; }

        [XmlElement("TaxPercentage")]
        public decimal? TaxPercentage { get; set; }

        [XmlElement("TaxAmount")]
        public decimal? TaxAmount { get; set; }
    }

    public class PaymentTax
    {
        [XmlElement("TaxType")]
        public string TaxType { get; set; }

        [XmlElement("TaxCountryRegion")]
        public string TaxCountryRegion { get; set; }

        [XmlElement("TaxCode")]
        public string TaxCode { get; set; }

        [XmlElement("TaxPercentage")]
        public decimal? TaxPercentage { get; set; }

        [XmlElement("TaxAmount")]
        public decimal? TaxAmount { get; set; }
    }

    public class OrderReferences
    {
        [XmlElement("OriginatingON")]
        public string OriginatingON { get; set; }

        [XmlElement("OrderDate")]
        public DateTime? OrderDate { get; set; }
    }

    public class PaymentMethod
    {
        [XmlElement("PaymentMechanism")]
        public string PaymentMechanism { get; set; }

        [XmlElement("PaymentAmount")]
        public decimal PaymentAmount { get; set; }

        [XmlElement("PaymentDate")]
        public DateTime PaymentDate { get; set; }
    }

    public class ProductSerialNumber
    {
        [XmlElement("SerialNumber")]
        public List<string> SerialNumber { get; set; }
    }

    public class References
    {
        [XmlElement("Reference")]
        public string Reference { get; set; }

        [XmlElement("Reason")]
        public string Reason { get; set; }
    }

    public class Settlement
    {
        [XmlElement("SettlementDiscount")]
        public string SettlementDiscount { get; set; }

        [XmlElement("SettlementAmount")]
        public decimal? SettlementAmount { get; set; }

        [XmlElement("SettlementDate")]
        public DateTime? SettlementDate { get; set; }

        [XmlElement("PaymentTerms")]
        public string PaymentTerms { get; set; }
    }

    public class PaymentSettlement
    {
        [XmlElement("SettlementAmount")]
        public decimal SettlementAmount { get; set; }
    }

    public class ShippingPoint
    {
        [XmlElement("DeliveryID")]
        public List<string> DeliveryID { get; set; }

        [XmlElement("DeliveryDate")]
        public DateTime? DeliveryDate { get; set; }

        [XmlElement("WarehouseID")]
        public string WarehouseID { get; set; }

        [XmlElement("LocationID")]
        public string LocationID { get; set; }

        [XmlElement("Address")]
        public Address Address { get; set; }
    }

    public class SpecialRegimes
    {
        [XmlElement("SelfBillingIndicator")]
        public int SelfBillingIndicator { get; set; }

        [XmlElement("CashVATSchemeIndicator")]
        public int CashVATSchemeIndicator { get; set; }

        [XmlElement("ThirdPartiesBillingIndicator")]
        public int ThirdPartiesBillingIndicator { get; set; }
    }

    public class WithholdingTax
    {
        [XmlElement("WithholdingTaxType")]
        public string WithholdingTaxType { get; set; }

        [XmlElement("WithholdingTaxDescription")]
        public string WithholdingTaxDescription { get; set; }

        [XmlElement("WithholdingTaxAmount")]
        public decimal WithholdingTaxAmount { get; set; }
    }

    public class SourceDocumentID
    {
        [XmlElement("OriginatingON")]
        public string OriginatingON { get; set; }

        [XmlElement("InvoiceDate")]
        public DateTime InvoiceDate { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }
    }

    #endregion
} 