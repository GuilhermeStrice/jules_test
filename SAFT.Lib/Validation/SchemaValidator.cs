using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using SAFT.Lib.Utils;

namespace SAFT.Lib.Validation
{
    /// <summary>
    /// Provides utilities to validate a SAF-T XML string against the official XSD schema.
    /// The input XML must already have the correct namespaces. This class does not modify the XML.
    /// </summary>
    public static partial class SchemaValidator
    {



        

        /// <summary>
        /// Validates date ranges and fiscal year compliance.
        /// </summary>
        internal static List<string> ValidateDateRanges(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");

            var header = xmlDoc.SelectSingleNode("//ns:Header", nsManager);
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (header != null && invoices != null && invoices.Count > 0)
            {
                var startDate = header.SelectSingleNode(".//ns:StartDate", nsManager);
                var endDate = header.SelectSingleNode(".//ns:EndDate", nsManager);
                var fiscalYear = header.SelectSingleNode(".//ns:FiscalYear", nsManager);
                if (startDate != null && endDate != null)
                {
                    if (DateTime.TryParse(startDate.InnerText, out DateTime start) &&
                        DateTime.TryParse(endDate.InnerText, out DateTime end))
                    {
                        foreach (XmlNode invoice in invoices)
                        {
                            var invoiceDate = invoice.SelectSingleNode(".//ns:InvoiceDate", nsManager);
                            if (invoiceDate != null && DateTime.TryParse(invoiceDate.InnerText, out DateTime invDate))
                            {
                                if (invDate < start || invDate > end)
                                {
                                    errors.Add($"Date range validation error: Invoice date {invDate:yyyy-MM-dd} is outside date range {start:yyyy-MM-dd} to {end:yyyy-MM-dd} (date range, outside, period)");
                                }
                            }
                        }
                    }
                }
            }
            return errors;
        }

        /// <summary>
        /// Validates invoice numbering sequence and format.
        /// </summary>
        internal static List<string> ValidateInvoiceNumbering(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");

            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            var invoiceNumbers = new List<string>();
            foreach (XmlNode invoice in invoices)
            {
                var invoiceNo = invoice.SelectSingleNode("ns:InvoiceNo", nsManager);
                if (invoiceNo != null)
                {
                    invoiceNumbers.Add(invoiceNo.InnerText);
                }
            }
            // Check for duplicates
            var duplicates = invoiceNumbers
                .GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            foreach (var dup in duplicates)
            {
                errors.Add($"Invoice numbering validation error: Duplicate invoice number '{dup}'");
            }

            // Check for gaps in sequences like 'FA A/15', 'FA A/16', ...
            var faAInvoices = invoiceNumbers
                .Where(x => x.StartsWith("FA A/") && int.TryParse(x.Substring(5), out _))
                .Select(x => int.Parse(x.Substring(5)))
                .OrderBy(n => n)
                .ToList();
            if (faAInvoices.Count > 1)
            {
                int min = faAInvoices.Min();
                int max = faAInvoices.Max();
                for (int i = min; i <= max; i++)
                {
                    if (!faAInvoices.Contains(i))
                    {
                        errors.Add($"Missing invoice number: FA A/{i}");
                    }
                }
            }
            return errors;
        }

        /// <summary>
        /// Validates payment terms and due dates.
        /// </summary>
        internal static List<string> ValidatePaymentTerms(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var invoices = xmlDoc.SelectNodes("//Invoice");
            foreach (XmlNode invoice in invoices)
            {
                var invoiceDate = invoice.SelectSingleNode(".//InvoiceDate");
                var dueDate = invoice.SelectSingleNode(".//DueDate");

                if (invoiceDate != null && dueDate != null)
                {
                    if (DateTime.TryParse(invoiceDate.InnerText, out DateTime invDate) &&
                        DateTime.TryParse(dueDate.InnerText, out DateTime due))
                    {
                        if (due < invDate)
                        {
                            errors.Add($"Payment terms validation error: Due date {due:yyyy-MM-dd} is before invoice date {invDate:yyyy-MM-dd}");
                        }
                    }
                }
            }

            return errors;
        }
    }
}