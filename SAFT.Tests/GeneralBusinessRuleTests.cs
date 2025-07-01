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
    public class GeneralBusinessRuleTests
    {
        [Fact]
        public void TestBusinessRuleCompliance()
        {
            // Test general business rule compliance validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidateBusinessRuleCompliance(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: create a business rule violation
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find an invoice and modify it to violate business rules
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (invoices != null && invoices.Count > 0)
            {
                var invoice = invoices[0];
                var invoiceDate = invoice.SelectSingleNode("ns:InvoiceDate", nsManager);
                var dueDate = invoice.SelectSingleNode("ns:DueDate", nsManager);
                
                if (invoiceDate != null && dueDate != null)
                {
                    // Set due date to be before invoice date (violates business rule)
                    dueDate.InnerText = "2017-12-31"; // Before the invoice date
                    
                    var invalidErrors = SchemaValidator.ValidateBusinessRuleCompliance(xmlDoc);
                    Assert.Contains(invalidErrors, error => 
                        error.Contains("business rule") || 
                        error.Contains("due date") || 
                        error.Contains("invoice date") ||
                        error.Contains("before"));
                }
            }
        }

        [Fact]
        public void TestStringConstraintsValidation()
        {
            // Test string constraints validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidateStringConstraints(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: modify string to exceed length constraint
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find CompanyName and modify it to exceed length constraint
            var companyName = xmlDoc.SelectSingleNode("//ns:CompanyName", nsManager);
            if (companyName != null)
            {
                // Create a string that exceeds the maximum length (typically 100 characters)
                var longName = new string('A', 150); // 150 characters
                companyName.InnerText = longName;
                
                var invalidErrors = SchemaValidator.ValidateStringConstraints(xmlDoc);
                Assert.Contains(invalidErrors, error => 
                    error.Contains("CompanyName") || 
                    error.Contains("length") || 
                    error.Contains("constraint") ||
                    error.Contains("maximum"));
            }
        }

        [Fact]
        public void TestStringFormatConstraints()
        {
            // Test string format constraints validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidateStringConstraints(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: modify email to invalid format
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find an email field and modify it to invalid format
            var emails = xmlDoc.SelectNodes("//ns:Email", nsManager);
            if (emails != null && emails.Count > 0)
            {
                var email = emails[0];
                email.InnerText = "invalid-email-format"; // Invalid email format
                
                var invalidErrors = SchemaValidator.ValidateStringConstraints(xmlDoc);
                Assert.Contains(invalidErrors, error => 
                    error.Contains("Email") || 
                    error.Contains("format") || 
                    error.Contains("constraint"));
            }
        }

        [Fact]
        public void TestStringLengthConstraints()
        {
            // Test string length constraints validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidateStringConstraints(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: modify ProductCode to exceed length constraint
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find a ProductCode and modify it to exceed the 30 character limit
            var productCodes = xmlDoc.SelectNodes("//ns:ProductCode", nsManager);
            if (productCodes != null && productCodes.Count > 0)
            {
                var productCode = productCodes[0];
                var longCode = new string('A', 35); // 35 characters, exceeds 30 limit
                productCode.InnerText = longCode;
                
                var invalidErrors = SchemaValidator.ValidateStringConstraints(xmlDoc);
                Assert.Contains(invalidErrors, error => 
                    error.Contains("ProductCode") || 
                    error.Contains("length") || 
                    error.Contains("30") ||
                    error.Contains("exceed"));
            }
        }

        [Fact]
        public void TestRequiredFieldsValidation()
        {
            // Test required fields validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidateRequiredFields(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: remove a required field
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find a required field and remove it
            var productCode = xmlDoc.SelectSingleNode("//ns:ProductCode", nsManager);
            if (productCode != null)
            {
                var parent = productCode.ParentNode;
                parent.RemoveChild(productCode);
                
                var invalidErrors = SchemaValidator.ValidateRequiredFields(xmlDoc);
                Assert.Contains(invalidErrors, error => 
                    error.Contains("ProductCode") || 
                    error.Contains("required") || 
                    error.Contains("missing"));
            }
        }
    }
} 