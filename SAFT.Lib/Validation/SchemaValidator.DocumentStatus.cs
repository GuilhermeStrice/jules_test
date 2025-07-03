using System;
using System.Collections.Generic;
using System.Xml;

namespace SAFT.Lib.Validation
{
    public static partial class SchemaValidator
    {
        /// <summary>
        /// Validates document status and ensures InvoiceStatusDate is not before InvoiceDate.
        /// </summary>
        internal static List<string> ValidateDocumentStatus(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");

            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            foreach (XmlNode invoice in invoices)
            {
                var invoiceDate = invoice.SelectSingleNode(".//ns:InvoiceDate", nsManager);
                var status = invoice.SelectSingleNode(".//ns:DocumentStatus", nsManager);
                if (invoiceDate != null && status != null)
                {
                    var statusDate = status.SelectSingleNode(".//ns:InvoiceStatusDate", nsManager);
                    if (statusDate != null && DateTime.TryParse(invoiceDate.InnerText, out DateTime invDate) && DateTime.TryParse(statusDate.InnerText, out DateTime statDate))
                    {
                        if (statDate < invDate)
                        {
                            errors.Add($"Document status validation error: Status date {statDate:yyyy-MM-dd} is before invoice date {invDate:yyyy-MM-dd} (document status, status date, before)");
                        }
                    }
                }
            }
            return errors;
        }

        /// <summary>
        /// Validates comprehensive cancellation rules for SAF-T documents
        /// </summary>
        /// <param name="xmlDoc">The XML document to validate</param>
        /// <returns>List of validation errors</returns>
        internal static List<string> ValidateCancellationRules(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");

            try
            {
                // Validate SalesInvoices cancellation rules
                ValidateSalesInvoicesCancellation(xmlDoc, nsManager, errors);

                // Validate MovementOfGoods cancellation rules
                ValidateMovementOfGoodsCancellation(xmlDoc, nsManager, errors);

                // Validate WorkingDocuments cancellation rules
                ValidateWorkingDocumentsCancellation(xmlDoc, nsManager, errors);

                // Validate Payments cancellation rules
                ValidatePaymentsCancellation(xmlDoc, nsManager, errors);

                // Validate cross-references for cancelled documents
                ValidateCancelledDocumentCrossReferences(xmlDoc, nsManager, errors);
            }
            catch (Exception ex)
            {
                errors.Add($"Cancellation validation error: {ex.Message}");
            }

            return errors;
        }

        /// <summary>
        /// Validates cancellation rules for SalesInvoices
        /// </summary>
        private static void ValidateSalesInvoicesCancellation(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (invoices == null) return;

            foreach (XmlNode invoice in invoices)
            {
                var status = invoice.SelectSingleNode("ns:DocumentStatus/ns:InvoiceStatus", nsManager)?.InnerText;
                if (status == "A") // Cancelled
                {
                    // Check for cancellation reason
                    var reason = invoice.SelectSingleNode("ns:DocumentStatus/ns:Reason", nsManager)?.InnerText;
                    if (string.IsNullOrEmpty(reason))
                        errors.Add($"Cancelled invoice {GetDocumentIdentifier(invoice, nsManager)} must have a cancellation reason");

                    // Check cancellation date is after invoice date
                    var invoiceDate = invoice.SelectSingleNode("ns:InvoiceDate", nsManager)?.InnerText;
                    var statusDate = invoice.SelectSingleNode("ns:DocumentStatus/ns:InvoiceStatusDate", nsManager)?.InnerText;

                    if (!string.IsNullOrEmpty(invoiceDate) && !string.IsNullOrEmpty(statusDate))
                    {
                        if (DateTime.TryParse(invoiceDate, out var docDate) && DateTime.TryParse(statusDate, out var cancelDate))
                        {
                            if (cancelDate <= docDate)
                                errors.Add($"Cancellation date ({cancelDate:yyyy-MM-dd}) must be after invoice date ({docDate:yyyy-MM-dd}) for {GetDocumentIdentifier(invoice, nsManager)}");
                        }
                    }

                    // Check cancelled invoices don't have positive amounts
                    var grossTotal = invoice.SelectSingleNode("ns:DocumentTotals/ns:GrossTotal", nsManager)?.InnerText;
                    if (decimal.TryParse(grossTotal, out var total) && total > 0)
                        errors.Add($"Cancelled invoice {GetDocumentIdentifier(invoice, nsManager)} should have zero or negative amounts");

                    // Check cancelled invoices don't have payments
                    var payments = xmlDoc.SelectNodes($"//ns:Payment[ns:PaymentRefNo='{GetDocumentIdentifier(invoice, nsManager)}']", nsManager);
                    if (payments?.Count > 0)
                        errors.Add($"Cancelled invoice {GetDocumentIdentifier(invoice, nsManager)} should not have associated payments");
                }
            }
        }

