using System.Collections.Generic;
using System.Linq;
using SAFT.Lib;
using SAFT.Lib.Files;
using SAFT.Lib.Documents;
using SAFT.Lib.GeneralLedger;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Text.Json;

namespace SAFT.Validation
{
    public static class CrossReferenceValidator
    {
        // Configurable valid values (could be loaded from config in future)
        public static HashSet<string> ValidUnits { get; set; } = new HashSet<string> { "UN", "KG", "L", "M", "H", "TON", "G", "ML", "CM", "MM", "BOX", "PACK" };
        public static HashSet<string> ValidCurrencies { get; set; } = new HashSet<string> { "EUR", "USD", "GBP", "JPY", "CHF", "BRL", "AOA", "MZN", "CVE" };
        public static HashSet<string> ValidWithholdingTaxTypes { get; set; } = new HashSet<string> { "IRS", "IRC", "IS" };
        // Custom error messages (could be loaded from resource file for localization)
        public static Dictionary<string, string> ErrorMessages { get; set; } = new Dictionary<string, string>
        {
            { "InvalidUnit", "UnitOfMeasure '{0}' is not valid." },
            { "InvalidCurrency", "CurrencyCode '{0}' is not a valid ISO 4217 code." },
            { "DuplicateDocumentNumber", "Duplicate {0} '{1}' found." },
            { "MissingReference", "{0} '{1}' does not exist in MasterFiles." },
            { "InvalidTaxonomyCode", "TaxonomyCode '{0}' is not in the valid range (1-999)." },
            { "InvalidWithholdingTaxType", "WithholdingTaxType '{0}' is not valid." },
            { "DuplicateSerial", "Duplicate ProductSerialNumber '{0}' in {1} '{2}'." },
            { "InvalidAccountID", "AccountID '{0}' does not exist in MasterFiles." },
            { "InvalidOrderReference", "OriginatingON '{0}' does not match any InvoiceNo." },
            // ... more codes as needed
        };
        // Selective/configurable validation (structure only)
        public static HashSet<string> EnabledValidations { get; set; } = new HashSet<string>
        {
            "UnitOfMeasure", "Currency", "DocumentNumberUniqueness", "SourceDocumentID", "OrderReferences", "ProductSerialNumber", "AccountID", "TaxonomyCode", "WithholdingTaxType"
        };
        // Plugin extensibility (structure only)
        public static List<Func<AuditFile, List<SAFTValidationResult>>> PluginValidators { get; set; } = new();

        public static List<SAFTValidationResult> ValidateReferences(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            if (auditFile == null)
                return results;

            // Always check required fields first
            results.AddRange(ValidateRequiredFields(auditFile));
            // If any required top-level fields are missing, return immediately
            if (results.Any(r => r.Type == SAFTValidationType.RequiredField))
                return results;

            results.AddRange(ValidateCustomerAndProductReferences(auditFile));
            results.AddRange(ValidateAccountReferences(auditFile));
            results.AddRange(ValidateTaxTableReferences(auditFile));
            results.AddRange(ValidateWithholdingTaxReferences(auditFile));
            results.AddRange(ValidateCurrencyCodes(auditFile));
            results.AddRange(ValidateSourceDocumentIDReferences(auditFile));
            results.AddRange(ValidateDocumentNumberUniqueness(auditFile));
            results.AddRange(ValidateUnitOfMeasure(auditFile));
            results.AddRange(ValidateOrderReferences(auditFile));
            results.AddRange(ValidateProductSerialNumbers(auditFile));
            results.AddRange(ValidateAccountIDInSupplierCustomer(auditFile));
            results.AddRange(ValidateTaxonomyCode(auditFile));
            results.AddRange(ValidateDateConsistency(auditFile));
            results.AddRange(ValidateAmountConsistency(auditFile));
            results.AddRange(ValidateEnumValues(auditFile));
            results.AddRange(ValidateDuplicateMasterData(auditFile));
            results.AddRange(ValidateNestedReferenceIntegrity(auditFile));
            results.AddRange(ValidateBusinessRules(auditFile));
            // Run plugin validators
            foreach (var plugin in PluginValidators)
                results.AddRange(plugin(auditFile));
            return results;
        }

        private static List<SAFTValidationResult> ValidateCustomerAndProductReferences(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            if (auditFile.MasterFiles == null || auditFile.SourceDocuments == null)
                return results;
            var customerDict = auditFile.MasterFiles.Customers?.ToDictionary(c => c.CustomerID) ?? new Dictionary<string, Customer>();
            var supplierDict = auditFile.MasterFiles.Suppliers?.ToDictionary(s => s.SupplierID) ?? new Dictionary<string, Supplier>();
            var productDict = auditFile.MasterFiles.Products?.ToDictionary(p => p.ProductCode) ?? new Dictionary<string, Product>();

            // Invoices
            var invoices = auditFile.SourceDocuments.SalesInvoices?.Invoices ?? new List<Invoice>();
            foreach (var invoice in invoices)
            {
                if (!string.IsNullOrWhiteSpace(invoice.CustomerID) && !customerDict.ContainsKey(invoice.CustomerID))
                {
                    results.Add(new SAFTValidationResult { Field = nameof(invoice.CustomerID), Message = $"CustomerID '{invoice.CustomerID}' does not exist in MasterFiles.", Section = "Invoice" });
                }
                foreach (var line in invoice.Lines ?? new List<InvoiceLine>())
                {
                    if (!string.IsNullOrWhiteSpace(line.ProductCode) && !productDict.ContainsKey(line.ProductCode))
                    {
                        results.Add(new SAFTValidationResult { Field = "Line.ProductCode", Message = $"ProductCode '{line.ProductCode}' does not exist in MasterFiles.", Section = "Invoice" });
                    }
                }
            }

            // Payments
            var payments = auditFile.SourceDocuments.Payments?.PaymentList ?? new List<Payment>();
            foreach (var payment in payments)
            {
                if (!string.IsNullOrWhiteSpace(payment.CustomerID) && !customerDict.ContainsKey(payment.CustomerID))
                {
                    results.Add(new SAFTValidationResult { Field = nameof(payment.CustomerID), Message = $"CustomerID '{payment.CustomerID}' does not exist in MasterFiles.", Section = "Payment" });
                }
            }

            // Stock Movements
            var stockMovements = auditFile.SourceDocuments.MovementOfGoods?.StockMovements ?? new List<StockMovement>();
            foreach (var movement in stockMovements)
            {
                if (!string.IsNullOrWhiteSpace(movement.CustomerID) && !customerDict.ContainsKey(movement.CustomerID))
                {
                    results.Add(new SAFTValidationResult { Field = nameof(movement.CustomerID), Message = $"CustomerID '{movement.CustomerID}' does not exist in MasterFiles.", Section = "StockMovement" });
                }
                if (!string.IsNullOrWhiteSpace(movement.SupplierID) && !supplierDict.ContainsKey(movement.SupplierID))
                {
                    results.Add(new SAFTValidationResult { Field = nameof(movement.SupplierID), Message = $"SupplierID '{movement.SupplierID}' does not exist in MasterFiles.", Section = "StockMovement" });
                }
                foreach (var line in movement.Lines ?? new List<StockMovementLine>())
                {
                    if (!string.IsNullOrWhiteSpace(line.ProductCode) && !productDict.ContainsKey(line.ProductCode))
                    {
                        results.Add(new SAFTValidationResult { Field = "Line.ProductCode", Message = $"ProductCode '{line.ProductCode}' does not exist in MasterFiles.", Section = "StockMovement" });
                    }
                }
            }

            // Work Documents
            var workDocuments = auditFile.SourceDocuments.WorkingDocuments?.WorkDocuments ?? new List<WorkDocument>();
            foreach (var doc in workDocuments)
            {
                if (!string.IsNullOrWhiteSpace(doc.CustomerID) && !customerDict.ContainsKey(doc.CustomerID))
                {
                    results.Add(new SAFTValidationResult { Field = nameof(doc.CustomerID), Message = $"CustomerID '{doc.CustomerID}' does not exist in MasterFiles.", Section = "WorkDocument" });
                }
                foreach (var line in doc.Lines ?? new List<WorkDocumentLine>())
                {
                    if (!string.IsNullOrWhiteSpace(line.ProductCode) && !productDict.ContainsKey(line.ProductCode))
                    {
                        results.Add(new SAFTValidationResult { Field = "Line.ProductCode", Message = $"ProductCode '{line.ProductCode}' does not exist in MasterFiles.", Section = "WorkDocument" });
                    }
                }
            }
            return results;
        }

        private static List<SAFTValidationResult> ValidateAccountReferences(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            var accountDict = auditFile.MasterFiles.GeneralLedgerAccounts?.ToDictionary(a => a.AccountID) ?? new Dictionary<string, Account>();
            var glEntries = auditFile.GeneralLedgerEntries?.Journals ?? new List<Journal>();
            foreach (var journal in glEntries)
            {
                foreach (var transaction in journal.Transactions ?? new List<Transaction>())
                {
                    var lines = transaction.Lines;
                    foreach (var line in lines?.DebitLines ?? new List<Line>())
                    {
                        if (!string.IsNullOrWhiteSpace(line.AccountID) && !accountDict.ContainsKey(line.AccountID))
                        {
                            results.Add(new SAFTValidationResult { Field = "DebitLine.AccountID", Message = $"AccountID '{line.AccountID}' does not exist in MasterFiles.", Section = "GeneralLedgerEntries" });
                        }
                    }
                    foreach (var line in lines?.CreditLines ?? new List<Line>())
                    {
                        if (!string.IsNullOrWhiteSpace(line.AccountID) && !accountDict.ContainsKey(line.AccountID))
                        {
                            results.Add(new SAFTValidationResult { Field = "CreditLine.AccountID", Message = $"AccountID '{line.AccountID}' does not exist in MasterFiles.", Section = "GeneralLedgerEntries" });
                        }
                    }
                }
            }
            return results;
        }

