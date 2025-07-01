using Xunit;
using SAFT.Lib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System;
using SAFT.Tests;
using SAFT.Lib.Validation;

namespace SAFT.Tests
{
    public class LineItemValidationTests
    {
        [Fact]
        public void TestQuantityAndUnitPriceValidation()
        {
            // Test quantity and unit price validation in invoice lines
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidateQuantityAndUnitPrice(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: modify quantity to be negative
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find invoice lines and modify quantity to be negative
            var invoiceLines = xmlDoc.SelectNodes("//ns:InvoiceLine", nsManager);
            if (invoiceLines != null && invoiceLines.Count > 0)
            {
                var firstLine = invoiceLines[0];
                var quantity = firstLine.SelectSingleNode("ns:Quantity", nsManager);
                
                if (quantity != null)
                {
                    quantity.InnerText = "-1.00"; // Negative quantity
                    
                    var invalidErrors = SchemaValidator.ValidateQuantityAndUnitPrice(xmlDoc);
                    Assert.Contains(invalidErrors, error => error.Contains("quantity") || error.Contains("negative") || error.Contains("invalid"));
                }
            }
        }

        [Fact]
        public void TestSpecialRegimesFieldValidation()
        {
            // Test special regimes field validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidateSpecialRegimesField(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: modify special regime to invalid value
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find special regime fields and modify to invalid value
            var specialRegimes = xmlDoc.SelectNodes("//ns:SpecialRegimes", nsManager);
            if (specialRegimes != null && specialRegimes.Count > 0)
            {
                var firstSpecialRegime = specialRegimes[0];
                var selfBilling = firstSpecialRegime.SelectSingleNode("ns:SelfBillingIndicator", nsManager);
                
                if (selfBilling != null)
                {
                    selfBilling.InnerText = "INVALID"; // Invalid value
                    
                    var invalidErrors = SchemaValidator.ValidateSpecialRegimesField(xmlDoc);
                    Assert.Contains(invalidErrors, error => error.Contains("SelfBillingIndicator must be 0 or 1"));
                }
            }
        }
    }
} 