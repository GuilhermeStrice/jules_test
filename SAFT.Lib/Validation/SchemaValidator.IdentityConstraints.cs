using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace SAFT.Lib.Validation
{
    public static partial class SchemaValidator
    {
        /// <summary>
        /// Represents a collection of identity constraints.
        /// </summary>
        private class IdentityConstraints
        {
            public List<UniqueConstraint> Uniques { get; set; } = new List<UniqueConstraint>();
            public List<KeyConstraint> Keys { get; set; } = new List<KeyConstraint>();
            public List<KeyRefConstraint> KeyRefs { get; set; } = new List<KeyRefConstraint>();
        }

        /// <summary>
        /// Represents a unique constraint.
        /// </summary>
        private class UniqueConstraint
        {
            public string Name { get; set; } = "";
            public string Selector { get; set; } = "";
            public string Field { get; set; } = "";
        }

        /// <summary>
        /// Represents a key constraint.
        /// </summary>
        private class KeyConstraint
        {
            public string Name { get; set; } = "";
            public string Selector { get; set; } = "";
            public string Field { get; set; } = "";
        }

        /// <summary>
        /// Represents a keyref constraint.
        /// </summary>
        private class KeyRefConstraint
        {
            public string Name { get; set; } = "";
            public string Refer { get; set; } = "";
            public string Selector { get; set; } = "";
            public string Field { get; set; } = "";
        }

        /// <summary>
        /// Validates identity constraints (xs:unique, xs:key, xs:keyref) in the XML document.
        /// </summary>
        /// <param name="xmlDoc">The XML document to validate.</param>
        /// <param name="schemaPath">Path to the schema file.</param>
        /// <returns>List of identity constraint validation errors.</returns>
        private static List<string> ValidateIdentityConstraints(XmlDocument xmlDoc, string schemaPath)
        {
            var errors = new List<string>();
            try
            {
                // Load the schema as XDocument to parse identity constraints
                var schemaDoc = XDocument.Load(schemaPath);
                var ns = XNamespace.Get("http://www.w3.org/2001/XMLSchema");
                // Parse all identity constraints
                var constraints = ParseIdentityConstraints(schemaDoc, ns);
                // Determine if this is a namespaced schema by checking if the root element has a targetNamespace
                var rootElement = schemaDoc.Root;
                var targetNamespace = rootElement?.Attribute("targetNamespace")?.Value;
                var hasNamespace = !string.IsNullOrEmpty(targetNamespace);
                // Create appropriate namespace manager
                var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
                if (hasNamespace)
                {
                    // Use the target namespace from the schema
                    nsManager.AddNamespace("ns", targetNamespace);
                }
                else
                {
                    // No namespace - use empty prefix for local names
                    nsManager.AddNamespace("", "");
                }
                // Validate unique constraints
                foreach (var unique in constraints.Uniques)
                {
                    var uniqueErrors = ValidateUniqueConstraint(xmlDoc, unique, nsManager, hasNamespace);
                    errors.AddRange(uniqueErrors);
                }
                // Validate key constraints
                foreach (var key in constraints.Keys)
                {
                    var keyErrors = ValidateKeyConstraint(xmlDoc, key, nsManager, hasNamespace);
                    errors.AddRange(keyErrors);
                }
                // Validate keyref constraints
                foreach (var keyref in constraints.KeyRefs)
                {
                    var keyrefErrors = ValidateKeyRefConstraint(xmlDoc, keyref, constraints, nsManager, hasNamespace);
                    errors.AddRange(keyrefErrors);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Identity constraint validation error: {ex.Message}");
            }
            return errors;
        }

        /// <summary>
        /// Parses identity constraints from the schema.
        /// </summary>
        /// <param name="schemaDoc">The schema document.</param>
        /// <param name="ns">The XSD namespace.</param>
        /// <returns>Collection of parsed identity constraints.</returns>
        private static IdentityConstraints ParseIdentityConstraints(XDocument schemaDoc, XNamespace ns)
        {
            var constraints = new IdentityConstraints();
            // Parse unique constraints
            var uniques = schemaDoc.Descendants(ns + "unique");
            foreach (var unique in uniques)
            {
                var name = unique.Attribute("name")?.Value;
                var selector = unique.Element(ns + "selector")?.Attribute("xpath")?.Value;
                var field = unique.Element(ns + "field")?.Attribute("xpath")?.Value;
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(selector) && !string.IsNullOrEmpty(field))
                {
                    constraints.Uniques.Add(new UniqueConstraint
                    {
                        Name = name,
                        Selector = selector,
                        Field = field
                    });
                }
            }
            // Parse key constraints
            var keys = schemaDoc.Descendants(ns + "key");
            foreach (var key in keys)
            {
                var name = key.Attribute("name")?.Value;
                var selector = key.Element(ns + "selector")?.Attribute("xpath")?.Value;
                var field = key.Element(ns + "field")?.Attribute("xpath")?.Value;
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(selector) && !string.IsNullOrEmpty(field))
                {
                    constraints.Keys.Add(new KeyConstraint
                    {
                        Name = name,
                        Selector = selector,
                        Field = field
                    });
                }
            }
            // Parse keyref constraints
            var keyrefs = schemaDoc.Descendants(ns + "keyref");
            foreach (var keyref in keyrefs)
            {
                var name = keyref.Attribute("name")?.Value;
                var refer = keyref.Attribute("refer")?.Value;
                var selector = keyref.Element(ns + "selector")?.Attribute("xpath")?.Value;
                var field = keyref.Element(ns + "field")?.Attribute("xpath")?.Value;
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(refer) && !string.IsNullOrEmpty(selector) && !string.IsNullOrEmpty(field))
                {
                    constraints.KeyRefs.Add(new KeyRefConstraint
                    {
                        Name = name,
                        Refer = refer,
                        Selector = selector,
                        Field = field
                    });
                }
            }
            return constraints;
        }

        /// <summary>
        /// Validates a unique constraint.
        /// </summary>
        /// <param name="xmlDoc">The XML document.</param>
        /// <param name="unique">The unique constraint to validate.</param>
        /// <param name="nsManager">The namespace manager.</param>
        /// <param name="hasNamespace">Whether the schema has namespaces.</param>
        /// <returns>List of validation errors.</returns>
        private static List<string> ValidateUniqueConstraint(XmlDocument xmlDoc, UniqueConstraint unique, XmlNamespaceManager nsManager, bool hasNamespace)
        {
            var errors = new List<string>();
            try
            {
                var selector = hasNamespace ? ProcessXPathForNamespaces(unique.Selector) : unique.Selector;
                var field = hasNamespace ? ProcessXPathForNamespaces(unique.Field) : unique.Field;
                Console.WriteLine($"[DEBUG] Unique constraint '{unique.Name}': selector='{selector}', field='{field}', hasNamespace={hasNamespace}");
                var nodes = EvaluateXPathSelector(xmlDoc, selector, nsManager);
                Console.WriteLine($"[DEBUG] Found {nodes.Count} nodes for selector '{selector}'");
                if (nodes.Count == 0)
                {
                    Console.WriteLine($"[DEBUG] No nodes found for selector '{selector}' - this might indicate an XPath issue");
                    Console.WriteLine($"[DEBUG] XML root element: {xmlDoc.DocumentElement?.Name}");
                    Console.WriteLine($"[DEBUG] XML root element children: {string.Join(", ", xmlDoc.DocumentElement?.ChildNodes.Cast<XmlNode>().Select(n => n.Name) ?? new string[0])}");
                }
                var values = new HashSet<string>();
                var duplicates = new HashSet<string>();
                foreach (XmlNode node in nodes)
                {
                    var value = EvaluateXPathField(node, field, nsManager);
                    Console.WriteLine($"[DEBUG] Node value: '{value}'");
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (values.Contains(value))
                        {
                            duplicates.Add(value);
                        }
                        else
                        {
                            values.Add(value);
                        }
                    }
                }
                if (duplicates.Count > 0)
                {
                    errors.Add($"duplicate key sequence '{string.Join(", ", duplicates)}' in unique identity constraint '{unique.Name}'");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Unique constraint validation error for '{unique.Name}': {ex.Message}");
            }
            return errors;
        }

        /// <summary>
        /// Validates a key constraint.
        /// </summary>
        /// <param name="xmlDoc">The XML document.</param>
        /// <param name="key">The key constraint to validate.</param>
        /// <param name="nsManager">The namespace manager.</param>
        /// <param name="hasNamespace">Whether the schema has namespaces.</param>
        /// <returns>List of validation errors.</returns>
        private static List<string> ValidateKeyConstraint(XmlDocument xmlDoc, KeyConstraint key, XmlNamespaceManager nsManager, bool hasNamespace)
        {
            var errors = new List<string>();
            try
            {
                var selector = hasNamespace ? ProcessXPathForNamespaces(key.Selector) : key.Selector;
                var field = hasNamespace ? ProcessXPathForNamespaces(key.Field) : key.Field;
                var nodes = EvaluateXPathSelector(xmlDoc, selector, nsManager);
                var values = new HashSet<string>();
                var duplicates = new HashSet<string>();
                var nullValues = new List<string>();
                foreach (XmlNode node in nodes)
                {
                    var value = EvaluateXPathField(node, field, nsManager);
                    if (string.IsNullOrEmpty(value))
                    {
                        nullValues.Add($"null value in key constraint '{key.Name}'");
                    }
                    else if (values.Contains(value))
                    {
                        duplicates.Add(value);
                    }
                    else
                    {
                        values.Add(value);
                    }
                }
                if (nullValues.Count > 0)
                {
                    errors.AddRange(nullValues);
                }
                if (duplicates.Count > 0)
                {
                    errors.Add($"duplicate key sequence '{string.Join(", ", duplicates)}' in key identity constraint '{key.Name}'");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Key constraint validation error for '{key.Name}': {ex.Message}");
            }
            return errors;
        }

        /// <summary>
        /// Validates a keyref constraint.
        /// </summary>
        /// <param name="xmlDoc">The XML document.</param>
        /// <param name="keyref">The keyref constraint to validate.</param>
        /// <param name="constraints">All identity constraints.</param>
        /// <param name="nsManager">The namespace manager.</param>
        /// <param name="hasNamespace">Whether the schema has namespaces.</param>
        /// <returns>List of validation errors.</returns>
        private static List<string> ValidateKeyRefConstraint(XmlDocument xmlDoc, KeyRefConstraint keyref, IdentityConstraints constraints, XmlNamespaceManager nsManager, bool hasNamespace)
        {
            var errors = new List<string>();
            try
            {
                // Find the referenced constraint
                var referencedConstraint = constraints.Keys.FirstOrDefault(k => k.Name == keyref.Refer) 
                    ?? constraints.Uniques.FirstOrDefault(u => u.Name == keyref.Refer) as object;
                if (referencedConstraint == null)
                {
                    errors.Add($"Keyref constraint '{keyref.Name}' refers to non-existent constraint '{keyref.Refer}'");
                    return errors;
                }
                // Get valid values from the referenced constraint
                var validValues = GetValidValuesFromConstraint(xmlDoc, referencedConstraint, nsManager, hasNamespace);
                var selector = hasNamespace ? ProcessXPathForNamespaces(keyref.Selector) : keyref.Selector;
                var field = hasNamespace ? ProcessXPathForNamespaces(keyref.Field) : keyref.Field;
                var nodes = EvaluateXPathSelector(xmlDoc, selector, nsManager);
                var invalidReferences = new HashSet<string>();
                foreach (XmlNode node in nodes)
                {
                    var value = EvaluateXPathField(node, field, nsManager);
                    if (!string.IsNullOrEmpty(value) && !validValues.Contains(value))
                    {
                        invalidReferences.Add(value);
                    }
                }
                if (invalidReferences.Count > 0)
                {
                    errors.Add($"Keyref fails to refer to key '{keyref.Refer}': invalid values '{string.Join(", ", invalidReferences)}'");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Keyref constraint validation error for '{keyref.Name}': {ex.Message}");
            }
            return errors;
        }
    }
}
