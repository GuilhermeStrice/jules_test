using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Linq;
using SAFT.Lib.Utils;
using System.Configuration;

namespace SAFT.Lib.Utils
{
    /// <summary>
    /// Utility class for XML operations related to SAFT documents
    /// </summary>
    public static class XmlUtils
    {
        /// <summary>
        /// Serializes an object to XML string with proper formatting
        /// </summary>
        /// <typeparam name="T">Type of object to serialize</typeparam>
        /// <param name="obj">Object to serialize</param>
        /// <param name="encoding">Encoding to use (default: UTF-8)</param>
        /// <param name="indent">Whether to indent the XML (default: true)</param>
        /// <returns>XML string representation</returns>
        public static string SerializeToXml<T>(T obj, Encoding? encoding = null, bool indent = true)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            encoding ??= Encoding.UTF8;

            var serializer = new XmlSerializer(typeof(T));
            var settings = new XmlWriterSettings
            {
                Encoding = encoding,
                Indent = indent,
                IndentChars = "  ",
                OmitXmlDeclaration = false
            };

            using var stringWriter = new StringWriter();
            using var xmlWriter = XmlWriter.Create(stringWriter, settings);
            
            var ns = new XmlSerializerNamespaces();
            ns.Add("", ""); // Remove default namespace
            
            serializer.Serialize(xmlWriter, obj, ns);
            return stringWriter.ToString();
        }

        /// <summary>
        /// Deserializes XML string to object
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize to</typeparam>
        /// <param name="xml">XML string to deserialize</param>
        /// <returns>Deserialized object</returns>
        public static T? DeserializeFromXml<T>(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
                throw new ArgumentException("XML string cannot be null or empty", nameof(xml));

            var serializer = new XmlSerializer(typeof(T));
            using var stringReader = new StringReader(xml);
            using var xmlReader = XmlReader.Create(stringReader);
            
            return (T?)serializer.Deserialize(xmlReader);
        }

