using System;
using System.Collections.Generic;
using System.Xml;

namespace SAFT.Lib.Validation
{
    public static partial class SchemaValidator
    {
        /// <summary>
        /// Validates that all required fields are present in the XML document according to the SAF-T schema.
        /// </summary>
        internal static List<string> ValidateRequiredFields(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");

            try
            {
                // Validate Header required fields
                ValidateHeaderRequiredFields(xmlDoc, nsManager, errors);
                // Validate MasterFiles required fields
                ValidateMasterFilesRequiredFields(xmlDoc, nsManager, errors);
                // Validate GeneralLedgerEntries required fields
                ValidateGeneralLedgerRequiredFields(xmlDoc, nsManager, errors);
                // Validate SourceDocuments required fields
                ValidateSourceDocumentsRequiredFields(xmlDoc, nsManager, errors);
            }
            catch (Exception ex)
            {
                errors.Add($"Required field validation error: {ex.Message}");
            }
            return errors;
        }

        private static void ValidateHeaderRequiredFields(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // Required Header fields
            var requiredHeaderFields = new[]
            {
                "AuditFileVersion",
                "CompanyID",
                "TaxRegistrationNumber",
                "TaxAccountingBasis",
                "CompanyName",
                "FiscalYear",
                "StartDate",
                "EndDate",
                "CurrencyCode",
                "DateCreated",
                "TaxEntity",
                "ProductCompanyTaxID",
                "SoftwareCertificateNumber",
                "ProductID",
                "ProductVersion"
            };
            var header = xmlDoc.SelectSingleNode("//ns:Header", nsManager);
            if (header != null)
            {
                foreach (var field in requiredHeaderFields)
                {
                    var node = header.SelectSingleNode($"ns:{field}", nsManager);
                    if (node == null || string.IsNullOrWhiteSpace(node.InnerText))
                    {
                        errors.Add($"Required Header field '{field}' is missing or empty");
                    }
                }
            }
            // Required CompanyAddress fields
            var companyAddress = xmlDoc.SelectSingleNode("//ns:CompanyAddress", nsManager);
            if (companyAddress != null)
            {
                var requiredAddressFields = new[] { "Country" };
                foreach (var field in requiredAddressFields)
                {
                    var node = companyAddress.SelectSingleNode($"ns:{field}", nsManager);
                    if (node == null || string.IsNullOrWhiteSpace(node.InnerText))
                    {
                        errors.Add($"Required CompanyAddress field '{field}' is missing or empty");
                    }
                }
            }
        }

        private static void ValidateMasterFilesRequiredFields(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // Validate GeneralLedgerAccounts required fields
            var accounts = xmlDoc.SelectNodes("//ns:Account", nsManager);
            if (accounts != null)
            {
                foreach (XmlNode account in accounts)
                {
                    var accountID = account.SelectSingleNode("ns:AccountID", nsManager);
                    var accountDescription = account.SelectSingleNode("ns:AccountDescription", nsManager);
                    var groupCategory = account.SelectSingleNode("ns:GroupingCategory", nsManager);
                    if (accountID == null || string.IsNullOrWhiteSpace(accountID.InnerText))
                        errors.Add("AccountID is required for all Account elements");
                    if (accountDescription == null || string.IsNullOrWhiteSpace(accountDescription.InnerText))
                        errors.Add("AccountDescription is required for all Account elements");
                    if (groupCategory == null || string.IsNullOrWhiteSpace(groupCategory.InnerText))
                        errors.Add("GroupingCategory is required for all Account elements");
                }
            }
            // Validate Products required fields
            var products = xmlDoc.SelectNodes("//ns:Product", nsManager);
            if (products != null)
            {
                foreach (XmlNode product in products)
                {
                    var productCode = product.SelectSingleNode("ns:ProductCode", nsManager);
                    var productDescription = product.SelectSingleNode("ns:ProductDescription", nsManager);
                    var productNumberCode = product.SelectSingleNode("ns:ProductNumberCode", nsManager);
                    if (productCode == null || string.IsNullOrWhiteSpace(productCode.InnerText))
                        errors.Add("ProductCode is required for all Product elements");
                    if (productDescription == null || string.IsNullOrWhiteSpace(productDescription.InnerText))
                        errors.Add("ProductDescription is required for all Product elements");
                    if (productNumberCode == null || string.IsNullOrWhiteSpace(productNumberCode.InnerText))
                        errors.Add("ProductNumberCode is required for all Product elements");
                }
            }
        }

