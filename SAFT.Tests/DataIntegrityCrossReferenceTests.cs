using Xunit;
using SAFT.Lib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAFT.Tests;

namespace SAFT.Tests
{
    public class DataIntegrityCrossReferenceTests
    {
        [Fact]
        public void TestCustomerReferenceIntegrity()
        {
            // Test customer references in invoices using the valid SAF-T file
            var validSaftPath = Path.Combine(Helpers.GetSchemaPath().Replace("schema1_04_fixed.xsd", ""), "valid_saft.xml");
            Assert.True(File.Exists(validSaftPath), "valid_saft.xml file not found in project root");
            
            var validSaftXml = File.ReadAllText(validSaftPath);
            var schemaPath = Helpers.GetSchemaPath();
            
            // Test valid SAF-T XML - should have no customer reference errors
            var validErrors = SchemaValidator.Validate(validSaftXml, schemaPath);
            Assert.DoesNotContain(validErrors, error => 
                error.Contains("customer reference") || 
                error.Contains("CustomerID") ||
                error.Contains("not found"));
            
            // Test invalid SAF-T XML (non-existent customer IDs)
            // Create a modified version of the valid XML with non-existent customer IDs
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find invoices and modify customer IDs to non-existent ones
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            Assert.True(invoices.Count > 0, "Need at least 1 invoice to test customer reference");
            
            var firstInvoice = invoices[0] as System.Xml.XmlElement;
            var customerId = firstInvoice.SelectSingleNode("ns:CustomerID", nsManager);
            
            // Change to a non-existent customer ID
            customerId.InnerText = "NONEXISTENT_CUSTOMER";
            
            var invalidXml = xmlDoc.OuterXml;
            var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
            Assert.Contains(invalidErrors, error => 
                error.Contains("customer reference") || 
                error.Contains("CustomerID") || 
                error.Contains("not found"));
        }

        [Fact]
        public void TestProductReferenceIntegrity()
        {
            // Test product references in invoice lines using the valid SAF-T file
            var validSaftPath = Path.Combine(Helpers.GetSchemaPath().Replace("schema1_04_fixed.xsd", ""), "valid_saft.xml");
            Assert.True(File.Exists(validSaftPath), "valid_saft.xml file not found in project root");
            
            var validSaftXml = File.ReadAllText(validSaftPath);
            var schemaPath = Helpers.GetSchemaPath();
            
            // Test valid SAF-T XML - should have no product reference errors
            var validErrors = SchemaValidator.Validate(validSaftXml, schemaPath);
            Assert.DoesNotContain(validErrors, error => 
                error.Contains("product reference") || 
                error.Contains("ProductCode") ||
                error.Contains("not found"));
            
            // Test invalid SAF-T XML (non-existent product codes)
            // Create a modified version of the valid XML with non-existent product codes
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find invoice lines and modify product codes to non-existent ones
            var lines = xmlDoc.SelectNodes("//ns:Line", nsManager);
            Assert.True(lines.Count > 0, "Need at least 1 invoice line to test product reference");
            
            var firstLine = lines[0] as System.Xml.XmlElement;
            var productCode = firstLine.SelectSingleNode("ns:ProductCode", nsManager);
            
            // Change to a non-existent product code
            productCode.InnerText = "NONEXISTENT_PRODUCT";
            
            var invalidXml = xmlDoc.OuterXml;
            var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
            Assert.Contains(invalidErrors, error => 
                error.Contains("product reference") || 
                error.Contains("ProductCode") || 
                error.Contains("not found"));
        }