        private static List<SAFTValidationResult> ValidateTaxTableReferences(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            if (auditFile.MasterFiles == null || auditFile.SourceDocuments == null)
                return results;
            var taxTable = auditFile.MasterFiles.TaxTable?.TaxTableEntries ?? new List<TaxTableEntry>();
            bool TaxExists(string type, string region, string code) =>
                taxTable.Any(t => t.TaxType == type && t.TaxCountryRegion == region && t.TaxCode == code);

            // Invoices
            var invoices = auditFile.SourceDocuments.SalesInvoices?.Invoices ?? new List<Invoice>();
            foreach (var invoice in invoices)
            {
                foreach (var line in invoice.Lines ?? new List<InvoiceLine>())
                {
                    var tax = line.Tax;
                    if (tax != null && (!TaxExists(tax.TaxType, tax.TaxCountryRegion, tax.TaxCode)))
                    {
                        results.Add(new SAFTValidationResult { Field = "Line.Tax", Message = $"Tax (Type: '{tax.TaxType}', Region: '{tax.TaxCountryRegion}', Code: '{tax.TaxCode}') does not exist in TaxTable.", Section = "Invoice" });
                    }
                }
            }
            // Stock Movements
            var stockMovements = auditFile.SourceDocuments.MovementOfGoods?.StockMovements ?? new List<StockMovement>();
            foreach (var movement in stockMovements)
            {
                foreach (var line in movement.Lines ?? new List<StockMovementLine>())
                {
                    var tax = line.Tax;
                    if (tax != null && (!TaxExists(tax.TaxType, tax.TaxCountryRegion, tax.TaxCode)))
                    {
                        results.Add(new SAFTValidationResult { Field = "Line.Tax", Message = $"Tax (Type: '{tax.TaxType}', Region: '{tax.TaxCountryRegion}', Code: '{tax.TaxCode}') does not exist in TaxTable.", Section = "StockMovement" });
                    }
                }
            }
            // Work Documents
            var workDocuments = auditFile.SourceDocuments.WorkingDocuments?.WorkDocuments ?? new List<WorkDocument>();
            foreach (var doc in workDocuments)
            {
                foreach (var line in doc.Lines ?? new List<WorkDocumentLine>())
                {
                    var tax = line.Tax;
                    if (tax != null && (!TaxExists(tax.TaxType, tax.TaxCountryRegion, tax.TaxCode)))
                    {
                        results.Add(new SAFTValidationResult { Field = "Line.Tax", Message = $"Tax (Type: '{tax.TaxType}', Region: '{tax.TaxCountryRegion}', Code: '{tax.TaxCode}') does not exist in TaxTable.", Section = "WorkDocument" });
                    }
                }
            }
            // Payments
            var payments = auditFile.SourceDocuments.Payments?.PaymentList ?? new List<Payment>();
            foreach (var payment in payments)
            {
                foreach (var line in payment.Lines ?? new List<PaymentLine>())
                {
                    var tax = line.Tax;
                    if (tax != null && (!TaxExists(tax.TaxType, tax.TaxCountryRegion, tax.TaxCode)))
                    {
                        results.Add(new SAFTValidationResult { Field = "Line.Tax", Message = $"Tax (Type: '{tax.TaxType}', Region: '{tax.TaxCountryRegion}', Code: '{tax.TaxCode}') does not exist in TaxTable.", Section = "Payment" });
                    }
                }
            }
            return results;
        }

        private static List<SAFTValidationResult> ValidateWithholdingTaxReferences(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            if (auditFile.MasterFiles == null || auditFile.SourceDocuments == null)
                return results;
            var validTypes = new HashSet<string> { "IRS", "IRC", "IS" };
            var payments = auditFile.SourceDocuments.Payments?.PaymentList ?? new List<Payment>();
            foreach (var payment in payments)
            {
                var withholdings = payment.DocumentTotals?.WithholdingTaxes ?? new List<WithholdingTax>();
                foreach (var w in withholdings)
                {
                    if (!string.IsNullOrWhiteSpace(w.WithholdingTaxType) && !validTypes.Contains(w.WithholdingTaxType))
                    {
                        results.Add(new SAFTValidationResult { Field = "WithholdingTax.WithholdingTaxType", Message = $"WithholdingTaxType '{w.WithholdingTaxType}' is not valid.", Section = "Payment" });
                    }
                }
            }
            return results;
        }

        private static List<SAFTValidationResult> ValidateCurrencyCodes(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            if (auditFile.MasterFiles == null || auditFile.SourceDocuments == null)
                return results;
            // ISO 4217 currency codes (partial, can be extended)
            var validCurrencies = new HashSet<string> { "EUR", "USD", "GBP", "JPY", "CHF", "BRL", "AOA", "MZN", "CVE" };
            // Header
            if (auditFile.Header != null && !string.IsNullOrWhiteSpace(auditFile.Header.CurrencyCode) && !validCurrencies.Contains(auditFile.Header.CurrencyCode))
            {
                results.Add(new SAFTValidationResult { Field = "Header.CurrencyCode", Message = $"CurrencyCode '{auditFile.Header.CurrencyCode}' is not a valid ISO 4217 code.", Section = "Header" });
            }
            // DocumentTotals in Invoices
            var invoices = auditFile.SourceDocuments.SalesInvoices?.Invoices ?? new List<Invoice>();
            foreach (var invoice in invoices)
            {
                // If DocumentTotals has a Currency property, check it here (not present in current model)
            }
            // PaymentDocumentTotals in Payments
            var payments = auditFile.SourceDocuments.Payments?.PaymentList ?? new List<Payment>();
            foreach (var payment in payments)
            {
                var currency = payment.DocumentTotals?.Currency;
                if (currency != null && !string.IsNullOrWhiteSpace(currency.CurrencyCode) && !validCurrencies.Contains(currency.CurrencyCode))
                {
                    results.Add(new SAFTValidationResult { Field = "Payment.DocumentTotals.Currency.CurrencyCode", Message = $"CurrencyCode '{currency.CurrencyCode}' is not a valid ISO 4217 code.", Section = "Payment" });
                }
            }
            return results;
        }

        private static List<SAFTValidationResult> ValidateSourceDocumentIDReferences(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            if (auditFile.SourceDocuments == null)
                return results;
            // Collect all InvoiceNo for reference
            var invoiceNos = new HashSet<string>(auditFile.SourceDocuments.SalesInvoices?.Invoices?.Select(i => i.InvoiceNo) ?? new List<string>());
            // Payments referencing invoices
            var payments = auditFile.SourceDocuments.Payments?.PaymentList ?? new List<Payment>();
            foreach (var payment in payments)
            {
                foreach (var line in payment.Lines ?? new List<PaymentLine>())
                {
                    foreach (var srcDoc in line.SourceDocumentIDs ?? new List<SourceDocumentID>())
                    {
                        if (!string.IsNullOrWhiteSpace(srcDoc.OriginatingON) && !invoiceNos.Contains(srcDoc.OriginatingON))
                        {
                            results.Add(new SAFTValidationResult { Field = "PaymentLine.SourceDocumentID.OriginatingON", Message = $"OriginatingON '{srcDoc.OriginatingON}' does not match any InvoiceNo.", Section = "Payment" });
                        }
                    }
                }
            }
            return results;
        }

        private static List<SAFTValidationResult> ValidateDocumentNumberUniqueness(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            if (auditFile.SourceDocuments == null)
                return results;
            // Invoices
            var invoices = auditFile.SourceDocuments.SalesInvoices?.Invoices ?? new List<Invoice>();
            var invoiceNos = new HashSet<string>();
            foreach (var invoice in invoices)
            {
                if (!string.IsNullOrWhiteSpace(invoice.InvoiceNo))
                {
                    if (!invoiceNos.Add(invoice.InvoiceNo))
                    {
                        results.Add(new SAFTValidationResult { Field = "Invoice.InvoiceNo", Message = $"Duplicate InvoiceNo '{invoice.InvoiceNo}' found.", Section = "Invoice" });
                    }
                }
            }
            // Stock Movements
            var stockMovements = auditFile.SourceDocuments.MovementOfGoods?.StockMovements ?? new List<StockMovement>();
            var stockDocNos = new HashSet<string>();
            foreach (var movement in stockMovements)
            {
                if (!string.IsNullOrWhiteSpace(movement.DocumentNumber))
                {
                    if (!stockDocNos.Add(movement.DocumentNumber))
                    {
                        results.Add(new SAFTValidationResult { Field = "StockMovement.DocumentNumber", Message = $"Duplicate DocumentNumber '{movement.DocumentNumber}' found.", Section = "StockMovement" });
                    }
                }
            }
            // Work Documents
            var workDocuments = auditFile.SourceDocuments.WorkingDocuments?.WorkDocuments ?? new List<WorkDocument>();
            var workDocNos = new HashSet<string>();
            foreach (var doc in workDocuments)
            {
                if (!string.IsNullOrWhiteSpace(doc.DocumentNumber))
                {
                    if (!workDocNos.Add(doc.DocumentNumber))
                    {
                        results.Add(new SAFTValidationResult { Field = "WorkDocument.DocumentNumber", Message = $"Duplicate DocumentNumber '{doc.DocumentNumber}' found.", Section = "WorkDocument" });
                    }
                }
            }
            // Payments
            var payments = auditFile.SourceDocuments.Payments?.PaymentList ?? new List<Payment>();
            var paymentRefNos = new HashSet<string>();
            foreach (var payment in payments)
            {
                if (!string.IsNullOrWhiteSpace(payment.PaymentRefNo))
                {
                    if (!paymentRefNos.Add(payment.PaymentRefNo))
                    {
                        results.Add(new SAFTValidationResult { Field = "Payment.PaymentRefNo", Message = $"Duplicate PaymentRefNo '{payment.PaymentRefNo}' found.", Section = "Payment" });
                    }
                }
            }
            return results;
        }

