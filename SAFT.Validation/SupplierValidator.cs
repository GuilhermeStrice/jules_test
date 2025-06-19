using System.Collections.Generic;
using System.Text.RegularExpressions;
using SAFT.Lib;
using SAFT.Lib.Files;

namespace SAFT.Validation
{
    public static class SupplierValidator
    {
        public static List<SAFTValidationResult> Validate(Supplier supplier)
        {
            var results = new List<SAFTValidationResult>();
            if (supplier == null)
            {
                results.Add(new SAFTValidationResult { Field = "Supplier", Message = "Supplier is required.", Section = "Supplier" });
                return results;
            }
            if (string.IsNullOrWhiteSpace(supplier.SupplierID))
            {
                results.Add(new SAFTValidationResult { Field = nameof(supplier.SupplierID), Message = "SupplierID is required.", Section = "Supplier" });
            }
            if (!string.IsNullOrWhiteSpace(supplier.SupplierTaxID) && !PortugueseNifValidator.IsValid(supplier.SupplierTaxID))
            {
                results.Add(new SAFTValidationResult { Field = nameof(supplier.SupplierTaxID), Message = "Invalid Portuguese NIF.", Section = "Supplier" });
            }
            if (string.IsNullOrWhiteSpace(supplier.CompanyName))
            {
                results.Add(new SAFTValidationResult { Field = nameof(supplier.CompanyName), Message = "CompanyName is required.", Section = "Supplier" });
            }
            if (!string.IsNullOrWhiteSpace(supplier.Email))
            {
                var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(supplier.Email, emailPattern))
                {
                    results.Add(new SAFTValidationResult { Field = nameof(supplier.Email), Message = "Invalid email format.", Section = "Supplier" });
                }
            }
            if (!string.IsNullOrWhiteSpace(supplier.Telephone))
            {
                var phonePattern = @"^[0-9+\-() ]{6,20}$";
                if (!Regex.IsMatch(supplier.Telephone, phonePattern))
                {
                    results.Add(new SAFTValidationResult { Field = nameof(supplier.Telephone), Message = "Invalid telephone format.", Section = "Supplier" });
                }
            }
            if (supplier.SelfBillingIndicator != 0 && supplier.SelfBillingIndicator != 1)
            {
                results.Add(new SAFTValidationResult { Field = nameof(supplier.SelfBillingIndicator), Message = "SelfBillingIndicator must be 0 or 1.", Section = "Supplier" });
            }
            // Add more supplier-specific rules as needed
            return results;
        }
    }
} 