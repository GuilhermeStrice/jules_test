using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Provides functionality to generate SAF-T XML files from AuditFile objects.
    /// </summary>
    public class SAFTGenerator
    {
        /// <summary>
        /// Serializes the given AuditFile object to a SAF-T XML string.
        /// </summary>
        /// <param name="auditFile">The AuditFile object to serialize.</param>
        /// <param name="encoding">The encoding to use for the XML output (default: UTF-8).</param>
        /// <returns>The serialized SAF-T XML as a string.</returns>
        public string GenerateXml(AuditFile auditFile, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var xmlSerializer = new XmlSerializer(typeof(AuditFile));
            var xmlSettings = new XmlWriterSettings
            {
                Indent = true,
                Encoding = encoding,
                OmitXmlDeclaration = false
            };
            using var stringWriter = new Utf8StringWriter(encoding);
            using var xmlWriter = XmlWriter.Create(stringWriter, xmlSettings);
            xmlSerializer.Serialize(xmlWriter, auditFile);
            return stringWriter.ToString();
        }

        /// <summary>
        /// Serializes the given AuditFile object to a SAF-T XML file.
        /// </summary>
        /// <param name="auditFile">The AuditFile object to serialize.</param>
        /// <param name="filePath">The file path to write the XML to.</param>
        /// <param name="encoding">The encoding to use for the XML output (default: UTF-8).</param>
        public void GenerateXmlFile(AuditFile auditFile, string filePath, Encoding? encoding = null)
        {
            var xml = GenerateXml(auditFile, encoding);
            File.WriteAllText(filePath, xml, encoding ?? Encoding.UTF8);
        }

        // Helper for correct encoding in StringWriter
        private class Utf8StringWriter : StringWriter
        {
            private readonly Encoding _encoding;
            public Utf8StringWriter(Encoding encoding) : base() => _encoding = encoding;
            public override Encoding Encoding => _encoding;
        }
    }
} 