using System;
using System.Globalization;
using SAFT.Lib.Constants;

namespace SAFT.Lib.Utils
{
    /// <summary>
    /// Utility class for date and time operations related to SAFT documents
    /// </summary>
    public static class DateTimeUtils
    {
        /// <summary>
        /// Portuguese culture info for date formatting
        /// </summary>
        public static readonly CultureInfo PortugueseCulture = new CultureInfo("pt-PT");

        /// <summary>
        /// Gets the fiscal year for a given date
        /// </summary>
        /// <param name="date">Date to get fiscal year for</param>
        /// <returns>Fiscal year (integer)</returns>
        public static int GetFiscalYear(DateTime date)
        {
            return date.Year;
        }

        /// <summary>
        /// Gets the fiscal year for current date
        /// </summary>
        /// <returns>Current fiscal year</returns>
        public static int GetCurrentFiscalYear()
        {
            return DateTime.Now.Year;
        }

        /// <summary>
        /// Validates if a fiscal year is within the allowed range
        /// </summary>
        /// <param name="fiscalYear">Fiscal year to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidFiscalYear(int fiscalYear)
        {
            return fiscalYear >= SAFTConstants.Ranges.FiscalYearMin && 
                   fiscalYear <= SAFTConstants.Ranges.FiscalYearMax;
        }

        /// <summary>
        /// Gets the accounting period (month) for a given date
        /// </summary>
        /// <param name="date">Date to get period for</param>
        /// <returns>Accounting period (1-12)</returns>
        public static int GetAccountingPeriod(DateTime date)
        {
            return date.Month;
        }

        /// <summary>
        /// Validates if an accounting period is within the allowed range
        /// </summary>
        /// <param name="period">Period to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidAccountingPeriod(int period)
        {
            return period >= SAFTConstants.Ranges.PeriodMin && 
                   period <= SAFTConstants.Ranges.PeriodMax;
        }

        /// <summary>
        /// Gets the first day of a fiscal year
        /// </summary>
        /// <param name="fiscalYear">Fiscal year</param>
        /// <returns>First day of the fiscal year</returns>
        public static DateTime GetFiscalYearStart(int fiscalYear)
        {
            return new DateTime(fiscalYear, 1, 1);
        }

        /// <summary>
        /// Gets the last day of a fiscal year
        /// </summary>
        /// <param name="fiscalYear">Fiscal year</param>
        /// <returns>Last day of the fiscal year</returns>
        public static DateTime GetFiscalYearEnd(int fiscalYear)
        {
            return new DateTime(fiscalYear, 12, 31);
        }

        /// <summary>
        /// Gets the first day of an accounting period
        /// </summary>
        /// <param name="fiscalYear">Fiscal year</param>
        /// <param name="period">Accounting period (1-12)</param>
        /// <returns>First day of the period</returns>
        public static DateTime GetPeriodStart(int fiscalYear, int period)
        {
            if (!IsValidAccountingPeriod(period))
                throw new ArgumentOutOfRangeException(nameof(period), $"Period must be between {SAFTConstants.Ranges.PeriodMin} and {SAFTConstants.Ranges.PeriodMax}");

            return new DateTime(fiscalYear, period, 1);
        }

        /// <summary>
        /// Gets the last day of an accounting period
        /// </summary>
        /// <param name="fiscalYear">Fiscal year</param>
        /// <param name="period">Accounting period (1-12)</param>
        /// <returns>Last day of the period</returns>
        public static DateTime GetPeriodEnd(int fiscalYear, int period)
        {
            if (!IsValidAccountingPeriod(period))
                throw new ArgumentOutOfRangeException(nameof(period), $"Period must be between {SAFTConstants.Ranges.PeriodMin} and {SAFTConstants.Ranges.PeriodMax}");

            return new DateTime(fiscalYear, period, DateTime.DaysInMonth(fiscalYear, period));
        }

        /// <summary>
        /// Checks if a date falls within a fiscal year
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <param name="fiscalYear">Fiscal year</param>
        /// <returns>True if date is within fiscal year, false otherwise</returns>
        public static bool IsDateInFiscalYear(DateTime date, int fiscalYear)
        {
            return date.Year == fiscalYear;
        }

        /// <summary>
        /// Checks if a date falls within an accounting period
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <param name="fiscalYear">Fiscal year</param>
        /// <param name="period">Accounting period</param>
        /// <returns>True if date is within period, false otherwise</returns>
        public static bool IsDateInPeriod(DateTime date, int fiscalYear, int period)
        {
            return date.Year == fiscalYear && date.Month == period;
        }

