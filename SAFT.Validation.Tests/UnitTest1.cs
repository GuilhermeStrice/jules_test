using Xunit;
using System;
using System.Collections.Generic;
using SAFT.Lib;
using SAFT.Lib.Files;
using SAFT.Lib.Documents;
using SAFT.Lib.GeneralLedger;
using SAFT.Validation;

namespace SAFT.Validation.Tests
{
    public class CrossReferenceValidatorTests
    {
        private static AuditFile CreateMinimalValidAuditFile()
        {
            var auditFile = new AuditFile
            {
                Header = new Header
                {
                    AuditFileVersion = "1.04_01",
                    CompanyID = "COMP1",
                    CompanyName = "Test Company",
                    TaxEntity = "Test",
                    CurrencyCode = "EUR",
                    FiscalYear = 2023,
                    TaxRegistrationNumber = 123456789, // valid Portuguese NIF
                    TaxAccountingBasis = "F",
                    BusinessName = "Test Business",
                    CompanyAddress = new AddressStructure
                    {
                        BuildingNumber = "1",
                        StreetName = "Main St",
                        AddressDetail = "Main St 1",
                        City = "Lisbon",
                        PostalCode = "1000-001",
                        Region = "Lisbon",
                        Country = "PT"
                    },
                    StartDate = new DateTime(2023, 1, 1),
                    EndDate = new DateTime(2023, 12, 31),
                    DateCreated = new DateTime(2023, 1, 1),
                    ProductCompanyTaxID = "123456789",
                    SoftwareCertificateNumber = 9999,
                    ProductID = "TestProduct",
                    ProductVersion = "1.0"
                },
                MasterFiles = new MasterFiles
                {
                    Customers = new List<Customer> { new Customer { CustomerID = "C1", CompanyName = "Test Customer" } },
                    Products = new List<Product> { new Product { ProductCode = "P1", ProductDescription = "Test Product", ProductType = "P", ProductNumberCode = "P1" } },
                    TaxTable = new TaxTable
                    {
                        TaxTableEntries = new List<TaxTableEntry>
                        {
                            new TaxTableEntry
                            {
                                TaxType = "IVA",
                                TaxCountryRegion = "PT",
                                TaxCode = "NOR",
                                Description = "Normal VAT",
                                TaxPercentage = 23m
                            }
                        }
                    }
                },
                SourceDocuments = new SourceDocuments()
            };
            Xunit.Assert.NotNull(auditFile.Header); // Debug assertion
            return auditFile;
        }

        [Fact]
        public void RequiredFields_EmptyAuditFile_ShouldReturnErrors()
        {
            var auditFile = new AuditFile();
            auditFile.MasterFiles = new MasterFiles {
                Customers = new List<Customer> { new Customer { CustomerID = "C1", CompanyName = "Test Customer" } },
                Products = new List<Product> { new Product { ProductCode = "P1", ProductDescription = "Test Product", ProductType = "P", ProductNumberCode = "P1" } }
            };
            var results = CrossReferenceValidator.ValidateReferences(auditFile);
            Assert.Contains(results, r => r.Type == SAFTValidationType.RequiredField);
            Assert.True(results.Count > 0);
        }

        [Fact]
        public void InvoiceLine_TaxAmount_ExemptionReason_Assertion()
        {
            var auditFile = CreateMinimalValidAuditFile();
            auditFile.SourceDocuments.SalesInvoices = new SalesInvoices
            {
                Invoices = new List<Invoice>
                {
                    new Invoice
                    {
                        InvoiceNo = "INV1",
                        CustomerID = "C1",
                        Lines = new List<InvoiceLine>
                        {
                            new InvoiceLine
                            {
                                LineNumber = 1,
                                ProductCode = "P1",
                                Tax = new Tax { TaxAmount = 10 },
                                TaxExemptionReason = "ShouldNotBePresent"
                            }
                        }
                    }
                }
            };
            var results = CrossReferenceValidator.ValidateReferences(auditFile);
            Assert.Contains(results, r => r.Message.Contains("TaxExemptionReason must be missing if TaxAmount is not 0"));
        }

