using System;
using System.Collections.Generic;
using System.Linq;

namespace SAFT.Lib.Utils
{
    /// <summary>
    /// Utility class for mathematical operations related to SAFT documents
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Default decimal places for currency calculations
        /// </summary>
        public const int DefaultDecimalPlaces = 2;

        /// <summary>
        /// Default decimal places for tax calculations
        /// </summary>
        public const int TaxDecimalPlaces = 2;

        /// <summary>
        /// Default decimal places for quantity calculations
        /// </summary>
        public const int QuantityDecimalPlaces = 3;

        /// <summary>
        /// Default decimal places for percentage calculations
        /// </summary>
        public const int PercentageDecimalPlaces = 2;

        /// <summary>
        /// Rounds a decimal value to the specified number of decimal places using MidpointRounding.AwayFromZero
        /// </summary>
        /// <param name="value">Value to round</param>
        /// <param name="decimalPlaces">Number of decimal places (default: 2)</param>
        /// <returns>Rounded value</returns>
        public static decimal Round(decimal value, int decimalPlaces = DefaultDecimalPlaces)
        {
            return Math.Round(value, decimalPlaces, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Rounds a double value to the specified number of decimal places using MidpointRounding.AwayFromZero
        /// </summary>
        /// <param name="value">Value to round</param>
        /// <param name="decimalPlaces">Number of decimal places (default: 2)</param>
        /// <returns>Rounded value</returns>
        public static double Round(double value, int decimalPlaces = DefaultDecimalPlaces)
        {
            return Math.Round(value, decimalPlaces, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Rounds a decimal value for currency calculations
        /// </summary>
        /// <param name="value">Value to round</param>
        /// <returns>Rounded currency value</returns>
        public static decimal RoundCurrency(decimal value)
        {
            return Round(value, DefaultDecimalPlaces);
        }

        /// <summary>
        /// Rounds a decimal value for tax calculations
        /// </summary>
        /// <param name="value">Value to round</param>
        /// <returns>Rounded tax value</returns>
        public static decimal RoundTax(decimal value)
        {
            return Round(value, TaxDecimalPlaces);
        }

        /// <summary>
        /// Rounds a decimal value for quantity calculations
        /// </summary>
        /// <param name="value">Value to round</param>
        /// <returns>Rounded quantity value</returns>
        public static decimal RoundQuantity(decimal value)
        {
            return Round(value, QuantityDecimalPlaces);
        }

        /// <summary>
        /// Rounds a decimal value for percentage calculations
        /// </summary>
        /// <param name="value">Value to round</param>
        /// <returns>Rounded percentage value</returns>
        public static decimal RoundPercentage(decimal value)
        {
            return Round(value, PercentageDecimalPlaces);
        }

        /// <summary>
        /// Calculates VAT amount from net amount and VAT rate
        /// </summary>
        /// <param name="netAmount">Net amount (excluding VAT)</param>
        /// <param name="vatRate">VAT rate as percentage (e.g., 23 for 23%)</param>
        /// <returns>VAT amount</returns>
        public static decimal CalculateVatAmount(decimal netAmount, decimal vatRate)
        {
            if (vatRate < 0)
                throw new ArgumentException("VAT rate cannot be negative", nameof(vatRate));

            var vatMultiplier = vatRate / 100m;
            return RoundTax(netAmount * vatMultiplier);
        }

        /// <summary>
        /// Calculates gross amount from net amount and VAT rate
        /// </summary>
        /// <param name="netAmount">Net amount (excluding VAT)</param>
        /// <param name="vatRate">VAT rate as percentage (e.g., 23 for 23%)</param>
        /// <returns>Gross amount (including VAT)</returns>
        public static decimal CalculateGrossAmount(decimal netAmount, decimal vatRate)
        {
            if (vatRate < 0)
                throw new ArgumentException("VAT rate cannot be negative", nameof(vatRate));

            var vatAmount = CalculateVatAmount(netAmount, vatRate);
            return RoundCurrency(netAmount + vatAmount);
        }

        /// <summary>
        /// Calculates net amount from gross amount and VAT rate
        /// </summary>
        /// <param name="grossAmount">Gross amount (including VAT)</param>
        /// <param name="vatRate">VAT rate as percentage (e.g., 23 for 23%)</param>
        /// <returns>Net amount (excluding VAT)</returns>
        public static decimal CalculateNetAmount(decimal grossAmount, decimal vatRate)
        {
            if (vatRate < 0)
                throw new ArgumentException("VAT rate cannot be negative", nameof(vatRate));
            if (vatRate == 0)
                return grossAmount;

            var vatMultiplier = vatRate / 100m;
            var divisor = 1 + vatMultiplier;
            return RoundCurrency(grossAmount / divisor);
        }

        /// <summary>
        /// Calculates VAT rate from net amount and VAT amount
        /// </summary>
        /// <param name="netAmount">Net amount (excluding VAT)</param>
        /// <param name="vatAmount">VAT amount</param>
        /// <returns>VAT rate as percentage</returns>
        public static decimal CalculateVatRate(decimal netAmount, decimal vatAmount)
        {
            if (netAmount == 0)
                throw new ArgumentException("Net amount cannot be zero", nameof(netAmount));

            var rate = (vatAmount / netAmount) * 100m;
            return RoundPercentage(rate);
        }

        /// <summary>
        /// Calculates line total from unit price and quantity
        /// </summary>
        /// <param name="unitPrice">Unit price</param>
        /// <param name="quantity">Quantity</param>
        /// <returns>Line total</returns>
        public static decimal CalculateLineTotal(decimal unitPrice, decimal quantity)
        {
            if (unitPrice < 0)
                throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));
            if (quantity < 0)
                throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

            return RoundCurrency(unitPrice * quantity);
        }

        /// <summary>
        /// Calculates discount amount from original amount and discount percentage
        /// </summary>
        /// <param name="originalAmount">Original amount</param>
        /// <param name="discountPercentage">Discount percentage (e.g., 10 for 10%)</param>
        /// <returns>Discount amount</returns>
        public static decimal CalculateDiscountAmount(decimal originalAmount, decimal discountPercentage)
        {
            if (discountPercentage < 0 || discountPercentage > 100)
                throw new ArgumentException("Discount percentage must be between 0 and 100", nameof(discountPercentage));

            var discountMultiplier = discountPercentage / 100m;
            return RoundCurrency(originalAmount * discountMultiplier);
        }

        /// <summary>
        /// Calculates amount after discount
        /// </summary>
        /// <param name="originalAmount">Original amount</param>
        /// <param name="discountPercentage">Discount percentage (e.g., 10 for 10%)</param>
        /// <returns>Amount after discount</returns>
        public static decimal CalculateAmountAfterDiscount(decimal originalAmount, decimal discountPercentage)
        {
            var discountAmount = CalculateDiscountAmount(originalAmount, discountPercentage);
            return RoundCurrency(originalAmount - discountAmount);
        }

        /// <summary>
        /// Calculates percentage from part and total
        /// </summary>
        /// <param name="part">Part value</param>
        /// <param name="total">Total value</param>
        /// <returns>Percentage</returns>
        public static decimal CalculatePercentage(decimal part, decimal total)
        {
            if (total == 0)
                throw new ArgumentException("Total cannot be zero", nameof(total));

            var percentage = (part / total) * 100m;
            return RoundPercentage(percentage);
        }

        /// <summary>
        /// Checks if two decimal values are equal within a specified tolerance
        /// </summary>
        /// <param name="value1">First value</param>
        /// <param name="value2">Second value</param>
        /// <param name="tolerance">Tolerance for comparison (default: 0.01m)</param>
        /// <returns>True if values are equal within tolerance, false otherwise</returns>
        public static bool AreEqual(decimal value1, decimal value2, decimal tolerance = 0.01m)
        {
            return Math.Abs(value1 - value2) <= tolerance;
        }

        /// <summary>
        /// Checks if a decimal value is zero within a specified tolerance
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <param name="tolerance">Tolerance for comparison (default: 0.01m)</param>
        /// <returns>True if value is zero within tolerance, false otherwise</returns>
        public static bool IsZero(decimal value, decimal tolerance = 0.01m)
        {
            return Math.Abs(value) <= tolerance;
        }

        /// <summary>
        /// Checks if a decimal value is positive (greater than zero)
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <returns>True if positive, false otherwise</returns>
        public static bool IsPositive(decimal value)
        {
            return value > 0;
        }

        /// <summary>
        /// Checks if a decimal value is negative (less than zero)
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <returns>True if negative, false otherwise</returns>
        public static bool IsNegative(decimal value)
        {
            return value < 0;
        }

        /// <summary>
        /// Gets the absolute value of a decimal
        /// </summary>
        /// <param name="value">Value to get absolute value for</param>
        /// <returns>Absolute value</returns>
        public static decimal Abs(decimal value)
        {
            return Math.Abs(value);
        }

        /// <summary>
        /// Gets the minimum of two decimal values
        /// </summary>
        /// <param name="value1">First value</param>
        /// <param name="value2">Second value</param>
        /// <returns>Minimum value</returns>
        public static decimal Min(decimal value1, decimal value2)
        {
            return Math.Min(value1, value2);
        }

        /// <summary>
        /// Gets the maximum of two decimal values
        /// </summary>
        /// <param name="value1">First value</param>
        /// <param name="value2">Second value</param>
        /// <returns>Maximum value</returns>
        public static decimal Max(decimal value1, decimal value2)
        {
            return Math.Max(value1, value2);
        }

        /// <summary>
        /// Gets the minimum value from a collection of decimals
        /// </summary>
        /// <param name="values">Collection of values</param>
        /// <returns>Minimum value</returns>
        public static decimal Min(params decimal[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("Values cannot be null or empty", nameof(values));

            return values.Min();
        }

        /// <summary>
        /// Gets the maximum value from a collection of decimals
        /// </summary>
        /// <param name="values">Collection of values</param>
        /// <returns>Maximum value</returns>
        public static decimal Max(params decimal[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("Values cannot be null or empty", nameof(values));

            return values.Max();
        }

        /// <summary>
        /// Calculates the sum of a collection of decimal values
        /// </summary>
        /// <param name="values">Collection of values</param>
        /// <returns>Sum of values</returns>
        public static decimal Sum(params decimal[] values)
        {
            if (values == null || values.Length == 0)
                return 0;

            return values.Sum();
        }

        /// <summary>
        /// Calculates the average of a collection of decimal values
        /// </summary>
        /// <param name="values">Collection of values</param>
        /// <returns>Average of values</returns>
        public static decimal Average(params decimal[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("Values cannot be null or empty", nameof(values));

            return values.Average();
        }

        /// <summary>
        /// Validates if a decimal value is within a specified range
        /// </summary>
        /// <param name="value">Value to validate</param>
        /// <param name="min">Minimum allowed value</param>
        /// <param name="max">Maximum allowed value</param>
        /// <returns>True if value is within range, false otherwise</returns>
        public static bool IsInRange(decimal value, decimal min, decimal max)
        {
            return value >= min && value <= max;
        }

        /// <summary>
        /// Clamps a decimal value to a specified range
        /// </summary>
        /// <param name="value">Value to clamp</param>
        /// <param name="min">Minimum allowed value</param>
        /// <param name="max">Maximum allowed value</param>
        /// <returns>Clamped value</returns>
        public static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (min > max)
                throw new ArgumentException("Minimum value cannot be greater than maximum value");

            return Math.Max(min, Math.Min(max, value));
        }

        /// <summary>
        /// Calculates compound interest
        /// </summary>
        /// <param name="principal">Principal amount</param>
        /// <param name="rate">Interest rate as percentage</param>
        /// <param name="time">Time period</param>
        /// <param name="compoundsPerYear">Number of times interest is compounded per year (default: 1)</param>
        /// <returns>Compound interest amount</returns>
        public static decimal CalculateCompoundInterest(decimal principal, decimal rate, decimal time, int compoundsPerYear = 1)
        {
            if (principal < 0)
                throw new ArgumentException("Principal cannot be negative", nameof(principal));
            if (rate < 0)
                throw new ArgumentException("Rate cannot be negative", nameof(rate));
            if (time < 0)
                throw new ArgumentException("Time cannot be negative", nameof(time));
            if (compoundsPerYear <= 0)
                throw new ArgumentException("Compounds per year must be positive", nameof(compoundsPerYear));

            var rateDecimal = rate / 100m;
            var exponent = compoundsPerYear * time;
            var baseValue = 1 + (rateDecimal / compoundsPerYear);
            var result = principal * (decimal)Math.Pow((double)baseValue, (double)exponent);
            
            return RoundCurrency(result - principal);
        }
    }
} 