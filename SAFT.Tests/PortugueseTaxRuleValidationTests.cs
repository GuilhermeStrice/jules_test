using Xunit;
using SAFT.Lib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SAFT.Tests;

namespace SAFT.Tests
{
    public class PortugueseTaxRuleValidationTests
    {
        [Fact]
        public void TestVATCalculationValidation()
        {
            // Test VAT (IVA) calculation validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidateVATCalculations(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: modify tax amount to create calculation mismatch
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find a line and modify its tax amount to create calculation mismatch
            var lines = xmlDoc.SelectNodes("//ns:Line", nsManager);
            if (lines != null && lines.Count > 0)
            {
                var line = lines[0];
                var tax = line.SelectSingleNode("ns:Tax", nsManager);
                if (tax != null)
                {
                    var taxAmount = tax.SelectSingleNode("ns:TaxAmount", nsManager);
                    if (taxAmount != null)
                    {
                        // Modify the tax amount to create a calculation mismatch
                        var originalAmount = decimal.Parse(taxAmount.InnerText);
                        taxAmount.InnerText = (originalAmount + 10.00m).ToString("F2"); // Add 10 to create mismatch
                        
                        var invalidErrors = SchemaValidator.ValidateVATCalculations(xmlDoc);
                        Assert.Contains(invalidErrors, error => error.Contains("VAT calculation") || error.Contains("tax calculation"));
                    }
                }
            }
        }

        [Fact]
        public void TestPortugueseTaxCodesValidation()
        {
            // Test Portuguese tax codes validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidatePortugueseTaxRules(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: modify tax code to invalid value
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find a tax table entry and modify its tax code to invalid value
            var taxTableEntries = xmlDoc.SelectNodes("//ns:TaxTableEntry", nsManager);
            if (taxTableEntries != null && taxTableEntries.Count > 0)
            {
                var taxTableEntry = taxTableEntries[0];
                var taxCode = taxTableEntry.SelectSingleNode("ns:TaxCode", nsManager);
                if (taxCode != null)
                {
                    taxCode.InnerText = "INVALID"; // Set to invalid tax code
                    
                    var invalidErrors = SchemaValidator.ValidatePortugueseTaxRules(xmlDoc);
                    Assert.Contains(invalidErrors, error => error.Contains("tax code") || error.Contains("Portuguese tax") || error.Contains("invalid"));
                }
            }
        }

        [Fact]
        public void TestPortugueseVATNumberValidation()
        {
            // Test Portuguese VAT number validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidatePortugueseTaxRules(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: modify VAT number to invalid format
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find TaxRegistrationNumber and modify it to invalid VAT number
            var taxRegistrationNumber = xmlDoc.SelectSingleNode("//ns:TaxRegistrationNumber", nsManager);
            if (taxRegistrationNumber != null)
            {
                taxRegistrationNumber.InnerText = "12345678"; // Invalid VAT number (8 digits instead of 9)
                
                var invalidErrors = SchemaValidator.ValidatePortugueseTaxRules(xmlDoc);
                Assert.Contains(invalidErrors, error => error.Contains("VAT number") || error.Contains("9 digits"));
            }
        }

        [Fact]
        public void TestDocumentSequenceValidation()
        {
            // Test document sequence and numbering validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidateInvoiceNumbering(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: create gap in invoice sequence
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find the second invoice and modify its number to create a gap
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            Assert.NotNull(invoices);
            Assert.True(invoices.Count >= 2, "Need at least 2 invoices for this test");
            
            var secondInvoice = invoices[1];
            var invoiceNo = secondInvoice.SelectSingleNode(".//ns:InvoiceNo", nsManager);
            Assert.NotNull(invoiceNo);
            
            // Change FA A/17 to FA A/23 to create a gap
            var originalNumber = invoiceNo.InnerText;
            invoiceNo.InnerText = "FA A/23";
            
            var invalidErrors = SchemaValidator.ValidateInvoiceNumbering(xmlDoc);
            Assert.NotEmpty(invalidErrors);
            Assert.Contains("Missing invoice number", invalidErrors[0]);
            
            // Restore original value
            invoiceNo.InnerText = originalNumber;
        }

        [Fact]
        public void TestFiscalYearValidation()
        {
            // Test fiscal year and period validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidateDateRanges(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: modify invoice date to be outside fiscal year
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find an invoice and modify its date to be outside the fiscal year
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (invoices != null && invoices.Count > 0)
            {
                var invoice = invoices[0];
                var invoiceDate = invoice.SelectSingleNode("ns:InvoiceDate", nsManager);
                if (invoiceDate != null)
                {
                    invoiceDate.InnerText = "2023-12-31"; // Set to date outside fiscal year
                    
                    var invalidErrors = SchemaValidator.ValidateDateRanges(xmlDoc);
                    Assert.Contains(invalidErrors, error => error.Contains("fiscal year") || error.Contains("date range") || error.Contains("outside"));
                }
            }
        }

        [Fact]
        public void TestPortugueseCurrencyValidation()
        {
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidatePortugueseTaxRules(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: change CurrencyCode to USD
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            var currencyCode = xmlDoc.SelectSingleNode("//ns:CurrencyCode", nsManager);
            if (currencyCode != null)
            {
                currencyCode.InnerText = "USD"; // Invalid for Portuguese SAF-T
                
                var invalidErrors = SchemaValidator.ValidatePortugueseTaxRules(xmlDoc);
                Assert.Contains(invalidErrors, error => error.Contains("currency") || error.Contains("EUR") || error.Contains("Portuguese"));
            }
        }

        [Fact]
        public void TestTaxExemptionValidation()
        {
            // Test tax exemption reason and code validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidatePortugueseTaxRules(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: modify tax percentage to 0% but remove exemption reason
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find a line and modify its tax percentage to 0% but remove exemption reason
            var lines = xmlDoc.SelectNodes("//ns:Line", nsManager);
            if (lines != null && lines.Count > 0)
            {
                var line = lines[0];
                var tax = line.SelectSingleNode("ns:Tax", nsManager);
                if (tax != null)
                {
                    var taxPercentage = tax.SelectSingleNode("ns:TaxPercentage", nsManager);
                    if (taxPercentage != null)
                    {
                        taxPercentage.InnerText = "0.00"; // Set to 0% tax
                        
                        // Remove exemption reason and code
                        var exemptionReason = line.SelectSingleNode("ns:TaxExemptionReason", nsManager);
                        var exemptionCode = line.SelectSingleNode("ns:TaxExemptionCode", nsManager);
                        if (exemptionReason != null) exemptionReason.InnerText = "";
                        if (exemptionCode != null) exemptionCode.InnerText = "";
                        
                        var invalidErrors = SchemaValidator.ValidatePortugueseTaxRules(xmlDoc);
                        Assert.Contains(invalidErrors, error => error.Contains("tax exemption") || error.Contains("exemption reason"));
                    }
                }
            }
        }
    }
} 