using System;
using System.Collections.Generic;
using System.Xml;

namespace SAFT.Lib.Validation
{
    public static partial class SchemaValidator
    {
        /// <summary>
        /// Validates document totals consistency.
        /// </summary>
        internal static List<string> ValidateDocumentTotals(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            System.Console.WriteLine($"[DEBUG] Found {invoices.Count} invoices");
            foreach (XmlNode invoice in invoices)
            {
                var lines = invoice.SelectNodes(".//ns:Line", nsManager);
                var totals = invoice.SelectSingleNode(".//ns:DocumentTotals", nsManager);
                System.Console.WriteLine($"[DEBUG] Invoice: Found {lines.Count} lines");
                if (lines != null && totals != null)
                {
                    decimal calculatedNetTotal = 0;
                    decimal calculatedTaxPayable = 0;
                    foreach (XmlNode line in lines)
                    {
                        var lineExtensionAmount = line.SelectSingleNode(".//ns:LineExtensionAmount", nsManager);
                        var creditAmount = line.SelectSingleNode(".//ns:CreditAmount", nsManager);
                        var tax = line.SelectSingleNode(".//ns:Tax", nsManager);

                        decimal lineAmount = 0;
                        if (lineExtensionAmount != null && decimal.TryParse(lineExtensionAmount.InnerText, out decimal le))
                        {
                            // Use LineExtensionAmount if available
                        }
                        else if (creditAmount != null && decimal.TryParse(creditAmount.InnerText, out decimal ca))
                        {
                            // Use CreditAmount as fallback
                        }

                        if (lineAmount > 0)
                        {
                            calculatedNetTotal += lineAmount;
                            if (tax != null)
                            {
                                var taxAmount = tax.SelectSingleNode(".//ns:TaxAmount", nsManager);
                                if (taxAmount != null && decimal.TryParse(taxAmount.InnerText, out decimal taxValue))
                                {
                                    calculatedTaxPayable += taxValue;
                                }
                            }
                        }
                    }
                    var netTotal = totals.SelectSingleNode(".//ns:NetTotal", nsManager);
                    var taxPayable = totals.SelectSingleNode(".//ns:TaxPayable", nsManager);
                    var grossTotal = totals.SelectSingleNode(".//ns:GrossTotal", nsManager);
                    System.Console.WriteLine($"[DEBUG] Calculated NetTotal: {calculatedNetTotal}, TaxPayable: {calculatedTaxPayable}");
                    if (netTotal != null && taxPayable != null && grossTotal != null)
                    {
                        if (decimal.TryParse(netTotal.InnerText, out decimal docNetTotal) &&
                            decimal.TryParse(taxPayable.InnerText, out decimal docTaxPayable) &&
                            decimal.TryParse(grossTotal.InnerText, out decimal docGrossTotal))
                        {
                            System.Console.WriteLine($"[DEBUG] Document NetTotal: {docNetTotal}, TaxPayable: {docTaxPayable}, GrossTotal: {docGrossTotal}");
                            if (Math.Abs(docNetTotal - calculatedNetTotal) >= 0.01m)
                            {
                                errors.Add($"Document totals consistency error: Net total mismatch (document totals, consistency, mismatch)");
                            }
                            if (Math.Abs(docTaxPayable - calculatedTaxPayable) >= 0.01m)
                            {
                                errors.Add($"Document totals consistency error: Tax payable mismatch (document totals, consistency, mismatch)");
                            }
                            var expectedGross = calculatedNetTotal + calculatedTaxPayable;
                            if (Math.Abs(docGrossTotal - expectedGross) >= 0.01m)
                            {
                                errors.Add($"Document totals consistency error: Gross total mismatch (document totals, consistency, mismatch)");
                            }
                        }
                    }
                }
            }
            return errors;
        }

        /// <summary>
        /// Validates credit/debit balance in general ledger entries.
        /// </summary>
        internal static List<string> ValidateCreditDebitBalance(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");

            var transactions = xmlDoc.SelectNodes("//ns:Transaction", nsManager);
            foreach (XmlNode transaction in transactions)
            {
                var creditLines = transaction.SelectNodes(".//ns:CreditLine", nsManager);
                var debitLines = transaction.SelectNodes(".//ns:DebitLine", nsManager);
                if (creditLines != null || debitLines != null)
                {
                    decimal totalDebits = 0;
                    decimal totalCredits = 0;

                    if (creditLines != null)
                    {
                        foreach (XmlNode line in creditLines)
                        {
                            var creditAmount = line.SelectSingleNode(".//ns:CreditAmount", nsManager);
                            if (creditAmount != null && decimal.TryParse(creditAmount.InnerText, out decimal credit))
                            {
                                totalCredits += credit;
                            }
                        }
                    }

                    if (debitLines != null)
                    {
                        foreach (XmlNode line in debitLines)
                        {
                            var debitAmount = line.SelectSingleNode(".//ns:DebitAmount", nsManager);
                            if (debitAmount != null && decimal.TryParse(debitAmount.InnerText, out decimal debit))
                            {
                                totalDebits += debit;
                            }
                        }
                    }

                    if (Math.Abs(totalDebits - totalCredits) >= 0.01m)
                    {
                        errors.Add($"Credit/debit balance validation error: Debits ({totalDebits}) do not equal credits ({totalCredits}) (credit/debit balance, balanced, mismatch)");
                    }
                }
            }
            return errors;
        }

