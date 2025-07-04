using Xunit;
using SAFT.Pdf;
using SAFT.Lib;
using System.Collections.Generic;

namespace SAFT.Pdf.Tests
{
    public class InvoicePdfGeneratorTests
    {
        [Fact]
        public void GeneratePortugueseCompliantInvoicePdf_GeneratesValidPdf()
        {
            // Arrange: create a minimal but legally structured invoice
            var invoice = new Invoice
            {
                InvoiceNo = "FT 2025/001",
                ATCUD = "20250703-12345",
                InvoiceDate = "2025-07-03",
                Lines = new List<InvoiceLine>
                {
                    new InvoiceLine
                    {
                        ProductDescription = "Serviço de Consultoria",
                        Quantity = 1,
                        UnitPrice = 100,
                        CreditAmount = 100,
                        Tax = new Tax { TaxPercentage = 23 }
                    }
                },
                DocumentTotals = new DocumentTotals
                {
                    GrossTotal = 123,
                    NetTotal = 100,
                    TaxPayable = 23
                }
            };
            var generator = new InvoicePdfGenerator();

            // Act
            var pdfBytes = generator.GeneratePortugueseCompliantInvoicePdf(invoice);

            // Write to temp file and open
            var fileName = $"InvoiceTest_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            System.IO.File.WriteAllBytes(fileName, pdfBytes);
            
            // Try to open PDF with default viewer (cross-platform)
            try
            {
#if WINDOWS
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(fileName) { UseShellExecute = true });
#elif LINUX
                System.Diagnostics.Process.Start("xdg-open", fileName);
#elif OSX
                System.Diagnostics.Process.Start("open", fileName);
#else
                // Fallback: do nothing
#endif
            }
            catch { /* Ignore errors opening viewer */ }

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 1000); // Should be a non-empty PDF
        }
    }
}
