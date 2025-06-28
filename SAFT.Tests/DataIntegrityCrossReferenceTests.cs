using Xunit;
using SAFT.Lib;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SAFT.Tests
{
    public class DataIntegrityCrossReferenceTests
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
        public void TestCustomerReferenceIntegrity()
        {
            // Test customer references in invoices
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""AuditFile"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""MasterFiles"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""Customer"">
                                <xs:complexType>
                                    <xs:sequence>
                                        <xs:element name=""CustomerID"" type=""xs:string""/>
                                        <xs:element name=""AccountID"" type=""xs:string""/>
                                        <xs:element name=""CustomerTaxID"" type=""xs:string""/>
                                    </xs:sequence>
                                </xs:complexType>
                            </xs:element>
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
            
            var schemaPath = Path.GetTempFileName() + ".xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid customer references
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
    <MasterFiles>
        <Customer>
            <CustomerID>CUST001</CustomerID>
            <AccountID>ACCT001</AccountID>
            <CustomerTaxID>123456789</CustomerTaxID>
        </Customer>
        <Customer>
            <CustomerID>CUST002</CustomerID>
            <AccountID>ACCT002</AccountID>
            <CustomerTaxID>987654321</CustomerTaxID>
        </Customer>
    </MasterFiles>
    <SourceDocuments>
        <SalesInvoices>
            <Invoice>
                <InvoiceNo>FT 2024/001</InvoiceNo>
                <CustomerID>CUST001</CustomerID>
            </Invoice>
            <Invoice>
                <InvoiceNo>FT 2024/002</InvoiceNo>
                <CustomerID>CUST002</CustomerID>
            </Invoice>
        </SalesInvoices>
    </SourceDocuments>
</AuditFile>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("customer reference") || error.Contains("CustomerID"));
                
                // Test invalid customer reference (non-existent customer)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
    <MasterFiles>
        <Customer>
            <CustomerID>CUST001</CustomerID>
            <AccountID>ACCT001</AccountID>
            <CustomerTaxID>123456789</CustomerTaxID>
        </Customer>
    </MasterFiles>
    <SourceDocuments>
        <SalesInvoices>
            <Invoice>
                <InvoiceNo>FT 2024/001</InvoiceNo>
                <CustomerID>CUST001</CustomerID>
            </Invoice>
            <Invoice>
                <InvoiceNo>FT 2024/002</InvoiceNo>
                <CustomerID>NONEXISTENT</CustomerID>
            </Invoice>
        </SalesInvoices>
    </SourceDocuments>
</AuditFile>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("customer reference") || error.Contains("CustomerID") || error.Contains("not found"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestProductReferenceIntegrity()
        {
            // Test product references in invoice lines
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""AuditFile"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""MasterFiles"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""Product"">
                                <xs:complexType>
                                    <xs:sequence>
                                        <xs:element name=""ProductCode"" type=""xs:string""/>
                                        <xs:element name=""ProductDescription"" type=""xs:string""/>
                                        <xs:element name=""ProductNumberCode"" type=""xs:string""/>
                                    </xs:sequence>
                                </xs:complexType>
                            </xs:element>
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
                                                    <xs:element name=""Line"" maxOccurs=""unbounded"">
                                                        <xs:complexType>
                                                            <xs:sequence>
                                                                <xs:element name=""ProductCode"" type=""xs:string""/>
                                                                <xs:element name=""Quantity"" type=""xs:decimal""/>
                                                                <xs:element name=""UnitPrice"" type=""xs:decimal""/>
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
            </xs:sequence>
        </xs:complexType>
    </xs:element>
</xs:schema>";
            
            var schemaPath = Path.GetTempFileName() + ".xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid product references
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
    <MasterFiles>
        <Product>
            <ProductCode>PROD001</ProductCode>
            <ProductDescription>Product 1</ProductDescription>
            <ProductNumberCode>P001</ProductNumberCode>
        </Product>
        <Product>
            <ProductCode>PROD002</ProductCode>
            <ProductDescription>Product 2</ProductDescription>
            <ProductNumberCode>P002</ProductNumberCode>
        </Product>
    </MasterFiles>
    <SourceDocuments>
        <SalesInvoices>
            <Invoice>
                <Line>
                    <ProductCode>PROD001</ProductCode>
                    <Quantity>2.0</Quantity>
                    <UnitPrice>50.00</UnitPrice>
                </Line>
                <Line>
                    <ProductCode>PROD002</ProductCode>
                    <Quantity>1.0</Quantity>
                    <UnitPrice>100.00</UnitPrice>
                </Line>
            </Invoice>
        </SalesInvoices>
    </SourceDocuments>
</AuditFile>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("product reference") || error.Contains("ProductCode"));
                
                // Test invalid product reference (non-existent product)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
    <MasterFiles>
        <Product>
            <ProductCode>PROD001</ProductCode>
            <ProductDescription>Product 1</ProductDescription>
            <ProductNumberCode>P001</ProductNumberCode>
        </Product>
    </MasterFiles>
    <SourceDocuments>
        <SalesInvoices>
            <Invoice>
                <Line>
                    <ProductCode>PROD001</ProductCode>
                    <Quantity>2.0</Quantity>
                    <UnitPrice>50.00</UnitPrice>
                </Line>
                <Line>
                    <ProductCode>NONEXISTENT</ProductCode>
                    <Quantity>1.0</Quantity>
                    <UnitPrice>100.00</UnitPrice>
                </Line>
            </Invoice>
        </SalesInvoices>
    </SourceDocuments>
</AuditFile>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("product reference") || error.Contains("ProductCode") || error.Contains("not found"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestTaxCodeReferenceIntegrity()
        {
            // Test tax code references in invoice lines
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""AuditFile"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""MasterFiles"">
                    <xs:complexType>
                        <xs:sequence>
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
                                                    <xs:element name=""Line"" maxOccurs=""unbounded"">
                                                        <xs:complexType>
                                                            <xs:sequence>
                                                                <xs:element name=""Tax"">
                                                                    <xs:complexType>
                                                                        <xs:sequence>
                                                                            <xs:element name=""TaxType"" type=""xs:string""/>
                                                                            <xs:element name=""TaxCountryRegion"" type=""xs:string""/>
                                                                            <xs:element name=""TaxCode"" type=""xs:string""/>
                                                                            <xs:element name=""TaxPercentage"" type=""xs:decimal""/>
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
                // Test valid tax code references
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
    <MasterFiles>
        <TaxTable>
            <TaxTableEntry>
                <TaxType>IVA</TaxType>
                <TaxCountryRegion>PT</TaxCountryRegion>
                <TaxCode>NOR</TaxCode>
                <Description>Normal Rate</Description>
                <TaxPercentage>23.00</TaxPercentage>
            </TaxTableEntry>
            <TaxTableEntry>
                <TaxType>IVA</TaxType>
                <TaxCountryRegion>PT</TaxCountryRegion>
                <TaxCode>RED</TaxCode>
                <Description>Reduced Rate</Description>
                <TaxPercentage>6.00</TaxPercentage>
            </TaxTableEntry>
        </TaxTable>
    </MasterFiles>
    <SourceDocuments>
        <SalesInvoices>
            <Invoice>
                <Line>
                    <Tax>
                        <TaxType>IVA</TaxType>
                        <TaxCountryRegion>PT</TaxCountryRegion>
                        <TaxCode>NOR</TaxCode>
                        <TaxPercentage>23.00</TaxPercentage>
                    </Tax>
                </Line>
                <Line>
                    <Tax>
                        <TaxType>IVA</TaxType>
                        <TaxCountryRegion>PT</TaxCountryRegion>
                        <TaxCode>RED</TaxCode>
                        <TaxPercentage>6.00</TaxPercentage>
                    </Tax>
                </Line>
            </Invoice>
        </SalesInvoices>
    </SourceDocuments>
</AuditFile>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("tax code reference") || error.Contains("TaxCode"));
                
                // Test invalid tax code reference (non-existent tax code)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
    <MasterFiles>
        <TaxTable>
            <TaxTableEntry>
                <TaxType>IVA</TaxType>
                <TaxCountryRegion>PT</TaxCountryRegion>
                <TaxCode>NOR</TaxCode>
                <Description>Normal Rate</Description>
                <TaxPercentage>23.00</TaxPercentage>
            </TaxTableEntry>
        </TaxTable>
    </MasterFiles>
    <SourceDocuments>
        <SalesInvoices>
            <Invoice>
                <Line>
                    <Tax>
                        <TaxType>IVA</TaxType>
                        <TaxCountryRegion>PT</TaxCountryRegion>
                        <TaxCode>NOR</TaxCode>
                        <TaxPercentage>23.00</TaxPercentage>
                    </Tax>
                </Line>
                <Line>
                    <Tax>
                        <TaxType>IVA</TaxType>
                        <TaxCountryRegion>PT</TaxCountryRegion>
                        <TaxCode>INVALID</TaxCode>
                        <TaxPercentage>25.00</TaxPercentage>
                    </Tax>
                </Line>
            </Invoice>
        </SalesInvoices>
    </SourceDocuments>
</AuditFile>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("tax code reference") || error.Contains("TaxCode") || error.Contains("not found"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestAccountReferenceIntegrity()
        {
            // Test account references in general ledger entries
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""AuditFile"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""MasterFiles"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""GeneralLedgerAccounts"">
                                <xs:complexType>
                                    <xs:sequence>
                                        <xs:element name=""Account"" maxOccurs=""unbounded"">
                                            <xs:complexType>
                                                <xs:sequence>
                                                    <xs:element name=""AccountID"" type=""xs:string""/>
                                                    <xs:element name=""AccountDescription"" type=""xs:string""/>
                                                    <xs:element name=""AccountType"" type=""xs:string""/>
                                                </xs:sequence>
                                            </xs:complexType>
                                        </xs:element>
                                    </xs:sequence>
                                </xs:complexType>
                            </xs:element>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
                <xs:element name=""GeneralLedgerEntries"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""Journal"" maxOccurs=""unbounded"">
                                <xs:complexType>
                                    
<xs:sequence>
                                        <xs:element name=""Transaction"" maxOccurs=""unbounded"">
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
                // Test valid account references
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
    <MasterFiles>
        <GeneralLedgerAccounts>
            <Account>
                <AccountID>1101</AccountID>
                <AccountDescription>Cash</AccountDescription>
                <AccountType>Asset</AccountType>
            </Account>
            <Account>
                <AccountID>4101</AccountID>
                <AccountDescription>Sales Revenue</AccountDescription>
                <AccountType>Revenue</AccountType>
            </Account>
        </GeneralLedgerAccounts>
    </MasterFiles>
    <GeneralLedgerEntries>
        <Journal>
            <Transaction>
                <Line>
                    <AccountID>1101</AccountID>
                    <DebitAmount>1000.00</DebitAmount>
                </Line>
                <Line>
                    <AccountID>4101</AccountID>
                    <CreditAmount>1000.00</CreditAmount>
                </Line>
            </Transaction>
        </Journal>
    </GeneralLedgerEntries>
</AuditFile>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("account reference") || error.Contains("AccountID"));
                
                // Test invalid account reference (non-existent account)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
    <MasterFiles>
        <GeneralLedgerAccounts>
            <Account>
                <AccountID>1101</AccountID>
                <AccountDescription>Cash</AccountDescription>
                <AccountType>Asset</AccountType>
            </Account>
        </GeneralLedgerAccounts>
    </MasterFiles>
    <GeneralLedgerEntries>
        <Journal>
            <Transaction>
                <Line>
                    <AccountID>1101</AccountID>
                    <DebitAmount>1000.00</DebitAmount>
                </Line>
                <Line>
                    <AccountID>9999</AccountID>
                    <CreditAmount>1000.00</CreditAmount>
                </Line>
            </Transaction>
        </Journal>
    </GeneralLedgerEntries>
</AuditFile>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("account reference") || error.Contains("AccountID") || error.Contains("not found"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestDocumentTotalsConsistency()
        {
            // Test document totals consistency across lines and totals
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
                // Test valid document totals (consistent)
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice>
    <Line>
        <LineExtensionAmount>100.00</LineExtensionAmount>
        <Tax>
            <TaxAmount>23.00</TaxAmount>
        </Tax>
    </Line>
    <Line>
        <LineExtensionAmount>50.00</LineExtensionAmount>
        <Tax>
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
                Assert.DoesNotContain(validErrors, error => error.Contains("document totals") || error.Contains("consistency"));
                
                // Test invalid document totals (inconsistent)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Invoice>
    <Line>
        <LineExtensionAmount>100.00</LineExtensionAmount>
        <Tax>
            <TaxAmount>23.00</TaxAmount>
        </Tax>
    </Line>
    <Line>
        <LineExtensionAmount>50.00</LineExtensionAmount>
        <Tax>
            <TaxAmount>11.50</TaxAmount>
        </Tax>
    </Line>
    <DocumentTotals>
        <NetTotal>150.00</NetTotal>
        <TaxPayable>30.00</TaxPayable>
        <GrossTotal>180.00</GrossTotal>
    </DocumentTotals>
</Invoice>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("document totals") || error.Contains("consistency") || error.Contains("mismatch"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestDateRangeConsistency()
        {
            // Test date range consistency across documents
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""AuditFile"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""Header"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""StartDate"" type=""xs:date""/>
                            <xs:element name=""EndDate"" type=""xs:date""/>
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
                                                    <xs:element name=""InvoiceDate"" type=""xs:date""/>
                                                    <xs:element name=""InvoiceNo"" type=""xs:string""/>
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
            
            var schemaPath = Path.GetTempFileName() + ".xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid date range (all dates within range)
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
    <Header>
        <StartDate>2024-01-01</StartDate>
        <EndDate>2024-12-31</EndDate>
    </Header>
    <SourceDocuments>
        <SalesInvoices>
            <Invoice>
                <InvoiceDate>2024-06-15</InvoiceDate>
                <InvoiceNo>FT 2024/001</InvoiceNo>
            </Invoice>
            <Invoice>
                <InvoiceDate>2024-12-20</InvoiceDate>
                <InvoiceNo>FT 2024/002</InvoiceNo>
            </Invoice>
        </SalesInvoices>
    </SourceDocuments>
</AuditFile>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("date range") || error.Contains("outside"));
                
                // Test invalid date range (date outside range)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
    <Header>
        <StartDate>2024-01-01</StartDate>
        <EndDate>2024-12-31</EndDate>
    </Header>
    <SourceDocuments>
        <SalesInvoices>
            <Invoice>
                <InvoiceDate>2024-06-15</InvoiceDate>
                <InvoiceNo>FT 2024/001</InvoiceNo>
            </Invoice>
            <Invoice>
                <InvoiceDate>2023-12-31</InvoiceDate>
                <InvoiceNo>FT 2024/002</InvoiceNo>
            </Invoice>
        </SalesInvoices>
    </SourceDocuments>
</AuditFile>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("date range") || error.Contains("outside") || error.Contains("period"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }
    }
} 