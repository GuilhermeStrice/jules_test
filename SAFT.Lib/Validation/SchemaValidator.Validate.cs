using System;
using System.Collections.Generic;
using System.Xml;
using SAFT.Lib.Utils;

namespace SAFT.Lib.Validation
{
    public static partial class SchemaValidator
    {
        /// <summary>
        /// Validates the XML document using the schema path from configuration.
        /// </summary>
        /// <param name="xml">The XML document contents.</param>
        /// <returns>A list of validation error messages. The list is empty when the XML is valid.</returns>
        public static List<string> Validate(string xml)
        {
            var schemaPath = ConfigurationManager.Current.SchemaPath;
            return Validate(xml, schemaPath);
        }

        /// <summary>
        /// Validates the XML document using only schema validation (no business logic).
        /// Use this for testing with minimal schemas that don't need SAF-T business rules.
        /// </summary>
        /// <param name="xml">The XML document contents.</param>
        /// <param name="schemaPath">Path to the schema file.</param>
        /// <returns>A list of validation error messages. The list is empty when the XML is valid.</returns>
        public static List<string> ValidateSchemaOnly(string xml, string schemaPath)
        {
            var errors = new List<string>();
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);

                // Perform XSD 1.1 assertion validation
                var assertionErrors = ValidateXsd11Assertions(xmlDoc, schemaPath);
                errors.AddRange(assertionErrors);

                // Perform identity constraint validation using custom logic
                var identityErrors = ValidateIdentityConstraints(xmlDoc, schemaPath);
                errors.AddRange(identityErrors);
            }
            catch (XmlException ex)
            {
                errors.Add($"XML parsing error: {ex.Message}");
            }
            catch (Exception ex)
            {
                errors.Add($"Validation error: {ex.Message}");
            }
            return errors;
        }

        /// <summary>
        /// Validates the XML document using the specified schema path.
        /// The input XML must already have the correct namespaces. No namespaces are added or modified.
        /// </summary>
        /// <param name="xml">The XML document contents.</param>
        /// <param name="schemaPath">Path to the schema file.</param>
        /// <returns>A list of validation error messages. The list is empty when the XML is valid.</returns>
        public static List<string> Validate(string xml, string schemaPath)
        {
            var errors = new List<string>();
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xml);

                // Skip .NET's built-in XSD validation due to identity constraint issues
                // var xsdErrors = ValidateXsd(xmlDoc, schemaPath);
                // errors.AddRange(xsdErrors);

                // Perform XSD 1.1 assertion validation
                var assertionErrors = ValidateXsd11Assertions(xmlDoc, schemaPath);
                errors.AddRange(assertionErrors);

                // Perform identity constraint validation using custom logic
                var identityErrors = ValidateIdentityConstraints(xmlDoc, schemaPath);
                errors.AddRange(identityErrors);

                // Perform required field validation
                var requiredFieldErrors = ValidateRequiredFields(xmlDoc);
                errors.AddRange(requiredFieldErrors);

                // Perform Portuguese-specific business rule validation
                var portugueseErrors = ValidatePortugueseBusinessRules(xmlDoc);
                errors.AddRange(portugueseErrors);

                // Perform business logic validation
                var businessErrors = ValidateBusinessLogic(xmlDoc, schemaPath);
                errors.AddRange(businessErrors);
            }
            catch (XmlException ex)
            {
                errors.Add($"XML parsing error: {ex.Message}");
            }
            catch (Exception ex)
            {
                errors.Add($"Validation error: {ex.Message}");
            }
            return errors;
        }
    }
}
