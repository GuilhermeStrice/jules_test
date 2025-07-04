using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using SAFT.Lib;
using System.Globalization;

namespace SAFT.Pdf
{
    public class InvoicePdfGenerator
    {
        public byte[] GeneratePortugueseCompliantInvoicePdf(Invoice invoice)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);

                    // Header: Company info and Invoice title
                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("[Your Company Name]").FontSize(16).Bold();
                            col.Item().Text("NIF: [Your NIF]");
                            col.Item().Text("Address: [Your Address]");
                            col.Item().Text("ATCUD: " + (invoice.ATCUD ?? "[ATCUD here]"));
                        });
                        // row.ConstantItem(120).Element(x => x.Image("/path/to/logo.png")).ShowIf(false); // Logo support: implement as needed
                    });

                    // Invoice Title and Meta
                    page.Content().Element(content =>
                    {
                        content.Column(col =>
                        {
                            col.Item().Text($"FATURA {invoice.InvoiceNo}").FontSize(20).Bold().AlignCenter();
                            col.Item().Text($"Data: {(string.IsNullOrEmpty(invoice.InvoiceDate) ? DateTime.Now.ToString("yyyy-MM-dd") : invoice.InvoiceDate)}").AlignCenter();
                            col.Item().Text($"Cliente: [Customer Name]").AlignCenter();
                            col.Item().Text($"NIF Cliente: [Customer NIF]").AlignCenter();
                            col.Item().Text($"Morada Cliente: [Customer Address]").AlignCenter();
                            col.Item().PaddingVertical(10);

                            // Invoice lines
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40); // Line
                                    columns.RelativeColumn(3); // Description
                                    columns.ConstantColumn(60); // Qty
                                    columns.ConstantColumn(80); // Unit Price
                                    columns.ConstantColumn(80); // VAT
                                    columns.ConstantColumn(80); // Total
                                });

                                // Header row
                                table.Header(header =>
                                {
                                    header.Cell().Text("#").Bold();
                                    header.Cell().Text("Descrição").Bold();
                                    header.Cell().Text("Qtd").Bold();
                                    header.Cell().Text("Preço Unit.").Bold();
                                    header.Cell().Text("IVA").Bold();
                                    header.Cell().Text("Total").Bold();
                                });

                                // Lines
                                int lineNumber = 1;
                                foreach (var line in invoice.Lines)
                                {
                                    table.Cell().Text(lineNumber.ToString());
                                    table.Cell().Text(line.ProductDescription ?? "");
                                    table.Cell().Text(line.Quantity.ToString("0.##", CultureInfo.InvariantCulture));
                                    table.Cell().Text($"{line.UnitPrice:C}");
                                    table.Cell().Text(line.Tax != null ? $"{line.Tax.TaxPercentage:0.##}%" : "");
                                    table.Cell().Text($"{line.CreditAmount:C}");
                                    lineNumber++;
                                }
                            });

                            // Totals
                            col.Item().AlignRight().Text($"Total: {invoice.DocumentTotals.GrossTotal:C}").FontSize(14).Bold();
                            col.Item().AlignRight().Text($"IVA: {invoice.DocumentTotals.TaxPayable:C}");
                            col.Item().AlignRight().Text($"Total a Pagar: {invoice.DocumentTotals.NetTotal:C}").FontSize(14).Bold();

                            // ATCUD, QR code, and legal notes
                            col.Item().PaddingTop(20).Text($"ATCUD: {invoice.ATCUD ?? "[ATCUD]"}");
                            // Generate and render QR code
                            var qrPayload = $"ATCUD:{invoice.ATCUD};InvoiceNo:{invoice.InvoiceNo};Date:{invoice.InvoiceDate};Total:{invoice.DocumentTotals.GrossTotal}";
                            var qrBytes = QrCodeHelper.GenerateQrCodePng(qrPayload);
                            col.Item().Image(qrBytes);
                            col.Item().Text("Este documento foi processado por programa certificado nº [Certificado], AT.").FontSize(8);
                            col.Item().Text("Não é permitida a alteração deste documento.").FontSize(8);
                        });
                    });

                    // Footer: Company legal info
                    page.Footer().AlignCenter().Text("[Nome da Empresa] | NIF: [NIF] | Morada: [Morada] | Certificado AT: [nº]").FontSize(9);
                });
            });
            using var ms = new MemoryStream();
            document.GeneratePdf(ms);
            return ms.ToArray();
        }
    }
}