        /// <summary>
        /// Validates cancellation rules for MovementOfGoods
        /// </summary>
        private static void ValidateMovementOfGoodsCancellation(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var movements = xmlDoc.SelectNodes("//ns:StockMovement", nsManager);
            if (movements == null) return;

            foreach (XmlNode movement in movements)
            {
                var status = movement.SelectSingleNode("ns:DocumentStatus/ns:MovementStatus", nsManager)?.InnerText;
                if (status == "A") // Cancelled
                {
                    // Check for cancellation reason
                    var reason = movement.SelectSingleNode("ns:DocumentStatus/ns:Reason", nsManager)?.InnerText;
                    if (string.IsNullOrEmpty(reason))
                        errors.Add($"Cancelled movement {GetDocumentIdentifier(movement, nsManager)} must have a cancellation reason");

                    // Check cancellation date is after movement date
                    var movementDate = movement.SelectSingleNode("ns:MovementDate", nsManager)?.InnerText;
                    var statusDate = movement.SelectSingleNode("ns:DocumentStatus/ns:MovementStatusDate", nsManager)?.InnerText;

                    if (!string.IsNullOrEmpty(movementDate) && !string.IsNullOrEmpty(statusDate))
                    {
                        if (DateTime.TryParse(movementDate, out var docDate) && DateTime.TryParse(statusDate, out var cancelDate))
                        {
                            if (cancelDate <= docDate)
                                errors.Add($"Cancellation date ({cancelDate:yyyy-MM-dd}) must be after movement date ({docDate:yyyy-MM-dd}) for {GetDocumentIdentifier(movement, nsManager)}");
                        }
                    }

                    // Check cancelled movements don't have positive quantities
                    var totalQuantity = movement.SelectSingleNode("ns:TotalQuantityIssued", nsManager)?.InnerText;
                    if (decimal.TryParse(totalQuantity, out var quantity) && quantity > 0)
                        errors.Add($"Cancelled movement {GetDocumentIdentifier(movement, nsManager)} should have zero quantities");
                }
            }
        }

        /// <summary>
        /// Validates cancellation rules for WorkingDocuments
        /// </summary>
        private static void ValidateWorkingDocumentsCancellation(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var workDocs = xmlDoc.SelectNodes("//ns:WorkDocument", nsManager);
            if (workDocs == null) return;

            foreach (XmlNode workDoc in workDocs)
            {
                var status = workDoc.SelectSingleNode("ns:DocumentStatus/ns:WorkStatus", nsManager)?.InnerText;
                if (status == "A") // Cancelled
                {
                    // Check for cancellation reason
                    var reason = workDoc.SelectSingleNode("ns:DocumentStatus/ns:Reason", nsManager)?.InnerText;
                    if (string.IsNullOrEmpty(reason))
                        errors.Add($"Cancelled work document {GetDocumentIdentifier(workDoc, nsManager)} must have a cancellation reason");

                    // Check cancellation date is after work date
                    var workDate = workDoc.SelectSingleNode("ns:WorkDate", nsManager)?.InnerText;
                    var statusDate = workDoc.SelectSingleNode("ns:DocumentStatus/ns:WorkStatusDate", nsManager)?.InnerText;

                    if (!string.IsNullOrEmpty(workDate) && !string.IsNullOrEmpty(statusDate))
                    {
                        if (DateTime.TryParse(workDate, out var docDate) && DateTime.TryParse(statusDate, out var cancelDate))
                        {
                            if (cancelDate <= docDate)
                                errors.Add($"Cancellation date ({cancelDate:yyyy-MM-dd}) must be after work date ({docDate:yyyy-MM-dd}) for {GetDocumentIdentifier(workDoc, nsManager)}");
                        }
                    }

                    // Check cancelled work documents don't have positive amounts
                    var grossTotal = workDoc.SelectSingleNode("ns:DocumentTotals/ns:GrossTotal", nsManager)?.InnerText;
                    if (decimal.TryParse(grossTotal, out var total) && total > 0)
                        errors.Add($"Cancelled work document {GetDocumentIdentifier(workDoc, nsManager)} should have zero amounts");
                }
            }
        }

