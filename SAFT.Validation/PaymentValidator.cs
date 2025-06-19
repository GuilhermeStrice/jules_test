using System;
using System.Collections.Generic;
using SAFT.Lib.Documents;

namespace SAFT.Validation
{
    public static class PaymentValidator
    {
        private static readonly HashSet<string> AllowedTypes = new HashSet<string> { "RC", "RG" };

        public static List<SAFTValidationResult> Validate(Payment payment)
        {
            var results = new List<SAFTValidationResult>();
            if (payment == null)
            {
                results.Add(new SAFTValidationResult { Field = "Payment", Message = "Payment is required.", Section = "Payment" });
                return results;
            }
            if (string.IsNullOrWhiteSpace(payment.PaymentRefNo) || payment.PaymentRefNo.Length > 60)
            {
                results.Add(new SAFTValidationResult { Field = nameof(payment.PaymentRefNo), Message = "PaymentRefNo is required and must be 1-60 characters.", Section = "Payment" });
            }
            if (payment.TransactionDate == default)
            {
                results.Add(new SAFTValidationResult { Field = nameof(payment.TransactionDate), Message = "TransactionDate is required.", Section = "Payment" });
            }
            if (string.IsNullOrWhiteSpace(payment.PaymentType) || !AllowedTypes.Contains(payment.PaymentType))
            {
                results.Add(new SAFTValidationResult { Field = nameof(payment.PaymentType), Message = "PaymentType must be RC or RG.", Section = "Payment" });
            }
            if (string.IsNullOrWhiteSpace(payment.CustomerID))
            {
                results.Add(new SAFTValidationResult { Field = nameof(payment.CustomerID), Message = "CustomerID is required.", Section = "Payment" });
            }
            if (payment.Lines == null || payment.Lines.Count == 0)
            {
                results.Add(new SAFTValidationResult { Field = nameof(payment.Lines), Message = "At least one payment line is required.", Section = "Payment" });
            }
            else
            {
                for (int i = 0; i < payment.Lines.Count; i++)
                {
                    var line = payment.Lines[i];
                    if ((line.DebitAmount ?? 0) <= 0 && (line.CreditAmount ?? 0) <= 0)
                    {
                        results.Add(new SAFTValidationResult { Field = $"Lines[{i}].DebitAmount/CreditAmount", Message = "At least one of DebitAmount or CreditAmount must be > 0.", Section = "Payment" });
                    }
                    if (line.Tax != null && string.IsNullOrWhiteSpace(line.Tax.TaxType))
                    {
                        results.Add(new SAFTValidationResult { Field = $"Lines[{i}].Tax.TaxType", Message = "TaxType is required if Tax is present.", Section = "Payment" });
                    }
                }
            }
            if (payment.DocumentTotals == null)
            {
                results.Add(new SAFTValidationResult { Field = nameof(payment.DocumentTotals), Message = "DocumentTotals is required.", Section = "Payment" });
            }
            // Add more payment-specific rules as needed
            return results;
        }
    }
} 