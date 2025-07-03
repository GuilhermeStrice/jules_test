using System;
using System.Collections.Generic;
using System.Xml;
using SAFT.Lib.Utils;

namespace SAFT.Lib.Validation
{
    public static partial class SchemaValidator
    {
        /// <summary>
        /// Validates VAT calculations in invoice lines and totals.
        /// </summary>
        internal static List<string> ValidateVATCalculations(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            var documentTotals = xmlDoc.SelectSingleNode("//ns:DocumentTotals", nsManager);
            var root = xmlDoc.DocumentElement?.Name;
            if (documentTotals != null && documentTotals.HasChildNodes && root != "Invoice")
            {
                foreach (XmlNode invoice in xmlDoc.SelectNodes("//ns:Invoice", nsManager))
                {
                    // Support both <Line> and <InvoiceLine>
                    var lines = invoice.SelectNodes(".//ns:Line", nsManager);
                    var invoiceLines = invoice.SelectNodes(".//ns:InvoiceLine", nsManager);
                    var allLines = new List<XmlNode>();
                    foreach (XmlNode l in lines) allLines.Add(l);
                    foreach (XmlNode l in invoiceLines) allLines.Add(l);
                    var totals = invoice.SelectSingleNode(".//ns:DocumentTotals", nsManager);
                    if (allLines.Count > 0 && totals != null)
                    {
                        decimal calculatedTaxPayable = 0;
                        decimal calculatedNetTotal = 0;
                        foreach (XmlNode line in allLines)
                        {
                            var taxBase = line.SelectSingleNode(".//ns:TaxBase", nsManager);
                            var lineExtensionAmount = line.SelectSingleNode(".//ns:LineExtensionAmount", nsManager);
                            var creditAmount = line.SelectSingleNode(".//ns:CreditAmount", nsManager);
                            var tax = line.SelectSingleNode(".//ns:Tax", nsManager);
                            decimal baseAmount = 0;
                            if (taxBase != null && decimal.TryParse(taxBase.InnerText, out decimal tb))
                                baseAmount = tb;
                            else if (lineExtensionAmount != null && decimal.TryParse(lineExtensionAmount.InnerText, out decimal le))
                                baseAmount = le;
                            else if (creditAmount != null && decimal.TryParse(creditAmount.InnerText, out decimal ca))
                                baseAmount = ca;
                            if (baseAmount > 0 && tax != null)
                            {
                                calculatedNetTotal += baseAmount;
                                var taxPercentage = tax.SelectSingleNode(".//ns:TaxPercentage", nsManager);
                                var taxAmount = tax.SelectSingleNode(".//ns:TaxAmount", nsManager);
                                if (taxPercentage != null && taxAmount != null)
                                {
                                    if (decimal.TryParse(taxPercentage.InnerText, out decimal percentage) && decimal.TryParse(taxAmount.InnerText, out decimal amount))
                                    {
                                        var expectedTax = Math.Round(baseAmount * percentage / 100, 2);
                                        if (Math.Abs(amount - expectedTax) >= 0.01m)
                                        {
                                            errors.Add($"VAT calculation error: Expected tax amount {expectedTax} for base {baseAmount} at {percentage}%, but got {amount}");
                                        }
                                        calculatedTaxPayable += amount;
                                    }
                                }
                            }
                        }
                        var netTotal = totals.SelectSingleNode(".//ns:NetTotal", nsManager);
                        var taxPayable = totals.SelectSingleNode(".//ns:TaxPayable", nsManager);
                        var grossTotal = totals.SelectSingleNode(".//ns:GrossTotal", nsManager);
                        if (netTotal != null && taxPayable != null && grossTotal != null)
                        {
                            if (decimal.TryParse(netTotal.InnerText, out decimal docNetTotal) &&
                                decimal.TryParse(taxPayable.InnerText, out decimal docTaxPayable) &&
                                decimal.TryParse(grossTotal.InnerText, out decimal docGrossTotal))
                            {
                                if (Math.Abs(docNetTotal - calculatedNetTotal) >= 0.01m)
                                {
                                    errors.Add($"VAT calculation error: Net total mismatch (tax calculation)");
                                }
                                if (Math.Abs(docTaxPayable - calculatedTaxPayable) >= 0.01m)
                                {
                                    errors.Add($"VAT calculation error: Tax payable mismatch (tax calculation)");
                                }
                                var expectedGross = calculatedNetTotal + calculatedTaxPayable;
                                if (Math.Abs(docGrossTotal - expectedGross) >= 0.01m)
                                {
                                    errors.Add($"VAT calculation error: Gross total mismatch (tax calculation)");
                                }
                            }
                        }
                    }
                }
            }
            // Always run line-level tax accuracy checks (handled in ValidateTaxCalculationAccuracy)
            return errors;
        }

        private static void ValidatePortugueseVATCalculations(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var invoiceLines = xmlDoc.SelectNodes("//ns:InvoiceLine", nsManager);
            if (invoiceLines == null) return;
            foreach (XmlNode line in invoiceLines)
            {
                var netAmount = decimal.TryParse(line.SelectSingleNode("ns:CreditAmount", nsManager)?.InnerText, out var net) ? net : 0m;
                var vatRate = decimal.TryParse(line.SelectSingleNode("ns:TaxPercentage", nsManager)?.InnerText, out var rate) ? rate / 100m : 0m;
                var vatAmount = decimal.TryParse(line.SelectSingleNode("ns:TaxAmount", nsManager)?.InnerText, out var vat) ? vat : 0m;
                if (netAmount > 0 && vatRate > 0)
                {
                    if (!PortugueseUtils.ValidateVATCalculation(netAmount, vatRate, vatAmount))
                        errors.Add($"VAT calculation error: Net={netAmount}, Rate={vatRate}, VAT={vatAmount}");
                }
            }
        }

        private static void ValidateVATExemptions(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var lines = xmlDoc.SelectNodes("//ns:Line", nsManager);
            if (lines == null) return;
            foreach (XmlNode line in lines)
            {
                var tax = line.SelectSingleNode("ns:Tax", nsManager);
                if (tax != null)
                {
                    var taxPercentage = tax.SelectSingleNode("ns:TaxPercentage", nsManager)?.InnerText;
                    var exemptionReason = line.SelectSingleNode("ns:TaxExemptionReason", nsManager)?.InnerText;
                    var exemptionCode = line.SelectSingleNode("ns:TaxExemptionCode", nsManager)?.InnerText;
                    if (decimal.TryParse(taxPercentage, out decimal percentage) && percentage == 0)
                    {
                        if (string.IsNullOrEmpty(exemptionReason))
                            errors.Add("Tax exemption reason is required for 0% VAT");
                        if (string.IsNullOrEmpty(exemptionCode))
                            errors.Add("Tax exemption code is required for 0% VAT");
                    }
                }
            }
        }
    }
}
