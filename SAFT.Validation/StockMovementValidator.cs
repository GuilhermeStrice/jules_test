using System;
using System.Collections.Generic;
using SAFT.Lib.Documents;

namespace SAFT.Validation
{
    public static class StockMovementValidator
    {
        private static readonly HashSet<string> AllowedTypes = new HashSet<string> { "GR", "GT", "GA", "GC", "GD" };

        public static List<SAFTValidationResult> Validate(StockMovement movement)
        {
            var results = new List<SAFTValidationResult>();
            if (movement == null)
            {
                results.Add(new SAFTValidationResult { Field = "StockMovement", Message = "StockMovement is required.", Section = "StockMovement" });
                return results;
            }
            if (string.IsNullOrWhiteSpace(movement.DocumentNumber) || movement.DocumentNumber.Length > 60)
            {
                results.Add(new SAFTValidationResult { Field = nameof(movement.DocumentNumber), Message = "DocumentNumber is required and must be 1-60 characters.", Section = "StockMovement" });
            }
            if (movement.MovementDate == default)
            {
                results.Add(new SAFTValidationResult { Field = nameof(movement.MovementDate), Message = "MovementDate is required.", Section = "StockMovement" });
            }
            if (string.IsNullOrWhiteSpace(movement.MovementType) || !AllowedTypes.Contains(movement.MovementType))
            {
                results.Add(new SAFTValidationResult { Field = nameof(movement.MovementType), Message = "MovementType must be one of GR, GT, GA, GC, GD.", Section = "StockMovement" });
            }
            if (movement.Lines == null || movement.Lines.Count == 0)
            {
                results.Add(new SAFTValidationResult { Field = nameof(movement.Lines), Message = "At least one stock movement line is required.", Section = "StockMovement" });
            }
            else
            {
                for (int i = 0; i < movement.Lines.Count; i++)
                {
                    var line = movement.Lines[i];
                    if (string.IsNullOrWhiteSpace(line.ProductCode))
                    {
                        results.Add(new SAFTValidationResult { Field = $"Lines[{i}].ProductCode", Message = "ProductCode is required.", Section = "StockMovement" });
                    }
                    if (line.Quantity <= 0)
                    {
                        results.Add(new SAFTValidationResult { Field = $"Lines[{i}].Quantity", Message = "Quantity must be greater than 0.", Section = "StockMovement" });
                    }
                    if (line.UnitPrice < 0)
                    {
                        results.Add(new SAFTValidationResult { Field = $"Lines[{i}].UnitPrice", Message = "UnitPrice must be >= 0.", Section = "StockMovement" });
                    }
                }
            }
            // Add more stock movement-specific rules as needed
            return results;
        }
    }
} 