        /// <summary>
        /// Validates quantity and unit price calculations.
        /// </summary>
        internal static List<string> ValidateQuantityAndUnitPrice(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            // Support both <Line> and <InvoiceLine>
            var lines = xmlDoc.SelectNodes("//Line");
            var invoiceLines = xmlDoc.SelectNodes("//InvoiceLine");
            var allLines = new List<XmlNode>();
            foreach (XmlNode l in lines) allLines.Add(l);
            foreach (XmlNode l in invoiceLines) allLines.Add(l);
            foreach (XmlNode line in allLines)
            {
                var quantity = line.SelectSingleNode(".//Quantity");
                var unitPrice = line.SelectSingleNode(".//UnitPrice");
                var lineExtensionAmount = line.SelectSingleNode(".//LineExtensionAmount");
                if (quantity != null && unitPrice != null && lineExtensionAmount != null)
                {
                    if (decimal.TryParse(quantity.InnerText, out decimal qty) &&
                        decimal.TryParse(unitPrice.InnerText, out decimal price) &&
                        decimal.TryParse(lineExtensionAmount.InnerText, out decimal amount))
                    {
                        var expectedAmount = qty * price;
                        if (Math.Abs(amount - expectedAmount) >= 0.01m)
                        {
                            errors.Add($"Quantity and unit price validation error: Expected {expectedAmount}, got {amount}");
                        }
                    }
                }
            }
            return errors;
        }

        /// <summary>
        /// Validates tax calculation accuracy and rounding.
        /// </summary>
        internal static List<string> ValidateTaxCalculationAccuracy(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");

            // Support both <Line> and <InvoiceLine>
            var lines = xmlDoc.SelectNodes("//ns:Line", nsManager);
            var invoiceLines = xmlDoc.SelectNodes("//ns:InvoiceLine", nsManager);
            var allLines = new List<XmlNode>();
            foreach (XmlNode l in lines) allLines.Add(l);
            foreach (XmlNode l in invoiceLines) allLines.Add(l);
            foreach (XmlNode line in allLines)
            {
                var lineExtensionAmount = line.SelectSingleNode(".//ns:LineExtensionAmount", nsManager);
                var taxBase = line.SelectSingleNode(".//ns:TaxBase", nsManager);
                var creditAmount = line.SelectSingleNode(".//ns:CreditAmount", nsManager);
                var tax = line.SelectSingleNode(".//ns:Tax", nsManager);
                if (tax != null)
                {
                    decimal baseAmount = 0;
                    if (lineExtensionAmount != null && decimal.TryParse(lineExtensionAmount.InnerText, out decimal lineAmount))
                    {
                        baseAmount = lineAmount;
                    }
                    else if (taxBase != null && decimal.TryParse(taxBase.InnerText, out decimal baseValue))
                    {
                        baseAmount = baseValue;
                    }
                    else if (creditAmount != null && decimal.TryParse(creditAmount.InnerText, out decimal creditValue))
                    {
                        baseAmount = creditValue;
                    }
                    var taxPercentage = tax.SelectSingleNode(".//ns:TaxPercentage", nsManager);
                    var taxAmount = tax.SelectSingleNode(".//ns:TaxAmount", nsManager);
                    if (baseAmount > 0 && taxPercentage != null && taxAmount != null)
                    {
                        if (decimal.TryParse(taxPercentage.InnerText, out decimal percentage) &&
                            decimal.TryParse(taxAmount.InnerText, out decimal amount))
                        {
                            var expectedTax = Math.Round(baseAmount * percentage / 100, 2);
                            if (Math.Abs(amount - expectedTax) >= 0.01m)
                            {
                                errors.Add($"Tax calculation accuracy error: Expected tax amount {expectedTax} for base {baseAmount} at {percentage}%, but got {amount} (tax calculation, rounding, accuracy)");
                            }
                        }
                    }
                }
            }
            return errors;
        }
    }
}