        private static void ValidateGeneralLedgerRequiredFields(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // Validate Journal required fields
            var journals = xmlDoc.SelectNodes("//ns:Journal", nsManager);
            if (journals != null)
            {
                foreach (XmlNode journal in journals)
                {
                    var journalID = journal.SelectSingleNode("ns:JournalID", nsManager);
                    if (journalID == null || string.IsNullOrWhiteSpace(journalID.InnerText))
                        errors.Add("JournalID is required for all Journal elements");
                }
            }
            // Validate CreditLine required fields
            var creditLines = xmlDoc.SelectNodes("//ns:CreditLine", nsManager);
            if (creditLines != null)
            {
                foreach (XmlNode creditLine in creditLines)
                {
                    var recordID = creditLine.SelectSingleNode("ns:RecordID", nsManager);
                    var accountID = creditLine.SelectSingleNode("ns:AccountID", nsManager);
                    var systemEntryDate = creditLine.SelectSingleNode("ns:SystemEntryDate", nsManager);
                    var description = creditLine.SelectSingleNode("ns:Description", nsManager);
                    var creditAmount = creditLine.SelectSingleNode("ns:CreditAmount", nsManager);
                    if (recordID == null || string.IsNullOrWhiteSpace(recordID.InnerText))
                        errors.Add("RecordID is required for all CreditLine elements");
                    if (accountID == null || string.IsNullOrWhiteSpace(accountID.InnerText))
                        errors.Add("AccountID is required for all CreditLine elements");
                    if (systemEntryDate == null || string.IsNullOrWhiteSpace(systemEntryDate.InnerText))
                        errors.Add("SystemEntryDate is required for all CreditLine elements");
                    if (description == null || string.IsNullOrWhiteSpace(description.InnerText))
                        errors.Add("Description is required for all CreditLine elements");
                    if (creditAmount == null || string.IsNullOrWhiteSpace(creditAmount.InnerText))
                        errors.Add("CreditAmount is required for all CreditLine elements");
                }
            }
        }

        private static void ValidateSourceDocumentsRequiredFields(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // Validate Invoice required fields
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (invoices != null)
            {
                foreach (XmlNode invoice in invoices)
                {
                    var invoiceNo = invoice.SelectSingleNode("ns:InvoiceNo", nsManager);
                    var atcud = invoice.SelectSingleNode("ns:ATCUD", nsManager);
                    var documentStatus = invoice.SelectSingleNode("ns:DocumentStatus", nsManager);
                    var invoiceDate = invoice.SelectSingleNode("ns:InvoiceDate", nsManager);
                    var customerID = invoice.SelectSingleNode("ns:CustomerID", nsManager);
                    var lines = invoice.SelectNodes("ns:Line", nsManager);
                    if (invoiceNo == null || string.IsNullOrWhiteSpace(invoiceNo.InnerText))
                        errors.Add("InvoiceNo is required for all Invoice elements");
                    if (atcud == null || string.IsNullOrWhiteSpace(atcud.InnerText))
                        errors.Add("ATCUD is required for all Invoice elements");
                    if (documentStatus == null)
                        errors.Add("DocumentStatus is required for all Invoice elements");
                    if (invoiceDate == null || string.IsNullOrWhiteSpace(invoiceDate.InnerText))
                        errors.Add("InvoiceDate is required for all Invoice elements");
                    if (customerID == null || string.IsNullOrWhiteSpace(customerID.InnerText))
                        errors.Add("CustomerID is required for all Invoice elements");
                    if (lines != null)
                    {
                        foreach (XmlNode line in lines)
                        {
                            var lineNumber = line.SelectSingleNode("ns:LineNumber", nsManager);
                            var productCode = line.SelectSingleNode("ns:ProductCode", nsManager);
                            var productDescription = line.SelectSingleNode("ns:ProductDescription", nsManager);
                            var quantity = line.SelectSingleNode("ns:Quantity", nsManager);
                            var unitOfMeasure = line.SelectSingleNode("ns:UnitOfMeasure", nsManager);
                            var tax = line.SelectSingleNode("ns:Tax", nsManager);
                            if (lineNumber == null || string.IsNullOrWhiteSpace(lineNumber.InnerText))
                                errors.Add("LineNumber is required for all Invoice Line elements");
                            if (productCode == null || string.IsNullOrWhiteSpace(productCode.InnerText))
                                errors.Add("ProductCode is required for all Invoice Line elements");
                            if (productDescription == null || string.IsNullOrWhiteSpace(productDescription.InnerText))
                                errors.Add("ProductDescription is required for all Invoice Line elements");
                            if (quantity == null || string.IsNullOrWhiteSpace(quantity.InnerText))
                                errors.Add("Quantity is required for all Invoice Line elements");
                            if (unitOfMeasure == null || string.IsNullOrWhiteSpace(unitOfMeasure.InnerText))
                                errors.Add("UnitOfMeasure is required for all Invoice Line elements");
                            if (tax == null)
                                errors.Add("Tax is required for all Invoice Line elements");
                            // At least one of DebitAmount or CreditAmount must be present
                            var debitAmount = line.SelectSingleNode("ns:DebitAmount", nsManager);
                            var creditAmount = line.SelectSingleNode("ns:CreditAmount", nsManager);
                            if ((debitAmount == null || string.IsNullOrWhiteSpace(debitAmount.InnerText)) &&
                                (creditAmount == null || string.IsNullOrWhiteSpace(creditAmount.InnerText)))
                            {
                                errors.Add("Either DebitAmount or CreditAmount must be present for all Invoice Line elements");
                            }
                        }
                    }
                }
            }
        }
    }
}
