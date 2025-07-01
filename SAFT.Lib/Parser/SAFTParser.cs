using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Text;

using SAFT.Lib.Utils;
using SAFT.Lib.Validation;

namespace SAFT.Lib.Parser
{
    /// <summary>
    /// Parser for SAF-T XML files that validates before parsing.
    /// </summary>
    public static class SAFTParser
    {
        /// <summary>
        /// Parses a SAF-T XML file from a file path, validating it first.
        /// </summary>
        /// <param name="filePath">Path to the XML file</param>
        /// <param name="validateOnly">If true, only validates without parsing</param>
        /// <returns>Parsed AuditFile object</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist</exception>
        /// <exception cref="SAFTValidationException">Thrown when validation fails</exception>
        /// <exception cref="SAFTParsingException">Thrown when parsing fails</exception>
        public static AuditFile ParseFromFile(string filePath, bool validateOnly = false)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"SAF-T file not found: {filePath}");

            string xmlContent = File.ReadAllText(filePath, Encoding.UTF8);
            return ParseFromXml(xmlContent, validateOnly);
        }

        /// <summary>
        /// Parses a SAF-T XML string, validating it first.
        /// </summary>
        /// <param name="xmlContent">XML content as string</param>
        /// <param name="validateOnly">If true, only validates without parsing</param>
        /// <returns>Parsed AuditFile object</returns>
        /// <exception cref="SAFTValidationException">Thrown when validation fails</exception>
        /// <exception cref="SAFTParsingException">Thrown when parsing fails</exception>
        public static AuditFile ParseFromXml(string xmlContent, bool validateOnly = false)
        {
            // Step 1: Validate the XML
            var validationErrors = SchemaValidator.Validate(xmlContent);
            if (validationErrors.Count > 0)
            {
                throw new SAFTValidationException("SAF-T XML validation failed", validationErrors);
            }

            if (validateOnly)
            {
                // Return a minimal AuditFile object for validation-only mode
                return new AuditFile();
            }

            // Step 2: Parse the XML into AuditFile object
            try
            {
                var auditFile = XmlUtils.DeserializeFromXml<AuditFile>(xmlContent);
                if (auditFile == null)
                {
                    throw new SAFTParsingException("Failed to deserialize XML into AuditFile object");
                }

                return auditFile;
            }
            catch (XmlException ex)
            {
                throw new SAFTParsingException("XML parsing failed", ex);
            }
            catch (InvalidOperationException ex)
            {
                throw new SAFTParsingException("XML deserialization failed", ex);
            }
        }

        /// <summary>
        /// Validates a SAF-T XML file without parsing it.
        /// </summary>
        /// <param name="filePath">Path to the XML file</param>
        /// <returns>List of validation errors (empty if valid)</returns>
        public static List<string> ValidateFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"SAF-T file not found: {filePath}");

            string xmlContent = File.ReadAllText(filePath, Encoding.UTF8);
            return SchemaValidator.Validate(xmlContent);
        }

        /// <summary>
        /// Validates a SAF-T XML string without parsing it.
        /// </summary>
        /// <param name="xmlContent">XML content as string</param>
        /// <returns>List of validation errors (empty if valid)</returns>
        public static List<string> ValidateXml(string xmlContent)
        {
            return SchemaValidator.Validate(xmlContent);
        }

        /// <summary>
        /// Parses a SAF-T XML file and returns both the parsed object and validation results.
        /// </summary>
        /// <param name="filePath">Path to the XML file</param>
        /// <returns>Tuple containing the parsed AuditFile and validation errors</returns>
        public static (AuditFile? AuditFile, List<string> ValidationErrors) ParseWithValidation(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"SAF-T file not found: {filePath}");

            string xmlContent = File.ReadAllText(filePath, Encoding.UTF8);
            return ParseWithValidationFromXml(xmlContent);
        }

        /// <summary>
        /// Parses a SAF-T XML string and returns both the parsed object and validation results.
        /// </summary>
        /// <param name="xmlContent">XML content as string</param>
        /// <returns>Tuple containing the parsed AuditFile and validation errors</returns>
        public static (AuditFile? AuditFile, List<string> ValidationErrors) ParseWithValidationFromXml(string xmlContent)
        {
            // Validate first
            var validationErrors = SchemaValidator.Validate(xmlContent);
            
            AuditFile? auditFile = null;
            if (validationErrors.Count == 0)
            {
                try
                {
                    auditFile = XmlUtils.DeserializeFromXml<AuditFile>(xmlContent);
                }
                catch (Exception)
                {
                    // If parsing fails, we still return the validation errors
                    // and null for the audit file
                }
            }

            return (auditFile, validationErrors);
        }
    }

    /// <summary>
    /// Exception thrown when SAF-T validation fails.
    /// </summary>
    public class SAFTValidationException : Exception
    {
        /// <summary>
        /// List of validation errors.
        /// </summary>
        public List<string> ValidationErrors { get; }

        public SAFTValidationException(string message, List<string> validationErrors) 
            : base(message)
        {
            ValidationErrors = validationErrors;
        }

        public SAFTValidationException(string message, List<string> validationErrors, Exception innerException) 
            : base(message, innerException)
        {
            ValidationErrors = validationErrors;
        }
    }

    /// <summary>
    /// Exception thrown when SAF-T parsing fails.
    /// </summary>
    public class SAFTParsingException : Exception
    {
        public SAFTParsingException(string message) : base(message) { }
        public SAFTParsingException(string message, Exception innerException) : base(message, innerException) { }
    }
} 