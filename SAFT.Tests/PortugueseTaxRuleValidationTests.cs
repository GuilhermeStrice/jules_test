using Xunit;
using SAFT.Lib;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SAFT.Tests
{
    public class PortugueseTaxRuleValidationTests
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
        public void TestVATCalculationValidation()
        {
            // Test VAT (IVA) calculation validation
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""Invoice"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""Line"" maxOccurs=""unbounded"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""TaxBase"" type=""xs:decimal""/>
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
            
            var schemaPath = Path.GetTempFileName() + ".xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid VAT calculation
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice>
    <Line>
        <TaxBase>100.00</TaxBase>
        <Tax>
            <TaxPercentage>23.00</TaxPercentage>
            <TaxAmount>23.00</TaxAmount>
        </Tax>
    </Line>
    <DocumentTotals>
        <NetTotal>100.00</NetTotal>
        <TaxPayable>23.00</TaxPayable>
        <GrossTotal>123.00</GrossTotal>
    </DocumentTotals>
</Invoice>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("VAT calculation"));
                
                // Test invalid VAT calculation (wrong tax amount)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice>
    <Line>
        <TaxBase>100.00</TaxBase>
        <Tax>
            <TaxPercentage>23.00</TaxPercentage>
            <TaxAmount>25.00</TaxAmount>
        </Tax>
    </Line>
    <DocumentTotals>
        <NetTotal>100.00</NetTotal>
        <TaxPayable>25.00</TaxPayable>
        <GrossTotal>125.00</GrossTotal>
    </DocumentTotals>
</Invoice>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                // Should detect VAT calculation error (23% of 100.00 should be 23.00, not 25.00)
                Assert.Contains(invalidErrors, error => error.Contains("VAT calculation") || error.Contains("tax calculation"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestPortugueseTaxCodesValidation()
        {
            // Test Portuguese tax codes validation
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""TaxTable"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""TaxTableEntry"" maxOccurs=""unbounded"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""TaxType"" type=""xs:string""/>
                            <xs:element name=""TaxCountryRegion"" type=""xs:string""/>
                            <xs:element name=""TaxCode"" type=""xs:string""/>
                            <xs:element name=""Description"" type=""xs:string""/>
                            <xs:element name=""TaxPercentage"" type=""xs:decimal""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:sequence>
        </xs:complexType>
    </xs:element>
</xs:schema>";
            
            var schemaPath = Path.GetTempFileName() + ".xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid Portuguese tax codes
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<TaxTable>
    <TaxTableEntry>
        <TaxType>IVA</TaxType>
        <TaxCountryRegion>PT</TaxCountryRegion>
        <TaxCode>RED</TaxCode>
        <Description>Reduced Rate</Description>
        <TaxPercentage>6.00</TaxPercentage>
    </TaxTableEntry>
    <TaxTableEntry>
        <TaxType>IVA</TaxType>
        <TaxCountryRegion>PT</TaxCountryRegion>
        <TaxCode>INT</TaxCode>
        <Description>Intermediate Rate</Description>
        <TaxPercentage>13.00</TaxPercentage>
    </TaxTableEntry>
    <TaxTableEntry>
        <TaxType>IVA</TaxType>
        <TaxCountryRegion>PT</TaxCountryRegion>
        <TaxCode>NOR</TaxCode>
        <Description>Normal Rate</Description>
        <TaxPercentage>23.00</TaxPercentage>
    </TaxTableEntry>
</TaxTable>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("tax code") || error.Contains("Portuguese tax"));
                
                // Test invalid Portuguese tax code
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<TaxTable>
    <TaxTableEntry>
        <TaxType>IVA</TaxType>
        <TaxCountryRegion>PT</TaxCountryRegion>
        <TaxCode>INVALID</TaxCode>
        <Description>Invalid Code</Description>
        <TaxPercentage>25.00</TaxPercentage>
    </TaxTableEntry>
</TaxTable>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("tax code") || error.Contains("Portuguese tax") || error.Contains("invalid"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestPortugueseVATNumberValidation()
        {
            // Test Portuguese VAT number validation (9 digits)
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""Company"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""CompanyID"" type=""xs:string""/>
                <xs:element name=""TaxRegistrationNumber"" type=""xs:integer""/>
            </xs:sequence>
        </xs:complexType>
    </xs:element>
</xs:schema>";
            
            var schemaPath = Path.GetTempFileName() + ".xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid Portuguese VAT number (9 digits)
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Company>
    <CompanyID>COMP001</CompanyID>
    <TaxRegistrationNumber>123456789</TaxRegistrationNumber>
</Company>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("VAT number") || error.Contains("tax registration"));
                
                // Test invalid Portuguese VAT number (wrong format)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Company>
    <CompanyID>COMP001</CompanyID>
    <TaxRegistrationNumber>12345678</TaxRegistrationNumber>
</Company>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("VAT number") || error.Contains("tax registration") || error.Contains("format"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestDocumentSequenceValidation()
        {
            // Test document sequence validation (invoice numbering compliance)
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
            
            var schemaPath = Path.GetTempFileName() + ".xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid document sequence
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
                Assert.DoesNotContain(validErrors, error => error.Contains("document sequence") || error.Contains("invoice numbering"));
                
                // Test invalid document sequence (missing invoice)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<SalesInvoices>
    <Invoice>
        <InvoiceNo>FT 2024/001</InvoiceNo>
        <InvoiceDate>2024-01-01</InvoiceDate>
        <InvoiceType>FT</InvoiceType>
    </Invoice>
    <Invoice>
        <InvoiceNo>FT 2024/003</InvoiceNo>
        <InvoiceDate>2024-01-03</InvoiceDate>
        <InvoiceType>FT</InvoiceType>
    </Invoice>
</SalesInvoices>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("document sequence") || error.Contains("invoice numbering") || error.Contains("missing"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestFiscalYearValidation()
        {
            // Test fiscal year and period validation
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""AuditFile"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""Header"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""FiscalYear"" type=""xs:string""/>
                            <xs:element name=""StartDate"" type=""xs:date""/>
                            <xs:element name=""EndDate"" type=""xs:date""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
                <xs:element name=""SourceDocuments"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""Invoice"" maxOccurs=""unbounded"">
                                <xs:complexType>
                                    <xs:sequence>
                                        <xs:element name=""InvoiceDate"" type=""xs:date""/>
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
            
            var schemaPath = Path.GetTempFileName() + ".xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid fiscal year
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
    <Header>
        <FiscalYear>2024</FiscalYear>
        <StartDate>2024-01-01</StartDate>
        <EndDate>2024-12-31</EndDate>
    </Header>
    <SourceDocuments>
        <Invoice>
            <InvoiceDate>2024-06-15</InvoiceDate>
        </Invoice>
        <Invoice>
            <InvoiceDate>2024-12-20</InvoiceDate>
        </Invoice>
    </SourceDocuments>
</AuditFile>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("fiscal year") || error.Contains("date range"));
                
                // Test invalid fiscal year (invoice date outside fiscal year)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
    <Header>
        <FiscalYear>2024</FiscalYear>
        <StartDate>2024-01-01</StartDate>
        <EndDate>2024-12-31</EndDate>
    </Header>
    <SourceDocuments>
        <Invoice>
            <InvoiceDate>2023-12-31</InvoiceDate>
        </Invoice>
    </SourceDocuments>
</AuditFile>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("fiscal year") || error.Contains("date range") || error.Contains("outside"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestPortugueseCurrencyValidation()
        {
            // Test Portuguese currency (EUR) compliance
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""AuditFile"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""Header"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""CurrencyCode"" type=""xs:string""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
                <xs:element name=""SourceDocuments"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""Invoice"" maxOccurs=""unbounded"">
                                <xs:complexType>
                                    <xs:sequence>
                                        <xs:element name=""Amount"" type=""xs:decimal""/>
                                        <xs:element name=""Currency"" type=""xs:string"" minOccurs=""0""/>
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
            
            var schemaPath = Path.GetTempFileName() + ".xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid Portuguese currency (EUR)
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
    <Header>
        <CurrencyCode>EUR</CurrencyCode>
    </Header>
    <SourceDocuments>
        <Invoice>
            <Amount>100.00</Amount>
            <Currency>EUR</Currency>
        </Invoice>
    </SourceDocuments>
</AuditFile>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("currency") || error.Contains("EUR"));
                
                // Test invalid currency (non-EUR)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
    <Header>
        <CurrencyCode>USD</CurrencyCode>
    </Header>
    <SourceDocuments>
        <Invoice>
            <Amount>100.00</Amount>
            <Currency>USD</Currency>
        </Invoice>
    </SourceDocuments>
</AuditFile>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("currency") || error.Contains("EUR") || error.Contains("Portuguese"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestTaxExemptionValidation()
        {
            // Test tax exemption reason and code validation
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""Invoice"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""Line"" maxOccurs=""unbounded"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""Tax"">
                                <xs:complexType>
                                    <xs:sequence>
                                        <xs:element name=""TaxPercentage"" type=""xs:decimal""/>
                                        <xs:element name=""TaxAmount"" type=""xs:decimal""/>
                                    </xs:sequence>
                                </xs:complexType>
                            </xs:element>
                            <xs:element name=""TaxExemptionReason"" type=""xs:string"" minOccurs=""0""/>
                            <xs:element name=""TaxExemptionCode"" type=""xs:string"" minOccurs=""0""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:sequence>
        </xs:complexType>
    </xs:element>
</xs:schema>";
            
            var schemaPath = Path.GetTempFileName() + ".xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid tax exemption (0% tax with reason)
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice>
    <Line>
        <Tax>
            <TaxPercentage>0.00</TaxPercentage>
            <TaxAmount>0.00</TaxAmount>
        </Tax>
        <TaxExemptionReason>Export of goods</TaxExemptionReason>
        <TaxExemptionCode>M01</TaxExemptionCode>
    </Line>
</Invoice>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("tax exemption") || error.Contains("exemption reason"));
                
                // Test invalid tax exemption (0% tax without reason)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice>
    <Line>
        <Tax>
            <TaxPercentage>0.00</TaxPercentage>
            <TaxAmount>0.00</TaxAmount>
        </Tax>
    </Line>
</Invoice>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("tax exemption") || error.Contains("exemption reason") || error.Contains("required"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }
    }
} 