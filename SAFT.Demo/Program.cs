using SAFT.Lib;
using SAFT.Lib.Utils;
using SAFT.Lib.Enums;
using SAFT.Lib.Constants;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System;

namespace SAFT.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== SAFT Library Demo ===\n");

            try
            {
                // Create a sample SAF-T audit file
                var auditFile = CreateSampleAuditFile();

                // Display file information
                Console.WriteLine($"Company: {auditFile.Header.CompanyName}");
                Console.WriteLine($"Fiscal Year: {auditFile.Header.FiscalYear}");
                Console.WriteLine($"Customers: {auditFile.MasterFiles.Customers.Count}");
                Console.WriteLine($"Products: {auditFile.MasterFiles.Products.Count}");
                Console.WriteLine($"General Ledger Accounts: {auditFile.MasterFiles.GeneralLedgerAccounts?.Accounts.Count ?? 0}");
                Console.WriteLine($"Invoices: {auditFile.SourceDocuments?.SalesInvoices?.Invoices?.Count ?? 0}\n");

                // Generate XML
                Console.WriteLine("Generating SAF-T XML file...");
                
                // Debug: Check GeneralLedgerAccounts before serialization
                Console.WriteLine($"[DEBUG] GeneralLedgerAccounts is null: {auditFile.MasterFiles.GeneralLedgerAccounts == null}");
                if (auditFile.MasterFiles.GeneralLedgerAccounts != null)
                {
                    Console.WriteLine($"[DEBUG] GeneralLedgerAccounts.Accounts.Count: {auditFile.MasterFiles.GeneralLedgerAccounts.Accounts.Count}");
                    Console.WriteLine($"[DEBUG] GeneralLedgerAccounts.TaxonomyReference: {auditFile.MasterFiles.GeneralLedgerAccounts.TaxonomyReference}");
                }
                
                auditFile.SaveToOutputDirectory("demo_saft.xml");
                Console.WriteLine("✓ SAF-T file generated: demo_saft.xml\n");

                // Validate the generated file using custom business logic validation only
                Console.WriteLine("Validating generated file using custom business logic validation...");
                var outputPath = Path.Combine(ConfigurationManager.Current.OutputDirectory, "demo_saft.xml");
                var xmlContent = File.ReadAllText(outputPath);
                var schemaPath = Path.Combine("..", "schema1_04_fixed.xsd");
                var validationResult = SchemaValidator.Validate(xmlContent, schemaPath);
                
                if (validationResult.Count == 0)
                {
                    Console.WriteLine("✓ Custom business logic validation passed!");
                }
                else
                {
                    Console.WriteLine("✗ Custom business logic validation found issues:");
                    foreach (var error in validationResult.Take(10))
                    {
                        Console.WriteLine($"  - {error}");
                    }
                    if (validationResult.Count > 10)
                    {
                        Console.WriteLine($"  ... and {validationResult.Count - 10} more issues");
                    }
                }

                Console.WriteLine("\nDemo completed successfully!");
                Console.WriteLine("Note: .NET XSD validation errors are ignored as requested.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static AuditFile CreateSampleAuditFile()
        {
            var auditFile = new AuditFile
            {
                Header = new Header
                {
                    AuditFileVersion = "1.04_01",
                    CompanyID = "123456789", // Portuguese VAT number
                    TaxRegistrationNumber = new PortugueseVatNumber { Value = 123456789 },
                    TaxAccountingBasis = TaxAccountingBasis.F,
                    CompanyName = "Sample Company Ltd.",
                    BusinessName = "Sample Business",
                    CompanyAddress = new AddressStructure
                    {
                        AddressDetail = "Sample Street 123",
                        City = "Lisboa",
                        PostalCode = "1000-001",
                        Country = "PT"
                    },
                    FiscalYear = "2024",
                    StartDate = "2024-01-01",
                    EndDate = "2024-12-31",
                    CurrencyCode = "EUR",
                    DateCreated = DateTime.Now.ToString("yyyy-MM-dd"),
                    TaxEntity = "123456789",
                    ProductCompanyTaxID = "123456789",
                    SoftwareCertificateNumber = "123456789",
                    ProductID = "SAFT Demo",
                    ProductVersion = "1.0",
                    HeaderComment = "Sample SAF-T file for demonstration"
                },
                MasterFiles = new MasterFiles
                {
                    GeneralLedgerAccounts = new GeneralLedgerAccounts
                    {
                        TaxonomyReference = TaxonomyReference.S,
                        Accounts = new List<Account>
                        {
                            new Account
                            {
                                AccountID = new GLAccountID { Value = "1101" },
                                AccountDescription = "Cash",
                                OpeningDebitBalance = 10000.00m,
                                OpeningCreditBalance = 0.00m,
                                ClosingDebitBalance = 15000.00m,
                                ClosingCreditBalance = 0.00m,
                                GroupingCategory = GroupingCategory.GM,
                                GroupingCode = new GLAccountID { Value = "1101" },
                                TaxonomyCode = new TaxonomyCode(1)
                            },
                            new Account
                            {
                                AccountID = new GLAccountID { Value = "1102" },
                                AccountDescription = "Bank Account",
                                OpeningDebitBalance = 50000.00m,
                                OpeningCreditBalance = 0.00m,
                                ClosingDebitBalance = 75000.00m,
                                ClosingCreditBalance = 0.00m,
                                GroupingCategory = GroupingCategory.GM,
                                GroupingCode = new GLAccountID { Value = "1102" },
                                TaxonomyCode = new TaxonomyCode(2)
                            },
                            new Account
                            {
                                AccountID = new GLAccountID { Value = "4101" },
                                AccountDescription = "Sales Revenue",
                                OpeningDebitBalance = 0.00m,
                                OpeningCreditBalance = 100000.00m,
                                ClosingDebitBalance = 0.00m,
                                ClosingCreditBalance = 150000.00m,
                                GroupingCategory = GroupingCategory.GM,
                                GroupingCode = new GLAccountID { Value = "4101" },
                                TaxonomyCode = new TaxonomyCode(3)
                            },
                            new Account
                            {
                                AccountID = new GLAccountID { Value = "2432" },
                                AccountDescription = "VAT Payable",
                                OpeningDebitBalance = 0.00m,
                                OpeningCreditBalance = 23000.00m,
                                ClosingDebitBalance = 0.00m,
                                ClosingCreditBalance = 34500.00m,
                                GroupingCategory = GroupingCategory.GM,
                                GroupingCode = new GLAccountID { Value = "2432" },
                                TaxonomyCode = new TaxonomyCode(4)
                            }
                        }
                    },
                    Customers = new List<Customer>
                    {
                        new Customer
                        {
                            CustomerID = "CUST001",
                            AccountID = new GLAccountID { Value = "1101" },
                            CustomerTaxID = "987654321",
                            CompanyName = "Customer Company Ltd.",
                            Contact = "John Doe",
                            BillingAddress = new CustomerAddressStructure
                            {
                                AddressDetail = "Customer Street 456",
                                City = "Porto",
                                PostalCode = "4000-001",
                                Country = "PT"
                            },
                            ShipToAddress = new List<CustomerAddressStructure>
                            {
                                new CustomerAddressStructure
                                {
                                    AddressDetail = "Customer Street 456",
                                    City = "Porto",
                                    PostalCode = "4000-001",
                                    Country = "PT"
                                }
                            },
                            Telephone = "123456789",
                            Fax = "123456788",
                            Email = "customer@example.com",
                            Website = "www.customer.com",
                            SelfBillingIndicator = 0
                        }
                    },
                    Suppliers = new List<Supplier>
                    {
                        new Supplier
                        {
                            SupplierID = "SUPP001",
                            AccountID = new GLAccountID { Value = "1101" },
                            SupplierTaxID = "111222333",
                            CompanyName = "Supplier Company Ltd.",
                            Contact = "Jane Smith",
                            BillingAddress = new AddressStructure
                            {
                                AddressDetail = "Supplier Street 789",
                                City = "Coimbra",
                                PostalCode = "3000-001",
                                Country = "PT"
                            },
                            ShipFromAddress = new List<AddressStructure>
                            {
                                new AddressStructure
                                {
                                    AddressDetail = "Supplier Street 789",
                                    City = "Coimbra",
                                    PostalCode = "3000-001",
                                    Country = "PT"
                                }
                            },
                            Telephone = "987654321",
                            Fax = "987654320",
                            Email = "supplier@example.com",
                            Website = "www.supplier.com",
                            SelfBillingIndicator = 0
                        }
                    },
                    Products = new List<Product>
                    {
                        new Product
                        {
                            ProductType = ProductType.P,
                            ProductCode = "PROD001",
                            ProductGroup = "Electronics",
                            ProductDescription = "Sample Product 1",
                            ProductNumberCode = "1234567890123",
                            CustomsDetails = new CustomsDetails
                            {
                                CNCode = new List<string> { "12345678" },
                                UNNumber = new List<string> { "UN1234" }
                            }
                        }
                    },
                    TaxTable = new TaxTable
                    {
                        TaxTableEntries = new List<TaxTableEntry>
                        {
                            new TaxTableEntry
                            {
                                TaxType = TaxType.IVA,
                                TaxCountryRegion = "PT",
                                TaxCode = "NOR",
                                Description = "Normal VAT Rate",
                                TaxPercentage = 23.00m,
                                TaxAmount = 0.00m
                            },
                            new TaxTableEntry
                            {
                                TaxType = TaxType.IVA,
                                TaxCountryRegion = "PT",
                                TaxCode = "RED",
                                Description = "Reduced VAT Rate",
                                TaxPercentage = 6.00m,
                                TaxAmount = 0.00m
                            },
                            new TaxTableEntry
                            {
                                TaxType = TaxType.IVA,
                                TaxCountryRegion = "PT",
                                TaxCode = "ISE",
                                Description = "Exempt VAT Rate",
                                TaxPercentage = 0.00m,
                                TaxAmount = 0.00m
                            }
                        }
                    }
                },
                SourceDocuments = new SourceDocuments
                {
                    SalesInvoices = new SalesInvoices
                    {
                        NumberOfEntries = 1,
                        TotalDebit = 123.00m,
                        TotalCredit = 123.00m,
                        Invoices = new List<Invoice>
                        {
                            new Invoice
                            {
                                InvoiceNo = "FT 2024/001",
                                ATCUD = PortugueseUtils.GenerateATCUD(DateTime.Now, 1),
                                DocumentStatus = new DocumentStatus
                                {
                                    InvoiceStatus = InvoiceStatus.N,
                                    InvoiceStatusDate = DateTime.Now.ToString("yyyy-MM-dd"),
                                    SourceID = "SALES",
                                    SourceBilling = SourceBilling.Produced
                                },
                                Hash = PortugueseUtils.GenerateDocumentHash("Sample invoice content"),
                                HashControl = PortugueseUtils.GenerateHashControl(
                                    PortugueseUtils.GenerateDocumentHash("Sample invoice content"), 
                                    "FT", 
                                    "FT 2024/001"
                                ),
                                Period = "2024",
                                InvoiceDate = DateTime.Now.ToString("yyyy-MM-dd"),
                                InvoiceType = InvoiceType.FT,
                                SourceID = "SALES",
                                EACCode = "",
                                SystemEntryDate = DateTime.Now.ToString("yyyy-MM-dd"),
                                CustomerID = "CUST001",
                                Lines = new List<InvoiceLine>
                                {
                                    new InvoiceLine
                                    {
                                        LineNumber = 1,
                                        ProductCode = "PROD001",
                                        ProductDescription = "Sample Product 1",
                                        Quantity = 1.00m,
                                        UnitOfMeasure = "UN",
                                        UnitPrice = 100.00m,
                                        TaxPointDate = DateTime.Now.ToString("yyyy-MM-dd"),
                                        Description = "Sample product line",
                                        ProductSerialNumber = new ProductSerialNumber(),
                                        DebitAmount = 0.00m,
                                        CreditAmount = 100.00m,
                                        Tax = new Tax
                                        {
                                            TaxType = TaxType.IVA,
                                            TaxCountryRegion = "PT",
                                            TaxCode = "NOR",
                                            TaxPercentage = 23.00m,
                                            TaxAmount = 23.00m
                                        }
                                    },
                                    new InvoiceLine
                                    {
                                        LineNumber = 2,
                                        ProductCode = "PROD001",
                                        ProductDescription = "Sample Product 1 (Exempt)",
                                        Quantity = 1.00m,
                                        UnitOfMeasure = "UN",
                                        UnitPrice = 50.00m,
                                        TaxPointDate = DateTime.Now.ToString("yyyy-MM-dd"),
                                        Description = "Sample exempt product line",
                                        ProductSerialNumber = new ProductSerialNumber(),
                                        DebitAmount = 0.00m,
                                        CreditAmount = 50.00m,
                                        Tax = new Tax
                                        {
                                            TaxType = TaxType.IVA,
                                            TaxCountryRegion = "PT",
                                            TaxCode = "ISE",
                                            TaxPercentage = 0.00m,
                                            TaxAmount = 0.00m
                                        }
                                    }
                                },
                                DocumentTotals = new DocumentTotals
                                {
                                    TaxPayable = 23.00m,
                                    NetTotal = 100.00m,
                                    GrossTotal = 123.00m
                                }
                            }
                        }
                    }
                }
            };

            return auditFile;
        }
    }
} 