using System.Collections.Generic;
using SAFT.Lib;
using SAFT.Lib.Files;

namespace SAFT.Validation
{
    public static class AccountValidator
    {
        private static readonly HashSet<string> AllowedCategories = new HashSet<string> { "GR", "GA", "GM", "AR", "AA", "AM" };

        public static List<SAFTValidationResult> Validate(Account account)
        {
            var results = new List<SAFTValidationResult>();
            if (account == null)
            {
                results.Add(new SAFTValidationResult { Field = "Account", Message = "Account is required.", Section = "Account" });
                return results;
            }
            if (string.IsNullOrWhiteSpace(account.AccountID) || account.AccountID.Length > 30)
            {
                results.Add(new SAFTValidationResult { Field = nameof(account.AccountID), Message = "AccountID is required and must be 1-30 characters.", Section = "Account" });
            }
            if (string.IsNullOrWhiteSpace(account.AccountDescription))
            {
                results.Add(new SAFTValidationResult { Field = nameof(account.AccountDescription), Message = "AccountDescription is required.", Section = "Account" });
            }
            if (string.IsNullOrWhiteSpace(account.GroupingCategory) || !AllowedCategories.Contains(account.GroupingCategory))
            {
                results.Add(new SAFTValidationResult { Field = nameof(account.GroupingCategory), Message = "GroupingCategory must be one of GR, GA, GM, AR, AA, AM.", Section = "Account" });
            }
            if (!string.IsNullOrWhiteSpace(account.GroupingCode) && (account.GroupingCode.Length < 1 || account.GroupingCode.Length > 30))
            {
                results.Add(new SAFTValidationResult { Field = nameof(account.GroupingCode), Message = "GroupingCode, if present, must be 1-30 characters.", Section = "Account" });
            }
            if (account.TaxonomyCode.HasValue && (account.TaxonomyCode < 1 || account.TaxonomyCode > 999))
            {
                results.Add(new SAFTValidationResult { Field = nameof(account.TaxonomyCode), Message = "TaxonomyCode, if present, must be between 1 and 999.", Section = "Account" });
            }
            // Add more account-specific rules as needed
            return results;
        }
    }
} 