using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace SAFT.Lib.Validation
{
    /// <summary>
    /// Provides utility methods for validating SAF-T objects using data annotations.
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates an object using data annotations and returns validation results.
        /// </summary>
        /// <param name="obj">The object to validate.</param>
        /// <returns>A list of validation error messages. Empty if validation passes.</returns>
        public static List<string> ValidateObject(object obj)
        {
            var errors = new List<string>();
            
            if (obj == null)
            {
                errors.Add("Object cannot be null");
                return errors;
            }

            var validationContext = new ValidationContext(obj);
            var validationResults = new List<ValidationResult>();
            
            if (!Validator.TryValidateObject(obj, validationContext, validationResults, true))
            {
                errors.AddRange(validationResults.Select(vr => vr.ErrorMessage));
            }

            return errors;
        }

        /// <summary>
        /// Validates a specific property of an object using data annotations.
        /// </summary>
        /// <param name="obj">The object containing the property.</param>
        /// <param name="propertyName">The name of the property to validate.</param>
        /// <returns>A list of validation error messages. Empty if validation passes.</returns>
        public static List<string> ValidateProperty(object obj, string propertyName)
        {
            var errors = new List<string>();
            
            if (obj == null)
            {
                errors.Add("Object cannot be null");
                return errors;
            }

            var property = obj.GetType().GetProperty(propertyName);
            if (property == null)
            {
                errors.Add($"Property '{propertyName}' not found on object");
                return errors;
            }

            var value = property.GetValue(obj);
            var validationContext = new ValidationContext(obj) { MemberName = propertyName };
            var validationResults = new List<ValidationResult>();
            
            if (!Validator.TryValidateProperty(value, validationContext, validationResults))
            {
                errors.AddRange(validationResults.Select(vr => vr.ErrorMessage));
            }

            return errors;
        }

        /// <summary>
        /// Validates an AuditFile object and all its nested objects.
        /// </summary>
        /// <param name="auditFile">The AuditFile object to validate.</param>
        /// <returns>A list of validation error messages. Empty if validation passes.</returns>
        public static List<string> ValidateAuditFile(AuditFile auditFile)
        {
            var errors = new List<string>();

            // Validate the main AuditFile object
            errors.AddRange(ValidateObject(auditFile));

            // Validate Header
            if (auditFile.Header != null)
            {
                var headerErrors = ValidateObject(auditFile.Header);
                errors.AddRange(headerErrors.Select(e => $"Header: {e}"));
            }

            // Validate MasterFiles
            if (auditFile.MasterFiles != null)
            {
                // Validate GeneralLedgerAccounts
                if (auditFile.MasterFiles.GeneralLedgerAccounts != null)
                {
                    var glErrors = ValidateObject(auditFile.MasterFiles.GeneralLedgerAccounts);
                    errors.AddRange(glErrors.Select(e => $"MasterFiles.GeneralLedgerAccounts: {e}"));

                    // Validate individual accounts
                    foreach (var account in auditFile.MasterFiles.GeneralLedgerAccounts.Accounts)
                    {
                        var accountErrors = ValidateObject(account);
                        errors.AddRange(accountErrors.Select(e => $"Account {account.AccountID}: {e}"));
                    }
                }

                // Validate Customers
                foreach (var customer in auditFile.MasterFiles.Customers)
                {
                    var customerErrors = ValidateObject(customer);
                    errors.AddRange(customerErrors.Select(e => $"Customer {customer.CustomerID}: {e}"));
                }

                // Validate Suppliers
                foreach (var supplier in auditFile.MasterFiles.Suppliers)
                {
                    var supplierErrors = ValidateObject(supplier);
                    errors.AddRange(supplierErrors.Select(e => $"Supplier {supplier.SupplierID}: {e}"));
                }

                // Validate Products
                foreach (var product in auditFile.MasterFiles.Products)
                {
                    var productErrors = ValidateObject(product);
                    errors.AddRange(productErrors.Select(e => $"Product {product.ProductCode}: {e}"));
                }

                // Validate TaxTable
                if (auditFile.MasterFiles.TaxTable != null)
                {
                    var taxTableErrors = ValidateObject(auditFile.MasterFiles.TaxTable);
                    errors.AddRange(taxTableErrors.Select(e => $"MasterFiles.TaxTable: {e}"));
                }
            }

            // Validate GeneralLedgerEntries
            if (auditFile.GeneralLedgerEntries != null)
            {
                var glEntriesErrors = ValidateObject(auditFile.GeneralLedgerEntries);
                errors.AddRange(glEntriesErrors.Select(e => $"GeneralLedgerEntries: {e}"));
            }

            // Validate SourceDocuments
            if (auditFile.SourceDocuments != null)
            {
                var sourceDocsErrors = ValidateObject(auditFile.SourceDocuments);
                errors.AddRange(sourceDocsErrors.Select(e => $"SourceDocuments: {e}"));

                // Validate SalesInvoices
                if (auditFile.SourceDocuments.SalesInvoices != null)
                {
                    var salesInvoicesErrors = ValidateObject(auditFile.SourceDocuments.SalesInvoices);
                    errors.AddRange(salesInvoicesErrors.Select(e => $"SourceDocuments.SalesInvoices: {e}"));

                    // Validate individual invoices
                    foreach (var invoice in auditFile.SourceDocuments.SalesInvoices.Invoices)
                    {
                        var invoiceErrors = ValidateObject(invoice);
                        errors.AddRange(invoiceErrors.Select(e => $"Invoice {invoice.InvoiceNo}: {e}"));

                        // Validate invoice lines
                        foreach (var line in invoice.Lines)
                        {
                            var lineErrors = ValidateObject(line);
                            errors.AddRange(lineErrors.Select(e => $"Invoice {invoice.InvoiceNo}, Line {line.LineNumber}: {e}"));
                        }
                    }
                }
            }

            return errors;
        }

        /// <summary>
        /// Validates a Portuguese VAT number format.
        /// </summary>
        /// <param name="vatNumber">The VAT number to validate.</param>
        /// <returns>True if the VAT number is valid, false otherwise.</returns>
        public static bool IsValidPortugueseVatNumber(int vatNumber)
        {
            return vatNumber >= 100000000 && vatNumber <= 999999999;
        }

        /// <summary>
        /// Validates a date string format (YYYY-MM-DD).
        /// </summary>
        /// <param name="dateString">The date string to validate.</param>
        /// <returns>True if the date string is valid, false otherwise.</returns>
        public static bool IsValidDateString(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return false;

            return System.Text.RegularExpressions.Regex.IsMatch(dateString, @"^\d{4}-\d{2}-\d{2}$") &&
                   DateTime.TryParse(dateString, out _);
        }

        /// <summary>
        /// Validates an email address format.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email address is valid, false otherwise.</returns>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Validates a URL format.
        /// </summary>
        /// <param name="url">The URL to validate.</param>
        /// <returns>True if the URL is valid, false otherwise.</returns>
        public static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
} 