using Xunit;
using SAFT.Lib;
using System.IO;
using SAFT.Lib.Validation;

namespace SAFT.Tests
{
    public class SAFTGeneratorTests
    {
        private Header CreateValidHeader()
        {
            return new Header
            {
                AuditFileVersion = "1.04_01",
                CompanyID = "123456789",
                TaxRegistrationNumber = new PortugueseVatNumber { Value = 123456789 },
                TaxAccountingBasis = TaxAccountingBasis.C,
                CompanyName = "Test Company",
                ProductID = "TestProduct/1.0",
                ProductCompanyTaxID = "123456789",
                SoftwareCertificateNumber = "12345",
                ProductVersion = "1.0",
                CompanyAddress = new AddressStructure
                {
                    BuildingNumber = "1",
                    StreetName = "Main Street",
                    AddressDetail = "Main Street, 1",
                    City = "Lisbon",
                    PostalCode = "1000-001",
                    Country = "PT"
                },
                FiscalYear = "2024",
                StartDate = "2024-01-01",
                EndDate = "2024-12-31",
                CurrencyCode = "EUR",
                DateCreated = "2024-06-28",
                TaxEntity = "123456789"
            };
        }

        [Fact]
        public void CanGenerateMinimalValidSAFTXml()
        {
            var auditFile = new AuditFile
            {
                Header = CreateValidHeader(),
                MasterFiles = new MasterFiles()
            };
            var generator = new SAFTGenerator();
            var xml = generator.GenerateXml(auditFile);
            Assert.Contains("<AuditFile", xml);
            Assert.Contains("<Header", xml);
            Assert.Contains("<MasterFiles", xml);
        }

        [Fact]
        public void CanRoundTripSerializeDeserialize()
        {
            var auditFile = new AuditFile
            {
                Header = CreateValidHeader(),
                MasterFiles = new MasterFiles()
            };
            var generator = new SAFTGenerator();
            var xml = generator.GenerateXml(auditFile);
            // Deserialize back
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(AuditFile));
            using var reader = new System.IO.StringReader(xml);
            var deserialized = (AuditFile)serializer.Deserialize(reader)!;
            Assert.Equal(auditFile.Header.AuditFileVersion, deserialized.Header.AuditFileVersion);
            Assert.Equal(auditFile.Header.FiscalYear, deserialized.Header.FiscalYear);
            Assert.Equal(auditFile.Header.TaxRegistrationNumber.Value, deserialized.Header.TaxRegistrationNumber.Value);
        }

        [Fact]
        public void CanGenerateXmlFile()
        {
            var auditFile = new AuditFile
            {
                Header = CreateValidHeader(),
                MasterFiles = new MasterFiles()
            };
            var generator = new SAFTGenerator();
            var tempFile = Path.GetTempFileName() + ".xml";
            try
            {
                generator.GenerateXmlFile(auditFile, tempFile);
                Assert.True(File.Exists(tempFile));
                var xml = File.ReadAllText(tempFile);
                Assert.Contains("<AuditFile", xml);
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void GeneratedXmlIsValidAccordingToValidator()
        {
            var auditFile = new AuditFile
            {
                Header = CreateValidHeader(),
                MasterFiles = new MasterFiles()
            };
            var generator = new SAFTGenerator();
            var xml = generator.GenerateXml(auditFile);
            // Use the schema path from the project root
            var solutionDir = Directory.GetCurrentDirectory();
            while (!File.Exists(Path.Combine(solutionDir, "SAFT.sln")))
            {
                solutionDir = Directory.GetParent(solutionDir)?.FullName;
                if (solutionDir == null)
                    throw new FileNotFoundException("Could not find SAFT.sln file");
            }
            var schemaPath = Path.Combine(solutionDir, "schema1_04_fixed.xsd");
            var errors = SchemaValidator.Validate(xml, schemaPath);
            Assert.True(errors.Count == 0, $"Validation errors: {string.Join(", ", errors)}");
        }
    }
} 