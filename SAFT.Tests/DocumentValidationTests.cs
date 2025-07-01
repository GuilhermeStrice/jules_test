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
    public class DocumentValidationTests
    {
        [Fact]
        public void TestInvoiceNumberingSequence()
        {
            // Test invoice numbering sequence validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidateInvoiceNumbering(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: create duplicate invoice numbers
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find all InvoiceNo elements and make the second one duplicate the first
            var invoiceNos = xmlDoc.SelectNodes("//ns:InvoiceNo", nsManager);
            if (invoiceNos != null && invoiceNos.Count > 1)
            {
                var firstInvoiceNo = invoiceNos[0].InnerText;
                invoiceNos[1].InnerText = firstInvoiceNo; // Create duplicate
                
                var invalidErrors = SchemaValidator.ValidateInvoiceNumbering(xmlDoc);
                Assert.Contains(invalidErrors, error => error.Contains("Duplicate invoice number"));
            }
        }

        [Fact]
        public void TestPaymentTermsValidation()
        {
            // Test payment terms and due date validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidatePaymentTerms(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: modify due date to be before invoice date
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find an invoice and modify its due date to be before the invoice date
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (invoices != null && invoices.Count > 0)
            {
                var invoice = invoices[0];
                var invoiceDate = invoice.SelectSingleNode("ns:InvoiceDate", nsManager);
                var dueDate = invoice.SelectSingleNode("ns:DueDate", nsManager);
                
                if (invoiceDate != null && dueDate != null)
                {
                    // Set due date to be before invoice date
                    dueDate.InnerText = "2023-12-31"; // Before the invoice date
                    
                    var invalidErrors = SchemaValidator.ValidatePaymentTerms(xmlDoc);
                    Assert.Contains(invalidErrors, error => error.Contains("payment terms") || error.Contains("due date") || error.Contains("before"));
                }
            }
        }

        [Fact]
        public void TestDocumentStatusValidation()
        {
            // Test document status and workflow validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidateDocumentStatus(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: modify status date to be before invoice date
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find an invoice and modify its status date to be before the invoice date
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (invoices != null && invoices.Count > 0)
            {
                var invoice = invoices[0];
                var invoiceDate = invoice.SelectSingleNode("ns:InvoiceDate", nsManager);
                var statusDate = invoice.SelectSingleNode(".//ns:InvoiceStatusDate", nsManager);
                
                if (invoiceDate != null && statusDate != null)
                {
                    // Set status date to be before invoice date (invoice date is in 2018)
                    statusDate.InnerText = "2017-12-31T18:42:34"; // Before the invoice date
                    
                    var invalidErrors = SchemaValidator.ValidateDocumentStatus(xmlDoc);
                    Assert.Contains(invalidErrors, error => error.Contains("document status") || error.Contains("status date") || error.Contains("before"));
                }
            }
        }

        [Fact]
        public void TestCancellationRulesValidation()
        {
            // Test cancellation rules validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var validErrors = SchemaValidator.ValidateCancellationRules(xmlDoc);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: create a cancelled invoice without proper cancellation reason
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Find an invoice and modify its status to cancelled without reason
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (invoices != null && invoices.Count > 0)
            {
                var invoice = invoices[0];
                var invoiceStatus = invoice.SelectSingleNode("ns:InvoiceStatus", nsManager);
                var cancellationReason = invoice.SelectSingleNode("ns:CancellationReason", nsManager);
                
                if (invoiceStatus != null)
                {
                    // Set status to cancelled
                    invoiceStatus.InnerText = "A"; // Cancelled
                    
                    // Remove cancellation reason if it exists
                    if (cancellationReason != null)
                    {
                        cancellationReason.InnerText = ""; // Empty reason
                    }
                    
                    var invalidErrors = SchemaValidator.ValidateCancellationRules(xmlDoc);
                    Assert.Contains(invalidErrors, error => error.Contains("cancellation") || error.Contains("reason") || error.Contains("required"));
                }
            }
        }

        [Fact]
        public void TestSelfBillingIndicatorValidation()
        {
            // Test self-billing indicator validation
            var saftPath = Helpers.GetSaftPath();
            Assert.True(File.Exists(saftPath), "valid_saft.xml file not found in project root");
            var validSaftXml = File.ReadAllText(saftPath);
            
            // Load the XML document
            var xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.LoadXml(validSaftXml);
            
            // Test valid scenario: use as-is
            var nsManager = new System.Xml.XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            var validErrors = new List<string>();
            SchemaValidator.ValidateSelfBillingIndicator(xmlDoc, nsManager, validErrors);
            Assert.Empty(validErrors);
            
            // Test invalid scenario: create self-billing invoice without proper customer VAT number
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (invoices != null && invoices.Count > 0)
            {
                var invoice = invoices[0];
                var specialRegimes = invoice.SelectSingleNode("ns:SpecialRegimes", nsManager);
                var selfBillingIndicator = specialRegimes?.SelectSingleNode("ns:SelfBillingIndicator", nsManager);
                var customerID = invoice.SelectSingleNode(".//ns:CustomerID", nsManager);
                
                if (selfBillingIndicator != null)
                {
                    // Set to self-billing
                    selfBillingIndicator.InnerText = "1"; // Self-billing
                    
                    // Remove customer ID if it exists
                    if (customerID != null)
                    {
                        customerID.InnerText = ""; // Empty customer ID
                    }
                    
                    var invalidErrors = new List<string>();
                    SchemaValidator.ValidateSelfBillingIndicator(xmlDoc, nsManager, invalidErrors);
                    Assert.Contains(invalidErrors, error => error.Contains("self-billing") || error.Contains("customer") || error.Contains("required"));
                }
            }
        }
    }
} 