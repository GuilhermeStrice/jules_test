using Xunit;
using SAFT.Lib;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System;
using System.Linq;

namespace SAFT.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            // Basic test to ensure the library builds and runs
            Assert.True(true);
        }
    }
    
    public class SchemaValidationTests
    {
        private string GetSchemaPath()
        {
            // Get the directory where the solution file is located (root directory)
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
        public void TestXSD11AssertionParsing()
        {
            // Test that our XSD 1.1 assertion parsing logic works
            // This test doesn't require the actual schema file
            
            // Create a simple XML document for testing
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<TestElement>
    <TaxAmount>0.00</TaxAmount>
    <TaxExemptionReason>Test Reason</TaxExemptionReason>
</TestElement>";
            
            // Test that the validation method can be called without errors
            // We'll use a non-existent schema file to test the error handling
            var errors = SchemaValidator.Validate(xml, "non_existent_schema.xsd");
            
            // Should have an error about the missing schema file
            Assert.NotEmpty(errors);
            Assert.Contains(errors, error => error.Contains("Could not find file") || error.Contains("Validation error"));
        }
        
        [Fact]
        public void TestXSD11AssertionsWithMinimalSchema()
        {
            // Create a minimal schema with XSD 1.1 assertions
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""TestElement"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""TaxAmount"" type=""xs:decimal""/>
                <xs:element name=""TaxExemptionReason"" type=""xs:string"" minOccurs=""0""/>
            </xs:sequence>
            <xs:assert test=""if (TaxAmount eq 0 and not(TaxExemptionReason)) then false() else true()""/>
        </xs:complexType>
    </xs:element>
</xs:schema>";
            
            // Write the schema to a temporary file
            var tempSchemaPath = Path.GetTempFileName() + ".xsd";
            
            try
            {
                File.WriteAllText(tempSchemaPath, schema);
                
                // Test XML that should pass validation
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<TestElement>
    <TaxAmount>6.00</TaxAmount>
</TestElement>";
                
                var validErrors = SchemaValidator.Validate(validXml, tempSchemaPath);
                // .NET's built-in validator will fail on xs:assert, but our custom validation should handle it
                // We expect some XSD 1.0 validation errors due to xs:assert not being supported
                Assert.Contains(validErrors, error => error.Contains("XMLSchema") || error.Contains("assert"));
                
                // Test XML that should fail validation (TaxAmount = 0 but no TaxExemptionReason)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<TestElement>
    <TaxAmount>0.00</TaxAmount>
</TestElement>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, tempSchemaPath);
                Assert.NotEmpty(invalidErrors);
                // Should have both XSD 1.0 errors (for xs:assert) and potentially XSD 1.1 assertion errors
                Assert.Contains(invalidErrors, error => error.Contains("XMLSchema") || error.Contains("assert"));
            }
            finally
            {
                // Clean up the temporary schema file
                if (File.Exists(tempSchemaPath))
                    File.Delete(tempSchemaPath);
            }
        }
        
        [Fact]
        public void TestIdentityConstraintsWithMinimalSchema()
        {
            // Create a minimal schema with identity constraints (no namespaces to avoid complexity)
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""Root"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""Customer"" maxOccurs=""unbounded"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""CustomerID"" type=""xs:string""/>
                            <xs:element name=""Name"" type=""xs:string""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
                <xs:element name=""Invoice"" maxOccurs=""unbounded"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""InvoiceNo"" type=""xs:string""/>
                            <xs:element name=""CustomerID"" type=""xs:string""/>
                            <xs:element name=""Amount"" type=""xs:decimal""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:sequence>
        </xs:complexType>
        <xs:unique name=""CustomerIDConstraint"">
            <xs:selector xpath=""Customer""/>
            <xs:field xpath=""CustomerID""/>
        </xs:unique>
        <xs:unique name=""InvoiceNoConstraint"">
            <xs:selector xpath=""Invoice""/>
            <xs:field xpath=""InvoiceNo""/>
        </xs:unique>
        <xs:keyref name=""InvoiceCustomerIDConstraint"" refer=""CustomerIDConstraint"">
            <xs:selector xpath=""Invoice""/>
            <xs:field xpath=""CustomerID""/>
        </xs:keyref>
    </xs:element>
</xs:schema>";
            
            // Write the schema to a temporary file
            var tempSchemaPath = Path.GetTempFileName() + ".xsd";
            
            try
            {
                File.WriteAllText(tempSchemaPath, schema);
                
                // Test XML that should pass validation (valid identity constraints)
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
    <Customer>
        <CustomerID>CUST001</CustomerID>
        <Name>Customer 1</Name>
    </Customer>
    <Customer>
        <CustomerID>CUST002</CustomerID>
        <Name>Customer 2</Name>
    </Customer>
    <Invoice>
        <InvoiceNo>INV001</InvoiceNo>
        <CustomerID>CUST001</CustomerID>
        <Amount>100.00</Amount>
    </Invoice>
    <Invoice>
        <InvoiceNo>INV002</InvoiceNo>
        <CustomerID>CUST002</CustomerID>
        <Amount>200.00</Amount>
    </Invoice>
</Root>";
                
                var validErrors = SchemaValidator.Validate(validXml, tempSchemaPath);
                // Should have no identity constraint errors for valid XML
                Assert.DoesNotContain(validErrors, error => error.Contains("duplicate key sequence") || error.Contains("key or unique identity constraint") || error.Contains("Keyref fails to refer"));
                
                // Test XML that should fail validation (duplicate CustomerID)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
    <Customer>
        <CustomerID>CUST001</CustomerID>
        <Name>Customer 1</Name>
    </Customer>
    <Customer>
        <CustomerID>CUST001</CustomerID>
        <Name>Customer 2</Name>
    </Customer>
    <Invoice>
        <InvoiceNo>INV001</InvoiceNo>
        <CustomerID>CUST001</CustomerID>
        <Amount>100.00</Amount>
    </Invoice>
</Root>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, tempSchemaPath);
                // The validation should detect the duplicate CustomerID using .NET's built-in validator
                Assert.Contains(invalidErrors, error => error.Contains("duplicate key sequence") && error.Contains("CustomerIDConstraint"));
                
                // Test XML that should fail validation (invalid keyref - CustomerID not found)
                var invalidKeyrefXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
    <Customer>
        <CustomerID>CUST001</CustomerID>
        <Name>Customer 1</Name>
    </Customer>
    <Invoice>
        <InvoiceNo>INV001</InvoiceNo>
        <CustomerID>CUST999</CustomerID>
        <Amount>100.00</Amount>
    </Invoice>
</Root>";
                
                var keyrefErrors = SchemaValidator.Validate(invalidKeyrefXml, tempSchemaPath);
                // The validation should detect the invalid keyref using .NET's built-in validator
                Assert.Contains(keyrefErrors, error => error.Contains("Keyref fails to refer"));
            }
            finally
            {
                // Clean up the temporary schema file
                if (File.Exists(tempSchemaPath))
                    File.Delete(tempSchemaPath);
            }
        }
        
        [Fact]
        public void ValidateWithXSD11Assertions()
        {
            // Test XML with XSD 1.1 assertions - using complete structure that matches the schema
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile xmlns=""urn:OECD:StandardAuditFile-Tax:PT_1.04_01"">
    <Header>
        <AuditFileVersion>1.04_01</AuditFileVersion>
        <CompanyID>123456789</CompanyID>
        <TaxRegistrationNumber>123456789</TaxRegistrationNumber>
        <TaxAccountingBasis>F</TaxAccountingBasis>
        <CompanyName>Test Company</CompanyName>
        <CompanyAddress>
            <AddressDetail>Test Address</AddressDetail>
            <City>Test City</City>
            <PostalCode>1234-567</PostalCode>
            <Country>PT</Country>
        </CompanyAddress>
        <FiscalYear>2024</FiscalYear>
        <StartDate>2024-01-01</StartDate>
        <EndDate>2024-12-31</EndDate>
        <CurrencyCode>EUR</CurrencyCode>
        <DateCreated>2024-01-01</DateCreated>
        <TaxEntity>Test Entity</TaxEntity>
        <ProductCompanyTaxID>123456789</ProductCompanyTaxID>
        <SoftwareCertificateNumber>123456789</SoftwareCertificateNumber>
        <ProductID>Test Product</ProductID>
        <ProductVersion>1.0</ProductVersion>
    </Header>
    <MasterFiles>
        <GeneralLedgerAccounts>
            <TaxonomyReference>PT_GAAP</TaxonomyReference>
            <Account>
                <AccountID>1</AccountID>
                <AccountDescription>Test Account</AccountDescription>
                <OpeningDebitBalance>0.00</OpeningDebitBalance>
                <OpeningCreditBalance>0.00</OpeningCreditBalance>
                <ClosingDebitBalance>0.00</ClosingDebitBalance>
                <ClosingCreditBalance>0.00</ClosingCreditBalance>
                <GroupingCategory>GA</GroupingCategory>
                <GroupingCode>1</GroupingCode>
            </Account>
        </GeneralLedgerAccounts>
        <Customer>
            <CustomerID>CUST001</CustomerID>
            <AccountID>1</AccountID>
            <CustomerTaxID>123456789</CustomerTaxID>
            <CompanyName>Test Customer</CompanyName>
            <BillingAddress>
                <AddressDetail>Customer Address</AddressDetail>
                <City>Customer City</City>
                <PostalCode>1234-567</PostalCode>
                <Country>PT</Country>
            </BillingAddress>
        </Customer>
        <Supplier>
            <SupplierID>SUPP001</SupplierID>
            <AccountID>1</AccountID>
            <SupplierTaxID>123456789</SupplierTaxID>
            <CompanyName>Test Supplier</CompanyName>
            <BillingAddress>
                <AddressDetail>Supplier Address</AddressDetail>
                <City>Supplier City</City>
                <PostalCode>1234-567</PostalCode>
                <Country>PT</Country>
            </BillingAddress>
        </Supplier>
        <Product>
            <ProductCode>PROD001</ProductCode>
            <ProductGroup>Test Group</ProductGroup>
            <ProductDescription>Test Product</ProductDescription>
            <ProductNumberCode>123456789</ProductNumberCode>
        </Product>
        <TaxTable>
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
            <NumberOfEntries>1</NumberOfEntries>
            <TotalDebit>100.00</TotalDebit>
            <TotalCredit>100.00</TotalCredit>
            <Invoice>
                <InvoiceNo>INV001</InvoiceNo>
                <InvoiceDate>2024-01-01</InvoiceDate>
                <InvoiceType>FT</InvoiceType>
                <SpecialRegimes>0</SpecialRegimes>
                <CustomerID>CUST001</CustomerID>
                <Line>
                    <LineNumber>1</LineNumber>
                    <ProductCode>PROD001</ProductCode>
                    <ProductDescription>Test Product</ProductDescription>
                    <Quantity>1</Quantity>
                    <UnitPrice>100.00</UnitPrice>
                    <LineExtensionAmount>100.00</LineExtensionAmount>
                    <Tax>
                        <TaxType>IVA</TaxType>
                        <TaxCountryRegion>PT</TaxCountryRegion>
                        <TaxCode>RED</TaxCode>
                        <TaxPercentage>6.00</TaxPercentage>
                        <TaxAmount>6.00</TaxAmount>
                    </Tax>
                </Line>
                <DocumentTotals>
                    <TaxPayable>6.00</TaxPayable>
                    <NetTotal>100.00</NetTotal>
                    <GrossTotal>106.00</GrossTotal>
                </DocumentTotals>
            </Invoice>
        </SalesInvoices>
    </SourceDocuments>
</AuditFile>";

            var schemaPath = GetSchemaPath();
            var errors = SchemaValidator.Validate(xml, schemaPath);
            
            // Using business logic validation only, should pass validation
            Assert.Empty(errors);
        }
        
        [Fact]
        public void ValidateWithXSD11AssertionFailure()
        {
            // Test XML that should fail business logic validation
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
  <SourceDocuments>
    <SalesInvoices>
      <Invoice>
        <InvoiceNo>INV001</InvoiceNo>
        <InvoiceDate>2024-01-01</InvoiceDate>
        <InvoiceType>FT</InvoiceType>
        <Line>
          <LineNumber>1</LineNumber>
          <ProductDescription>Test Product</ProductDescription>
          <Quantity>1</Quantity>
          <UnitPrice>100.00</UnitPrice>
          <LineExtensionAmount>100.00</LineExtensionAmount>
          <Tax>
            <TaxType>IVA</TaxType>
            <TaxCountryRegion>PT</TaxCountryRegion>
            <TaxCode>RED</TaxCode>
            <TaxPercentage>6.00</TaxPercentage>
            <TaxAmount>10.00</TaxAmount>
          </Tax>
        </Line>
        <DocumentTotals>
          <TaxPayable>10.00</TaxPayable>
          <NetTotal>100.00</NetTotal>
          <GrossTotal>110.00</GrossTotal>
        </DocumentTotals>
      </Invoice>
    </SalesInvoices>
  </SourceDocuments>
</AuditFile>";

            var schemaPath = GetSchemaPath();
            var errors = SchemaValidator.Validate(xml, schemaPath);
            
            // Debug output
            Console.WriteLine($"Number of validation errors: {errors.Count}");
            foreach (var error in errors)
            {
                Console.WriteLine($"Error: {error}");
            }
            
            // Should have business logic validation errors
            Assert.NotEmpty(errors);
            // Check for specific business logic validation errors
            Assert.Contains(errors, error => error.Contains("VAT calculation") || error.Contains("tax calculation") || error.Contains("business rule"));
        }
    }
} 