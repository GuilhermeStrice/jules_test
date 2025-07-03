using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;

namespace SAFT.Lib.Validation
{
    public static partial class SchemaValidator
    {
        /// <summary>
        /// Gets valid values from a constraint for keyref validation.
        /// </summary>
        /// <param name="xmlDoc">The XML document.</param>
        /// <param name="constraint">The constraint to get values from.</param>
        /// <param name="nsManager">The namespace manager.</param>
        /// <param name="hasNamespace">Whether the schema has namespaces.</param>
        /// <returns>Set of valid values.</returns>
        private static HashSet<string> GetValidValuesFromConstraint(XmlDocument xmlDoc, object constraint, XmlNamespaceManager nsManager, bool hasNamespace)
        {
            var values = new HashSet<string>();
            if (constraint is KeyConstraint key)
            {
                var selector = hasNamespace ? ProcessXPathForNamespaces(key.Selector) : key.Selector;
                var field = hasNamespace ? ProcessXPathForNamespaces(key.Field) : key.Field;

                var selectorNodes = EvaluateXPathSelector(xmlDoc, selector, nsManager);
                foreach (XmlNode node in selectorNodes)
                {
                    var value = EvaluateXPathField(node, field, nsManager);
                    if (!string.IsNullOrEmpty(value))
                    {
                        values.Add(value);
                    }
                }
            }
            else if (constraint is UniqueConstraint unique)
            {
                var selector = hasNamespace ? ProcessXPathForNamespaces(unique.Selector) : unique.Selector;
                var field = hasNamespace ? ProcessXPathForNamespaces(unique.Field) : unique.Field;

                var selectorNodes = EvaluateXPathSelector(xmlDoc, selector, nsManager);
                foreach (XmlNode node in selectorNodes)
                {
                    var value = EvaluateXPathField(node, field, nsManager);
                    if (!string.IsNullOrEmpty(value))
                    {
                        values.Add(value);
                    }
                }
            }
            return values;
        }

        /// <summary>
        /// Evaluates an XPath selector expression.
        /// </summary>
        /// <param name="xmlDoc">The XML document.</param>
        /// <param name="selector">The XPath selector expression.</param>
        /// <param name="nsManager">The namespace manager.</param>
        /// <returns>Collection of matching nodes.</returns>
        private static XmlNodeList EvaluateXPathSelector(XmlDocument xmlDoc, string selector, XmlNamespaceManager nsManager)
        {
            try
            {
                // If the selector starts with /, it's an absolute path
                if (selector.StartsWith("/"))
                {
                    return xmlDoc.SelectNodes(selector, nsManager);
                }

                // For relative selectors, convert them to absolute paths for better compatibility
                // This is especially important for non-namespaced schemas
                var absoluteSelector = "//" + selector;
                return xmlDoc.SelectNodes(absoluteSelector, nsManager);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DEBUG] XPath selector error: {ex.Message}");
                return xmlDoc.SelectNodes("//*[false()]", nsManager); // Return empty node list
            }
        }

        /// <summary>
        /// Evaluates an XPath field expression relative to a context node.
        /// </summary>
        /// <param name="contextNode">The context node.</param>
        /// <param name="field">The XPath field expression.</param>
        /// <returns>The field value.</returns>
        private static string EvaluateXPathField(XmlNode contextNode, string field, XmlNamespaceManager nsManager)
        {
            try
            {
                // Check if the namespace manager has the 'ns' prefix to determine if we should process namespaces
                var hasNamespace = nsManager.HasNamespace("ns");
                var processedField = hasNamespace ? ProcessXPathForNamespaces(field) : field;
                var node = contextNode.SelectSingleNode(processedField, nsManager);
                return node?.InnerText ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Processes XPath expressions to handle namespace prefixes.
        /// </summary>
        /// <param name="xpath">The XPath expression.</param>
        /// <returns>The processed XPath expression.</returns>
        private static string ProcessXPathForNamespaces(string xpath)
        {
            // Add ns: prefix to element names that do not already have a prefix and are not attributes
            // This regex matches element names not preceded by @ or a prefix
            return Regex.Replace(
                xpath,
                @"(?<=/|^)(?!@)([a-zA-Z_][\w\-\.]*)(?!:)\b",
                "ns:$1"
            );
        }
    }
}
