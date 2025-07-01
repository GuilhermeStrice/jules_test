using Xunit;
using SAFT.Lib;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System;
using SAFT.Tests;

namespace SAFT.Tests
{
    public class FinancialValidationTests
    {
        [Fact]
        public void TestCreditDebitBalanceValidation()
        {
            // Test credit/debit balance validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidateCreditDebitBalance(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: create unbalanced transaction
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find a transaction and modify one of the credit/debit amounts to create imbalance
            var creditLines = xmlDoc.SelectNodes("//ns:CreditLine", nsManager);
            if (creditLines != null && creditLines.Count > 0)
            {
                // Modify the credit amount of the first credit line to create imbalance
                var firstCreditLine = creditLines[0];
                var creditAmount = firstCreditLine.SelectSingleNode("ns:CreditAmount", nsManager);
                if (creditAmount != null)
                {
                    var originalAmount = decimal.Parse(creditAmount.InnerText);
                    creditAmount.InnerText = (originalAmount - 100.00m).ToString("F2"); // Reduce by 100 to create imbalance
                    
                    var invalidErrors = SchemaValidator.ValidateCreditDebitBalance(xmlDoc);
                    Assert.Contains(invalidErrors, error => error.Contains("credit/debit balance") || error.Contains("balanced") || error.Contains("mismatch"));
                }
            }
        }

        [Fact]
        public void TestTaxCalculationAccuracy()
        {
            // Test tax calculation accuracy validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidateTaxCalculationAccuracy(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: modify tax amount to be incorrect
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find a tax line and modify its amount to be incorrect
            var taxLines = xmlDoc.SelectNodes("//ns:Tax", nsManager);
            if (taxLines != null && taxLines.Count > 0)
            {
                var firstTaxLine = taxLines[0];
                var taxAmount = firstTaxLine.SelectSingleNode("ns:TaxAmount", nsManager);
                var taxBase = firstTaxLine.SelectSingleNode("ns:TaxBase", nsManager);
                var taxPercentage = firstTaxLine.SelectSingleNode("ns:TaxPercentage", nsManager);
                
                if (taxAmount != null)
                {
                    var originalTaxAmount = decimal.Parse(taxAmount.InnerText);
                    taxAmount.InnerText = (originalTaxAmount + 10.00m).ToString("F2"); // Add 10 to make it incorrect
                    
                    var invalidErrors = SchemaValidator.ValidateTaxCalculationAccuracy(xmlDoc);
                    Assert.Contains(invalidErrors, error => error.Contains("tax calculation") || error.Contains("accuracy") || error.Contains("mismatch"));
                }
            }
        }
    }
} 