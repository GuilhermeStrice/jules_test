using System.Collections.Generic;
using SAFT.Lib;
using SAFT.Lib.Files;

namespace SAFT.Validation
{
    public static class TaxTableEntryValidator
    {
        private static readonly HashSet<string> AllowedTypes = new HashSet<string> { "IVA", "IS", "NS" };

        public static List<SAFTValidationResult> Validate(TaxTableEntry entry)
        {
            var results = new List<SAFTValidationResult>();
            if (entry == null)
            {
                results.Add(new SAFTValidationResult { Field = "TaxTableEntry", Message = "TaxTableEntry is required.", Section = "TaxTableEntry" });
                return results;
            }
            if (string.IsNullOrWhiteSpace(entry.TaxType) || !AllowedTypes.Contains(entry.TaxType))
            {
                results.Add(new SAFTValidationResult { Field = nameof(entry.TaxType), Message = "TaxType must be one of IVA, IS, NS.", Section = "TaxTableEntry" });
            }
            if (string.IsNullOrWhiteSpace(entry.TaxCountryRegion))
            {
                results.Add(new SAFTValidationResult { Field = nameof(entry.TaxCountryRegion), Message = "TaxCountryRegion is required.", Section = "TaxTableEntry" });
            }
            if (string.IsNullOrWhiteSpace(entry.TaxCode) || entry.TaxCode.Length > 10)
            {
                results.Add(new SAFTValidationResult { Field = nameof(entry.TaxCode), Message = "TaxCode is required and must be 1-10 characters.", Section = "TaxTableEntry" });
            }
            if (string.IsNullOrWhiteSpace(entry.Description) || entry.Description.Length > 255)
            {
                results.Add(new SAFTValidationResult { Field = nameof(entry.Description), Message = "Description is required and must be 1-255 characters.", Section = "TaxTableEntry" });
            }
            if (!entry.TaxPercentage.HasValue && !entry.TaxAmount.HasValue)
            {
                results.Add(new SAFTValidationResult { Field = "TaxPercentage/TaxAmount", Message = "At least one of TaxPercentage or TaxAmount is required.", Section = "TaxTableEntry" });
            }
            if (entry.TaxPercentage.HasValue && entry.TaxPercentage < 0)
            {
                results.Add(new SAFTValidationResult { Field = nameof(entry.TaxPercentage), Message = "TaxPercentage must be >= 0.", Section = "TaxTableEntry" });
            }
            if (entry.TaxAmount.HasValue && entry.TaxAmount < 0)
            {
                results.Add(new SAFTValidationResult { Field = nameof(entry.TaxAmount), Message = "TaxAmount must be >= 0.", Section = "TaxTableEntry" });
            }
            // Add more tax table entry-specific rules as needed
            return results;
        }
    }
} 