using Xunit;
using SAFT.Lib;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SAFT.Tests
{
    public class BusinessLogicValidationTests
    {
        private string GetSchemaPath()
        {
            var solutionDir = Directory.GetCurrentDirectory();
            while (!File.Exists(Path.Combine(solutionDir, "SAFT.sln")))
            {
                solutionDir = Directory.GetParent(solutionDir)?.FullName;
                if (solutionDir == null)
                    throw new FileNotFoundException("Could not find SAFT.sln file");
            }
            return Path.Combine(solutionDir, "schema1_04_fixed.xsd");
        }

        [Fact]
        public void TestInvoiceNumberingSequence()
        {
            // Test invoice numbering sequence and format compliance
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""SalesInvoices"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""Invoice"" maxOccurs=""unbounded"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""InvoiceNo"" type=""xs:string""/>
                            <xs:element name=""InvoiceDate"" type=""xs:date""/>
                            <xs:element name=""InvoiceType"" type=""xs:string""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:sequence>
        </xs:complexType>
    </xs:element>
</xs:schema>";
            
            var schemaPath = "test_invoice_numbering.xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid invoice numbering sequence
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<SalesInvoices>
    <Invoice>
        <InvoiceNo>FT 2024/001</InvoiceNo>
        <InvoiceDate>2024-01-01</InvoiceDate>
        <InvoiceType>FT</InvoiceType>
    </Invoice>
    <Invoice>
        <InvoiceNo>FT 2024/002</InvoiceNo>
        <InvoiceDate>2024-01-02</InvoiceDate>
        <InvoiceType>FT</InvoiceType>
    </Invoice>
    <Invoice>
        <InvoiceNo>FT 2024/003</InvoiceNo>
        <InvoiceDate>2024-01-03</InvoiceDate>
        <InvoiceType>FT</InvoiceType>
    </Invoice>
</SalesInvoices>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("invoice numbering") || error.Contains("sequence"));
                
                // Test invalid invoice numbering (duplicate numbers)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<SalesInvoices>
    <Invoice>
        <InvoiceNo>FT 2024/001</InvoiceNo>
        <InvoiceDate>2024-01-01</InvoiceDate>
        <InvoiceType>FT</InvoiceType>
    </Invoice>
    <Invoice>
        <InvoiceNo>FT 2024/001</InvoiceNo>
        <InvoiceDate>2024-01-02</InvoiceDate>
        <InvoiceType>FT</InvoiceType>
    </Invoice>
</SalesInvoices>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("invoice numbering") || error.Contains("duplicate") || error.Contains("sequence"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestPaymentTermsValidation()
        {
            // Test payment terms and due date validation
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""Invoice"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""InvoiceDate"" type=""xs:date""/>
                <xs:element name=""DueDate"" type=""xs:date""/>
                <xs:element name=""PaymentTerms"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""PaymentTerm"" type=""xs:string""/>
                            <xs:element name=""Days"" type=""xs:integer""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:sequence>
        </xs:complexType>
    </xs:element>
</xs:schema>";
            
            var schemaPath = "test_payment_terms.xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid payment terms (30 days from invoice date)
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice>
    <InvoiceDate>2024-01-01</InvoiceDate>
    <DueDate>2024-01-31</DueDate>
    <PaymentTerms>
        <PaymentTerm>Net 30</PaymentTerm>
        <Days>30</Days>
    </PaymentTerms>
</Invoice>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("payment terms") || error.Contains("due date"));
                
                // Test invalid payment terms (due date before invoice date)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice>
    <InvoiceDate>2024-01-15</InvoiceDate>
    <DueDate>2024-01-10</DueDate>
    <PaymentTerms>
        <PaymentTerm>Net 30</PaymentTerm>
        <Days>30</Days>
    </PaymentTerms>
</Invoice>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("payment terms") || error.Contains("due date") || error.Contains("before"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestCreditDebitBalanceValidation()
        {
            // Test credit/debit balance validation in general ledger entries
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""Transaction"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""Line"" maxOccurs=""unbounded"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""AccountID"" type=""xs:string""/>
                            <xs:element name=""CreditAmount"" type=""xs:decimal"" minOccurs=""0""/>
                            <xs:element name=""DebitAmount"" type=""xs:decimal"" minOccurs=""0""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:sequence>
        </xs:complexType>
    </xs:element>
</xs:schema>";
            
            var schemaPath = "test_credit_debit_balance.xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid credit/debit balance (balanced transaction)
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Transaction>
    <Line>
        <AccountID>1101</AccountID>
        <DebitAmount>1000.00</DebitAmount>
    </Line>
    <Line>
        <AccountID>4101</AccountID>
        <CreditAmount>1000.00</CreditAmount>
    </Line>
</Transaction>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("credit/debit balance") || error.Contains("balanced"));
                
                // Test invalid credit/debit balance (unbalanced transaction)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Transaction>
    <Line>
        <AccountID>1101</AccountID>
        <DebitAmount>1000.00</DebitAmount>
    </Line>
    <Line>
        <AccountID>4101</AccountID>
        <CreditAmount>900.00</CreditAmount>
    </Line>
</Transaction>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("credit/debit balance") || error.Contains("balanced") || error.Contains("mismatch"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestDocumentStatusValidation()
        {
            // Test document status and workflow validation
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""Invoice"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""InvoiceNo"" type=""xs:string""/>
                <xs:element name=""InvoiceDate"" type=""xs:date""/>
                <xs:element name=""DocumentStatus"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""InvoiceStatus"" type=""xs:string""/>
                            <xs:element name=""InvoiceStatusDate"" type=""xs:date""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:sequence>
        </xs:complexType>
    </xs:element>
</xs:schema>";
            
            var schemaPath = "test_document_status.xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid document status (status date after invoice date)
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice>
    <InvoiceNo>FT 2024/001</InvoiceNo>
    <InvoiceDate>2024-01-01</InvoiceDate>
    <DocumentStatus>
        <InvoiceStatus>N</InvoiceStatus>
        <InvoiceStatusDate>2024-01-02</InvoiceStatusDate>
    </DocumentStatus>
</Invoice>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("document status") || error.Contains("status date"));
                
                // Test invalid document status (status date before invoice date)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice>
    <InvoiceNo>FT 2024/001</InvoiceNo>
    <InvoiceDate>2024-01-15</InvoiceDate>
    <DocumentStatus>
        <InvoiceStatus>N</InvoiceStatus>
        <InvoiceStatusDate>2024-01-10</InvoiceStatusDate>
    </DocumentStatus>
</Invoice>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("document status") || error.Contains("status date") || error.Contains("before"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestQuantityAndUnitPriceValidation()
        {
            // Test quantity and unit price validation in invoice lines
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""Invoice"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""Line"" maxOccurs=""unbounded"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""Quantity"" type=""xs:decimal""/>
                            <xs:element name=""UnitPrice"" type=""xs:decimal""/>
                            <xs:element name=""LineExtensionAmount"" type=""xs:decimal""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:sequence>
        </xs:complexType>
    </xs:element>
</xs:schema>";
            
            var schemaPath = "test_quantity_unit_price.xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid quantity and unit price calculation
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice>
    <Line>
        <Quantity>2.0</Quantity>
        <UnitPrice>50.00</UnitPrice>
        <LineExtensionAmount>100.00</LineExtensionAmount>
    </Line>
    <Line>
        <Quantity>1.5</Quantity>
        <UnitPrice>100.00</UnitPrice>
        <LineExtensionAmount>150.00</LineExtensionAmount>
    </Line>
</Invoice>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("quantity") || error.Contains("unit price") || error.Contains("calculation"));
                
                // Test invalid quantity and unit price calculation
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice>
    <Line>
        <Quantity>2.0</Quantity>
        <UnitPrice>50.00</UnitPrice>
        <LineExtensionAmount>100.00</LineExtensionAmount>
    </Line>
    <Line>
        <Quantity>1.5</Quantity>
        <UnitPrice>100.00</UnitPrice>
        <LineExtensionAmount>200.00</LineExtensionAmount>
    </Line>
</Invoice>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("quantity") || error.Contains("unit price") || error.Contains("calculation") || error.Contains("mismatch"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestTaxCalculationAccuracy()
        {
            // Test tax calculation accuracy and rounding
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""Invoice"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""Line"" maxOccurs=""unbounded"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""LineExtensionAmount"" type=""xs:decimal""/>
                            <xs:element name=""Tax"">
                                <xs:complexType>
                                    <xs:sequence>
                                        <xs:element name=""TaxPercentage"" type=""xs:decimal""/>
                                        <xs:element name=""TaxAmount"" type=""xs:decimal""/>
                                    </xs:sequence>
                                </xs:complexType>
                            </xs:element>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
                <xs:element name=""DocumentTotals"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""NetTotal"" type=""xs:decimal""/>
                            <xs:element name=""TaxPayable"" type=""xs:decimal""/>
                            <xs:element name=""GrossTotal"" type=""xs:decimal""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:sequence>
        </xs:complexType>
    </xs:element>
</xs:schema>";
            
            var schemaPath = "test_tax_calculation_accuracy.xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid tax calculation with proper rounding
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice>
    <Line>
        <LineExtensionAmount>100.00</LineExtensionAmount>
        <Tax>
            <TaxPercentage>23.00</TaxPercentage>
            <TaxAmount>23.00</TaxAmount>
        </Tax>
    </Line>
    <Line>
        <LineExtensionAmount>50.00</LineExtensionAmount>
        <Tax>
            <TaxPercentage>23.00</TaxPercentage>
            <TaxAmount>11.50</TaxAmount>
        </Tax>
    </Line>
    <DocumentTotals>
        <NetTotal>150.00</NetTotal>
        <TaxPayable>34.50</TaxPayable>
        <GrossTotal>184.50</GrossTotal>
    </DocumentTotals>
</Invoice>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("tax calculation") || error.Contains("rounding"));
                
                // Test invalid tax calculation (incorrect rounding)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice>
    <Line>
        <LineExtensionAmount>100.00</LineExtensionAmount>
        <Tax>
            <TaxPercentage>23.00</TaxPercentage>
            <TaxAmount>23.00</TaxAmount>
        </Tax>
    </Line>
    <Line>
        <LineExtensionAmount>33.33</LineExtensionAmount>
        <Tax>
            <TaxPercentage>23.00</TaxPercentage>
            <TaxAmount>7.66</TaxAmount>
        </Tax>
    </Line>
    <DocumentTotals>
        <NetTotal>133.33</NetTotal>
        <TaxPayable>30.66</TaxPayable>
        <GrossTotal>164.00</GrossTotal>
    </DocumentTotals>
</Invoice>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("tax calculation") || error.Contains("rounding") || error.Contains("accuracy"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestBusinessRuleCompliance()
        {
            // Test general business rule compliance
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""AuditFile"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""Header"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""CompanyID"" type=""xs:string""/>
                            <xs:element name=""TaxRegistrationNumber"" type=""xs:integer""/>
                            <xs:element name=""FiscalYear"" type=""xs:string""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
                <xs:element name=""SourceDocuments"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""SalesInvoices"">
                                <xs:complexType>
                                    <xs:sequence>
                                        <xs:element name=""Invoice"" maxOccurs=""unbounded"">
                                            <xs:complexType>
                                                <xs:sequence>
                                                    <xs:element name=""InvoiceNo"" type=""xs:string""/>
                                                    <xs:element name=""InvoiceDate"" type=""xs:date""/>
                                                    <xs:element name=""CustomerID"" type=""xs:string""/>
                                                </xs:sequence>
                                            </xs:complexType>
                                        </xs:element>
                                    </xs:sequence>
                                </xs:complexType>
                            </xs:element>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:sequence>
        </xs:complexType>
    </xs:element>
</xs:schema>";
            
            var schemaPath = "test_business_rules.xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid business rule compliance
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
    <Header>
        <CompanyID>COMP001</CompanyID>
        <TaxRegistrationNumber>123456789</TaxRegistrationNumber>
        <FiscalYear>2024</FiscalYear>
    </Header>
    <SourceDocuments>
        <SalesInvoices>
            <Invoice>
                <InvoiceNo>FT 2024/001</InvoiceNo>
                <InvoiceDate>2024-06-15</InvoiceDate>
                <CustomerID>CUST001</CustomerID>
            </Invoice>
        </SalesInvoices>
    </SourceDocuments>
</AuditFile>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("business rule") || error.Contains("compliance"));
                
                // Test invalid business rule (invoice date outside fiscal year)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
    <Header>
        <CompanyID>COMP001</CompanyID>
        <TaxRegistrationNumber>123456789</TaxRegistrationNumber>
        <FiscalYear>2024</FiscalYear>
    </Header>
    <SourceDocuments>
        <SalesInvoices>
            <Invoice>
                <InvoiceNo>FT 2024/001</InvoiceNo>
                <InvoiceDate>2023-12-31</InvoiceDate>
                <CustomerID>CUST001</CustomerID>
            </Invoice>
        </SalesInvoices>
    </SourceDocuments>
</AuditFile>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("business rule") || error.Contains("compliance") || error.Contains("fiscal year"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }
    }
} 