        [Fact]
        public void TestTaxCodeReferenceIntegrity()
        {
            // Test tax code references in invoice lines
            var saftPath = Path.Combine(Helpers.GetSchemaPath().Replace("schema1_04_fixed.xsd", ""), "valid_saft.xml");
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidateCrossReferences(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: modify tax code to non-existent value
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find a tax element and modify its tax code to non-existent value
            var taxElements = xmlDoc.SelectNodes("//ns:Tax", nsManager);
            if (taxElements != null && taxElements.Count > 0)
            {
                var tax = taxElements[0];
                var taxCode = tax.SelectSingleNode("ns:TaxCode", nsManager);
                if (taxCode != null)
                {
                    taxCode.InnerText = "NONEXISTENT_TAX_CODE"; // Set to non-existent tax code
                    
                    var invalidErrors = SchemaValidator.ValidateCrossReferences(xmlDoc);
                    Assert.Contains(invalidErrors, error => 
                        error.Contains("tax code reference") || 
                        error.Contains("TaxCode") || 
                        error.Contains("not found"));
                }
            }
        }

        [Fact]
        public void TestAccountReferenceIntegrity()
        {
            // Test account references in general ledger entries
            var saftPath = Path.Combine(Helpers.GetSchemaPath().Replace("schema1_04_fixed.xsd", ""), "valid_saft.xml");
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidateCrossReferences(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: modify account ID to non-existent value
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find a line with AccountID and modify it to non-existent value
            var lines = xmlDoc.SelectNodes("//ns:Line[ns:AccountID]", nsManager);
            if (lines != null && lines.Count > 0)
            {
                var line = lines[0];
                var accountId = line.SelectSingleNode("ns:AccountID", nsManager);
                if (accountId != null)
                {
                    accountId.InnerText = "NONEXISTENT_ACCOUNT"; // Set to non-existent account ID
                    
                    var invalidErrors = SchemaValidator.ValidateCrossReferences(xmlDoc);
                    Assert.Contains(invalidErrors, error => 
                        error.Contains("account reference") || 
                        error.Contains("AccountID") || 
                        error.Contains("not found"));
                }
            }
        }

        [Fact]
        public void TestDocumentTotalsConsistency()
        {
            // Test document totals consistency using the valid SAF-T file
            var validSaftPath = Path.Combine(Helpers.GetSchemaPath().Replace("schema1_04_fixed.xsd", ""), "valid_saft.xml");
            Assert.True(File.Exists(validSaftPath), "valid_saft.xml file not found in project root");
            
            var validSaftXml = File.ReadAllText(validSaftPath);
            var schemaPath = Helpers.GetSchemaPath();
            
            // Test valid SAF-T XML - should have no document totals consistency errors
            var validErrors = SchemaValidator.Validate(validSaftXml, schemaPath);
            Assert.DoesNotContain(validErrors, error => 
                error.Contains("document totals") || 
                error.Contains("consistency") ||
                error.Contains("mismatch"));
            
            // Test invalid SAF-T XML (incorrect document totals)
            // Create a modified version of the valid XML with incorrect totals
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find the first invoice and modify its document totals
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            Assert.True(invoices.Count > 0, "Need at least 1 invoice to test document totals");
            
            var firstInvoice = invoices[0] as System.Xml.XmlElement;
            var documentTotals = firstInvoice.SelectSingleNode("ns:DocumentTotals", nsManager);
            
            if (documentTotals != null)
            {
                var netTotal = documentTotals.SelectSingleNode("ns:NetTotal", nsManager);
                if (netTotal != null)
                {
                    // Change the net total to an incorrect value
                    netTotal.InnerText = "999999.99";
                }
            }
            
            var invalidXml = xmlDoc.OuterXml;
            var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
            Assert.Contains(invalidErrors, error => 
                error.Contains("document totals") || 
                error.Contains("consistency") || 
                error.Contains("mismatch"));
        }

        [Fact]
        public void TestDateRangeConsistency()
        {
            // Test date range consistency across documents
            var saftPath = Path.Combine(Helpers.GetSchemaPath().Replace("schema1_04_fixed.xsd", ""), "valid_saft.xml");
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidateDateRanges(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: modify invoice date to be outside the fiscal year
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
                    Assert.Contains(invalidErrors, error => error.Contains("date range") || error.Contains("outside") || error.Contains("period"));
                }
            }
        }
    }
} 