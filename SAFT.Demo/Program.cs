using SAFT.Lib;
using SAFT.Lib.Utils;
using SAFT.Lib.Enums;
using SAFT.Lib.Constants;
using System.Configuration;
using System.IO;
using System.Linq;

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
                Console.WriteLine($"Invoices: {auditFile.SourceDocuments?.SalesInvoices?.Invoices?.Count ?? 0}\n");

                // Generate XML
                Console.WriteLine("Generating SAF-T XML file...");
                auditFile.SaveToOutputDirectory("demo_saft.xml");
                Console.WriteLine("✓ SAF-T file generated: demo_saft.xml\n");

                // Validate the generated file using custom business logic validation only
                Console.WriteLine("Validating generated file using custom business logic validation...");
                var outputPath = Path.Combine(ConfigurationManager.Current.OutputDirectory, "demo_saft.xml");
                var xmlContent = File.ReadAllText(outputPath);
                var validationResult = SchemaValidator.Validate(xmlContent, "schema1_04_fixed.xsd");
                
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
            var auditFile = AuditFile.CreateDefault();

            // Configure header
            auditFile.Header.CompanyName = "Demo Company Ltd";
            auditFile.Header.BusinessName = "Demo Business";
            auditFile.Header.TaxRegistrationNumber = new PortugueseVatNumber { Value = 123456789 };
            auditFile.Header.FiscalYear = "2024";
            auditFile.Header.StartDate = "2024-01-01";
            auditFile.Header.EndDate = "2024-12-31";
            auditFile.Header.CurrencyCode = CurrencyCodes.Euro;
            auditFile.Header.DateCreated = DateTime.Now.ToString("yyyy-MM-dd");
            auditFile.Header.ProductID = "SAFT Demo";
            auditFile.Header.ProductVersion = "1.0.0";
            auditFile.Header.CompanyAddress = new AddressStructure
            {
                AddressDetail = "123 Business Street",
                City = "Lisbon",
                PostalCode = "1000-001",
                Country = CountryCodes.Portugal
            };

            // Add customers
            auditFile.MasterFiles.Customers.Add(new Customer
            {
                CustomerID = "CUST001",
                AccountID = new GLAccountID { Value = "1101" },
                CustomerTaxID = "123456789",
                CompanyName = "John Doe",
                BillingAddress = new CustomerAddressStructure
                {
                    AddressDetail = "456 Customer Ave",
                    City = "Porto",
                    PostalCode = "4000-001",
                    Country = CountryCodes.Portugal
                },
                SelfBillingIndicator = 0
            });

            auditFile.MasterFiles.Customers.Add(new Customer
            {
                CustomerID = "CUST002",
                AccountID = new GLAccountID { Value = "1102" },
                CustomerTaxID = "987654321",
                CompanyName = "Jane Smith",
                BillingAddress = new CustomerAddressStructure
                {
                    AddressDetail = "789 Client Blvd",
                    City = "Coimbra",
                    PostalCode = "3000-001",
                    Country = CountryCodes.Portugal
                },
                SelfBillingIndicator = 0
            });

            // Add products
            auditFile.MasterFiles.Products.Add(new Product
            {
                ProductCode = "PROD001",
                ProductDescription = "Software License",
                ProductNumberCode = "SW001",
                ProductType = ProductType.P
            });

            auditFile.MasterFiles.Products.Add(new Product
            {
                ProductCode = "PROD002",
                ProductDescription = "Consulting Service",
                ProductNumberCode = "CS001",
                ProductType = ProductType.S
            });

            // Add tax table
            auditFile.MasterFiles.TaxTable = new TaxTable
            {
                TaxTableEntries = new List<TaxTableEntry>
                {
                    new TaxTableEntry
                    {
                        TaxType = TaxType.IVA,
                        TaxCountryRegion = CountryCodes.Portugal,
                        TaxCode = MovementTaxCode.NOR.ToString(),
                        Description = "Normal VAT Rate",
                        TaxPercentage = 23.00m
                    },
                    new TaxTableEntry
                    {
                        TaxType = TaxType.IVA,
                        TaxCountryRegion = CountryCodes.Portugal,
                        TaxCode = MovementTaxCode.RED.ToString(),
                        Description = "Reduced VAT Rate",
                        TaxPercentage = 6.00m
                    }
                }
            };

            // Add sample invoice
            auditFile.SourceDocuments = new SourceDocuments
            {
                SalesInvoices = new SalesInvoices
                {
                    Invoices = new List<Invoice>
                    {
                        new Invoice
                        {
                            InvoiceNo = "FT 2024/001",
                            CustomerID = "CUST001",
                            DocumentStatus = new DocumentStatus
                            {
                                InvoiceStatus = InvoiceStatus.N,
                                InvoiceStatusDate = "2024-01-15",
                                SourceID = "system",
                                SourceBilling = SourceBilling.Produced
                            },
                            InvoiceDate = "2024-01-15",
                            InvoiceType = InvoiceType.FT,
                            Lines = new List<InvoiceLine>
                            {
                                new InvoiceLine
                                {
                                    LineNumber = 1,
                                    ProductCode = "PROD001",
                                    ProductDescription = "Software License",
                                    Quantity = 1,
                                    UnitOfMeasure = "UN",
                                    UnitPrice = 100.00m,
                                    TaxPointDate = "2024-01-15",
                                    Description = "Annual software license",
                                    Tax = new Tax
                                    {
                                        TaxType = TaxType.IVA,
                                        TaxCountryRegion = CountryCodes.Portugal,
                                        TaxCode = MovementTaxCode.NOR.ToString(),
                                        TaxPercentage = 23.00m,
                                        TaxAmount = 23.00m
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
            };

            return auditFile;
        }
    }
} 