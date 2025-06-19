using System;
using System.Collections.Generic;
using SAFT.Lib.Documents;

namespace SAFT.Validation
{
    public static class InvoiceValidator
    {
        private static readonly HashSet<string> AllowedTypes = new HashSet<string> { "FT", "FS", "FR", "ND", "NC", "VD", "TV", "TD", "AA", "DA", "RP", "RE", "CS", "LD", "RA" };

        public static List<SAFTValidationResult> Validate(Invoice invoice)
        {
            var results = new List<SAFTValidationResult>();
            if (invoice == null)
            {
                results.Add(new SAFTValidationResult { Field = "Invoice", Message = "Invoice is required.", Section = "Invoice" });
                return results;
            }
            if (string.IsNullOrWhiteSpace(invoice.InvoiceNo) || invoice.InvoiceNo.Length > 60)
            {
                results.Add(new SAFTValidationResult { Field = nameof(invoice.InvoiceNo), Message = "InvoiceNo is required and must be 1-60 characters.", Section = "Invoice" });
            }
            if (invoice.InvoiceDate == default)
            {
                results.Add(new SAFTValidationResult { Field = nameof(invoice.InvoiceDate), Message = "InvoiceDate is required.", Section = "Invoice" });
            }
            if (string.IsNullOrWhiteSpace(invoice.InvoiceType) || !AllowedTypes.Contains(invoice.InvoiceType))
            {
                results.Add(new SAFTValidationResult { Field = nameof(invoice.InvoiceType), Message = "InvoiceType must be a valid type.", Section = "Invoice" });
            }
            if (string.IsNullOrWhiteSpace(invoice.CustomerID))
            {
                results.Add(new SAFTValidationResult { Field = nameof(invoice.CustomerID), Message = "CustomerID is required.", Section = "Invoice" });
            }
            if (invoice.Lines == null || invoice.Lines.Count == 0)
            {
                results.Add(new SAFTValidationResult { Field = nameof(invoice.Lines), Message = "At least one invoice line is required.", Section = "Invoice" });
            }
            else
            {
                for (int i = 0; i < invoice.Lines.Count; i++)
                {
                    var line = invoice.Lines[i];
                    if (string.IsNullOrWhiteSpace(line.ProductCode))
                    {
                        results.Add(new SAFTValidationResult { Field = $"Lines[{i}].ProductCode", Message = "ProductCode is required.", Section = "Invoice" });
                    }
                    if (line.Quantity <= 0)
                    {
                        results.Add(new SAFTValidationResult { Field = $"Lines[{i}].Quantity", Message = "Quantity must be greater than 0.", Section = "Invoice" });
                    }
                    if (line.UnitPrice < 0)
                    {
                        results.Add(new SAFTValidationResult { Field = $"Lines[{i}].UnitPrice", Message = "UnitPrice must be >= 0.", Section = "Invoice" });
                    }
                    if (line.Tax == null)
                    {
                        results.Add(new SAFTValidationResult { Field = $"Lines[{i}].Tax", Message = "Tax is required.", Section = "Invoice" });
                    }
                }
            }
            if (invoice.DocumentTotals == null)
            {
                results.Add(new SAFTValidationResult { Field = nameof(invoice.DocumentTotals), Message = "DocumentTotals is required.", Section = "Invoice" });
            }
            // Add more invoice-specific rules as needed
            return results;
        }
    }
} 