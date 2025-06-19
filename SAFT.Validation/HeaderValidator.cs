using System.Collections.Generic;
using SAFT.Lib;

namespace SAFT.Validation
{
    public static class HeaderValidator
    {
        public static List<SAFTValidationResult> Validate(Header header)
        {
            var results = new List<SAFTValidationResult>();
            if (header == null)
            {
                results.Add(new SAFTValidationResult { Field = "Header", Message = "Header is required.", Section = "Header" });
                return results;
            }
            if (!PortugueseNifValidator.IsValid(header.TaxRegistrationNumber.ToString()))
            {
                results.Add(new SAFTValidationResult { Field = nameof(header.TaxRegistrationNumber), Message = "Invalid Portuguese NIF.", Section = "Header" });
            }
            if (string.IsNullOrWhiteSpace(header.CompanyID))
            {
                results.Add(new SAFTValidationResult { Field = nameof(header.CompanyID), Message = "CompanyID is required.", Section = "Header" });
            }
            if (header.FiscalYear < 2000 || header.FiscalYear > 9999)
            {
                results.Add(new SAFTValidationResult { Field = nameof(header.FiscalYear), Message = "FiscalYear must be between 2000 and 9999.", Section = "Header" });
            }
            if (header.CurrencyCode != "EUR")
            {
                results.Add(new SAFTValidationResult { Field = nameof(header.CurrencyCode), Message = "CurrencyCode must be 'EUR'.", Section = "Header" });
            }
            if (header.SoftwareCertificateNumber <= 0)
            {
                results.Add(new SAFTValidationResult { Field = nameof(header.SoftwareCertificateNumber), Message = "SoftwareCertificateNumber must be positive.", Section = "Header" });
            }
            // Add more header-specific rules as needed
            return results;
        }
    }
} 