        [Fact]
        public void InvoiceLine_TaxPercentage_ExemptionReason_Assertion()
        {
            var auditFile = CreateMinimalValidAuditFile();
            auditFile.SourceDocuments.SalesInvoices = new SalesInvoices
            {
                Invoices = new List<Invoice>
                {
                    new Invoice
                    {
                        InvoiceNo = "INV2",
                        CustomerID = "C1",
                        Lines = new List<InvoiceLine>
                        {
                            new InvoiceLine
                            {
                                LineNumber = 1,
                                ProductCode = "P1",
                                Tax = new Tax { TaxPercentage = 23 },
                                TaxExemptionReason = "ShouldNotBePresent"
                            }
                        }
                    }
                }
            };
            var results = CrossReferenceValidator.ValidateReferences(auditFile);
            Assert.Contains(results, r => r.Message.Contains("TaxExemptionReason must be missing if TaxPercentage is not 0"));
        }

        [Fact]
        public void InvoiceLine_ExemptionReason_ExemptionCode_MutualPresence_Assertion()
        {
            var auditFile = CreateMinimalValidAuditFile();
            auditFile.SourceDocuments.SalesInvoices = new SalesInvoices
            {
                Invoices = new List<Invoice>
                {
                    new Invoice
                    {
                        InvoiceNo = "INV3",
                        CustomerID = "C1",
                        Lines = new List<InvoiceLine>
                        {
                            new InvoiceLine
                            {
                                LineNumber = 1,
                                ProductCode = "P1",
                                TaxExemptionReason = "Present",
                                TaxExemptionCode = null
                            }
                        }
                    }
                }
            };
            var results = CrossReferenceValidator.ValidateReferences(auditFile);
            Assert.Contains(results, r => r.Message.Contains("TaxExemptionReason and TaxExemptionCode must both be present or both be missing"));
        }

        [Fact]
        public void InvoiceLine_TaxBase_UnitPrice_Assertion()
        {
            var auditFile = CreateMinimalValidAuditFile();
            auditFile.SourceDocuments.SalesInvoices = new SalesInvoices
            {
                Invoices = new List<Invoice>
                {
                    new Invoice
                    {
                        InvoiceNo = "INV4",
                        CustomerID = "C1",
                        Lines = new List<InvoiceLine>
                        {
                            new InvoiceLine
                            {
                                LineNumber = 1,
                                ProductCode = "P1",
                                TaxBase = 100,
                                UnitPrice = 10
                            }
                        }
                    }
                }
            };
            var results = CrossReferenceValidator.ValidateReferences(auditFile);
            Assert.Contains(results, r => r.Message.Contains("If TaxBase is present, UnitPrice must be 0"));
        }

        [Fact]
        public void InvoiceLine_TaxBase_DebitAmount_CreditAmount_Assertion()
        {
            var auditFile = CreateMinimalValidAuditFile();
            auditFile.SourceDocuments.SalesInvoices = new SalesInvoices
            {
                Invoices = new List<Invoice>
                {
                    new Invoice
                    {
                        InvoiceNo = "INV5",
                        CustomerID = "C1",
                        Lines = new List<InvoiceLine>
                        {
                            new InvoiceLine
                            {
                                LineNumber = 1,
                                ProductCode = "P1",
                                TaxBase = 100,
                                DebitAmount = 10
                            }
                        }
                    }
                }
            };
            var results = CrossReferenceValidator.ValidateReferences(auditFile);
            Assert.Contains(results, r => r.Message.Contains("If TaxBase is present, DebitAmount and CreditAmount must be 0"));
        }

        [Fact]
        public void StockMovementLine_TaxPercentage_ExemptionReason_Assertion()
        {
            var auditFile = CreateMinimalValidAuditFile();
            auditFile.SourceDocuments.MovementOfGoods = new MovementOfGoods
            {
                StockMovements = new List<StockMovement>
                {
                    new StockMovement
                    {
                        DocumentNumber = "SM1",
                        Lines = new List<StockMovementLine>
                        {
                            new StockMovementLine
                            {
                                LineNumber = 1,
                                ProductCode = "P1",
                                Tax = new SAFT.Lib.MovementTax { TaxPercentage = 10 },
                                TaxExemptionReason = "ShouldNotBePresent"
                            }
                        }
                    }
                }
            };
            var results = CrossReferenceValidator.ValidateReferences(auditFile);
            Assert.Contains(results, r => r.Message.Contains("TaxExemptionReason must be missing if TaxPercentage is not 0"));
        }

