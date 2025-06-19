using System.Collections.Generic;
using System.Text.RegularExpressions;
using SAFT.Lib;
using SAFT.Lib.Files;

namespace SAFT.Validation
{
    public static class CustomerValidator
    {
        public static List<SAFTValidationResult> Validate(Customer customer)
        {
            var results = new List<SAFTValidationResult>();
            if (customer == null)
            {
                results.Add(new SAFTValidationResult { Field = "Customer", Message = "Customer is required.", Section = "Customer" });
                return results;
            }
            if (string.IsNullOrWhiteSpace(customer.CustomerID))
            {
                results.Add(new SAFTValidationResult { Field = nameof(customer.CustomerID), Message = "CustomerID is required.", Section = "Customer" });
            }
            if (!string.IsNullOrWhiteSpace(customer.CustomerTaxID) && !PortugueseNifValidator.IsValid(customer.CustomerTaxID))
            {
                results.Add(new SAFTValidationResult { Field = nameof(customer.CustomerTaxID), Message = "Invalid Portuguese NIF.", Section = "Customer" });
            }
            if (string.IsNullOrWhiteSpace(customer.CompanyName))
            {
                results.Add(new SAFTValidationResult { Field = nameof(customer.CompanyName), Message = "CompanyName is required.", Section = "Customer" });
            }
            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                if (!Regex.IsMatch(customer.Email, emailPattern))
                {
                    results.Add(new SAFTValidationResult { Field = nameof(customer.Email), Message = "Invalid email format.", Section = "Customer" });
                }
            }
            if (!string.IsNullOrWhiteSpace(customer.Telephone))
            {
                var phonePattern = @"^[0-9+\-() ]{6,20}$";
                if (!Regex.IsMatch(customer.Telephone, phonePattern))
                {
                    results.Add(new SAFTValidationResult { Field = nameof(customer.Telephone), Message = "Invalid telephone format.", Section = "Customer" });
                }
            }
            if (customer.SelfBillingIndicator != 0 && customer.SelfBillingIndicator != 1)
            {
                results.Add(new SAFTValidationResult { Field = nameof(customer.SelfBillingIndicator), Message = "SelfBillingIndicator must be 0 or 1.", Section = "Customer" });
            }
            // Add more customer-specific rules as needed
            return results;
        }
    }
} 