        /// <summary>
        /// Serializes object to XML file. If filePath is not rooted, it is placed in the configured output directory.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize</typeparam>
        /// <param name="obj">Object to serialize</param>
        /// <param name="filePath">File name or full file path</param>
        /// <param name="encoding">Encoding to use (default: UTF-8)</param>
        /// <param name="indent">Whether to indent the XML (default: true)</param>
        public static void SerializeToFile<T>(T obj, string filePath, Encoding? encoding = null, bool indent = true)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            string resolvedPath = filePath;
            if (!Path.IsPathRooted(filePath) || string.IsNullOrEmpty(Path.GetDirectoryName(filePath)))
            {
                var outputDir = ConfigurationManager.Current.OutputDirectory;
                Directory.CreateDirectory(outputDir);
                resolvedPath = Path.Combine(outputDir, Path.GetFileName(filePath));
            }
            var xml = SerializeToXml(obj, encoding, indent);
            File.WriteAllText(resolvedPath, xml, encoding ?? Encoding.UTF8);
        }

        /// <summary>
        /// Deserializes XML file to object
        /// </summary>
        /// <typeparam name="T">Type of object to deserialize to</typeparam>
        /// <param name="filePath">Path to XML file</param>
        /// <returns>Deserialized object</returns>
        public static T? DeserializeFromFile<T>(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"XML file not found: {filePath}");

            var serializer = new XmlSerializer(typeof(T));
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var xmlReader = XmlReader.Create(fileStream);
            
            return (T?)serializer.Deserialize(xmlReader);
        }

        /// <summary>
        /// Validates XML against XSD schema
        /// </summary>
        /// <param name="xml">XML string to validate</param>
        /// <param name="schemaPath">Path to XSD schema file</param>
        /// <returns>Validation result with any errors</returns>
        public static ValidationResult ValidateAgainstSchema(string xml, string schemaPath)
        {
            if (string.IsNullOrWhiteSpace(xml))
                throw new ArgumentException("XML string cannot be null or empty", nameof(xml));
            if (string.IsNullOrWhiteSpace(schemaPath))
                throw new ArgumentException("Schema path cannot be null or empty", nameof(schemaPath));
            if (!File.Exists(schemaPath))
                throw new FileNotFoundException($"Schema file not found: {schemaPath}");

            var errors = new List<string>();
            var schemaSet = new XmlSchemaSet();
            schemaSet.Add(null, schemaPath);

            var settings = new XmlReaderSettings
            {
                ValidationType = ValidationType.Schema,
                ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings
            };

            settings.ValidationEventHandler += (sender, e) =>
            {
                errors.Add($"{e.Severity}: {e.Message}");
            };

            try
            {
                using var stringReader = new StringReader(xml);
                using var xmlReader = XmlReader.Create(stringReader, settings);
                
                while (xmlReader.Read()) { } // Read through the document to trigger validation
            }
            catch (Exception ex)
            {
                errors.Add($"Validation exception: {ex.Message}");
            }

            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }

        /// <summary>
        /// Formats XML string with proper indentation
        /// </summary>
        /// <param name="xml">XML string to format</param>
        /// <param name="indentChars">Indentation characters (default: "  ")</param>
        /// <returns>Formatted XML string</returns>
        public static string FormatXml(string xml, string indentChars = "  ")
        {
            if (string.IsNullOrWhiteSpace(xml))
                return xml;

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);

                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = indentChars,
                    OmitXmlDeclaration = false
                };

                using var stringWriter = new StringWriter();
                using var xmlWriter = XmlWriter.Create(stringWriter, settings);
                
                doc.Save(xmlWriter);
                return stringWriter.ToString();
            }
            catch (XmlException)
            {
                // Return original if not valid XML
                return xml;
            }
        }

        /// <summary>
        /// Extracts specific element value from XML string
        /// </summary>
        /// <param name="xml">XML string</param>
        /// <param name="xpath">XPath expression</param>
        /// <returns>Element value or null if not found</returns>
        public static string? ExtractElementValue(string xml, string xpath)
        {
            if (string.IsNullOrWhiteSpace(xml) || string.IsNullOrWhiteSpace(xpath))
                return null;

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);

                var node = doc.SelectSingleNode(xpath);
                return node?.InnerText;
            }
            catch (XmlException)
            {
                return null;
            }
        }

        /// <summary>
        /// Extracts multiple element values from XML string
        /// </summary>
        /// <param name="xml">XML string</param>
        /// <param name="xpath">XPath expression</param>
        /// <returns>List of element values</returns>
        public static List<string> ExtractElementValues(string xml, string xpath)
        {
            if (string.IsNullOrWhiteSpace(xml) || string.IsNullOrWhiteSpace(xpath))
                return new List<string>();

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);

                var nodes = doc.SelectNodes(xpath);
                return nodes?.Cast<XmlNode>().Select(n => n.InnerText).ToList() ?? new List<string>();
            }
            catch (XmlException)
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Creates a minimal XML document for testing
        /// </summary>
        /// <param name="rootElement">Root element name</param>
        /// <param name="content">Content to add</param>
        /// <returns>Minimal XML string</returns>
        public static string CreateMinimalXml(string rootElement, Dictionary<string, string> content)
        {
            if (string.IsNullOrWhiteSpace(rootElement))
                throw new ArgumentException("Root element cannot be null or empty", nameof(rootElement));

            var doc = new XmlDocument();
            var root = doc.CreateElement(rootElement);
            doc.AppendChild(root);

            if (content != null)
            {
                foreach (var kvp in content)
                {
                    var element = doc.CreateElement(kvp.Key);
                    element.InnerText = kvp.Value;
                    root.AppendChild(element);
                }
            }

            return doc.OuterXml;
        }
    }

    /// <summary>
    /// Result of XML validation
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Whether the XML is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// List of validation errors
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
    }
} 