        [Fact]
        public void StockMovementLine_ExemptionReason_ExemptionCode_MutualPresence_Assertion()
        {
            var auditFile = CreateMinimalValidAuditFile();
            auditFile.SourceDocuments.MovementOfGoods = new MovementOfGoods
            {
                StockMovements = new List<StockMovement>
                {
                    new StockMovement
                    {
                        DocumentNumber = "SM2",
                        Lines = new List<StockMovementLine>
                        {
                            new StockMovementLine
                            {
                                LineNumber = 1,
                                ProductCode = "P1",
                                TaxExemptionReason = "Present",
                                TaxExemptionCode = null
                            }
                        }
                    }
                }
            };
            var results = CrossReferenceValidator.ValidateReferences(auditFile);
            Assert.Contains(results, r => r.Message.Contains("TaxExemptionReason and TaxExemptionCode must both be present or both be missing"));
        }

        [Fact]
        public void PaymentLine_PaymentType_RC_TaxRequired_Assertion()
        {
            var auditFile = CreateMinimalValidAuditFile();
            auditFile.SourceDocuments.Payments = new Payments
            {
                PaymentList = new List<Payment>
                {
                    new Payment
                    {
                        PaymentRefNo = "PAY1",
                        PaymentType = "RC",
                        Lines = new List<PaymentLine>
                        {
                            new PaymentLine
                            {
                                LineNumber = 1,
                                DebitAmount = 10
                                // Tax is missing
                            }
                        },
                        CustomerID = "C1"
                    }
                }
            };
            var results = CrossReferenceValidator.ValidateReferences(auditFile);
            Assert.Contains(results, r => r.Message.Contains("If PaymentType is 'RC', Tax must be present"));
        }

        [Fact]
        public void Payment_PaymentType_TaxType_Assertion()
        {
            var auditFile = CreateMinimalValidAuditFile();
            auditFile.SourceDocuments.Payments = new Payments
            {
                PaymentList = new List<Payment>
                {
                    new Payment
                    {
                        PaymentRefNo = "PAY2",
                        PaymentType = "XX",
                        Lines = new List<PaymentLine>
                        {
                            new PaymentLine
                            {
                                LineNumber = 1,
                                Tax = new PaymentTax { TaxType = "IVA" }
                            }
                        },
                        CustomerID = "C1"
                    }
                }
            };
            var results = CrossReferenceValidator.ValidateReferences(auditFile);
            Assert.Contains(results, r => r.Message.Contains("If any line has TaxType, PaymentType must be 'RC' or 'RG'"));
        }

        [Fact]
        public void WorkDocumentLine_AllAssertions()
        {
            var auditFile = CreateMinimalValidAuditFile();
            auditFile.SourceDocuments.WorkingDocuments = new WorkingDocuments
            {
                WorkDocuments = new List<WorkDocument>
                {
                    new WorkDocument
                    {
                        DocumentNumber = "WD1",
                        CustomerID = "C1",
                        Lines = new List<WorkDocumentLine>
                        {
                            // TaxAmount != 0 and ExemptionReason present
                            new WorkDocumentLine
                            {
                                LineNumber = 1,
                                ProductCode = "P1",
                                Tax = new Tax { TaxAmount = 10 },
                                TaxExemptionReason = "ShouldNotBePresent"
                            },
                            // TaxAmount == 0 and ExemptionReason missing
                            new WorkDocumentLine
                            {
                                LineNumber = 2,
                                ProductCode = "P2",
                                Tax = new Tax { TaxAmount = 0 },
                                TaxExemptionReason = null
                            },
                            // TaxPercentage != 0 and ExemptionReason present
                            new WorkDocumentLine
                            {
                                LineNumber = 3,
                                ProductCode = "P3",
                                Tax = new Tax { TaxPercentage = 23 },
                                TaxExemptionReason = "ShouldNotBePresent"
                            },
                            // TaxPercentage == 0 and ExemptionReason missing
                            new WorkDocumentLine
                            {
                                LineNumber = 4,
                                ProductCode = "P4",
                                Tax = new Tax { TaxPercentage = 0 },
                                TaxExemptionReason = null
                            },
                            // ExemptionReason present, ExemptionCode missing
                            new WorkDocumentLine
                            {
                                LineNumber = 5,
                                ProductCode = "P5",
                                TaxExemptionReason = "Present",
                                TaxExemptionCode = null
                            },
                            // TaxBase present, UnitPrice != 0
                            new WorkDocumentLine
                            {
                                LineNumber = 6,
                                ProductCode = "P6",
                                TaxBase = 100,
                                UnitPrice = 10
                            },
                            // TaxBase present, DebitAmount != 0
                            new WorkDocumentLine
                            {
                                LineNumber = 7,
                                ProductCode = "P7",
                                TaxBase = 100,
                                DebitAmount = 10
                            }
                        }
                    }
                }
            };
            var results = CrossReferenceValidator.ValidateReferences(auditFile);
            Assert.Contains(results, r => r.Message.Contains("TaxExemptionReason must be missing if TaxAmount is not 0"));
            Assert.Contains(results, r => r.Message.Contains("TaxExemptionReason must be present if TaxAmount is 0"));
            Assert.Contains(results, r => r.Message.Contains("TaxExemptionReason must be missing if TaxPercentage is not 0"));
            Assert.Contains(results, r => r.Message.Contains("TaxExemptionReason must be present if TaxPercentage is 0"));
            Assert.Contains(results, r => r.Message.Contains("TaxExemptionReason and TaxExemptionCode must both be present or both be missing"));
            Assert.Contains(results, r => r.Message.Contains("If TaxBase is present, UnitPrice must be 0"));
            Assert.Contains(results, r => r.Message.Contains("If TaxBase is present, DebitAmount and CreditAmount must be 0"));
        }