        /// <summary>
        /// Formats a date for SAFT XML output (YYYY-MM-DD)
        /// </summary>
        /// <param name="date">Date to format</param>
        /// <returns>Formatted date string</returns>
        public static string FormatDateForSaft(DateTime date)
        {
            return date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a date and time for SAFT XML output (YYYY-MM-DDTHH:mm:ss)
        /// </summary>
        /// <param name="dateTime">Date and time to format</param>
        /// <returns>Formatted date and time string</returns>
        public static string FormatDateTimeForSaft(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parses a SAFT date string (YYYY-MM-DD)
        /// </summary>
        /// <param name="dateString">Date string to parse</param>
        /// <returns>Parsed DateTime or null if invalid</returns>
        public static DateTime? ParseSaftDate(string? dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return null;

            if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                return result;

            return null;
        }

        /// <summary>
        /// Parses a SAFT date and time string (YYYY-MM-DDTHH:mm:ss)
        /// </summary>
        /// <param name="dateTimeString">Date and time string to parse</param>
        /// <returns>Parsed DateTime or null if invalid</returns>
        public static DateTime? ParseSaftDateTime(string? dateTimeString)
        {
            if (string.IsNullOrWhiteSpace(dateTimeString))
                return null;

            if (DateTime.TryParseExact(dateTimeString, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                return result;

            return null;
        }

        /// <summary>
        /// Gets the number of days between two dates
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Number of days between dates</returns>
        public static int GetDaysBetween(DateTime startDate, DateTime endDate)
        {
            return (endDate - startDate).Days;
        }

        /// <summary>
        /// Gets the number of working days between two dates (excluding weekends)
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Number of working days between dates</returns>
        public static int GetWorkingDaysBetween(DateTime startDate, DateTime endDate)
        {
            var days = 0;
            var current = startDate.Date;

            while (current <= endDate.Date)
            {
                if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                    days++;
                current = current.AddDays(1);
            }

            return days;
        }

        /// <summary>
        /// Checks if a date is a working day (not weekend)
        /// </summary>
        /// <param name="date">Date to check</param>
        /// <returns>True if working day, false otherwise</returns>
        public static bool IsWorkingDay(DateTime date)
        {
            return date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday;
        }

        /// <summary>
        /// Gets the next working day from a given date
        /// </summary>
        /// <param name="date">Starting date</param>
        /// <returns>Next working day</returns>
        public static DateTime GetNextWorkingDay(DateTime date)
        {
            var nextDay = date.AddDays(1);
            while (!IsWorkingDay(nextDay))
            {
                nextDay = nextDay.AddDays(1);
            }
            return nextDay;
        }

        /// <summary>
        /// Gets the previous working day from a given date
        /// </summary>
        /// <param name="date">Starting date</param>
        /// <returns>Previous working day</returns>
        public static DateTime GetPreviousWorkingDay(DateTime date)
        {
            var previousDay = date.AddDays(-1);
            while (!IsWorkingDay(previousDay))
            {
                previousDay = previousDay.AddDays(-1);
            }
            return previousDay;
        }

        /// <summary>
        /// Validates if a date range is valid (start date before end date)
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>True if valid range, false otherwise</returns>
        public static bool IsValidDateRange(DateTime startDate, DateTime endDate)
        {
            return startDate <= endDate;
        }

        /// <summary>
        /// Gets the quarter number for a given date
        /// </summary>
        /// <param name="date">Date to get quarter for</param>
        /// <returns>Quarter number (1-4)</returns>
        public static int GetQuarter(DateTime date)
        {
            return (date.Month - 1) / 3 + 1;
        }

        /// <summary>
        /// Gets the quarter start date for a given date
        /// </summary>
        /// <param name="date">Date to get quarter start for</param>
        /// <returns>Quarter start date</returns>
        public static DateTime GetQuarterStart(DateTime date)
        {
            var quarter = GetQuarter(date);
            var quarterStartMonth = (quarter - 1) * 3 + 1;
            return new DateTime(date.Year, quarterStartMonth, 1);
        }

        /// <summary>
        /// Gets the quarter end date for a given date
        /// </summary>
        /// <param name="date">Date to get quarter end for</param>
        /// <returns>Quarter end date</returns>
        public static DateTime GetQuarterEnd(DateTime date)
        {
            var quarter = GetQuarter(date);
            var quarterEndMonth = quarter * 3;
            return new DateTime(date.Year, quarterEndMonth, DateTime.DaysInMonth(date.Year, quarterEndMonth));
        }
    }
} 