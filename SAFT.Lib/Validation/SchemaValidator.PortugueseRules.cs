using System.Xml;
using SAFT.Lib.Utils;

namespace SAFT.Lib.Validation
{
    public static partial class SchemaValidator
    {
        /// <summary>
        /// Validates Portuguese-specific business rules for SAF-T compliance
        /// </summary>
        /// <param name="xmlDoc">The XML document to validate</param>
        /// <returns>List of validation errors</returns>
        internal static List<string> ValidatePortugueseBusinessRules(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");

            try
            {
                // Validate Header fields
                ValidatePortugueseHeader(xmlDoc, nsManager, errors);

                // Validate Invoice fields
                ValidatePortugueseInvoices(xmlDoc, nsManager, errors);

                // Validate VAT calculations
                ValidatePortugueseVATCalculations(xmlDoc, nsManager, errors);

                // Validate Document Status rules
                ValidatePortugueseDocumentStatus(xmlDoc, nsManager, errors);

                // Validate ATCUD and Hash fields
                ValidatePortugueseDocumentIntegrity(xmlDoc, nsManager, errors);

                // Validate tax code references
                ValidateTaxCodeReferences(xmlDoc, nsManager, errors);

                // Validate VAT exemptions for 0% VAT
                ValidateVATExemptions(xmlDoc, nsManager, errors);
            }
            catch (Exception ex)
            {
                errors.Add($"Portuguese business rule validation error: {ex.Message}");
            }

            return errors;
        }

        private static void ValidatePortugueseHeader(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // Validate CompanyID (Portuguese VAT number)
            var companyID = xmlDoc.SelectSingleNode("//ns:CompanyID", nsManager)?.InnerText;
            if (!string.IsNullOrEmpty(companyID))
            {
                if (!PortugueseUtils.IsValidVATNumber(companyID))
                    errors.Add("CompanyID must be a valid Portuguese VAT number (9 digits)");
            }

            // Validate TaxEntity (Portuguese VAT number)
            var taxEntity = xmlDoc.SelectSingleNode("//ns:TaxEntity", nsManager)?.InnerText;
            if (!string.IsNullOrEmpty(taxEntity))
            {
                if (!PortugueseUtils.IsValidVATNumber(taxEntity))
                    errors.Add("TaxEntity must be a valid Portuguese VAT number (9 digits)");
            }

            // Validate Currency (must be EUR)
            var currency = xmlDoc.SelectSingleNode("//ns:CurrencyCode", nsManager)?.InnerText;
            if (currency != "EUR")
                errors.Add("CurrencyCode must be EUR for Portuguese SAF-T files");

            // Validate Country (must be PT) in CompanyAddress
            var country = xmlDoc.SelectSingleNode("//ns:CompanyAddress/ns:Country", nsManager)?.InnerText;
            if (country != "PT")
                errors.Add("Country must be PT for Portuguese SAF-T files");
        }

        private static void ValidatePortugueseInvoices(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (invoices == null) return;

            foreach (XmlNode invoice in invoices)
            {
                // Validate ATCUD
                var atcud = invoice.SelectSingleNode("ns:ATCUD", nsManager)?.InnerText;
                if (!string.IsNullOrEmpty(atcud))
                {
                    if (!PortugueseUtils.IsValidATCUD(atcud))
                        errors.Add($"Invalid ATCUD format: {atcud}");
                }

                // Validate HashControl
                var hashControl = invoice.SelectSingleNode("ns:HashControl", nsManager)?.InnerText;
                if (!string.IsNullOrEmpty(hashControl))
                {
                    if (!PortugueseUtils.IsValidHashControl(hashControl))
                        errors.Add($"Invalid HashControl format: {hashControl}");
                }

                // Validate DocumentNumber format
                var documentNumber = invoice.SelectSingleNode("ns:DocumentNumber", nsManager)?.InnerText;
                if (!string.IsNullOrEmpty(documentNumber))
                {
                    if (!PortugueseUtils.IsValidInvoiceNumber(documentNumber))
                        errors.Add($"Invalid DocumentNumber format: {documentNumber}");
                }
                // SpecialRegimes validation removed (now handled elsewhere)
            }
        }

