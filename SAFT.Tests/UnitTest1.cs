using Xunit;
using SAFT.Lib;
using System.Collections.Generic;

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
        [Fact]
        public void ValidateWithXSD11Assertions()
        {
            // Test XML with XSD 1.1 assertions
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile xmlns=""urn:OECD:StandardAuditFile-Tax:PT_1.04_01"">
    <Header>
        <AuditFileVersion>1.04_01</AuditFileVersion>
        <CompanyID>123456789</CompanyID>
        <TaxRegistrationNumber>123456789</TaxRegistrationNumber>
        <TaxPeriodStart>2024-01-01</TaxPeriodStart>
        <TaxPeriodEnd>2024-12-31</TaxPeriodEnd>
        <CurrencyCode>EUR</CurrencyCode>
        <DateCreated>2024-01-01</DateCreated>
        <TaxEntity>Company Name</TaxEntity>
        <ProductCompanyTaxID>123456789</ProductCompanyTaxID>
        <SoftwareValidationNumber>123456789</SoftwareValidationNumber>
        <ProductID>Test Product</ProductID>
        <ProductVersion>1.0</ProductVersion>
    </Header>
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
                <Line>
                    <LineNumber>1</LineNumber>
                    <ProductDescription>Test Product</ProductDescription>
                    <Quantity>1</Quantity>
                    <UnitPrice>100.00</UnitPrice>
                    <TaxBase>100.00</TaxBase>
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

            var errors = SchemaValidator.Validate(xml, "../schema1_04.xsd");
            
            // The validation should pass without errors
            Assert.Empty(errors);
        }
        
        [Fact]
        public void ValidateWithXSD11AssertionFailure()
        {
            // Test XML that should fail XSD 1.1 assertions
            var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile xmlns=""urn:OECD:StandardAuditFile-Tax:PT_1.04_01"">
    <Header>
        <AuditFileVersion>1.04_01</AuditFileVersion>
        <CompanyID>123456789</CompanyID>
        <TaxRegistrationNumber>123456789</TaxRegistrationNumber>
        <TaxPeriodStart>2024-01-01</TaxPeriodStart>
        <TaxPeriodEnd>2024-12-31</TaxPeriodEnd>
        <CurrencyCode>EUR</CurrencyCode>
        <DateCreated>2024-01-01</DateCreated>
        <TaxEntity>Company Name</TaxEntity>
        <ProductCompanyTaxID>123456789</ProductCompanyTaxID>
        <SoftwareValidationNumber>123456789</SoftwareValidationNumber>
        <ProductID>Test Product</ProductID>
        <ProductVersion>1.0</ProductVersion>
    </Header>
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
                <Line>
                    <LineNumber>1</LineNumber>
                    <ProductDescription>Test Product</ProductDescription>
                    <Quantity>1</Quantity>
                    <UnitPrice>100.00</UnitPrice>
                    <TaxBase>100.00</TaxBase>
                    <Tax>
                        <TaxType>IVA</TaxType>
                        <TaxCountryRegion>PT</TaxCountryRegion>
                        <TaxCode>RED</TaxCode>
                        <TaxPercentage>0.00</TaxPercentage>
                        <TaxAmount>0.00</TaxAmount>
                    </Tax>
                    <!-- This should trigger an assertion failure because TaxAmount is 0 but no TaxExemptionReason is provided -->
                </Line>
                <DocumentTotals>
                    <TaxPayable>0.00</TaxPayable>
                    <NetTotal>100.00</NetTotal>
                    <GrossTotal>100.00</GrossTotal>
                </DocumentTotals>
            </Invoice>
        </SalesInvoices>
    </SourceDocuments>
</AuditFile>";

            var errors = SchemaValidator.Validate(xml, "../schema1_04.xsd");
            
            // Should have XSD 1.1 assertion errors
            Assert.NotEmpty(errors);
            Assert.Contains(errors, error => error.Contains("XSD 1.1 assertion failed"));
        }
    }
} 