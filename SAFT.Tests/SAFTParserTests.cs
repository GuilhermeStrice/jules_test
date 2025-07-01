using System;
using System.IO;
using Xunit;
using SAFT.Lib.Parser;
using SAFT.Lib.Utils;

namespace SAFT.Tests
{
    public class SAFTParserTests
    {
        public SAFTParserTests()
        {
            // Set the correct schema path for all tests
            var schemaPath = Helpers.GetSchemaPath();
            ConfigurationManager.Current.SchemaPath = schemaPath;
            Console.WriteLine($"Set schema path to: {schemaPath}");
        }

        [Fact]
        public void ParseFromFile_ValidFile_ReturnsAuditFile()
        {
            // Arrange
            var validSaftPath = Helpers.GetSaftPath();

            // Act
            var auditFile = SAFTParser.ParseFromFile(validSaftPath);

            // Assert
            Assert.NotNull(auditFile);
            Assert.NotNull(auditFile.Header);
            Assert.Equal("1.04_01", auditFile.Header.AuditFileVersion);
            Assert.Equal("503140600", auditFile.Header.CompanyID);
            Assert.Equal("Empresa de demonstração (PRIVA)", auditFile.Header.CompanyName);
        }

        [Fact]
        public void ValidateFile_ValidFile_ReturnsEmptyList()
        {
            // Arrange
            var validSaftPath = Helpers.GetSaftPath();

            // Act
            var errors = SAFTParser.ValidateFile(validSaftPath);

            // Debug: Print out the actual errors
            if (errors.Count > 0)
            {
                Console.WriteLine($"Found {errors.Count} validation errors:");
                foreach (var error in errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void ParseFromFile_NonExistentFile_ThrowsFileNotFoundException()
        {
            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => 
                SAFTParser.ParseFromFile("nonexistent.xml"));
        }

        [Fact]
        public void ParseWithValidation_ValidFile_ReturnsAuditFileAndNoErrors()
        {
            var validSaftPath = Helpers.GetSaftPath();
            var (auditFile, errors) = SAFTParser.ParseWithValidation(validSaftPath);
            Assert.NotNull(auditFile);
            Assert.NotNull(auditFile.Header);
            Assert.Equal("1.04_01", auditFile.Header.AuditFileVersion);
            Assert.Equal("503140600", auditFile.Header.CompanyID);
            Assert.Equal("Empresa de demonstração (PRIVA)", auditFile.Header.CompanyName);
            Assert.Empty(errors);
        }

        [Fact]
        public void ParseFromXml_ValidXml_ReturnsAuditFile()
        {
            var validSaftPath = Helpers.GetSaftPath();
            var xmlContent = File.ReadAllText(validSaftPath);
            var auditFile = SAFTParser.ParseFromXml(xmlContent);
            Assert.NotNull(auditFile);
            Assert.Equal("Empresa de demonstração (PRIVA)", auditFile.Header.CompanyName);
        }

        [Fact]
        public void ParseFromXml_InvalidXml_ThrowsSAFTValidationException()
        {
            var invalidXml = "<InvalidXML>This is not a valid SAF-T file</InvalidXML>";
            var ex = Assert.Throws<SAFTValidationException>(() => SAFTParser.ParseFromXml(invalidXml));
            Assert.NotNull(ex.ValidationErrors);
            Assert.NotEmpty(ex.ValidationErrors);
        }
    }
} 