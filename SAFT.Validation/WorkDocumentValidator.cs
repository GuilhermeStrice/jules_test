using System;
using System.Collections.Generic;
using SAFT.Lib.Documents;

namespace SAFT.Validation
{
    public static class WorkDocumentValidator
    {
        private static readonly HashSet<string> AllowedTypes = new HashSet<string> { "CM", "CC", "FC", "FO", "NE", "OU", "OR", "PF", "DC", "RP", "RE", "CS", "LD", "RA" };

        public static List<SAFTValidationResult> Validate(WorkDocument doc)
        {
            var results = new List<SAFTValidationResult>();
            if (doc == null)
            {
                results.Add(new SAFTValidationResult { Field = "WorkDocument", Message = "WorkDocument is required.", Section = "WorkDocument" });
                return results;
            }
            if (string.IsNullOrWhiteSpace(doc.DocumentNumber) || doc.DocumentNumber.Length > 60)
            {
                results.Add(new SAFTValidationResult { Field = nameof(doc.DocumentNumber), Message = "DocumentNumber is required and must be 1-60 characters.", Section = "WorkDocument" });
            }
            if (doc.WorkDate == default)
            {
                results.Add(new SAFTValidationResult { Field = nameof(doc.WorkDate), Message = "WorkDate is required.", Section = "WorkDocument" });
            }
            if (string.IsNullOrWhiteSpace(doc.WorkType) || !AllowedTypes.Contains(doc.WorkType))
            {
                results.Add(new SAFTValidationResult { Field = nameof(doc.WorkType), Message = "WorkType must be a valid type.", Section = "WorkDocument" });
            }
            if (string.IsNullOrWhiteSpace(doc.CustomerID))
            {
                results.Add(new SAFTValidationResult { Field = nameof(doc.CustomerID), Message = "CustomerID is required.", Section = "WorkDocument" });
            }
            if (doc.Lines == null || doc.Lines.Count == 0)
            {
                results.Add(new SAFTValidationResult { Field = nameof(doc.Lines), Message = "At least one work document line is required.", Section = "WorkDocument" });
            }
            else
            {
                for (int i = 0; i < doc.Lines.Count; i++)
                {
                    var line = doc.Lines[i];
                    if (string.IsNullOrWhiteSpace(line.ProductCode))
                    {
                        results.Add(new SAFTValidationResult { Field = $"Lines[{i}].ProductCode", Message = "ProductCode is required.", Section = "WorkDocument" });
                    }
                    if (line.Quantity <= 0)
                    {
                        results.Add(new SAFTValidationResult { Field = $"Lines[{i}].Quantity", Message = "Quantity must be greater than 0.", Section = "WorkDocument" });
                    }
                    if (line.UnitPrice < 0)
                    {
                        results.Add(new SAFTValidationResult { Field = $"Lines[{i}].UnitPrice", Message = "UnitPrice must be >= 0.", Section = "WorkDocument" });
                    }
                }
            }
            // Add more work document-specific rules as needed
            return results;
        }
    }
} 