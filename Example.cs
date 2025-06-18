using System;
using System.Collections.Generic;

namespace SaftPtGenerator
{
    /// <summary>
    /// Example usage of the SAFT-PT Generator
    /// </summary>
    public class Example
    {
        public static void Main()
        {
            try
            {
                // Initialize the SAFT-PT generator
                var generator = new SaftPtGenerator();

                // Initialize header
                generator.InitializeHeader(
                    companyId: "123456789",
                    taxRegistrationNumber: 123456789,
                    companyName: "Example Company Lda",
                    businessName: "Example Business",
                    companyAddress: new Address
                    {
                        BuildingNumber = "123",
                        StreetName = "Example Street",
                        City = "Lisbon",
                        PostalCode = "1000-000",
                        Country = "PT"
                    },
                    fiscalYear: 2024,
                    startDate: new DateTime(2024, 1, 1),
                    endDate: new DateTime(2024, 12, 31),
                    dateCreated: DateTime.Now,
                    taxEntity: "Lisboa",
                    productCompanyTaxId: "123456789",
                    softwareCertificateNumber: 123456,
                    productId: "Example Software",
                    productVersion: "1.0.0",
                    headerComment: "Example SAFT-PT file",
                    telephone: "+351 123 456 789",
                    email: "info@example.com"
                );

                // Add tax table
                var taxTable = new TaxTable
                {
                    TaxTableEntry = new List<TaxTableEntry>
                    {
                        new TaxTableEntry
                        {
                            TaxType = "IVA",
                            TaxCountryRegion = "PT",
                            TaxCode = "NOR",
                            Description = "IVA Normal",
                            TaxPercentage = 23.00m
                        },
                        new TaxTableEntry
                        {
                            TaxType = "IVA",
                            TaxCountryRegion = "PT",
                            TaxCode = "RED",
                            Description = "IVA Reduzido",
                            TaxPercentage = 6.00m
                        }
                    }
                };
                generator.SetTaxTable(taxTable);

                // Add customer
                var customer = new Customer
                {
                    CustomerID = "CUST001",
                    AccountID = "1101",
                    CustomerTaxID = "123456789",
                    CompanyName = "Customer Company Lda",
                    Contact = "John Doe",
                    BillingAddress = new CustomerAddress
                    {
                        BuildingNumber = "456",
                        StreetName = "Customer Street",
                        City = "Porto",
                        PostalCode = "4000-000",
                        Country = "PT"
                    },
                    SelfBillingIndicator = 0
                };
                generator.AddCustomer(customer);

                // Add product
                var product = new Product
                {
                    ProductType = "P",
                    ProductCode = "PROD001",
                    ProductGroup = "Services",
                    ProductDescription = "Consulting Service"
                };
                generator.AddProduct(product);

                // Create source documents
                var sourceDocuments = new SourceDocuments();

                // Create sales invoices
                var salesInvoices = new SalesInvoices
                {
                    NumberOfEntries = 1,
                    TotalDebit = 123.00m,
                    TotalCredit = 123.00m,
                    Invoice = new List<Invoice>
                    {
                        new Invoice
                        {
                            InvoiceNo = "FT 2024/001",
                            ATCUD = "0",
                            DocumentStatus = new InvoiceDocumentStatus
                            {
                                InvoiceStatus = "N",
                                InvoiceStatusDate = DateTime.Now,
                                Reason = "Normal"
                            },
                            Hash = "ABC123",
                            HashControl = "1",
                            Period = 1,
                            InvoiceDate = DateTime.Now,
                            InvoiceType = "FT",
                            SpecialRegimes = new SpecialRegimes
                            {
                                SelfBillingIndicator = 0,
                                CashVATSchemeIndicator = 0,
                                ThirdPartiesBillingIndicator = 0
                            },
                            SourceID = "Example System",
                            SystemEntryDate = DateTime.Now,
                            TransactionID = "TXN001",
                            CustomerID = "CUST001",
                            Line = new List<InvoiceLine>
                            {
                                new InvoiceLine
                                {
                                    LineNumber = 1,
                                    ProductCode = "PROD001",
                                    ProductDescription = "Consulting Service",
                                    Quantity = 1,
                                    UnitOfMeasure = "UN",
                                    UnitPrice = 100.00m,
                                    TaxBase = 100.00m,
                                    TaxPointDate = DateTime.Now,
                                    Description = "Professional consulting services",
                                    Tax = new Tax
                                    {
                                        TaxType = "IVA",
                                        TaxCountryRegion = "PT",
                                        TaxCode = "NOR",
                                        TaxPercentage = 23.00m,
                                        TaxAmount = 23.00m
                                    }
                                }
                            },
                            DocumentTotals = new InvoiceDocumentTotals
                            {
                                TaxPayable = 23.00m,
                                NetTotal = 100.00m,
                                GrossTotal = 123.00m,
                                Currency = new Currency
                                {
                                    CurrencyCode = "EUR",
                                    CurrencyAmount = 123.00m,
                                    ExchangeRate = 1.00m
                                }
                            }
                        }
                    }
                };

                sourceDocuments.SalesInvoices = salesInvoices;
                generator.SetSourceDocuments(sourceDocuments);

                // Validate the audit file before generating
                Console.WriteLine("Validating audit file...");
                var validationErrors = generator.ValidateAuditFile();
                
                if (validationErrors.Count > 0)
                {
                    Console.WriteLine("Validation errors found:");
                    foreach (var error in validationErrors)
                    {
                        Console.WriteLine($"- {error}");
                    }
                    return;
                }

                Console.WriteLine("Validation passed successfully!");

                // Generate the XML file
                generator.GenerateFile("saft_pt_example.xml");
                Console.WriteLine("SAFT-PT file generated successfully!");

                // Example of digital signature (if keys are available)
                try
                {
                    var signature = generator.CreateSignature(
                        docDate: DateTime.Now,
                        systemEntryDate: DateTime.Now,
                        docNumber: "FT 2024/001",
                        grossTotal: 123.00m
                    );
                    Console.WriteLine($"Digital signature created: {signature}");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Digital signature not available: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
} 