using System.Collections.Generic;
using SAFT.Lib;
using SAFT.Lib.Files;

namespace SAFT.Validation
{
    public static class ProductValidator
    {
        private static readonly HashSet<string> AllowedTypes = new HashSet<string> { "P", "S", "O", "E", "I" };

        public static List<SAFTValidationResult> Validate(Product product)
        {
            var results = new List<SAFTValidationResult>();
            if (product == null)
            {
                results.Add(new SAFTValidationResult { Field = "Product", Message = "Product is required.", Section = "Product" });
                return results;
            }
            if (string.IsNullOrWhiteSpace(product.ProductType) || !AllowedTypes.Contains(product.ProductType))
            {
                results.Add(new SAFTValidationResult { Field = nameof(product.ProductType), Message = "ProductType must be one of P, S, O, E, I.", Section = "Product" });
            }
            if (string.IsNullOrWhiteSpace(product.ProductCode) || product.ProductCode.Length > 60)
            {
                results.Add(new SAFTValidationResult { Field = nameof(product.ProductCode), Message = "ProductCode is required and must be 1-60 characters.", Section = "Product" });
            }
            if (string.IsNullOrWhiteSpace(product.ProductDescription) || product.ProductDescription.Length < 2 || product.ProductDescription.Length > 200)
            {
                results.Add(new SAFTValidationResult { Field = nameof(product.ProductDescription), Message = "ProductDescription is required and must be 2-200 characters.", Section = "Product" });
            }
            if (string.IsNullOrWhiteSpace(product.ProductNumberCode) || product.ProductNumberCode.Length > 60)
            {
                results.Add(new SAFTValidationResult { Field = nameof(product.ProductNumberCode), Message = "ProductNumberCode is required and must be 1-60 characters.", Section = "Product" });
            }
            // Add more product-specific rules as needed
            return results;
        }
    }
} 