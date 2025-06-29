using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SAFT.Lib.Utils
{
    /// <summary>
    /// Portuguese-specific utilities for SAF-T compliance
    /// Includes ATCUD generation, hash generation, and Portuguese business rule validation
    /// </summary>
    public static class PortugueseUtils
    {
        /// <summary>
        /// Generates an ATCUD (Unique Document Code) for Portuguese invoices
        /// Format: YYYYMMDD-XXXXX where XXXXX is a sequential number
        /// </summary>
        /// <param name="documentDate">The document date</param>
        /// <param name="sequenceNumber">The sequential number (1-99999)</param>
        /// <returns>The generated ATCUD</returns>
        public static string GenerateATCUD(DateTime documentDate, int sequenceNumber)
        {
            if (sequenceNumber < 1 || sequenceNumber > 99999)
                throw new ArgumentException("Sequence number must be between 1 and 99999", nameof(sequenceNumber));

            var datePart = documentDate.ToString("yyyyMMdd");
            var sequencePart = sequenceNumber.ToString("D5");
            
            return $"{datePart}-{sequencePart}";
        }

        /// <summary>
        /// Validates an ATCUD format
        /// </summary>
        /// <param name="atcud">The ATCUD to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidATCUD(string atcud)
        {
            if (string.IsNullOrWhiteSpace(atcud))
                return false;

            // Pattern: YYYYMMDD-XXXXX
            var pattern = @"^\d{8}-\d{5}$";
            if (!Regex.IsMatch(atcud, pattern))
                return false;

            // Validate date part
            var datePart = atcud.Substring(0, 8);
            if (!DateTime.TryParseExact(datePart, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out _))
                return false;

            // Validate sequence number
            var sequencePart = atcud.Substring(9, 5);
            if (!int.TryParse(sequencePart, out int sequence) || sequence < 1 || sequence > 99999)
                return false;

            return true;
        }

        /// <summary>
        /// Generates a SHA-256 hash for document integrity
        /// </summary>
        /// <param name="documentContent">The document content to hash</param>
        /// <returns>The SHA-256 hash as a hexadecimal string</returns>
        public static string GenerateDocumentHash(string documentContent)
        {
            if (string.IsNullOrEmpty(documentContent))
                throw new ArgumentException("Document content cannot be null or empty", nameof(documentContent));

            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(documentContent);
                var hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }

        /// <summary>
        /// Generates a hash control string for Portuguese documents
        /// Format: [Hash]-[DocumentType] [DocumentNumber]
        /// </summary>
        /// <param name="documentHash">The document hash</param>
        /// <param name="documentType">The document type (FT, FR, etc.)</param>
        /// <param name="documentNumber">The document number</param>
        /// <returns>The hash control string</returns>
        public static string GenerateHashControl(string documentHash, string documentType, string documentNumber)
        {
            if (string.IsNullOrEmpty(documentHash))
                throw new ArgumentException("Document hash cannot be null or empty", nameof(documentHash));
            if (string.IsNullOrEmpty(documentType))
                throw new ArgumentException("Document type cannot be null or empty", nameof(documentType));
            if (string.IsNullOrEmpty(documentNumber))
                throw new ArgumentException("Document number cannot be null or empty", nameof(documentNumber));

            return $"{documentHash}-{documentType} {documentNumber}";
        }

        /// <summary>
        /// Validates a hash control string format
        /// </summary>
        /// <param name="hashControl">The hash control to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidHashControl(string hashControl)
        {
            if (string.IsNullOrWhiteSpace(hashControl))
                return false;

            // Pattern from SAFTConstants: [0-9]+|[0-9]+[\.][0-9]+|[0-9]+-[A-Z]{2}(M )([^/^ ]+[/][0-9]+)|[0-9]+-[A-Z]{2}(D )([^ ]+ [^/^ ]+[/][0-9]+)
            var pattern = @"^[0-9]+|[0-9]+\.[0-9]+|[0-9]+-[A-Z]{2}(M )([^/^ ]+[/][0-9]+)|[0-9]+-[A-Z]{2}(D )([^ ]+ [^/^ ]+[/][0-9]+)$";
            return Regex.IsMatch(hashControl, pattern);
        }

        /// <summary>
        /// Generates a Portuguese invoice number in the required format
        /// Format: [DocumentType] [Year]/[Sequence]
        /// Example: FT 2024/001
        /// </summary>
        /// <param name="documentType">The document type (FT, FR, etc.)</param>
        /// <param name="year">The fiscal year</param>
        /// <param name="sequence">The sequence number</param>
        /// <returns>The formatted invoice number</returns>
        public static string GenerateInvoiceNumber(string documentType, int year, int sequence)
        {
            if (string.IsNullOrEmpty(documentType))
                throw new ArgumentException("Document type cannot be null or empty", nameof(documentType));
            if (year < 2000 || year > 9999)
                throw new ArgumentException("Year must be between 2000 and 9999", nameof(year));
            if (sequence < 1 || sequence > 999999)
                throw new ArgumentException("Sequence must be between 1 and 999999", nameof(sequence));

            return $"{documentType} {year}/{sequence:D3}";
        }

        /// <summary>
        /// Validates a Portuguese invoice number format
        /// </summary>
        /// <param name="invoiceNumber">The invoice number to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidInvoiceNumber(string invoiceNumber)
        {
            if (string.IsNullOrWhiteSpace(invoiceNumber))
                return false;

            // Pattern: [DocumentType] [Year]/[Sequence]
            var pattern = @"^[A-Z]{2}\s\d{4}/\d{3}$";
            return Regex.IsMatch(invoiceNumber, pattern);
        }

        /// <summary>
        /// Validates Portuguese VAT calculation rules
        /// Ensures VAT calculations follow Portuguese tax rules
        /// </summary>
        /// <param name="netAmount">The net amount</param>
        /// <param name="vatRate">The VAT rate (as decimal, e.g., 0.23 for 23%)</param>
        /// <param name="vatAmount">The calculated VAT amount</param>
        /// <returns>True if calculation is correct, false otherwise</returns>
        public static bool ValidateVATCalculation(decimal netAmount, decimal vatRate, decimal vatAmount)
        {
            if (netAmount < 0 || vatRate < 0 || vatAmount < 0)
                return false;

            var expectedVAT = Math.Round(netAmount * vatRate, 2, MidpointRounding.AwayFromZero);
            return Math.Abs(expectedVAT - vatAmount) < 0.01m;
        }

        /// <summary>
        /// Validates Portuguese document status rules
        /// Ensures document status follows Portuguese business rules
        /// </summary>
        /// <param name="documentStatus">The document status</param>
        /// <param name="documentType">The document type</param>
        /// <returns>True if status is valid for document type, false otherwise</returns>
        public static bool ValidateDocumentStatus(string documentStatus, string documentType)
        {
            if (string.IsNullOrEmpty(documentStatus) || string.IsNullOrEmpty(documentType))
                return false;

            // Portuguese business rules for document status
            switch (documentType.ToUpper())
            {
                case "FT": // Invoice
                    return documentStatus == "N" || documentStatus == "A" || documentStatus == "F";
                case "FR": // Credit Note
                    return documentStatus == "N" || documentStatus == "A" || documentStatus == "F";
                case "FS": // Debit Note
                    return documentStatus == "N" || documentStatus == "A" || documentStatus == "F";
                default:
                    return true; // Unknown document type, assume valid
            }
        }

        /// <summary>
        /// Generates a Portuguese transaction ID in the required format
        /// Format: YYYY-MM-DD [Description] [Reference]
        /// </summary>
        /// <param name="date">The transaction date</param>
        /// <param name="description">The transaction description</param>
        /// <param name="reference">The transaction reference</param>
        /// <returns>The formatted transaction ID</returns>
        public static string GenerateTransactionID(DateTime date, string description, string reference)
        {
            if (string.IsNullOrEmpty(description))
                throw new ArgumentException("Description cannot be null or empty", nameof(description));
            if (string.IsNullOrEmpty(reference))
                throw new ArgumentException("Reference cannot be null or empty", nameof(reference));

            if (description.Length > 30)
                description = description.Substring(0, 30);
            if (reference.Length > 20)
                reference = reference.Substring(0, 20);

            return $"{date:yyyy-MM-dd} {description} {reference}";
        }

        /// <summary>
        /// Validates a Portuguese transaction ID format
        /// </summary>
        /// <param name="transactionID">The transaction ID to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidTransactionID(string transactionID)
        {
            if (string.IsNullOrWhiteSpace(transactionID))
                return false;

            // Pattern: YYYY-MM-DD [Description] [Reference]
            var pattern = @"^\d{4}-\d{2}-\d{2}\s[^\s]{1,30}\s[^\s]{1,20}$";
            return Regex.IsMatch(transactionID, pattern);
        }

        /// <summary>
        /// Validates a Portuguese VAT number format
        /// Must be 9 digits between 100000000 and 999999999
        /// </summary>
        /// <param name="vatNumber">The VAT number to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsValidVATNumber(string vatNumber)
        {
            if (string.IsNullOrWhiteSpace(vatNumber))
                return false;

            // Must be exactly 9 digits
            if (!Regex.IsMatch(vatNumber, @"^\d{9}$"))
                return false;

            // Must be between 100000000 and 999999999
            if (!int.TryParse(vatNumber, out int number))
                return false;

            return number >= 100000000 && number <= 999999999;
        }
    }
} 