        [Fact]
        public void InvoiceLine_ExemptionReason_ExemptionCode_BothPresent_NoError()
        {
            var auditFile = CreateMinimalValidAuditFile();
            auditFile.SourceDocuments.SalesInvoices = new SalesInvoices
            {
                Invoices = new List<Invoice>
                {
                    new Invoice
                    {
                        InvoiceNo = "INV6",
                        CustomerID = "C1",
                        Lines = new List<InvoiceLine>
                        {
                            new InvoiceLine
                            {
                                LineNumber = 1,
                                ProductCode = "P1",
                                TaxExemptionReason = "Present",
                                TaxExemptionCode = "M01"
                            }
                        }
                    }
                }
            };
            var results = CrossReferenceValidator.ValidateReferences(auditFile);
            Assert.DoesNotContain(results, r => r.Message.Contains("TaxExemptionReason and TaxExemptionCode must both be present or both be missing"));
        }

        [Fact]
        public void InvoiceLine_ExemptionReason_ExemptionCode_BothMissing_NoError()
        {
            var auditFile = CreateMinimalValidAuditFile();
            auditFile.SourceDocuments.SalesInvoices = new SalesInvoices
            {
                Invoices = new List<Invoice>
                {
                    new Invoice
                    {
                        InvoiceNo = "INV7",
                        CustomerID = "C1",
                        Lines = new List<InvoiceLine>
                        {
                            new InvoiceLine
                            {
                                LineNumber = 1,
                                ProductCode = "P1"
                                // Both missing
                            }
                        }
                    }
                }
            };
            var results = CrossReferenceValidator.ValidateReferences(auditFile);
            Assert.DoesNotContain(results, r => r.Message.Contains("TaxExemptionReason and TaxExemptionCode must both be present or both be missing"));
        }

        [Fact]
        public void StockMovementLine_TaxPercentage_Zero_ExemptionReason_Present()
        {
            var auditFile = CreateMinimalValidAuditFile();
            auditFile.SourceDocuments.MovementOfGoods = new MovementOfGoods
            {
                StockMovements = new List<StockMovement>
                {
                    new StockMovement
                    {
                        DocumentNumber = "SM3",
                        Lines = new List<StockMovementLine>
                        {
                            new StockMovementLine
                            {
                                LineNumber = 1,
                                ProductCode = "P1",
                                Tax = new SAFT.Lib.MovementTax { TaxPercentage = 0 },
                                TaxExemptionReason = "Present"
                            }
                        }
                    }
                }
            };
            var results = CrossReferenceValidator.ValidateReferences(auditFile);
            Assert.DoesNotContain(results, r => r.Message.Contains("TaxExemptionReason must be present if TaxPercentage is 0"));
        }

        [Fact]
        public void PaymentLine_Tax_Null_ExemptionReason_Null()
        {
            var auditFile = CreateMinimalValidAuditFile();
            auditFile.SourceDocuments.Payments = new Payments
            {
                PaymentList = new List<Payment>
                {
                    new Payment
                    {
                        PaymentRefNo = "PAY3",
                        PaymentType = "RC",
                        Lines = new List<PaymentLine>
                        {
                            new PaymentLine
                            {
                                LineNumber = 1,
                                Tax = null,
                                TaxExemptionReason = null
                            }
                        },
                        CustomerID = "C1"
                    }
                }
            };
            var results = CrossReferenceValidator.ValidateReferences(auditFile);
            Assert.Contains(results, r => r.Message.Contains("If PaymentType is 'RC', Tax must be present"));
        }
    }
}
