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

            // <xs:assert> GroupingCategory/TaxonomyCode logic
            // If GroupingCategory != 'GM', TaxonomyCode must be missing. If 'GM', must be present.
            if (account.GroupingCategory != "GM" && account.TaxonomyCode.HasValue)
            {
                results.Add(new SAFTValidationResult {
                    Field = nameof(account.TaxonomyCode),
                    Message = "TaxonomyCode must be present only if GroupingCategory is 'GM' (SAF-T <xs:assert> Account 1).",
                    Section = "Account"
                });
            }
            if (account.GroupingCategory == "GM" && !account.TaxonomyCode.HasValue)
            {
                results.Add(new SAFTValidationResult {
                    Field = nameof(account.TaxonomyCode),
                    Message = "TaxonomyCode is required when GroupingCategory is 'GM' (SAF-T <xs:assert> Account 1).",
                    Section = "Account"
                });
            }

            // <xs:assert> GroupingCategory/GroupingCode logic
            // If GroupingCategory == 'GR' or 'AR', GroupingCode must be missing.
            if ((account.GroupingCategory == "GR" || account.GroupingCategory == "AR") && !string.IsNullOrWhiteSpace(account.GroupingCode))
            {
                results.Add(new SAFTValidationResult {
                    Field = nameof(account.GroupingCode),
                    Message = "GroupingCode must be missing when GroupingCategory is 'GR' or 'AR' (SAF-T <xs:assert> Account 2).",
                    Section = "Account"
                });
            }
            // If GroupingCategory == 'GA', 'AA', 'GM', or 'AM', GroupingCode must be present.
            if ((account.GroupingCategory == "GA" || account.GroupingCategory == "AA" || account.GroupingCategory == "GM" || account.GroupingCategory == "AM") && string.IsNullOrWhiteSpace(account.GroupingCode))
            {
                results.Add(new SAFTValidationResult {
                    Field = nameof(account.GroupingCode),
                    Message = "GroupingCode is required when GroupingCategory is 'GA', 'AA', 'GM', or 'AM' (SAF-T <xs:assert> Account 2).",
                    Section = "Account"
                });
            }
            // Add more account-specific rules as needed
            return results;
        }
    }
} 