        private static List<SAFTValidationResult> ValidateUnitOfMeasure(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            if (auditFile.SourceDocuments == null)
                return results;
            if (!EnabledValidations.Contains("UnitOfMeasure")) return results;
            var validUnits = ValidUnits;
            var invoices = auditFile.SourceDocuments.SalesInvoices?.Invoices ?? new List<Invoice>();
            foreach (var invoice in invoices)
            {
                foreach (var line in invoice.Lines ?? new List<InvoiceLine>())
                {
                    if (!string.IsNullOrWhiteSpace(line.UnitOfMeasure) && !validUnits.Contains(line.UnitOfMeasure))
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "InvoiceLine.UnitOfMeasure",
                            Message = string.Format(ErrorMessages["InvalidUnit"], line.UnitOfMeasure),
                            MessageCode = "InvalidUnit",
                            Section = "Invoice",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = invoice.InvoiceNo,
                            LineNumber = line.LineNumber
                        });
                    }
                }
            }
            // StockMovementLines
            var stockMovements = auditFile.SourceDocuments.MovementOfGoods?.StockMovements ?? new List<StockMovement>();
            foreach (var movement in stockMovements)
            {
                foreach (var line in movement.Lines ?? new List<StockMovementLine>())
                {
                    if (!string.IsNullOrWhiteSpace(line.UnitOfMeasure) && !validUnits.Contains(line.UnitOfMeasure))
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "StockMovementLine.UnitOfMeasure",
                            Message = string.Format(ErrorMessages["InvalidUnit"], line.UnitOfMeasure),
                            MessageCode = "InvalidUnit",
                            Section = "StockMovement",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = movement.DocumentNumber,
                            LineNumber = line.LineNumber
                        });
                    }
                }
            }
            // WorkDocumentLines
            var workDocuments = auditFile.SourceDocuments.WorkingDocuments?.WorkDocuments ?? new List<WorkDocument>();
            foreach (var doc in workDocuments)
            {
                foreach (var line in doc.Lines ?? new List<WorkDocumentLine>())
                {
                    if (!string.IsNullOrWhiteSpace(line.UnitOfMeasure) && !validUnits.Contains(line.UnitOfMeasure))
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "WorkDocumentLine.UnitOfMeasure",
                            Message = string.Format(ErrorMessages["InvalidUnit"], line.UnitOfMeasure),
                            MessageCode = "InvalidUnit",
                            Section = "WorkDocument",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = doc.DocumentNumber,
                            LineNumber = line.LineNumber
                        });
                    }
                }
            }
            return results;
        }

        private static List<SAFTValidationResult> ValidateOrderReferences(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            if (auditFile.SourceDocuments == null)
                return results;
            // Collect all InvoiceNo for reference
            var invoiceNos = new HashSet<string>(auditFile.SourceDocuments.SalesInvoices?.Invoices?.Select(i => i.InvoiceNo) ?? new List<string>());
            // InvoiceLines
            var invoices = auditFile.SourceDocuments.SalesInvoices?.Invoices ?? new List<Invoice>();
            foreach (var invoice in invoices)
            {
                foreach (var line in invoice.Lines ?? new List<InvoiceLine>())
                {
                    // No OrderReferences in InvoiceLine in current model
                }
            }
            // StockMovementLines
            var stockMovements = auditFile.SourceDocuments.MovementOfGoods?.StockMovements ?? new List<StockMovement>();
            foreach (var movement in stockMovements)
            {
                foreach (var line in movement.Lines ?? new List<StockMovementLine>())
                {
                    foreach (var orderRef in line.OrderReferences ?? new List<OrderReferences>())
                    {
                        if (!string.IsNullOrWhiteSpace(orderRef.OriginatingON) && !invoiceNos.Contains(orderRef.OriginatingON))
                        {
                            results.Add(new SAFTValidationResult { Field = "StockMovementLine.OrderReferences.OriginatingON", Message = $"OriginatingON '{orderRef.OriginatingON}' does not match any InvoiceNo.", Section = "StockMovement" });
                        }
                    }
                }
            }
            // WorkDocumentLines
            var workDocuments = auditFile.SourceDocuments.WorkingDocuments?.WorkDocuments ?? new List<WorkDocument>();
            foreach (var doc in workDocuments)
            {
                foreach (var line in doc.Lines ?? new List<WorkDocumentLine>())
                {
                    foreach (var orderRef in line.OrderReferences ?? new List<OrderReferences>())
                    {
                        if (!string.IsNullOrWhiteSpace(orderRef.OriginatingON) && !invoiceNos.Contains(orderRef.OriginatingON))
                        {
                            results.Add(new SAFTValidationResult { Field = "WorkDocumentLine.OrderReferences.OriginatingON", Message = $"OriginatingON '{orderRef.OriginatingON}' does not match any InvoiceNo.", Section = "WorkDocument" });
                        }
                    }
                }
            }
            return results;
        }

        private static List<SAFTValidationResult> ValidateProductSerialNumbers(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            // Check uniqueness within each document
            // StockMovementLines
            var stockMovements = auditFile.SourceDocuments.MovementOfGoods?.StockMovements ?? new List<StockMovement>();
            foreach (var movement in stockMovements)
            {
                var serials = new HashSet<string>();
                foreach (var line in movement.Lines ?? new List<StockMovementLine>())
                {
                    var serial = line.ProductSerialNumber?.ToString();
                    if (!string.IsNullOrWhiteSpace(serial))
                    {
                        if (!serials.Add(serial))
                        {
                            results.Add(new SAFTValidationResult { Field = "StockMovementLine.ProductSerialNumber", Message = $"Duplicate ProductSerialNumber '{serial}' in StockMovement '{movement.DocumentNumber}'.", Section = "StockMovement" });
                        }
                    }
                }
            }
            // WorkDocumentLines
            var workDocuments = auditFile.SourceDocuments.WorkingDocuments?.WorkDocuments ?? new List<WorkDocument>();
            foreach (var doc in workDocuments)
            {
                var serials = new HashSet<string>();
                foreach (var line in doc.Lines ?? new List<WorkDocumentLine>())
                {
                    var serial = line.ProductSerialNumber?.ToString();
                    if (!string.IsNullOrWhiteSpace(serial))
                    {
                        if (!serials.Add(serial))
                        {
                            results.Add(new SAFTValidationResult { Field = "WorkDocumentLine.ProductSerialNumber", Message = $"Duplicate ProductSerialNumber '{serial}' in WorkDocument '{doc.DocumentNumber}'.", Section = "WorkDocument" });
                        }
                    }
                }
            }
            return results;
        }

        private static List<SAFTValidationResult> ValidateAccountIDInSupplierCustomer(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            if (auditFile.MasterFiles == null)
                return results;
            var accountDict = auditFile.MasterFiles.GeneralLedgerAccounts?.ToDictionary(a => a.AccountID) ?? new Dictionary<string, Account>();
            // Customers
            var customers = auditFile.MasterFiles.Customers ?? new List<Customer>();
            foreach (var customer in customers)
            {
                if (!string.IsNullOrWhiteSpace(customer.AccountID) && !accountDict.ContainsKey(customer.AccountID))
                {
                    results.Add(new SAFTValidationResult { Field = "Customer.AccountID", Message = $"AccountID '{customer.AccountID}' in Customer does not exist in MasterFiles.", Section = "Customer" });
                }
            }
            // Suppliers
            var suppliers = auditFile.MasterFiles.Suppliers ?? new List<Supplier>();
            foreach (var supplier in suppliers)
            {
                if (!string.IsNullOrWhiteSpace(supplier.AccountID) && !accountDict.ContainsKey(supplier.AccountID))
                {
                    results.Add(new SAFTValidationResult { Field = "Supplier.AccountID", Message = $"AccountID '{supplier.AccountID}' in Supplier does not exist in MasterFiles.", Section = "Supplier" });
                }
            }
            return results;
        }

        private static List<SAFTValidationResult> ValidateTaxonomyCode(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            var accounts = auditFile.MasterFiles.GeneralLedgerAccounts ?? new List<Account>();
            foreach (var account in accounts)
            {
                if (account.TaxonomyCode.HasValue && (account.TaxonomyCode < 1 || account.TaxonomyCode > 999))
                {
                    results.Add(new SAFTValidationResult { Field = "Account.TaxonomyCode", Message = $"TaxonomyCode '{account.TaxonomyCode}' is not in the valid range (1-999).", Section = "Account" });
                }
            }
            return results;
        }

        // Advanced validation stubs
        private static List<SAFTValidationResult> ValidateDateConsistency(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            if (auditFile.SourceDocuments == null)
                return results;
            var fiscalYear = auditFile.Header?.FiscalYear;
            var now = DateTime.Now;
            // Helper to check fiscal year
            bool InFiscalYear(DateTime date) => date.Year == fiscalYear;
            // Invoices
            var invoices = auditFile.SourceDocuments.SalesInvoices?.Invoices ?? new List<Invoice>();
            foreach (var invoice in invoices)
            {
                if (invoice.InvoiceDate > now)
                {
                    results.Add(new SAFTValidationResult {
                        Field = "Invoice.InvoiceDate",
                        Message = "InvoiceDate is in the future.",
                        Section = "Invoice",
                        Severity = SAFTValidationSeverity.Error,
                        DocumentNumber = invoice.InvoiceNo,
                        Type = SAFTValidationType.Consistency
                    });
                }
                if (!InFiscalYear(invoice.InvoiceDate))
                {
                    results.Add(new SAFTValidationResult {
                        Field = "Invoice.InvoiceDate",
                        Message = $"InvoiceDate '{invoice.InvoiceDate:yyyy-MM-dd}' is not in fiscal year {fiscalYear}.",
                        Section = "Invoice",
                        Severity = SAFTValidationSeverity.Error,
                        DocumentNumber = invoice.InvoiceNo,
                        Type = SAFTValidationType.Consistency
                    });
                }
                if (invoice.SystemEntryDate < invoice.InvoiceDate)
                {
                    results.Add(new SAFTValidationResult {
                        Field = "Invoice.SystemEntryDate",
                        Message = "SystemEntryDate is before InvoiceDate.",
                        Section = "Invoice",
                        Severity = SAFTValidationSeverity.Warning,
                        DocumentNumber = invoice.InvoiceNo,
                        Type = SAFTValidationType.Consistency
                    });
                }
            }
            // Stock Movements
            var stockMovements = auditFile.SourceDocuments.MovementOfGoods?.StockMovements ?? new List<StockMovement>();
            foreach (var movement in stockMovements)
            {
                if (movement.MovementDate > now)
                {
                    results.Add(new SAFTValidationResult {
                        Field = "StockMovement.MovementDate",
                        Message = "MovementDate is in the future.",
                        Section = "StockMovement",
                        Severity = SAFTValidationSeverity.Error,
                        DocumentNumber = movement.DocumentNumber,
                        Type = SAFTValidationType.Consistency
                    });
                }
                if (!InFiscalYear(movement.MovementDate))
                {
                    results.Add(new SAFTValidationResult {
                        Field = "StockMovement.MovementDate",
                        Message = $"MovementDate '{movement.MovementDate:yyyy-MM-dd}' is not in fiscal year {fiscalYear}.",
                        Section = "StockMovement",
                        Severity = SAFTValidationSeverity.Error,
                        DocumentNumber = movement.DocumentNumber,
                        Type = SAFTValidationType.Consistency
                    });
                }
                if (movement.SystemEntryDate < movement.MovementDate)
                {
                    results.Add(new SAFTValidationResult {
                        Field = "StockMovement.SystemEntryDate",
                        Message = "SystemEntryDate is before MovementDate.",
                        Section = "StockMovement",
                        Severity = SAFTValidationSeverity.Warning,
                        DocumentNumber = movement.DocumentNumber,
                        Type = SAFTValidationType.Consistency
                    });
                }
            }
            // Work Documents
            var workDocuments = auditFile.SourceDocuments.WorkingDocuments?.WorkDocuments ?? new List<WorkDocument>();
            foreach (var doc in workDocuments)
            {
                if (doc.WorkDate > now)
                {
                    results.Add(new SAFTValidationResult {
                        Field = "WorkDocument.WorkDate",
                        Message = "WorkDate is in the future.",
                        Section = "WorkDocument",
                        Severity = SAFTValidationSeverity.Error,
                        DocumentNumber = doc.DocumentNumber,
                        Type = SAFTValidationType.Consistency
                    });
                }
                if (!InFiscalYear(doc.WorkDate))
                {
                    results.Add(new SAFTValidationResult {
                        Field = "WorkDocument.WorkDate",
                        Message = $"WorkDate '{doc.WorkDate:yyyy-MM-dd}' is not in fiscal year {fiscalYear}.",
                        Section = "WorkDocument",
                        Severity = SAFTValidationSeverity.Error,
                        DocumentNumber = doc.DocumentNumber,
                        Type = SAFTValidationType.Consistency
                    });
                }
                if (doc.SystemEntryDate < doc.WorkDate)
                {
                    results.Add(new SAFTValidationResult {
                        Field = "WorkDocument.SystemEntryDate",
                        Message = "SystemEntryDate is before WorkDate.",
                        Section = "WorkDocument",
                        Severity = SAFTValidationSeverity.Warning,
                        DocumentNumber = doc.DocumentNumber,
                        Type = SAFTValidationType.Consistency
                    });
                }
            }
            // Payments
            var payments = auditFile.SourceDocuments.Payments?.PaymentList ?? new List<Payment>();
            foreach (var payment in payments)
            {
                if (payment.TransactionDate > now)
                {
                    results.Add(new SAFTValidationResult {
                        Field = "Payment.TransactionDate",
                        Message = "TransactionDate is in the future.",
                        Section = "Payment",
                        Severity = SAFTValidationSeverity.Error,
                        DocumentNumber = payment.PaymentRefNo,
                        Type = SAFTValidationType.Consistency
                    });
                }
                if (!InFiscalYear(payment.TransactionDate))
                {
                    results.Add(new SAFTValidationResult {
                        Field = "Payment.TransactionDate",
                        Message = $"TransactionDate '{payment.TransactionDate:yyyy-MM-dd}' is not in fiscal year {fiscalYear}.",
                        Section = "Payment",
                        Severity = SAFTValidationSeverity.Error,
                        DocumentNumber = payment.PaymentRefNo,
                        Type = SAFTValidationType.Consistency
                    });
                }
                if (payment.SystemEntryDate < payment.TransactionDate)
                {
                    results.Add(new SAFTValidationResult {
                        Field = "Payment.SystemEntryDate",
                        Message = "SystemEntryDate is before TransactionDate.",
                        Section = "Payment",
                        Severity = SAFTValidationSeverity.Warning,
                        DocumentNumber = payment.PaymentRefNo,
                        Type = SAFTValidationType.Consistency
                    });
                }
            }
            return results;
        }
        private static List<SAFTValidationResult> ValidateAmountConsistency(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            if (auditFile.SourceDocuments == null)
                return results;
            // Invoices
            var invoices = auditFile.SourceDocuments.SalesInvoices?.Invoices ?? new List<Invoice>();
            foreach (var invoice in invoices)
            {
                if (invoice.Lines != null && invoice.DocumentTotals != null)
                {
                    decimal sumNet = invoice.Lines.Sum(l => (l.DebitAmount ?? 0) - (l.CreditAmount ?? 0));
                    decimal sumGross = invoice.Lines.Sum(l => (l.DebitAmount ?? 0) - (l.CreditAmount ?? 0) + (l.Tax?.TaxAmount ?? 0));
                    if (Math.Abs(invoice.DocumentTotals.NetTotal - sumNet) > 0.01m)
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "Invoice.DocumentTotals.NetTotal",
                            Message = $"NetTotal ({invoice.DocumentTotals.NetTotal}) does not match sum of lines ({sumNet}).",
                            Section = "Invoice",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = invoice.InvoiceNo,
                            Type = SAFTValidationType.Consistency
                        });
                    }
                    if (Math.Abs(invoice.DocumentTotals.GrossTotal - sumGross) > 0.01m)
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "Invoice.DocumentTotals.GrossTotal",
                            Message = $"GrossTotal ({invoice.DocumentTotals.GrossTotal}) does not match sum of lines + tax ({sumGross}).",
                            Section = "Invoice",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = invoice.InvoiceNo,
                            Type = SAFTValidationType.Consistency
                        });
                    }
                }
                // === SAF-T PT 1.04_01 <xs:assert> rules for InvoiceLine ===
                foreach (var line in invoice.Lines ?? new List<InvoiceLine>())
                {
                    // TaxBase/DebitAmount/CreditAmount assertion
                    if (line.TaxBase.HasValue)
                    {
                        if ((line.DebitAmount ?? 0) != 0 || (line.CreditAmount ?? 0) != 0)
                        {
                            results.Add(new SAFTValidationResult
                            {
                                Field = "InvoiceLine.TaxBase",
                                Message = "If TaxBase is present, DebitAmount and CreditAmount must be 0 (SAF-T <xs:assert> InvoiceLine).",
                                Section = "Invoice",
                                DocumentNumber = invoice.InvoiceNo,
                                LineNumber = line.LineNumber,
                                Type = SAFTValidationType.Consistency
                            });
                        }
                    }
                    else
                    {
                        if ((line.DebitAmount ?? 0) == 0 && (line.CreditAmount ?? 0) == 0)
                        {
                            results.Add(new SAFTValidationResult
                            {
                                Field = "InvoiceLine.TaxBase",
                                Message = "If TaxBase is not present, either DebitAmount or CreditAmount must be nonzero (SAF-T <xs:assert> InvoiceLine).",
                                Section = "Invoice",
                                DocumentNumber = invoice.InvoiceNo,
                                LineNumber = line.LineNumber,
                                Type = SAFTValidationType.Consistency
                            });
                        }
                    }
                    // TaxPercentage/TaxExemptionReason assertion
                    if (line.Tax != null && line.Tax.TaxPercentage.HasValue)
                    {
                        if (line.Tax.TaxPercentage.Value != 0 && !string.IsNullOrWhiteSpace(line.TaxExemptionReason))
                        {
                            results.Add(new SAFTValidationResult
                            {
                                Field = "InvoiceLine.TaxExemptionReason",
                                Message = "TaxExemptionReason must be missing if TaxPercentage is not 0 (SAF-T <xs:assert> InvoiceLine).",
                                Section = "Invoice",
                                DocumentNumber = invoice.InvoiceNo,
                                LineNumber = line.LineNumber,
                                Type = SAFTValidationType.Consistency
                            });
                        }
                        if (line.Tax.TaxPercentage.Value == 0 && string.IsNullOrWhiteSpace(line.TaxExemptionReason))
                        {
                            results.Add(new SAFTValidationResult
                            {
                                Field = "InvoiceLine.TaxExemptionReason",
                                Message = "TaxExemptionReason must be present if TaxPercentage is 0 (SAF-T <xs:assert> InvoiceLine).",
                                Section = "Invoice",
                                DocumentNumber = invoice.InvoiceNo,
                                LineNumber = line.LineNumber,
                                Type = SAFTValidationType.Consistency
                            });
                        }
                    }
                    // TaxBase/UnitPrice assertion
                    if (line.TaxBase.HasValue && line.UnitPrice != 0)
                    {
                        results.Add(new SAFTValidationResult
                        {
                            Field = "InvoiceLine.TaxBase/UnitPrice",
                            Message = "If TaxBase is present, UnitPrice must be 0 (SAF-T <xs:assert> InvoiceLine).",
                            Section = "Invoice",
                            DocumentNumber = invoice.InvoiceNo,
                            LineNumber = line.LineNumber,
                            Type = SAFTValidationType.Consistency
                        });
                    }

                    if (line.Tax != null && line.Tax.TaxAmount.HasValue)
                    {
                        if (line.Tax.TaxAmount.Value != 0 && !string.IsNullOrWhiteSpace(line.TaxExemptionReason))
                        {
                            results.Add(new SAFTValidationResult
                            {
                                Field = "InvoiceLine.TaxExemptionReason",
                                Message = "TaxExemptionReason must be missing if TaxAmount is not 0 (SAF-T <xs:assert> InvoiceLine).",
                                Section = "Invoice",
                                DocumentNumber = invoice.InvoiceNo,
                                LineNumber = line.LineNumber,
                                Type = SAFTValidationType.Consistency
                            });
                        }
                        if (line.Tax.TaxAmount.Value == 0 && string.IsNullOrWhiteSpace(line.TaxExemptionReason))
                        {
                            results.Add(new SAFTValidationResult
                            {
                                Field = "InvoiceLine.TaxExemptionReason",
                                Message = "TaxExemptionReason must be present if TaxAmount is 0 (SAF-T <xs:assert> InvoiceLine).",
                                Section = "Invoice",
                                DocumentNumber = invoice.InvoiceNo,
                                LineNumber = line.LineNumber,
                                Type = SAFTValidationType.Consistency
                            });
                        }
                    }
                    // TaxExemptionReason/TaxExemptionCode mutual presence assertion
                    bool hasReason = !string.IsNullOrWhiteSpace(line.TaxExemptionReason);
                    bool hasCode = !string.IsNullOrWhiteSpace(line.TaxExemptionCode);
                    if ((hasReason && !hasCode) || (hasCode && !hasReason))
                    {
                        results.Add(new SAFTValidationResult
                        {
                            Field = "InvoiceLine.TaxExemptionReason/TaxExemptionCode",
                            Message = "TaxExemptionReason and TaxExemptionCode must both be present or both be missing (SAF-T <xs:assert> InvoiceLine).",
                            Section = "Invoice",
                            DocumentNumber = invoice.InvoiceNo,
                            LineNumber = line.LineNumber,
                            Type = SAFTValidationType.Consistency
                        });
                    }
                }
            }
            // Stock Movements
            var stockMovements = auditFile.SourceDocuments.MovementOfGoods?.StockMovements ?? new List<StockMovement>();
            foreach (var movement in stockMovements)
            {
                if (movement.Lines != null && movement.DocumentTotals != null)
                {
                    decimal sumNet = movement.Lines.Sum(l => (l.DebitAmount ?? 0) - (l.CreditAmount ?? 0));
                    decimal sumGross = movement.Lines.Sum(l => (l.DebitAmount ?? 0) - (l.CreditAmount ?? 0) + (l.Tax?.TaxPercentage ?? 0));
                    if (Math.Abs(movement.DocumentTotals.NetTotal - sumNet) > 0.01m)
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "StockMovement.DocumentTotals.NetTotal",
                            Message = $"NetTotal ({movement.DocumentTotals.NetTotal}) does not match sum of lines ({sumNet}).",
                            Section = "StockMovement",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = movement.DocumentNumber,
                            Type = SAFTValidationType.Consistency
                        });
                    }
                    if (Math.Abs(movement.DocumentTotals.GrossTotal - sumGross) > 0.01m)
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "StockMovement.DocumentTotals.GrossTotal",
                            Message = $"GrossTotal ({movement.DocumentTotals.GrossTotal}) does not match sum of lines + tax ({sumGross}).",
                            Section = "StockMovement",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = movement.DocumentNumber,
                            Type = SAFTValidationType.Consistency
                        });
                    }
                }
                // === SAF-T PT 1.04_01 <xs:assert> rules for StockMovementLine ===
                foreach (var line in movement.Lines ?? new List<StockMovementLine>())
                {
                    // 1. TaxPercentage/TaxExemptionReason logic
                    // <xs:assert test="if (not(ns:Tax/ns:TaxPercentage) or ((ns:Tax/ns:TaxPercentage != 0 and not(ns:TaxExemptionReason)) or (ns:Tax/ns:TaxPercentage eq 0 and ns:TaxExemptionReason))) then true() else false()"/>
                    if (line.Tax != null)
                    {
                        if (line.Tax.TaxPercentage != 0 && !string.IsNullOrWhiteSpace(line.TaxExemptionReason))
                        {
                            results.Add(new SAFTValidationResult
                            {
                                Field = "StockMovementLine.TaxExemptionReason",
                                Message = "TaxExemptionReason must be missing if TaxPercentage is not 0 (SAF-T <xs:assert> StockMovementLine 1).",
                                Section = "StockMovement",
                                DocumentNumber = movement.DocumentNumber,
                                LineNumber = line.LineNumber,
                                Type = SAFTValidationType.Consistency
                            });
                        }
                        if (line.Tax.TaxPercentage == 0 && string.IsNullOrWhiteSpace(line.TaxExemptionReason))
                        {
                            results.Add(new SAFTValidationResult
                            {
                                Field = "StockMovementLine.TaxExemptionReason",
                                Message = "TaxExemptionReason must be present if TaxPercentage is 0 (SAF-T <xs:assert> StockMovementLine 1).",
                                Section = "StockMovement",
                                DocumentNumber = movement.DocumentNumber,
                                LineNumber = line.LineNumber,
                                Type = SAFTValidationType.Consistency
                            });
                        }
                    }
                    // 2. TaxExemptionReason/TaxExemptionCode mutual presence
                    // <xs:assert test="if ((ns:TaxExemptionReason and not(ns:TaxExemptionCode)) or (ns:TaxExemptionCode and not(ns:TaxExemptionReason))) then false() else true()"/>
                    bool hasReason = !string.IsNullOrWhiteSpace(line.TaxExemptionReason);
                    bool hasCode = !string.IsNullOrWhiteSpace(line.TaxExemptionCode);
                    if ((hasReason && !hasCode) || (hasCode && !hasReason))
                    {
                        results.Add(new SAFTValidationResult
                        {
                            Field = "StockMovementLine.TaxExemptionReason/TaxExemptionCode",
                            Message = "TaxExemptionReason and TaxExemptionCode must both be present or both be missing (SAF-T <xs:assert> StockMovementLine 2).",
                            Section = "StockMovement",
                            DocumentNumber = movement.DocumentNumber,
                            LineNumber = line.LineNumber,
                            Type = SAFTValidationType.Consistency
                        });
                    }
                }
            }
            // Work Documents
            var workDocuments = auditFile.SourceDocuments.WorkingDocuments?.WorkDocuments ?? new List<WorkDocument>();
            foreach (var doc in workDocuments)
            {
                if (doc.Lines != null && doc.DocumentTotals != null)
                {
                    decimal sumNet = doc.Lines.Sum(l => (l.DebitAmount ?? 0) - (l.CreditAmount ?? 0));
                    decimal sumGross = doc.Lines.Sum(l => (l.DebitAmount ?? 0) - (l.CreditAmount ?? 0) + (l.Tax?.TaxAmount ?? 0));
                    if (Math.Abs(doc.DocumentTotals.NetTotal - sumNet) > 0.01m)
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "WorkDocument.DocumentTotals.NetTotal",
                            Message = $"NetTotal ({doc.DocumentTotals.NetTotal}) does not match sum of lines ({sumNet}).",
                            Section = "WorkDocument",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = doc.DocumentNumber,
                            Type = SAFTValidationType.Consistency
                        });
                    }
                    if (Math.Abs(doc.DocumentTotals.GrossTotal - sumGross) > 0.01m)
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "WorkDocument.DocumentTotals.GrossTotal",
                            Message = $"GrossTotal ({doc.DocumentTotals.GrossTotal}) does not match sum of lines + tax ({sumGross}).",
                            Section = "WorkDocument",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = doc.DocumentNumber,
                            Type = SAFTValidationType.Consistency
                        });
                    }
                }
                // === SAF-T PT 1.04_01 <xs:assert> rules for WorkDocumentLine ===
                foreach (var line in doc.Lines ?? new List<WorkDocumentLine>())
                {
                    // 1. TaxAmount/TaxExemptionReason logic
                    if (line.Tax != null && line.Tax.TaxAmount.HasValue)
                    {
                        if (line.Tax.TaxAmount.Value != 0 && !string.IsNullOrWhiteSpace(line.TaxExemptionReason))
                        {
                            results.Add(new SAFTValidationResult
                            {
                                Field = "WorkDocumentLine.TaxExemptionReason",
                                Message = "TaxExemptionReason must be missing if TaxAmount is not 0 (SAF-T <xs:assert> WorkDocumentLine 1).",
                                Section = "WorkDocument",
                                DocumentNumber = doc.DocumentNumber,
                                LineNumber = line.LineNumber,
                                Type = SAFTValidationType.Consistency
                            });
                        }
                        if (line.Tax.TaxAmount.Value == 0 && string.IsNullOrWhiteSpace(line.TaxExemptionReason))
                        {
                            results.Add(new SAFTValidationResult
                            {
                                Field = "WorkDocumentLine.TaxExemptionReason",
                                Message = "TaxExemptionReason must be present if TaxAmount is 0 (SAF-T <xs:assert> WorkDocumentLine 1).",
                                Section = "WorkDocument",
                                DocumentNumber = doc.DocumentNumber,
                                LineNumber = line.LineNumber,
                                Type = SAFTValidationType.Consistency
                            });
                        }
                    }
                    // 2. TaxPercentage/TaxExemptionReason logic
                    if (line.Tax != null && line.Tax.TaxPercentage.HasValue)
                    {
                        if (line.Tax.TaxPercentage.Value != 0 && !string.IsNullOrWhiteSpace(line.TaxExemptionReason))
                        {
                            results.Add(new SAFTValidationResult
                            {
                                Field = "WorkDocumentLine.TaxExemptionReason",
                                Message = "TaxExemptionReason must be missing if TaxPercentage is not 0 (SAF-T <xs:assert> WorkDocumentLine 2).",
                                Section = "WorkDocument",
                                DocumentNumber = doc.DocumentNumber,
                                LineNumber = line.LineNumber,
                                Type = SAFTValidationType.Consistency
                            });
                        }
                        if (line.Tax.TaxPercentage.Value == 0 && string.IsNullOrWhiteSpace(line.TaxExemptionReason))
                        {
                            results.Add(new SAFTValidationResult
                            {
                                Field = "WorkDocumentLine.TaxExemptionReason",
                                Message = "TaxExemptionReason must be present if TaxPercentage is 0 (SAF-T <xs:assert> WorkDocumentLine 2).",
                                Section = "WorkDocument",
                                DocumentNumber = doc.DocumentNumber,
                                LineNumber = line.LineNumber,
                                Type = SAFTValidationType.Consistency
                            });
                        }
                    }
                    // 3. TaxExemptionReason/TaxExemptionCode mutual presence
                    bool hasReason = !string.IsNullOrWhiteSpace(line.TaxExemptionReason);
                    bool hasCode = !string.IsNullOrWhiteSpace(line.TaxExemptionCode);
                    if ((hasReason && !hasCode) || (hasCode && !hasReason))
                    {
                        results.Add(new SAFTValidationResult
                        {
                            Field = "WorkDocumentLine.TaxExemptionReason/TaxExemptionCode",
                            Message = "TaxExemptionReason and TaxExemptionCode must both be present or both be missing (SAF-T <xs:assert> WorkDocumentLine 3).",
                            Section = "WorkDocument",
                            DocumentNumber = doc.DocumentNumber,
                            LineNumber = line.LineNumber,
                            Type = SAFTValidationType.Consistency
                        });
                    }
                    // 4. TaxBase/UnitPrice logic
                    if (line.TaxBase.HasValue && line.UnitPrice != 0)
                    {
                        results.Add(new SAFTValidationResult
                        {
                            Field = "WorkDocumentLine.TaxBase/UnitPrice",
                            Message = "If TaxBase is present, UnitPrice must be 0 (SAF-T <xs:assert> WorkDocumentLine 4).",
                            Section = "WorkDocument",
                            DocumentNumber = doc.DocumentNumber,
                            LineNumber = line.LineNumber,
                            Type = SAFTValidationType.Consistency
                        });
                    }
                    // 5. TaxBase/DebitAmount/CreditAmount logic
                    if (line.TaxBase.HasValue && ((line.DebitAmount ?? 0) != 0 || (line.CreditAmount ?? 0) != 0))
                    {
                        results.Add(new SAFTValidationResult
                        {
                            Field = "WorkDocumentLine.TaxBase/DebitAmount/CreditAmount",
                            Message = "If TaxBase is present, DebitAmount and CreditAmount must be 0 (SAF-T <xs:assert> WorkDocumentLine 5).",
                            Section = "WorkDocument",
                            DocumentNumber = doc.DocumentNumber,
                            LineNumber = line.LineNumber,
                            Type = SAFTValidationType.Consistency
                        });
                    }
                }
            }
            // Payments
            var payments = auditFile.SourceDocuments.Payments?.PaymentList ?? new List<Payment>();
            foreach (var payment in payments)
            {
                if (payment.Lines != null && payment.DocumentTotals != null)
                {
                    decimal sumNet = payment.Lines.Sum(l => (l.DebitAmount ?? 0) - (l.CreditAmount ?? 0));
                    decimal sumGross = payment.Lines.Sum(l => (l.DebitAmount ?? 0) - (l.CreditAmount ?? 0) + (l.Tax?.TaxAmount ?? 0));
                    if (Math.Abs(payment.DocumentTotals.NetTotal - sumNet) > 0.01m)
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "Payment.DocumentTotals.NetTotal",
                            Message = $"NetTotal ({payment.DocumentTotals.NetTotal}) does not match sum of lines ({sumNet}).",
                            Section = "Payment",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = payment.PaymentRefNo,
                            Type = SAFTValidationType.Consistency
                        });
                    }
                    if (Math.Abs(payment.DocumentTotals.GrossTotal - sumGross) > 0.01m)
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "Payment.DocumentTotals.GrossTotal",
                            Message = $"GrossTotal ({payment.DocumentTotals.GrossTotal}) does not match sum of lines + tax ({sumGross}).",
                            Section = "Payment",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = payment.PaymentRefNo,
                            Type = SAFTValidationType.Consistency
                        });
                    }
                }
                // === SAF-T PT 1.04_01 <xs:assert> rules for PaymentLine ===
                foreach (var line in payment.Lines ?? new List<PaymentLine>())
                {
                    // 1. TaxAmount/TaxExemptionReason logic
                    if (line.Tax != null && line.Tax.TaxAmount.HasValue)
                    {
                        if (line.Tax.TaxAmount.Value != 0 && !string.IsNullOrWhiteSpace(line.TaxExemptionReason))
                        {
                            results.Add(new SAFTValidationResult
                            {
                                Field = "PaymentLine.TaxExemptionReason",
                                Message = "TaxExemptionReason must be missing if TaxAmount is not 0 (SAF-T <xs:assert> PaymentLine 1).",
                                Section = "Payment",
                                DocumentNumber = payment.PaymentRefNo,
                                LineNumber = line.LineNumber,
                                Type = SAFTValidationType.Consistency
                            });
                        }
                        if (line.Tax.TaxAmount.Value == 0 && string.IsNullOrWhiteSpace(line.TaxExemptionReason))
                        {
                            results.Add(new SAFTValidationResult
                            {
                                Field = "PaymentLine.TaxExemptionReason",
                                Message = "TaxExemptionReason must be present if TaxAmount is 0 (SAF-T <xs:assert> PaymentLine 1).",
                                Section = "Payment",
                                DocumentNumber = payment.PaymentRefNo,
                                LineNumber = line.LineNumber,
                                Type = SAFTValidationType.Consistency
                            });
                        }
                    }
                    // 2. TaxPercentage/TaxExemptionReason logic
                    if (line.Tax != null && line.Tax.TaxPercentage.HasValue)
                    {
                        if (line.Tax.TaxPercentage.Value != 0 && !string.IsNullOrWhiteSpace(line.TaxExemptionReason))
                        {
                            results.Add(new SAFTValidationResult
                            {
                                Field = "PaymentLine.TaxExemptionReason",
                                Message = "TaxExemptionReason must be missing if TaxPercentage is not 0 (SAF-T <xs:assert> PaymentLine 2).",
                                Section = "Payment",
                                DocumentNumber = payment.PaymentRefNo,
                                LineNumber = line.LineNumber,
                                Type = SAFTValidationType.Consistency
                            });
                        }
                        if (line.Tax.TaxPercentage.Value == 0 && string.IsNullOrWhiteSpace(line.TaxExemptionReason))
                        {
                            results.Add(new SAFTValidationResult
                            {
                                Field = "PaymentLine.TaxExemptionReason",
                                Message = "TaxExemptionReason must be present if TaxPercentage is 0 (SAF-T <xs:assert> PaymentLine 2).",
                                Section = "Payment",
                                DocumentNumber = payment.PaymentRefNo,
                                LineNumber = line.LineNumber,
                                Type = SAFTValidationType.Consistency
                            });
                        }
                    }
                    // 3. TaxExemptionReason/TaxExemptionCode mutual presence
                    bool hasReason = !string.IsNullOrWhiteSpace(line.TaxExemptionReason);
                    bool hasCode = !string.IsNullOrWhiteSpace(line.TaxExemptionCode);
                    if ((hasReason && !hasCode) || (hasCode && !hasReason))
                    {
                        results.Add(new SAFTValidationResult
                        {
                            Field = "PaymentLine.TaxExemptionReason/TaxExemptionCode",
                            Message = "TaxExemptionReason and TaxExemptionCode must both be present or both be missing (SAF-T <xs:assert> PaymentLine 3).",
                            Section = "Payment",
                            DocumentNumber = payment.PaymentRefNo,
                            LineNumber = line.LineNumber,
                            Type = SAFTValidationType.Consistency
                        });
                    }
                    // 4. If parent PaymentType == 'RC', Tax must be present
                    if (string.Equals(payment.PaymentType, "RC", StringComparison.OrdinalIgnoreCase) && line.Tax == null)
                    {
                        results.Add(new SAFTValidationResult
                        {
                            Field = "PaymentLine.Tax",
                            Message = "If PaymentType is 'RC', Tax must be present (SAF-T <xs:assert> PaymentLine 4).",
                            Section = "Payment",
                            DocumentNumber = payment.PaymentRefNo,
                            LineNumber = line.LineNumber,
                            Type = SAFTValidationType.Consistency
                        });
                    }
                }
                // Payment-level assertion: If any line has TaxType, PaymentType must be 'RC' or 'RG'
                bool anyLineHasTaxType = (payment.Lines ?? new List<PaymentLine>()).Any(l => l.Tax != null && !string.IsNullOrWhiteSpace(l.Tax.TaxType));
                if (anyLineHasTaxType && !(string.Equals(payment.PaymentType, "RC", StringComparison.OrdinalIgnoreCase) || string.Equals(payment.PaymentType, "RG", StringComparison.OrdinalIgnoreCase)))
                {
                    results.Add(new SAFTValidationResult
                    {
                        Field = "Payment.PaymentType",
                        Message = "If any line has TaxType, PaymentType must be 'RC' or 'RG' (SAF-T <xs:assert> Payment 1).",
                        Section = "Payment",
                        DocumentNumber = payment.PaymentRefNo,
                        Type = SAFTValidationType.Consistency
                    });
                }
            }
            // GeneralLedgerEntries: debits = credits
            var journals = auditFile.GeneralLedgerEntries?.Journals ?? new List<Journal>();
            foreach (var journal in journals)
            {
                foreach (var transaction in journal.Transactions ?? new List<Transaction>())
                {
                    var lines = transaction.Lines;
                    decimal debit = lines?.DebitLines?.Sum(l => l.DebitAmount ?? 0) ?? 0;
                    decimal credit = lines?.CreditLines?.Sum(l => l.CreditAmount ?? 0) ?? 0;
                    if (Math.Abs(debit - credit) > 0.01m)
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "Transaction.Lines",
                            Message = $"Debits ({debit}) do not match Credits ({credit}) in Transaction '{transaction.TransactionID}'.",
                            Section = "GeneralLedgerEntries",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = transaction.TransactionID,
                            Type = SAFTValidationType.Consistency
                        });
                    }
                }
            }
            return results;
        }
        private static List<SAFTValidationResult> ValidateRequiredFields(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            System.Diagnostics.Debug.WriteLine($"Header: {auditFile.Header}");
            if (auditFile.Header != null)
            {
                System.Diagnostics.Debug.WriteLine($"Header.CompanyID: {auditFile.Header.CompanyID}");
                System.Diagnostics.Debug.WriteLine($"Header.TaxRegistrationNumber: {auditFile.Header.TaxRegistrationNumber}");
                System.Diagnostics.Debug.WriteLine($"Header.FiscalYear: {auditFile.Header.FiscalYear}");
            }
            // Top-level required fields
            if (auditFile.Header == null)
            {
                results.Add(new SAFTValidationResult {
                    Field = "AuditFile.Header",
                    Message = "Header is required.",
                    Section = "AuditFile",
                    Severity = SAFTValidationSeverity.Error,
                    Type = SAFTValidationType.RequiredField
                });
            }
            if (auditFile.MasterFiles == null)
            {
                results.Add(new SAFTValidationResult {
                    Field = "AuditFile.MasterFiles",
                    Message = "MasterFiles is required.",
                    Section = "AuditFile",
                    Severity = SAFTValidationSeverity.Error,
                    Type = SAFTValidationType.RequiredField
                });
            }
            if (auditFile.SourceDocuments == null)
            {
                results.Add(new SAFTValidationResult {
                    Field = "AuditFile.SourceDocuments",
                    Message = "SourceDocuments is required.",
                    Section = "AuditFile",
                    Severity = SAFTValidationSeverity.Error,
                    Type = SAFTValidationType.RequiredField
                });
            }
            if (auditFile.Header == null || auditFile.MasterFiles == null || auditFile.SourceDocuments == null)
                return results;
            // Invoices
            var invoices = auditFile.SourceDocuments.SalesInvoices?.Invoices ?? new List<Invoice>();
            foreach (var invoice in invoices)
            {
                if (string.IsNullOrWhiteSpace(invoice.InvoiceNo))
                {
                    results.Add(new SAFTValidationResult {
                        Field = "Invoice.InvoiceNo",
                        Message = "InvoiceNo is required.",
                        Section = "Invoice",
                        Severity = SAFTValidationSeverity.Error,
                        DocumentNumber = invoice.InvoiceNo,
                        Type = SAFTValidationType.RequiredField
                    });
                }
                if (string.IsNullOrWhiteSpace(invoice.CustomerID))
                {
                    results.Add(new SAFTValidationResult {
                        Field = "Invoice.CustomerID",
                        Message = "CustomerID is required.",
                        Section = "Invoice",
                        Severity = SAFTValidationSeverity.Error,
                        DocumentNumber = invoice.InvoiceNo,
                        Type = SAFTValidationType.RequiredField
                    });
                }
                foreach (var line in invoice.Lines ?? new List<InvoiceLine>())
                {
                    if (string.IsNullOrWhiteSpace(line.ProductCode))
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "InvoiceLine.ProductCode",
                            Message = "ProductCode is required.",
                            Section = "Invoice",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = invoice.InvoiceNo,
                            LineNumber = line.LineNumber,
                            Type = SAFTValidationType.RequiredField
                        });
                    }
                }
            }
            // Stock Movements
            var stockMovements = auditFile.SourceDocuments.MovementOfGoods?.StockMovements ?? new List<StockMovement>();
            foreach (var movement in stockMovements)
            {
                if (string.IsNullOrWhiteSpace(movement.DocumentNumber))
                {
                    results.Add(new SAFTValidationResult {
                        Field = "StockMovement.DocumentNumber",
                        Message = "DocumentNumber is required.",
                        Section = "StockMovement",
                        Severity = SAFTValidationSeverity.Error,
                        DocumentNumber = movement.DocumentNumber,
                        Type = SAFTValidationType.RequiredField
                    });
                }
                foreach (var line in movement.Lines ?? new List<StockMovementLine>())
                {
                    if (string.IsNullOrWhiteSpace(line.ProductCode))
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "StockMovementLine.ProductCode",
                            Message = "ProductCode is required.",
                            Section = "StockMovement",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = movement.DocumentNumber,
                            LineNumber = line.LineNumber,
                            Type = SAFTValidationType.RequiredField
                        });
                    }
                }
            }
            // Work Documents
            var workDocuments = auditFile.SourceDocuments.WorkingDocuments?.WorkDocuments ?? new List<WorkDocument>();
            foreach (var doc in workDocuments)
            {
                if (string.IsNullOrWhiteSpace(doc.DocumentNumber))
                {
                    results.Add(new SAFTValidationResult {
                        Field = "WorkDocument.DocumentNumber",
                        Message = "DocumentNumber is required.",
                        Section = "WorkDocument",
                        Severity = SAFTValidationSeverity.Error,
                        DocumentNumber = doc.DocumentNumber,
                        Type = SAFTValidationType.RequiredField
                    });
                }
                foreach (var line in doc.Lines ?? new List<WorkDocumentLine>())
                {
                    if (string.IsNullOrWhiteSpace(line.ProductCode))
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "WorkDocumentLine.ProductCode",
                            Message = "ProductCode is required.",
                            Section = "WorkDocument",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = doc.DocumentNumber,
                            LineNumber = line.LineNumber,
                            Type = SAFTValidationType.RequiredField
                        });
                    }
                }
            }
            // Payments
            var payments = auditFile.SourceDocuments.Payments?.PaymentList ?? new List<Payment>();
            foreach (var payment in payments)
            {
                if (string.IsNullOrWhiteSpace(payment.PaymentRefNo))
                {
                    results.Add(new SAFTValidationResult {
                        Field = "Payment.PaymentRefNo",
                        Message = "PaymentRefNo is required.",
                        Section = "Payment",
                        Severity = SAFTValidationSeverity.Error,
                        DocumentNumber = payment.PaymentRefNo,
                        Type = SAFTValidationType.RequiredField
                    });
                }
                if (string.IsNullOrWhiteSpace(payment.CustomerID))
                {
                    results.Add(new SAFTValidationResult {
                        Field = "Payment.CustomerID",
                        Message = "CustomerID is required.",
                        Section = "Payment",
                        Severity = SAFTValidationSeverity.Error,
                        DocumentNumber = payment.PaymentRefNo,
                        Type = SAFTValidationType.RequiredField
                    });
                }
            }
            // MasterFiles: Customers
            var customers = auditFile.MasterFiles.Customers ?? new List<Customer>();
            foreach (var customer in customers)
            {
                if (string.IsNullOrWhiteSpace(customer.CustomerID))
                {
                    results.Add(new SAFTValidationResult {
                        Field = "Customer.CustomerID",
                        Message = "CustomerID is required.",
                        Section = "Customer",
                        Severity = SAFTValidationSeverity.Error,
                        Type = SAFTValidationType.RequiredField
                    });
                }
            }
            // MasterFiles: Suppliers
            var suppliers = auditFile.MasterFiles.Suppliers ?? new List<Supplier>();
            foreach (var supplier in suppliers)
            {
                if (string.IsNullOrWhiteSpace(supplier.SupplierID))
                {
                    results.Add(new SAFTValidationResult {
                        Field = "Supplier.SupplierID",
                        Message = "SupplierID is required.",
                        Section = "Supplier",
                        Severity = SAFTValidationSeverity.Error,
                        Type = SAFTValidationType.RequiredField
                    });
                }
            }
            // MasterFiles: Products
            var products = auditFile.MasterFiles.Products ?? new List<Product>();
            foreach (var product in products)
            {
                if (string.IsNullOrWhiteSpace(product.ProductCode))
                {
                    results.Add(new SAFTValidationResult {
                        Field = "Product.ProductCode",
                        Message = "ProductCode is required.",
                        Section = "Product",
                        Severity = SAFTValidationSeverity.Error,
                        Type = SAFTValidationType.RequiredField
                    });
                }
            }
            // MasterFiles: Accounts
            var accounts = auditFile.MasterFiles.GeneralLedgerAccounts ?? new List<Account>();
            foreach (var account in accounts)
            {
                if (string.IsNullOrWhiteSpace(account.AccountID))
                {
                    results.Add(new SAFTValidationResult {
                        Field = "Account.AccountID",
                        Message = "AccountID is required.",
                        Section = "Account",
                        Severity = SAFTValidationSeverity.Error,
                        Type = SAFTValidationType.RequiredField
                    });
                }
            }
            // MasterFiles: TaxTableEntries
            var taxEntries = auditFile.MasterFiles.TaxTable?.TaxTableEntries ?? new List<TaxTableEntry>();
            foreach (var entry in taxEntries)
            {
                if (string.IsNullOrWhiteSpace(entry.TaxType))
                {
                    results.Add(new SAFTValidationResult {
                        Field = "TaxTableEntry.TaxType",
                        Message = "TaxType is required.",
                        Section = "TaxTableEntry",
                        Severity = SAFTValidationSeverity.Error,
                        Type = SAFTValidationType.RequiredField
                    });
                }
                if (string.IsNullOrWhiteSpace(entry.TaxCode))
                {
                    results.Add(new SAFTValidationResult {
                        Field = "TaxTableEntry.TaxCode",
                        Message = "TaxCode is required.",
                        Section = "TaxTableEntry",
                        Severity = SAFTValidationSeverity.Error,
                        Type = SAFTValidationType.RequiredField
                    });
                }
            }
            return results;
        }
        private static List<SAFTValidationResult> ValidateEnumValues(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            // TODO: Implement enum value checks (InvoiceType, MovementType, etc.)
            return results;
        }
        private static List<SAFTValidationResult> ValidateDuplicateMasterData(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            // Customers
            var customers = auditFile.MasterFiles.Customers ?? new List<Customer>();
            var customerIds = new HashSet<string>();
            foreach (var customer in customers)
            {
                if (!string.IsNullOrWhiteSpace(customer.CustomerID))
                {
                    if (!customerIds.Add(customer.CustomerID))
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "Customer.CustomerID",
                            Message = $"Duplicate CustomerID '{customer.CustomerID}' found.",
                            Section = "Customer",
                            Severity = SAFTValidationSeverity.Error,
                            Type = SAFTValidationType.Duplicate
                        });
                    }
                }
            }
            // Suppliers
            var suppliers = auditFile.MasterFiles.Suppliers ?? new List<Supplier>();
            var supplierIds = new HashSet<string>();
            foreach (var supplier in suppliers)
            {
                if (!string.IsNullOrWhiteSpace(supplier.SupplierID))
                {
                    if (!supplierIds.Add(supplier.SupplierID))
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "Supplier.SupplierID",
                            Message = $"Duplicate SupplierID '{supplier.SupplierID}' found.",
                            Section = "Supplier",
                            Severity = SAFTValidationSeverity.Error,
                            Type = SAFTValidationType.Duplicate
                        });
                    }
                }
            }
            // Products
            var products = auditFile.MasterFiles.Products ?? new List<Product>();
            var productCodes = new HashSet<string>();
            foreach (var product in products)
            {
                if (!string.IsNullOrWhiteSpace(product.ProductCode))
                {
                    if (!productCodes.Add(product.ProductCode))
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "Product.ProductCode",
                            Message = $"Duplicate ProductCode '{product.ProductCode}' found.",
                            Section = "Product",
                            Severity = SAFTValidationSeverity.Error,
                            Type = SAFTValidationType.Duplicate
                        });
                    }
                }
            }
            // Accounts
            var accounts = auditFile.MasterFiles.GeneralLedgerAccounts ?? new List<Account>();
            var accountIds = new HashSet<string>();
            foreach (var account in accounts)
            {
                if (!string.IsNullOrWhiteSpace(account.AccountID))
                {
                    if (!accountIds.Add(account.AccountID))
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "Account.AccountID",
                            Message = $"Duplicate AccountID '{account.AccountID}' found.",
                            Section = "Account",
                            Severity = SAFTValidationSeverity.Error,
                            Type = SAFTValidationType.Duplicate
                        });
                    }
                }
            }
            return results;
        }
        private static List<SAFTValidationResult> ValidateNestedReferenceIntegrity(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            if (auditFile.SourceDocuments == null)
                return results;
            // Collect all InvoiceNo for reference
            var invoiceNos = new HashSet<string>(auditFile.SourceDocuments.SalesInvoices?.Invoices?.Select(i => i.InvoiceNo) ?? new List<string>());
            // Credit Notes (InvoiceType 'NC', 'ND') referencing other invoices
            var invoices = auditFile.SourceDocuments.SalesInvoices?.Invoices ?? new List<Invoice>();
            foreach (var invoice in invoices)
            {
                if (invoice.InvoiceType == "NC" || invoice.InvoiceType == "ND")
                {
                    // Check References in lines (if present)
                    foreach (var line in invoice.Lines ?? new List<InvoiceLine>())
                    {
                        // If line has a References property with OriginatingON, check it
                        var referencesProp = line.GetType().GetProperty("References");
                        if (referencesProp != null)
                        {
                            var references = referencesProp.GetValue(line) as IEnumerable<object>;
                            if (references != null)
                            {
                                foreach (var reference in references)
                                {
                                    var originatingOnProp = reference.GetType().GetProperty("OriginatingON");
                                    if (originatingOnProp != null)
                                    {
                                        var originatingOn = originatingOnProp.GetValue(reference) as string;
                                        if (!string.IsNullOrWhiteSpace(originatingOn) && !invoiceNos.Contains(originatingOn))
                                        {
                                            results.Add(new SAFTValidationResult {
                                                Field = "InvoiceLine.References.OriginatingON",
                                                Message = $"OriginatingON '{originatingOn}' does not match any InvoiceNo.",
                                                Section = "Invoice",
                                                Severity = SAFTValidationSeverity.Error,
                                                DocumentNumber = invoice.InvoiceNo,
                                                LineNumber = line.LineNumber,
                                                Type = SAFTValidationType.CrossReference
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            // OrderReferences in StockMovementLine
            var stockMovements = auditFile.SourceDocuments.MovementOfGoods?.StockMovements ?? new List<StockMovement>();
            foreach (var movement in stockMovements)
            {
                foreach (var line in movement.Lines ?? new List<StockMovementLine>())
                {
                    foreach (var orderRef in line.OrderReferences ?? new List<OrderReferences>())
                    {
                        if (!string.IsNullOrWhiteSpace(orderRef.OriginatingON) && !invoiceNos.Contains(orderRef.OriginatingON))
                        {
                            results.Add(new SAFTValidationResult {
                                Field = "StockMovementLine.OrderReferences.OriginatingON",
                                Message = $"OriginatingON '{orderRef.OriginatingON}' does not match any InvoiceNo.",
                                Section = "StockMovement",
                                Severity = SAFTValidationSeverity.Error,
                                DocumentNumber = movement.DocumentNumber,
                                LineNumber = line.LineNumber,
                                Type = SAFTValidationType.CrossReference
                            });
                        }
                    }
                }
            }
            // OrderReferences in WorkDocumentLine
            var workDocuments = auditFile.SourceDocuments.WorkingDocuments?.WorkDocuments ?? new List<WorkDocument>();
            foreach (var doc in workDocuments)
            {
                foreach (var line in doc.Lines ?? new List<WorkDocumentLine>())
                {
                    foreach (var orderRef in line.OrderReferences ?? new List<OrderReferences>())
                    {
                        if (!string.IsNullOrWhiteSpace(orderRef.OriginatingON) && !invoiceNos.Contains(orderRef.OriginatingON))
                        {
                            results.Add(new SAFTValidationResult {
                                Field = "WorkDocumentLine.OrderReferences.OriginatingON",
                                Message = $"OriginatingON '{orderRef.OriginatingON}' does not match any InvoiceNo.",
                                Section = "WorkDocument",
                                Severity = SAFTValidationSeverity.Error,
                                DocumentNumber = doc.DocumentNumber,
                                LineNumber = line.LineNumber,
                                Type = SAFTValidationType.CrossReference
                            });
                        }
                    }
                }
            }
            return results;
        }
        private static List<SAFTValidationResult> ValidateBusinessRules(AuditFile auditFile)
        {
            var results = new List<SAFTValidationResult>();
            if (auditFile.MasterFiles == null)
                return results;
            // Self-billing: if SelfBillingIndicator=1, ensure business rules
            var customers = auditFile.MasterFiles.Customers ?? new List<Customer>();
            foreach (var customer in customers)
            {
                if (customer.SelfBillingIndicator == 1 && string.IsNullOrWhiteSpace(customer.CustomerID))
                {
                    results.Add(new SAFTValidationResult {
                        Field = "Customer.SelfBillingIndicator",
                        Message = "SelfBillingIndicator is 1 but CustomerID is missing.",
                        Section = "Customer",
                        Severity = SAFTValidationSeverity.Error,
                        Type = SAFTValidationType.BusinessRule
                    });
                }
            }
            var suppliers = auditFile.MasterFiles.Suppliers ?? new List<Supplier>();
            foreach (var supplier in suppliers)
            {
                if (supplier.SelfBillingIndicator == 1 && string.IsNullOrWhiteSpace(supplier.SupplierID))
                {
                    results.Add(new SAFTValidationResult {
                        Field = "Supplier.SelfBillingIndicator",
                        Message = "SelfBillingIndicator is 1 but SupplierID is missing.",
                        Section = "Supplier",
                        Severity = SAFTValidationSeverity.Error,
                        Type = SAFTValidationType.BusinessRule
                    });
                }
            }
            // Tax exemption: if TaxExemptionReason is set, TaxExemptionCode must be set
            // Invoices
            var invoices = auditFile.SourceDocuments.SalesInvoices?.Invoices ?? new List<Invoice>();
            foreach (var invoice in invoices)
            {
                foreach (var line in invoice.Lines ?? new List<InvoiceLine>())
                {
                    if (!string.IsNullOrWhiteSpace(line.TaxExemptionReason) && string.IsNullOrWhiteSpace(line.TaxExemptionCode))
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "InvoiceLine.TaxExemptionCode",
                            Message = "TaxExemptionCode is required if TaxExemptionReason is set.",
                            Section = "Invoice",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = invoice.InvoiceNo,
                            LineNumber = line.LineNumber,
                            Type = SAFTValidationType.BusinessRule
                        });
                    }
                }
            }
            // Stock Movements
            var stockMovements = auditFile.SourceDocuments.MovementOfGoods?.StockMovements ?? new List<StockMovement>();
            foreach (var movement in stockMovements)
            {
                foreach (var line in movement.Lines ?? new List<StockMovementLine>())
                {
                    if (!string.IsNullOrWhiteSpace(line.TaxExemptionReason) && string.IsNullOrWhiteSpace(line.TaxExemptionCode))
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "StockMovementLine.TaxExemptionCode",
                            Message = "TaxExemptionCode is required if TaxExemptionReason is set.",
                            Section = "StockMovement",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = movement.DocumentNumber,
                            LineNumber = line.LineNumber,
                            Type = SAFTValidationType.BusinessRule
                        });
                    }
                }
            }
            // Work Documents
            var workDocuments = auditFile.SourceDocuments.WorkingDocuments?.WorkDocuments ?? new List<WorkDocument>();
            foreach (var doc in workDocuments)
            {
                foreach (var line in doc.Lines ?? new List<WorkDocumentLine>())
                {
                    if (!string.IsNullOrWhiteSpace(line.TaxExemptionReason) && string.IsNullOrWhiteSpace(line.TaxExemptionCode))
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "WorkDocumentLine.TaxExemptionCode",
                            Message = "TaxExemptionCode is required if TaxExemptionReason is set.",
                            Section = "WorkDocument",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = doc.DocumentNumber,
                            LineNumber = line.LineNumber,
                            Type = SAFTValidationType.BusinessRule
                        });
                    }
                }
            }
            // Payments
            var payments = auditFile.SourceDocuments.Payments?.PaymentList ?? new List<Payment>();
            foreach (var payment in payments)
            {
                foreach (var line in payment.Lines ?? new List<PaymentLine>())
                {
                    if (!string.IsNullOrWhiteSpace(line.TaxExemptionReason) && string.IsNullOrWhiteSpace(line.TaxExemptionCode))
                    {
                        results.Add(new SAFTValidationResult {
                            Field = "PaymentLine.TaxExemptionCode",
                            Message = "TaxExemptionCode is required if TaxExemptionReason is set.",
                            Section = "Payment",
                            Severity = SAFTValidationSeverity.Error,
                            DocumentNumber = payment.PaymentRefNo,
                            LineNumber = line.LineNumber,
                            Type = SAFTValidationType.BusinessRule
                        });
                    }
                }
            }
            return results;
        }
        private static List<SAFTValidationResult> ValidateSchema(AuditFile auditFile, string xmlContent = null, string[] xsdPaths = null, string version = null)
        {
            var results = new List<SAFTValidationResult>();
            if (!string.IsNullOrWhiteSpace(xmlContent) && xsdPaths != null && xsdPaths.Length > 0)
            {
                results.AddRange(XmlSchemaValidator.Validate(xmlContent, xsdPaths));
            }
            // Optionally, use 'version' to select the correct XSD(s) if not provided
            return results;
        }

        // Example: Add a custom plugin for business rule (flag invoices over a certain amount)
        static CrossReferenceValidator()
        {
            PluginValidators.Add(auditFile =>
            {
                var results = new List<SAFTValidationResult>();
                var invoices = auditFile.SourceDocuments.SalesInvoices?.Invoices ?? new List<Invoice>();
                foreach (var invoice in invoices)
                {
                    if (invoice.DocumentTotals != null && invoice.DocumentTotals.GrossTotal > 100000)
                    {
                        results.Add(new SAFTValidationResult
                        {
                            Field = "Invoice.DocumentTotals.GrossTotal",
                            Message = $"Invoice gross total exceeds 100,000: {invoice.DocumentTotals.GrossTotal}",
                            Section = "Invoice",
                            Severity = SAFTValidationSeverity.Warning,
                            DocumentNumber = invoice.InvoiceNo,
                            Type = SAFTValidationType.BusinessRule
                        });
                    }
                }
                return results;
            });
        }
    }

    public static class XmlSchemaValidator
    {
        // Accepts one or more XSD paths for multi-schema support
        public static List<SAFTValidationResult> Validate(string xmlContent, params string[] xsdPaths)
        {
            var results = new List<SAFTValidationResult>();
            var settings = new XmlReaderSettings();
            foreach (var xsdPath in xsdPaths)
            {
                settings.Schemas.Add(null, xsdPath);
            }
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += (sender, e) =>
            {
                results.Add(new SAFTValidationResult
                {
                    Field = "XML",
                    Message = e.Message,
                    Section = "Schema",
                    Severity = e.Severity == XmlSeverityType.Error ? SAFTValidationSeverity.Error : SAFTValidationSeverity.Warning,
                    Type = SAFTValidationType.Schema
                });
            };
            using (var stringReader = new System.IO.StringReader(xmlContent))
            using (var reader = XmlReader.Create(stringReader, settings))
            {
                try
                {
                    while (reader.Read()) { }
                }
                catch (XmlException ex)
                {
                    results.Add(new SAFTValidationResult
                    {
                        Field = "XML",
                        Message = ex.Message,
                        Section = "Schema",
                        Severity = SAFTValidationSeverity.Error,
                        Type = SAFTValidationType.Schema
                    });
                }
            }
            return results;
        }
    }

    public static class ConfigLoader
    {
        private static string _lastConfigPath;
        public class ValidatorConfig
        {
            public List<string> ValidUnits { get; set; }
            public List<string> ValidCurrencies { get; set; }
            public List<string> ValidWithholdingTaxTypes { get; set; }
            public Dictionary<string, string> ErrorMessages { get; set; }
        }

        public static void LoadConfig(string jsonPath)
        {
            _lastConfigPath = jsonPath;
            ReloadConfig();
        }

        public static void ReloadConfig()
        {
            if (string.IsNullOrWhiteSpace(_lastConfigPath) || !System.IO.File.Exists(_lastConfigPath))
                return;
            var json = System.IO.File.ReadAllText(_lastConfigPath);
            var config = JsonSerializer.Deserialize<ValidatorConfig>(json);
            if (config == null) return;
            if (config.ValidUnits != null)
                CrossReferenceValidator.ValidUnits = new HashSet<string>(config.ValidUnits);
            if (config.ValidCurrencies != null)
                CrossReferenceValidator.ValidCurrencies = new HashSet<string>(config.ValidCurrencies);
            if (config.ValidWithholdingTaxTypes != null)
                CrossReferenceValidator.ValidWithholdingTaxTypes = new HashSet<string>(config.ValidWithholdingTaxTypes);
            if (config.ErrorMessages != null)
                CrossReferenceValidator.ErrorMessages = config.ErrorMessages;
        }
    }
} 