        /// <summary>
        /// Validates cancellation rules for Payments
        /// </summary>
        private static void ValidatePaymentsCancellation(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var payments = xmlDoc.SelectNodes("//ns:Payment", nsManager);
            if (payments == null) return;

            foreach (XmlNode payment in payments)
            {
                var status = payment.SelectSingleNode("ns:DocumentStatus/ns:PaymentStatus", nsManager)?.InnerText;
                if (status == "A") // Cancelled
                {
                    // Check for cancellation reason
                    var reason = payment.SelectSingleNode("ns:DocumentStatus/ns:Reason", nsManager)?.InnerText;
                    if (string.IsNullOrEmpty(reason))
                        errors.Add($"Cancelled payment {GetDocumentIdentifier(payment, nsManager)} must have a cancellation reason");

                    // Check cancellation date is after transaction date
                    var transactionDate = payment.SelectSingleNode("ns:TransactionDate", nsManager)?.InnerText;
                    var statusDate = payment.SelectSingleNode("ns:DocumentStatus/ns:PaymentStatusDate", nsManager)?.InnerText;

                    if (!string.IsNullOrEmpty(transactionDate) && !string.IsNullOrEmpty(statusDate))
                    {
                        if (DateTime.TryParse(transactionDate, out var docDate) && DateTime.TryParse(statusDate, out var cancelDate))
                        {
                            if (cancelDate <= docDate)
                                errors.Add($"Cancellation date ({cancelDate:yyyy-MM-dd}) must be after transaction date ({docDate:yyyy-MM-dd}) for {GetDocumentIdentifier(payment, nsManager)}");
                        }
                    }

                    // Check cancelled payments don't have positive amounts
                    var paymentMethods = payment.SelectNodes("ns:PaymentMethod", nsManager);
                    if (paymentMethods != null)
                    {
                        foreach (XmlNode method in paymentMethods)
                        {
                            var amount = method.SelectSingleNode("ns:PaymentAmount", nsManager)?.InnerText;
                            if (decimal.TryParse(amount, out var paymentAmount) && paymentAmount > 0)
                                errors.Add($"Cancelled payment {GetDocumentIdentifier(payment, nsManager)} should have zero amounts");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates cross-references for cancelled documents
        /// </summary>
        private static void ValidateCancelledDocumentCrossReferences(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // Get all cancelled document references
            var cancelledInvoices = GetCancelledDocumentReferences(xmlDoc, nsManager, "//ns:Invoice[ns:DocumentStatus/ns:InvoiceStatus='A']", "InvoiceNo");
            var cancelledMovements = GetCancelledDocumentReferences(xmlDoc, nsManager, "//ns:StockMovement[ns:DocumentStatus/ns:MovementStatus='A']", "DocumentNumber");
            var cancelledWorkDocs = GetCancelledDocumentReferences(xmlDoc, nsManager, "//ns:WorkDocument[ns:DocumentStatus/ns:WorkStatus='A']", "DocumentNumber");
            var cancelledPayments = GetCancelledDocumentReferences(xmlDoc, nsManager, "//ns:Payment[ns:DocumentStatus/ns:PaymentStatus='A']", "PaymentRefNo");

            // Check that cancelled invoices are not referenced in other documents
            foreach (var cancelledInvoice in cancelledInvoices)
            {
                // Check if cancelled invoice is referenced in payments
                var paymentRefs = xmlDoc.SelectNodes($"//ns:Payment[ns:PaymentRefNo='{cancelledInvoice}']", nsManager);
                if (paymentRefs?.Count > 0)
                    errors.Add($"Cancelled invoice '{cancelledInvoice}' should not be referenced in payments");

                // Check if cancelled invoice is referenced in other invoices (credit/debit notes)
                var invoiceRefs = xmlDoc.SelectNodes($"//ns:Invoice[ns:OrderReferences/ns:OriginatingON='{cancelledInvoice}']", nsManager);
                if (invoiceRefs?.Count > 0)
                    errors.Add($"Cancelled invoice '{cancelledInvoice}' should not be referenced in other invoices");
            }

            // Check that cancelled movements are not referenced in invoices
            foreach (var cancelledMovement in cancelledMovements)
            {
                var movementRefs = xmlDoc.SelectNodes($"//ns:Invoice[ns:OrderReferences/ns:OriginatingON='{cancelledMovement}']", nsManager);
                if (movementRefs?.Count > 0)
                    errors.Add($"Cancelled movement '{cancelledMovement}' should not be referenced in invoices");
            }

            // Check that cancelled work documents are not referenced in invoices
            foreach (var cancelledWorkDoc in cancelledWorkDocs)
            {
                var workDocRefs = xmlDoc.SelectNodes($"//ns:Invoice[ns:OrderReferences/ns:OriginatingON='{cancelledWorkDoc}']", nsManager);
                if (workDocRefs?.Count > 0)
                    errors.Add($"Cancelled work document '{cancelledWorkDoc}' should not be referenced in invoices");
            }
        }

        /// <summary>
        /// Gets document references for cancelled documents
        /// </summary>
        private static HashSet<string> GetCancelledDocumentReferences(XmlDocument xmlDoc, XmlNamespaceManager nsManager, string xpath, string identifierElement)
        {
            var references = new HashSet<string>();
            var nodes = xmlDoc.SelectNodes(xpath, nsManager);

            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    var identifier = node.SelectSingleNode($"ns:{identifierElement}", nsManager)?.InnerText;
                    if (!string.IsNullOrEmpty(identifier))
                        references.Add(identifier);
                }
            }

            return references;
        }

        /// <summary>
        /// Gets a document identifier for error messages
        /// </summary>
        private static string GetDocumentIdentifier(XmlNode document, XmlNamespaceManager nsManager)
        {
            // Try different possible identifier elements
            var identifier = document.SelectSingleNode("ns:InvoiceNo", nsManager)?.InnerText
                            ?? document.SelectSingleNode("ns:DocumentNumber", nsManager)?.InnerText
                            ?? document.SelectSingleNode("ns:PaymentRefNo", nsManager)?.InnerText
                            ?? "Unknown";

            return identifier;
        }
    }
}
