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
            Console.WriteLine("SAFT-PT Generator Example");
            Console.WriteLine("=========================");

            // Create generator instance
            var generator = new SaftPtGenerator();

            // Initialize header
            generator.InitializeHeader(
                companyId: "123456789",
                taxRegistrationNumber: 123456789,
                companyName: "Empresa Exemplo Lda",
                businessName: "Empresa Exemplo",
                companyAddress: new Address
                {
                    AddressDetail = "Rua das Flores, 123",
                    City = "Lisboa",
                    PostalCode = "1000-001",
                    Country = "PT"
                },
                fiscalYear: 2024,
                startDate: new DateTime(2024, 1, 1),
                endDate: new DateTime(2024, 12, 31),
                dateCreated: DateTime.Now,
                taxEntity: "AT",
                productCompanyTaxId: "123456789",
                softwareCertificateNumber: 12345,
                productId: "Software/1.0",
                productVersion: "1.0"
            );

            // Add customer
            generator.AddCustomer(new Customer
            {
                CustomerID = "CUST001",
                AccountID = "211",
                CustomerTaxID = "987654321",
                CompanyName = "Cliente Exemplo Lda",
                BillingAddress = new CustomerAddress
                {
                    AddressDetail = "Rua do Cliente, 456",
                    City = "Porto",
                    PostalCode = "4000-001",
                    Country = "PT"
                },
                SelfBillingIndicator = 0
            });

            // Add supplier
            generator.AddSupplier(new Supplier
            {
                SupplierID = "SUPP001",
                AccountID = "22",
                SupplierTaxID = "111222333",
                CompanyName = "Fornecedor Exemplo Lda",
                BillingAddress = new Address
                {
                    AddressDetail = "Rua do Fornecedor, 789",
                    City = "Braga",
                    PostalCode = "4700-001",
                    Country = "PT"
                },
                SelfBillingIndicator = 0
            });

            // Add product
            generator.AddProduct(new Product
            {
                ProductType = "P", // Produtos
                ProductCode = "PROD001",
                ProductDescription = "Produto Exemplo",
                ProductNumberCode = "123456789"
            });

            // Add general ledger accounts
            generator.SetGeneralLedgerAccounts(new GeneralLedgerAccounts
            {
                TaxonomyReference = "S", // SNC base
                Account = new List<Account>
                {
                    new Account
                    {
                        AccountID = "211",
                        AccountDescription = "Clientes",
                        OpeningDebitBalance = 0,
                        OpeningCreditBalance = 0,
                        ClosingDebitBalance = 123.00m,
                        ClosingCreditBalance = 0,
                        GroupingCategory = "GM" // Conta de movimento
                    },
                    new Account
                    {
                        AccountID = "22",
                        AccountDescription = "Fornecedores",
                        OpeningDebitBalance = 0,
                        OpeningCreditBalance = 0,
                        ClosingDebitBalance = 0,
                        ClosingCreditBalance = 0,
                        GroupingCategory = "GM" // Conta de movimento
                    }
                }
            });

            // Add tax table
            generator.SetTaxTable(new TaxTable
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
                    },
                    new TaxTableEntry
                    {
                        TaxType = "IVA",
                        TaxCountryRegion = "PT",
                        TaxCode = "INT",
                        Description = "IVA Intermédio",
                        TaxPercentage = 13.00m
                    }
                }
            });

            // Create invoice
            var invoice = new Invoice
            {
                InvoiceNo = "FT 2024/001",
                ATCUD = "ATCUD123456789",
                DocumentStatus = new InvoiceDocumentStatus
                {
                    InvoiceStatus = "N", // Normal
                    InvoiceStatusDate = DateTime.Now,
                    SourceID = "SYS001",
                    SourceBilling = "P" // Documento produzido na aplicação
                },
                Hash = "hash123456789",
                HashControl = "1-AT(FT) 2024/001",
                InvoiceDate = DateTime.Now,
                InvoiceType = "FT", // Fatura
                SpecialRegimes = new SpecialRegimes
                {
                    SelfBillingIndicator = 0,
                    CashVATSchemeIndicator = 0,
                    ThirdPartiesBillingIndicator = 0
                },
                SourceID = "SYS001",
                SystemEntryDate = DateTime.Now,
                CustomerID = "CUST001",
                Line = new List<InvoiceLine>
                {
                    new InvoiceLine
                    {
                        LineNumber = 1,
                        ProductCode = "PROD001",
                        ProductDescription = "Produto Exemplo",
                        Quantity = 1,
                        UnitOfMeasure = "UN",
                        UnitPrice = 100.00m,
                        TaxPointDate = DateTime.Now,
                        Description = "Descrição da linha",
                        Tax = new Tax
                        {
                            TaxType = "IVA",
                            TaxCountryRegion = "PT",
                            TaxCode = "NOR",
                            TaxPercentage = 23.00m
                        }
                    }
                },
                DocumentTotals = new InvoiceDocumentTotals
                {
                    TaxPayable = 23.00m,
                    NetTotal = 100.00m,
                    GrossTotal = 123.00m
                }
            };

            // Add source documents
            generator.SetSourceDocuments(new SourceDocuments
            {
                SalesInvoices = new SalesInvoices
                {
                    NumberOfEntries = 1,
                    TotalDebit = 123.00m,
                    TotalCredit = 123.00m,
                    Invoice = new List<Invoice> { invoice }
                }
            });

            // Generate the XML file
            generator.GenerateFile("saft_pt_example.xml");
            Console.WriteLine("SAFT-PT file generated successfully!");
            Console.WriteLine("File saved as: saft_pt_example.xml");

            // Example of creating digital signature (requires private key file)
            try
            {
                // This would require a private key file
                // string signature = generator.CreateSignature(
                //     DateTime.Now, 
                //     DateTime.Now, 
                //     "FT 2024/001", 
                //     123.00m
                // );
                // Console.WriteLine($"Digital signature created: {signature}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Signature creation skipped: {ex.Message}");
            }
        }
    }
} 