        private static void ValidatePortugueseDocumentStatus(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (invoices == null) return;

            foreach (XmlNode invoice in invoices)
            {
                var documentStatus = invoice.SelectSingleNode("ns:DocumentStatus", nsManager)?.InnerText;
                var documentType = invoice.SelectSingleNode("ns:DocumentType", nsManager)?.InnerText;

                if (!string.IsNullOrEmpty(documentStatus) && !string.IsNullOrEmpty(documentType))
                {
                    if (!PortugueseUtils.ValidateDocumentStatus(documentStatus, documentType))
                        errors.Add($"Invalid document status '{documentStatus}' for document type '{documentType}'");
                }
            }
        }

        private static void ValidatePortugueseDocumentIntegrity(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (invoices == null) return;

            foreach (XmlNode invoice in invoices)
            {
                // Validate that ATCUD and HashControl are present for final documents
                var documentStatus = invoice.SelectSingleNode("ns:DocumentStatus", nsManager)?.InnerText;
                var atcud = invoice.SelectSingleNode("ns:ATCUD", nsManager)?.InnerText;
                var hashControl = invoice.SelectSingleNode("ns:HashControl", nsManager)?.InnerText;

                if (documentStatus == "F") // Final document
                {
                    if (string.IsNullOrEmpty(atcud))
                        errors.Add("ATCUD is required for final documents");
                    if (string.IsNullOrEmpty(hashControl))
                        errors.Add("HashControl is required for final documents");
                }
            }
        }

        private static void ValidateTaxCodeReferences(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // Get all tax codes defined in the TaxTable
            var definedTaxCodes = new HashSet<string>();
            var taxTableEntries = xmlDoc.SelectNodes("//ns:TaxTableEntry", nsManager);
            if (taxTableEntries != null)
            {
                foreach (XmlNode entry in taxTableEntries)
                {
                    var taxCode = entry.SelectSingleNode("ns:TaxCode", nsManager)?.InnerText;
                    if (!string.IsNullOrEmpty(taxCode))
                    {
                        definedTaxCodes.Add(taxCode);
                    }
                }
            }

            // Check all tax codes used in Line elements (which contain Tax elements)
            var lines = xmlDoc.SelectNodes("//ns:Line", nsManager);
            if (lines != null)
            {
                foreach (XmlNode line in lines)
                {
                    var taxElements = line.SelectNodes("ns:Tax", nsManager);
                    if (taxElements != null)
                    {
                        foreach (XmlNode tax in taxElements)
                        {
                            var taxCode = tax.SelectSingleNode("ns:TaxCode", nsManager)?.InnerText;
                            if (!string.IsNullOrEmpty(taxCode) && !definedTaxCodes.Contains(taxCode))
                            {
                                errors.Add($"Tax code reference error: TaxCode '{taxCode}' not found in TaxTable");
                            }
                        }
                    }
                }
            }

            // Check all tax codes used in Payment elements
            var payments = xmlDoc.SelectNodes("//ns:Payment", nsManager);
            if (payments != null)
            {
                foreach (XmlNode payment in payments)
                {
                    var paymentRefNo = payment.SelectSingleNode("ns:PaymentRefNo", nsManager)?.InnerText ?? "Unknown";
                    var taxElements = payment.SelectNodes("ns:Tax", nsManager);

                    if (taxElements != null)
                    {
                        foreach (XmlNode tax in taxElements)
                        {
                            var taxCode = tax.SelectSingleNode("ns:TaxCode", nsManager)?.InnerText;
                            if (!string.IsNullOrEmpty(taxCode) && !definedTaxCodes.Contains(taxCode))
                            {
                                errors.Add($"Tax code reference error: TaxCode '{taxCode}' used in payment '{paymentRefNo}' not found in TaxTable");
                            }
                        }
                    }
                }
            }
        }
    }
}