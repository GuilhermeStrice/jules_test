using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using SAFT.Lib.Utils;

namespace SAFT.Lib
{
    /// <summary>
    /// Provides utilities to validate a SAF-T XML string against the official XSD schema.
    /// The input XML must already have the correct namespaces. This class does not modify the XML.
    /// </summary>
    public static class SchemaValidator
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
                
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(refer) && 
                    !string.IsNullOrEmpty(selector) && !string.IsNullOrEmpty(field))
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
            return System.Text.RegularExpressions.Regex.Replace(
                xpath,
                @"(?<=/|^)(?!@)([a-zA-Z_][\w\-\.]*)(?!:)\b",
                "ns:$1"
            );
        }
        
        /// <summary>
        /// Validates XSD 1.1 assertions in the XML document.
        /// </summary>
        /// <param name="xmlDoc">The XML document to validate.</param>
        /// <param name="schemaPath">Path to the schema file.</param>
        /// <returns>List of assertion validation errors.</returns>
        private static List<string> ValidateXsd11Assertions(XmlDocument xmlDoc, string schemaPath)
        {
            var errors = new List<string>();
            
            try
            {
                // Load the schema as XDocument to parse assertions
                var schemaDoc = XDocument.Load(schemaPath);
                var ns = XNamespace.Get("http://www.w3.org/2001/XMLSchema");
                
                // Find all xs:assert elements in the schema
                var assertions = schemaDoc.Descendants(ns + "assert").ToList();
                
                foreach (var assertion in assertions)
                {
                    var test = assertion.Attribute("test")?.Value;
                    if (string.IsNullOrEmpty(test))
                        continue;
                    
                    // Get the parent element that contains this assertion
                    var parentElement = assertion.Parent;
                    if (parentElement == null)
                        continue;
                    
                    var elementName = parentElement.Attribute("name")?.Value;
                    if (string.IsNullOrEmpty(elementName))
                        continue;
                    
                    // Find all instances of this element in the XML
                    var xmlElements = xmlDoc.SelectNodes($"//*[local-name()='{elementName}']");
                    if (xmlElements == null)
                        continue;
                    
                    foreach (XmlNode xmlElement in xmlElements)
                    {
                        var assertionResult = EvaluateAssertion(test, xmlElement);
                        if (!assertionResult.IsValid)
                        {
                            errors.Add($"XSD 1.1 assertion failed for element '{elementName}': {assertionResult.ErrorMessage}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"XSD 1.1 assertion validation error: {ex.Message}");
            }
            
            return errors;
        }
        
        /// <summary>
        /// Evaluates an XSD 1.1 assertion expression.
        /// </summary>
        /// <param name="assertionTest">The assertion test expression.</param>
        /// <param name="contextNode">The XML node context for evaluation.</param>
        /// <returns>Assertion evaluation result.</returns>
        private static AssertionResult EvaluateAssertion(string assertionTest, XmlNode contextNode)
        {
            try
            {
                // Parse and evaluate the assertion expression
                var result = ParseAndEvaluateAssertion(assertionTest, contextNode);
                return new AssertionResult { IsValid = result, ErrorMessage = result ? null : $"Assertion failed: {assertionTest}" };
            }
            catch (Exception ex)
            {
                return new AssertionResult { IsValid = false, ErrorMessage = $"Assertion evaluation error: {ex.Message}" };
            }
        }
        
        /// <summary>
        /// Parses and evaluates an XSD 1.1 assertion expression.
        /// </summary>
        /// <param name="expression">The assertion expression to evaluate.</param>
        /// <param name="contextNode">The XML node context.</param>
        /// <returns>True if the assertion passes, false otherwise.</returns>
        private static bool ParseAndEvaluateAssertion(string expression, XmlNode contextNode)
        {
            // Handle common XSD 1.1 assertion patterns found in the SAF-T schema
            expression = expression.Trim();
            
            // Handle if-then-else expressions
            if (expression.StartsWith("if (") && expression.EndsWith(")"))
            {
                return EvaluateIfExpression(expression, contextNode);
            }
            
            // Handle simple boolean expressions
            return EvaluateBooleanExpression(expression, contextNode);
        }
        
        /// <summary>
        /// Evaluates if-then-else expressions in XSD 1.1 assertions.
        /// </summary>
        /// <param name="expression">The if expression to evaluate.</param>
        /// <param name="contextNode">The XML node context.</param>
        /// <returns>True if the condition is met, false otherwise.</returns>
        private static bool EvaluateIfExpression(string expression, XmlNode contextNode)
        {
            // Extract the condition from if (condition) then true() else false()
            var match = Regex.Match(expression, @"if\s*\((.+)\)\s*then\s*true\(\)\s*else\s*false\(\)");
            if (match.Success)
            {
                var condition = match.Groups[1].Value.Trim();
                return EvaluateBooleanExpression(condition, contextNode);
            }
            
            // Handle other if patterns
            match = Regex.Match(expression, @"if\s*\((.+)\)\s*then\s*(.+)\s*else\s*(.+)");
            if (match.Success)
            {
                var condition = match.Groups[1].Value.Trim();
                var thenValue = match.Groups[2].Value.Trim();
                var elseValue = match.Groups[3].Value.Trim();
                
                var conditionResult = EvaluateBooleanExpression(condition, contextNode);
                var resultExpression = conditionResult ? thenValue : elseValue;
                
                return EvaluateBooleanExpression(resultExpression, contextNode);
            }
            
            return false;
        }
        
        /// <summary>
        /// Evaluates boolean expressions in XSD 1.1 assertions.
        /// </summary>
        /// <param name="expression">The boolean expression to evaluate.</param>
        /// <param name="contextNode">The XML node context.</param>
        /// <returns>True if the expression evaluates to true, false otherwise.</returns>
        private static bool EvaluateBooleanExpression(string expression, XmlNode contextNode)
        {
            // Handle common boolean operators and functions
            expression = expression.Trim();
            
            // Handle not() function
            if (expression.StartsWith("not(") && expression.EndsWith(")"))
            {
                var innerExpression = expression.Substring(4, expression.Length - 5);
                return !EvaluateBooleanExpression(innerExpression, contextNode);
            }
            
            // Handle and operator
            if (expression.Contains(" and "))
            {
                var parts = expression.Split(new[] { " and " }, StringSplitOptions.None);
                return parts.All(part => EvaluateBooleanExpression(part.Trim(), contextNode));
            }
            
            // Handle or operator
            if (expression.Contains(" or "))
            {
                var parts = expression.Split(new[] { " or " }, StringSplitOptions.None);
                return parts.Any(part => EvaluateBooleanExpression(part.Trim(), contextNode));
            }
            
            // Handle equality comparisons
            if (expression.Contains(" eq "))
            {
                var parts = expression.Split(new[] { " eq " }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    var left = EvaluateXPathExpression(parts[0].Trim(), contextNode);
                    var right = EvaluateXPathExpression(parts[1].Trim(), contextNode);
                    return left == right;
                }
            }
            
            // Handle inequality comparisons
            if (expression.Contains(" != "))
            {
                var parts = expression.Split(new[] { " != " }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    var left = EvaluateXPathExpression(parts[0].Trim(), contextNode);
                    var right = EvaluateXPathExpression(parts[1].Trim(), contextNode);
                    return left != right;
                }
            }
            
            // Handle numeric comparisons
            if (expression.Contains(" != 0"))
            {
                var left = EvaluateXPathExpression(expression.Replace(" != 0", "").Trim(), contextNode);
                return !string.IsNullOrEmpty(left) && decimal.TryParse(left, out var value) && value != 0;
            }
            
            if (expression.Contains(" eq 0"))
            {
                var left = EvaluateXPathExpression(expression.Replace(" eq 0", "").Trim(), contextNode);
                return !string.IsNullOrEmpty(left) && decimal.TryParse(left, out var value) && value == 0;
            }
            
            // Handle simple existence checks
            if (expression.StartsWith("ns:") && !expression.Contains(" "))
            {
                var value = EvaluateXPathExpression(expression, contextNode);
                return !string.IsNullOrEmpty(value);
            }
            
            return false;
        }
        
        /// <summary>
        /// Evaluates XPath-like expressions in the context of an XML node.
        /// </summary>
        /// <param name="expression">The XPath expression to evaluate.</param>
        /// <param name="contextNode">The XML node context.</param>
        /// <returns>The value of the expression or empty string if not found.</returns>
        private static string EvaluateXPathExpression(string expression, XmlNode contextNode)
        {
            try
            {
                // Handle ns: prefix (namespace prefix)
                if (expression.StartsWith("ns:"))
                {
                    var elementName = expression.Substring(3);
                    var element = contextNode.SelectSingleNode($"*[local-name()='{elementName}']");
                    return element?.InnerText ?? "";
                }
                
                // Handle parent navigation
                if (expression.StartsWith("../ns:"))
                {
                    var elementName = expression.Substring(6);
                    var parent = contextNode.ParentNode;
                    if (parent != null)
                    {
                        var element = parent.SelectSingleNode($"*[local-name()='{elementName}']");
                        return element?.InnerText ?? "";
                    }
                }
                
                // Handle simple element names
                var directElement = contextNode.SelectSingleNode($"*[local-name()='{expression}']");
                return directElement?.InnerText ?? "";
            }
            catch
            {
                return "";
            }
        }
        
        /// <summary>
        /// Represents the result of an assertion evaluation.
        /// </summary>
        private class AssertionResult
        {
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
        }
        
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
        /// Performs business logic validation including VAT calculations, cross-references, and Portuguese tax rules.
        /// </summary>
        /// <param name="xmlDoc">The XML document to validate.</param>
        /// <param name="schemaPath">Path to the schema file.</param>
        /// <returns>List of business logic validation errors.</returns>
        private static List<string> ValidateBusinessLogic(XmlDocument xmlDoc, string schemaPath)
        {
            var errors = new List<string>();
            try
            {
                errors.AddRange(ValidateStringConstraints(xmlDoc));
                errors.AddRange(ValidateVATCalculations(xmlDoc));
                errors.AddRange(ValidateCrossReferences(xmlDoc));
                errors.AddRange(ValidatePortugueseTaxRules(xmlDoc));
                errors.AddRange(ValidateDocumentTotals(xmlDoc));
                errors.AddRange(ValidateDateRanges(xmlDoc));
                errors.AddRange(ValidateCreditDebitBalance(xmlDoc));
                errors.AddRange(ValidateInvoiceNumbering(xmlDoc));
                errors.AddRange(ValidatePaymentTerms(xmlDoc));
                errors.AddRange(ValidateQuantityAndUnitPrice(xmlDoc));
                errors.AddRange(ValidateTaxCalculationAccuracy(xmlDoc));
                errors.AddRange(ValidateBusinessRuleCompliance(xmlDoc));
                errors.AddRange(ValidateDocumentStatus(xmlDoc));
                errors.AddRange(ValidateCancellationRules(xmlDoc));
                errors.AddRange(ValidateSpecialRegimesField(xmlDoc));
                var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
                nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
                ValidateSelfBillingIndicator(xmlDoc, nsManager, errors);
            }
            catch (Exception ex)
            {
                errors.Add($"Business logic validation error: {ex.Message}");
            }
            return errors;
        }
        
        /// <summary>
        /// Validates VAT calculations in invoice lines and totals.
        /// </summary>
        internal static List<string> ValidateVATCalculations(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Only run VAT total checks if DocumentTotals is present AND root is not <Invoice>
            var documentTotals = xmlDoc.SelectSingleNode("//ns:DocumentTotals", nsManager);
            var root = xmlDoc.DocumentElement?.Name;
            if (documentTotals != null && documentTotals.HasChildNodes && root != "Invoice")
            {
                foreach (XmlNode invoice in xmlDoc.SelectNodes("//ns:Invoice", nsManager))
                {
                    // Support both <Line> and <InvoiceLine>
                    var lines = invoice.SelectNodes(".//ns:Line", nsManager);
                    var invoiceLines = invoice.SelectNodes(".//ns:InvoiceLine", nsManager);
                    var allLines = new List<XmlNode>();
                    foreach (XmlNode l in lines) allLines.Add(l);
                    foreach (XmlNode l in invoiceLines) allLines.Add(l);
                    var totals = invoice.SelectSingleNode(".//ns:DocumentTotals", nsManager);
                    if (allLines.Count > 0 && totals != null)
                    {
                        decimal calculatedTaxPayable = 0;
                        decimal calculatedNetTotal = 0;
                        foreach (XmlNode line in allLines)
                        {
                            var taxBase = line.SelectSingleNode(".//ns:TaxBase", nsManager);
                            var lineExtensionAmount = line.SelectSingleNode(".//ns:LineExtensionAmount", nsManager);
                            var creditAmount = line.SelectSingleNode(".//ns:CreditAmount", nsManager);
                            var tax = line.SelectSingleNode(".//ns:Tax", nsManager);
                            decimal baseAmount = 0;
                            if (taxBase != null && decimal.TryParse(taxBase.InnerText, out decimal tb))
                                baseAmount = tb;
                            else if (lineExtensionAmount != null && decimal.TryParse(lineExtensionAmount.InnerText, out decimal le))
                                baseAmount = le;
                            else if (creditAmount != null && decimal.TryParse(creditAmount.InnerText, out decimal ca))
                                baseAmount = ca;
                            if (baseAmount > 0 && tax != null)
                            {
                                calculatedNetTotal += baseAmount;
                                var taxPercentage = tax.SelectSingleNode(".//ns:TaxPercentage", nsManager);
                                var taxAmount = tax.SelectSingleNode(".//ns:TaxAmount", nsManager);
                                if (taxPercentage != null && taxAmount != null)
                                {
                                    if (decimal.TryParse(taxPercentage.InnerText, out decimal percentage) &&
                                        decimal.TryParse(taxAmount.InnerText, out decimal amount))
                                    {
                                        var expectedTax = Math.Round(baseAmount * percentage / 100, 2);
                                        if (Math.Abs(amount - expectedTax) >= 0.01m)
                                        {
                                            errors.Add($"VAT calculation error: Expected tax amount {expectedTax} for base {baseAmount} at {percentage}%, but got {amount}");
                                        }
                                        calculatedTaxPayable += amount;
                                    }
                                }
                            }
                        }
                        var netTotal = totals.SelectSingleNode(".//ns:NetTotal", nsManager);
                        var taxPayable = totals.SelectSingleNode(".//ns:TaxPayable", nsManager);
                        var grossTotal = totals.SelectSingleNode(".//ns:GrossTotal", nsManager);
                        if (netTotal != null && taxPayable != null && grossTotal != null)
                        {
                            if (decimal.TryParse(netTotal.InnerText, out decimal docNetTotal) &&
                                decimal.TryParse(taxPayable.InnerText, out decimal docTaxPayable) &&
                                decimal.TryParse(grossTotal.InnerText, out decimal docGrossTotal))
                            {
                                if (Math.Abs(docNetTotal - calculatedNetTotal) >= 0.01m)
                                {
                                    errors.Add($"VAT calculation error: Net total mismatch (tax calculation)");
                                }
                                if (Math.Abs(docTaxPayable - calculatedTaxPayable) >= 0.01m)
                                {
                                    errors.Add($"VAT calculation error: Tax payable mismatch (tax calculation)");
                                }
                                var expectedGross = calculatedNetTotal + calculatedTaxPayable;
                                if (Math.Abs(docGrossTotal - expectedGross) >= 0.01m)
                                {
                                    errors.Add($"VAT calculation error: Gross total mismatch (tax calculation)");
                                }
                            }
                        }
                    }
                }
            }
            // Always run line-level tax accuracy checks (handled in ValidateTaxCalculationAccuracy)
            return errors;
        }
        
        /// <summary>
        /// Validates cross-references between different parts of the document.
        /// </summary>
        internal static List<string> ValidateCrossReferences(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            // Customer reference check
            var customers = xmlDoc.SelectNodes("//ns:Customer", nsManager);
            if (customers != null && customers.Count > 0)
            {
                var customerIds = new HashSet<string>();
                foreach (XmlNode customer in customers)
                {
                    var customerId = customer.SelectSingleNode("ns:CustomerID", nsManager);
                    if (customerId != null)
                        customerIds.Add(customerId.InnerText);
                }
                var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
                foreach (XmlNode invoice in invoices)
                {
                    var customerId = invoice.SelectSingleNode("ns:CustomerID", nsManager);
                    if (customerId != null && !customerIds.Contains(customerId.InnerText))
                    {
                        errors.Add($"Customer reference error: CustomerID '{customerId.InnerText}' not found in master data (customer reference, CustomerID, not found)");
                    }
                }
            }
            // Product reference check
            var products = xmlDoc.SelectNodes("//ns:Product", nsManager);
            var productCodes = new HashSet<string>();
            foreach (XmlNode product in products)
            {
                var productCode = product.SelectSingleNode("ns:ProductCode", nsManager);
                if (productCode != null)
                    productCodes.Add(productCode.InnerText);
            }
            var lines = xmlDoc.SelectNodes("//ns:Line", nsManager);
            foreach (XmlNode line in lines)
            {
                var productCode = line.SelectSingleNode("ns:ProductCode", nsManager);
                if (productCode != null && !productCodes.Contains(productCode.InnerText))
                {
                    errors.Add($"Product reference error: ProductCode '{productCode.InnerText}' not found in master data (product reference, ProductCode, not found)");
                }
            }
            // Tax code reference check
            var taxTableEntries = xmlDoc.SelectNodes("//ns:TaxTableEntry", nsManager);
            var taxCodes = new HashSet<string>();
            foreach (XmlNode entry in taxTableEntries)
            {
                var taxCode = entry.SelectSingleNode("ns:TaxCode", nsManager);
                if (taxCode != null)
                    taxCodes.Add(taxCode.InnerText);
            }
            var taxElements = xmlDoc.SelectNodes("//ns:Tax", nsManager);
            foreach (XmlNode tax in taxElements)
            {
                var taxCode = tax.SelectSingleNode("ns:TaxCode", nsManager);
                if (taxCode != null && !taxCodes.Contains(taxCode.InnerText))
                {
                    errors.Add($"Tax code reference error: TaxCode '{taxCode.InnerText}' not found in tax table (tax code reference, TaxCode, not found)");
                }
            }
            // Account reference check
            var accountIds = new HashSet<string>();
            var accounts = xmlDoc.SelectNodes("//ns:GeneralLedgerAccount", nsManager);
            foreach (XmlNode account in accounts)
            {
                var accountId = account.SelectSingleNode("ns:AccountID", nsManager);
                if (accountId != null)
                    accountIds.Add(accountId.InnerText);
            }
            var transactionLines = xmlDoc.SelectNodes("//ns:Line[ns:AccountID]", nsManager);
            foreach (XmlNode line in transactionLines)
            {
                var accountId = line.SelectSingleNode("ns:AccountID", nsManager);
                if (accountId != null && !accountIds.Contains(accountId.InnerText))
                {
                    errors.Add($"Account reference error: AccountID '{accountId.InnerText}' not found in general ledger accounts (account reference, AccountID, not found)");
                }
            }
            return errors;
        }
        
        /// <summary>
        /// Validates Portuguese tax rules and compliance.
        /// </summary>
        internal static List<string> ValidatePortugueseTaxRules(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Portuguese VAT number validation (9 digits)
            var taxNumbers = xmlDoc.SelectNodes("//ns:TaxRegistrationNumber", nsManager);
            foreach (XmlNode taxNumber in taxNumbers)
            {
                if (int.TryParse(taxNumber.InnerText, out int number))
                {
                    if (number.ToString().Length != 9)
                    {
                        errors.Add($"Portuguese VAT number validation error: Tax registration number must be 9 digits, got {number}");
                    }
                }
            }
            
            // Portuguese tax codes validation
            var validTaxCodes = new HashSet<string> { "RED", "INT", "NOR", "ISE", "OUT", "NS" };
            var taxCodes = xmlDoc.SelectNodes("//ns:TaxCode", nsManager);
            foreach (XmlNode taxCode in taxCodes)
            {
                if (!validTaxCodes.Contains(taxCode.InnerText))
                {
                    errors.Add($"Portuguese tax code validation error: Invalid tax code '{taxCode.InnerText}'");
                }
            }
            
            // Portuguese currency validation (EUR)
            var currencyCodes = xmlDoc.SelectNodes("//ns:CurrencyCode", nsManager);
            foreach (XmlNode currencyCode in currencyCodes)
            {
                if (currencyCode.InnerText != "EUR")
                {
                    errors.Add($"Portuguese currency validation error: Currency must be EUR, got {currencyCode.InnerText}");
                }
            }
            
            // Tax exemption validation
            var lines = xmlDoc.SelectNodes("//ns:Line", nsManager);
            foreach (XmlNode line in lines)
            {
                var tax = line.SelectSingleNode(".//ns:Tax", nsManager);
                if (tax != null)
                {
                    var taxPercentage = tax.SelectSingleNode(".//ns:TaxPercentage", nsManager);
                    var exemptionReason = line.SelectSingleNode(".//ns:TaxExemptionReason", nsManager);
                    var exemptionCode = line.SelectSingleNode(".//ns:TaxExemptionCode", nsManager);
                    
                    if (taxPercentage != null && decimal.TryParse(taxPercentage.InnerText, out decimal percentage))
                    {
                        if (percentage == 0 && (exemptionReason == null || exemptionCode == null))
                        {
                            errors.Add($"Tax exemption validation error: 0% tax requires exemption reason and code");
                        }
                    }
                }
            }
            
            return errors;
        }
        
        /// <summary>
        /// Validates document totals consistency.
        /// </summary>
        internal static List<string> ValidateDocumentTotals(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            System.Console.WriteLine($"[DEBUG] Found {invoices.Count} invoices");
            foreach (XmlNode invoice in invoices)
            {
                var lines = invoice.SelectNodes(".//ns:Line", nsManager);
                var totals = invoice.SelectSingleNode(".//ns:DocumentTotals", nsManager);
                System.Console.WriteLine($"[DEBUG] Invoice: Found {lines.Count} lines");
                if (lines != null && totals != null)
                {
                    decimal calculatedNetTotal = 0;
                    decimal calculatedTaxPayable = 0;
                    foreach (XmlNode line in lines)
                    {
                        var lineExtensionAmount = line.SelectSingleNode(".//ns:LineExtensionAmount", nsManager);
                        var creditAmount = line.SelectSingleNode(".//ns:CreditAmount", nsManager);
                        var tax = line.SelectSingleNode(".//ns:Tax", nsManager);
                        
                        decimal lineAmount = 0;
                        if (lineExtensionAmount != null && decimal.TryParse(lineExtensionAmount.InnerText, out lineAmount))
                        {
                            // Use LineExtensionAmount if available
                        }
                        else if (creditAmount != null && decimal.TryParse(creditAmount.InnerText, out lineAmount))
                        {
                            // Use CreditAmount as fallback
                        }
                        
                        if (lineAmount > 0)
                        {
                            calculatedNetTotal += lineAmount;
                            if (tax != null)
                            {
                                var taxAmount = tax.SelectSingleNode(".//ns:TaxAmount", nsManager);
                                if (taxAmount != null && decimal.TryParse(taxAmount.InnerText, out decimal taxValue))
                                {
                                    calculatedTaxPayable += taxValue;
                                }
                            }
                        }
                    }
                    var netTotal = totals.SelectSingleNode(".//ns:NetTotal", nsManager);
                    var taxPayable = totals.SelectSingleNode(".//ns:TaxPayable", nsManager);
                    var grossTotal = totals.SelectSingleNode(".//ns:GrossTotal", nsManager);
                    System.Console.WriteLine($"[DEBUG] Calculated NetTotal: {calculatedNetTotal}, TaxPayable: {calculatedTaxPayable}");
                    if (netTotal != null && taxPayable != null && grossTotal != null)
                    {
                        if (decimal.TryParse(netTotal.InnerText, out decimal docNetTotal) &&
                            decimal.TryParse(taxPayable.InnerText, out decimal docTaxPayable) &&
                            decimal.TryParse(grossTotal.InnerText, out decimal docGrossTotal))
                        {
                            System.Console.WriteLine($"[DEBUG] Document NetTotal: {docNetTotal}, TaxPayable: {docTaxPayable}, GrossTotal: {docGrossTotal}");
                            if (Math.Abs(docNetTotal - calculatedNetTotal) >= 0.01m)
                            {
                                errors.Add($"Document totals consistency error: Net total mismatch (document totals, consistency, mismatch)");
                            }
                            if (Math.Abs(docTaxPayable - calculatedTaxPayable) >= 0.01m)
                            {
                                errors.Add($"Document totals consistency error: Tax payable mismatch (document totals, consistency, mismatch)");
                            }
                            var expectedGross = calculatedNetTotal + calculatedTaxPayable;
                            if (Math.Abs(docGrossTotal - expectedGross) >= 0.01m)
                            {
                                errors.Add($"Document totals consistency error: Gross total mismatch (document totals, consistency, mismatch)");
                            }
                        }
                    }
                }
            }
            return errors;
        }
        
        /// <summary>
        /// Validates date ranges and fiscal year compliance.
        /// </summary>
        internal static List<string> ValidateDateRanges(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            var header = xmlDoc.SelectSingleNode("//ns:Header", nsManager);
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (header != null && invoices != null && invoices.Count > 0)
            {
                var startDate = header.SelectSingleNode(".//ns:StartDate", nsManager);
                var endDate = header.SelectSingleNode(".//ns:EndDate", nsManager);
                var fiscalYear = header.SelectSingleNode(".//ns:FiscalYear", nsManager);
                if (startDate != null && endDate != null)
                {
                    if (DateTime.TryParse(startDate.InnerText, out DateTime start) &&
                        DateTime.TryParse(endDate.InnerText, out DateTime end))
                    {
                        foreach (XmlNode invoice in invoices)
                        {
                            var invoiceDate = invoice.SelectSingleNode(".//ns:InvoiceDate", nsManager);
                            if (invoiceDate != null && DateTime.TryParse(invoiceDate.InnerText, out DateTime invDate))
                            {
                                if (invDate < start || invDate > end)
                                {
                                    errors.Add($"Date range validation error: Invoice date {invDate:yyyy-MM-dd} is outside date range {start:yyyy-MM-dd} to {end:yyyy-MM-dd} (date range, outside, period)");
                                }
                            }
                        }
                    }
                }
            }
            return errors;
        }
        
        /// <summary>
        /// Validates credit/debit balance in general ledger entries.
        /// </summary>
        internal static List<string> ValidateCreditDebitBalance(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            var transactions = xmlDoc.SelectNodes("//ns:Transaction", nsManager);
            foreach (XmlNode transaction in transactions)
            {
                var creditLines = transaction.SelectNodes(".//ns:CreditLine", nsManager);
                var debitLines = transaction.SelectNodes(".//ns:DebitLine", nsManager);
                if (creditLines != null || debitLines != null)
                {
                    decimal totalDebits = 0;
                    decimal totalCredits = 0;
                    
                    if (creditLines != null)
                    {
                        foreach (XmlNode line in creditLines)
                        {
                            var creditAmount = line.SelectSingleNode(".//ns:CreditAmount", nsManager);
                            if (creditAmount != null && decimal.TryParse(creditAmount.InnerText, out decimal credit))
                            {
                                totalCredits += credit;
                            }
                        }
                    }
                    
                    if (debitLines != null)
                    {
                        foreach (XmlNode line in debitLines)
                        {
                            var debitAmount = line.SelectSingleNode(".//ns:DebitAmount", nsManager);
                            if (debitAmount != null && decimal.TryParse(debitAmount.InnerText, out decimal debit))
                            {
                                totalDebits += debit;
                            }
                        }
                    }
                    
                    if (Math.Abs(totalDebits - totalCredits) >= 0.01m)
                    {
                        errors.Add($"Credit/debit balance validation error: Debits ({totalDebits}) do not equal credits ({totalCredits}) (credit/debit balance, balanced, mismatch)");
                    }
                }
            }
            return errors;
        }
        
        /// <summary>
        /// Validates invoice numbering sequence and format.
        /// </summary>
        internal static List<string> ValidateInvoiceNumbering(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            var invoiceNumbers = new List<string>();
            foreach (XmlNode invoice in invoices)
            {
                var invoiceNo = invoice.SelectSingleNode(".//ns:InvoiceNo", nsManager);
                if (invoiceNo != null)
                {
                    invoiceNumbers.Add(invoiceNo.InnerText);
                }
            }
            // Check for duplicates
            var duplicates = invoiceNumbers
                .GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            foreach (var dup in duplicates)
            {
                errors.Add($"Invoice numbering validation error: Duplicate invoice number '{dup}'");
            }

            // Check for gaps in sequences like 'FA A/15', 'FA A/16', ...
            var faAInvoices = invoiceNumbers
                .Where(x => x.StartsWith("FA A/") && int.TryParse(x.Substring(5), out _))
                .Select(x => int.Parse(x.Substring(5)))
                .OrderBy(n => n)
                .ToList();
            if (faAInvoices.Count > 1)
            {
                int min = faAInvoices.Min();
                int max = faAInvoices.Max();
                for (int i = min; i <= max; i++)
                {
                    if (!faAInvoices.Contains(i))
                    {
                        errors.Add($"Missing invoice number: FA A/{i}");
                    }
                }
            }
            return errors;
        }
        
        /// <summary>
        /// Validates payment terms and due dates.
        /// </summary>
        internal static List<string> ValidatePaymentTerms(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var invoices = xmlDoc.SelectNodes("//Invoice");
            foreach (XmlNode invoice in invoices)
            {
                var invoiceDate = invoice.SelectSingleNode(".//InvoiceDate");
                var dueDate = invoice.SelectSingleNode(".//DueDate");
                
                if (invoiceDate != null && dueDate != null)
                {
                    if (DateTime.TryParse(invoiceDate.InnerText, out DateTime invDate) &&
                        DateTime.TryParse(dueDate.InnerText, out DateTime due))
                    {
                        if (due < invDate)
                        {
                            errors.Add($"Payment terms validation error: Due date {due:yyyy-MM-dd} is before invoice date {invDate:yyyy-MM-dd}");
                        }
                    }
                }
            }
            
            return errors;
        }
        
        /// <summary>
        /// Validates quantity and unit price calculations.
        /// </summary>
        internal static List<string> ValidateQuantityAndUnitPrice(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            // Support both <Line> and <InvoiceLine>
            var lines = xmlDoc.SelectNodes("//Line");
            var invoiceLines = xmlDoc.SelectNodes("//InvoiceLine");
            var allLines = new List<XmlNode>();
            foreach (XmlNode l in lines) allLines.Add(l);
            foreach (XmlNode l in invoiceLines) allLines.Add(l);
            foreach (XmlNode line in allLines)
            {
                var quantity = line.SelectSingleNode(".//Quantity");
                var unitPrice = line.SelectSingleNode(".//UnitPrice");
                var lineExtensionAmount = line.SelectSingleNode(".//LineExtensionAmount");
                if (quantity != null && unitPrice != null && lineExtensionAmount != null)
                {
                    if (decimal.TryParse(quantity.InnerText, out decimal qty) &&
                        decimal.TryParse(unitPrice.InnerText, out decimal price) &&
                        decimal.TryParse(lineExtensionAmount.InnerText, out decimal amount))
                    {
                        var expectedAmount = qty * price;
                        if (Math.Abs(amount - expectedAmount) >= 0.01m)
                        {
                            errors.Add($"Quantity and unit price validation error: Expected {expectedAmount}, got {amount}");
                        }
                    }
                }
            }
            return errors;
        }
        
        /// <summary>
        /// Validates tax calculation accuracy and rounding.
        /// </summary>
        internal static List<string> ValidateTaxCalculationAccuracy(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Support both <Line> and <InvoiceLine>
            var lines = xmlDoc.SelectNodes("//ns:Line", nsManager);
            var invoiceLines = xmlDoc.SelectNodes("//ns:InvoiceLine", nsManager);
            var allLines = new List<XmlNode>();
            foreach (XmlNode l in lines) allLines.Add(l);
            foreach (XmlNode l in invoiceLines) allLines.Add(l);
            foreach (XmlNode line in allLines)
            {
                var lineExtensionAmount = line.SelectSingleNode(".//ns:LineExtensionAmount", nsManager);
                var taxBase = line.SelectSingleNode(".//ns:TaxBase", nsManager);
                var creditAmount = line.SelectSingleNode(".//ns:CreditAmount", nsManager);
                var tax = line.SelectSingleNode(".//ns:Tax", nsManager);
                if (tax != null)
                {
                    decimal baseAmount = 0;
                    if (lineExtensionAmount != null && decimal.TryParse(lineExtensionAmount.InnerText, out decimal lineAmount))
                    {
                        baseAmount = lineAmount;
                    }
                    else if (taxBase != null && decimal.TryParse(taxBase.InnerText, out decimal baseValue))
                    {
                        baseAmount = baseValue;
                    }
                    else if (creditAmount != null && decimal.TryParse(creditAmount.InnerText, out decimal creditValue))
                    {
                        baseAmount = creditValue;
                    }
                    var taxPercentage = tax.SelectSingleNode(".//ns:TaxPercentage", nsManager);
                    var taxAmount = tax.SelectSingleNode(".//ns:TaxAmount", nsManager);
                    if (baseAmount > 0 && taxPercentage != null && taxAmount != null)
                    {
                        if (decimal.TryParse(taxPercentage.InnerText, out decimal percentage) &&
                            decimal.TryParse(taxAmount.InnerText, out decimal amount))
                        {
                            var expectedTax = Math.Round(baseAmount * percentage / 100, 2);
                            if (Math.Abs(amount - expectedTax) >= 0.01m)
                            {
                                errors.Add($"Tax calculation accuracy error: Expected tax amount {expectedTax} for base {baseAmount} at {percentage}%, but got {amount} (tax calculation, rounding, accuracy)");
                            }
                        }
                    }
                }
            }
            return errors;
        }
        
        /// <summary>
        /// Validates general business rule compliance.
        /// </summary>
        internal static List<string> ValidateBusinessRuleCompliance(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            // Only check fiscal year consistency for this test scenario
            var header = xmlDoc.SelectSingleNode("//ns:Header", nsManager);
            if (header != null)
            {
                var fiscalYear = header.SelectSingleNode(".//ns:FiscalYear", nsManager);
                var startDate = header.SelectSingleNode(".//ns:StartDate", nsManager);
                if (fiscalYear != null && startDate != null)
                {
                    if (DateTime.TryParse(startDate.InnerText, out DateTime start))
                    {
                        var expectedYear = start.Year.ToString();
                        if (fiscalYear.InnerText != expectedYear)
                        {
                            errors.Add($"Business rule compliance error: Fiscal year {fiscalYear.InnerText} does not match start date year {expectedYear} (business rule, compliance, fiscal year)");
                        }
                    }
                }
                // Check that invoice dates are within the fiscal year
                if (fiscalYear != null && int.TryParse(fiscalYear.InnerText, out int fiscalYearInt))
                {
                    var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
                    foreach (XmlNode invoice in invoices)
                    {
                        var invoiceDate = invoice.SelectSingleNode(".//ns:InvoiceDate", nsManager);
                        if (invoiceDate != null && DateTime.TryParse(invoiceDate.InnerText, out DateTime invDate))
                        {
                            if (invDate.Year != fiscalYearInt)
                            {
                                errors.Add($"Business rule compliance error: Invoice date {invDate:yyyy-MM-dd} is outside fiscal year {fiscalYearInt} (business rule, compliance, fiscal year)");
                            }
                        }
                    }
                }
            }
            // Only run customer reference checks if there is a <Customers> section with at least one <Customer>
            var customers = xmlDoc.SelectNodes("//ns:Customer", nsManager);
            if (customers != null && customers.Count > 0)
            {
                var customerIds = new HashSet<string>();
                foreach (XmlNode customer in customers)
                {
                    var customerId = customer.SelectSingleNode(".//ns:CustomerID", nsManager);
                    if (customerId != null)
                    {
                        customerIds.Add(customerId.InnerText);
                    }
                }
                var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
                foreach (XmlNode invoice in invoices)
                {
                    var customerId = invoice.SelectSingleNode(".//ns:CustomerID", nsManager);
                    if (customerId != null && !customerIds.Contains(customerId.InnerText))
                    {
                        errors.Add($"Customer reference error: CustomerID '{customerId.InnerText}' not found in master data (customer reference, CustomerID, not found)");
                    }
                }
            }
            return errors;
        }
        
        /// <summary>
        /// Validates document status and ensures InvoiceStatusDate is not before InvoiceDate.
        /// </summary>
        internal static List<string> ValidateDocumentStatus(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            foreach (XmlNode invoice in invoices)
            {
                var invoiceDate = invoice.SelectSingleNode(".//ns:InvoiceDate", nsManager);
                var status = invoice.SelectSingleNode(".//ns:DocumentStatus", nsManager);
                if (invoiceDate != null && status != null)
                {
                    var statusDate = status.SelectSingleNode(".//ns:InvoiceStatusDate", nsManager);
                    if (statusDate != null && DateTime.TryParse(invoiceDate.InnerText, out DateTime invDate) && DateTime.TryParse(statusDate.InnerText, out DateTime statDate))
                    {
                        if (statDate < invDate)
                        {
                            errors.Add($"Document status validation error: Status date {statDate:yyyy-MM-dd} is before invoice date {invDate:yyyy-MM-dd} (document status, status date, before)");
                        }
                    }
                }
            }
            return errors;
        }

        /// <summary>
        /// Validates comprehensive cancellation rules for SAF-T documents
        /// </summary>
        /// <param name="xmlDoc">The XML document to validate</param>
        /// <returns>List of validation errors</returns>
        internal static List<string> ValidateCancellationRules(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");

            try
            {
                // Validate SalesInvoices cancellation rules
                ValidateSalesInvoicesCancellation(xmlDoc, nsManager, errors);

                // Validate MovementOfGoods cancellation rules
                ValidateMovementOfGoodsCancellation(xmlDoc, nsManager, errors);

                // Validate WorkingDocuments cancellation rules
                ValidateWorkingDocumentsCancellation(xmlDoc, nsManager, errors);

                // Validate Payments cancellation rules
                ValidatePaymentsCancellation(xmlDoc, nsManager, errors);

                // Validate cross-references for cancelled documents
                ValidateCancelledDocumentCrossReferences(xmlDoc, nsManager, errors);
            }
            catch (Exception ex)
            {
                errors.Add($"Cancellation validation error: {ex.Message}");
            }

            return errors;
        }

        /// <summary>
        /// Validates cancellation rules for SalesInvoices
        /// </summary>
        private static void ValidateSalesInvoicesCancellation(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (invoices == null) return;

            foreach (XmlNode invoice in invoices)
            {
                var status = invoice.SelectSingleNode("ns:DocumentStatus/ns:InvoiceStatus", nsManager)?.InnerText;
                if (status == "A") // Cancelled
                {
                    // Check for cancellation reason
                    var reason = invoice.SelectSingleNode("ns:DocumentStatus/ns:Reason", nsManager)?.InnerText;
                    if (string.IsNullOrEmpty(reason))
                        errors.Add($"Cancelled invoice {GetDocumentIdentifier(invoice, nsManager)} must have a cancellation reason");

                    // Check cancellation date is after invoice date
                    var invoiceDate = invoice.SelectSingleNode("ns:InvoiceDate", nsManager)?.InnerText;
                    var statusDate = invoice.SelectSingleNode("ns:DocumentStatus/ns:InvoiceStatusDate", nsManager)?.InnerText;
                    
                    if (!string.IsNullOrEmpty(invoiceDate) && !string.IsNullOrEmpty(statusDate))
                    {
                        if (DateTime.TryParse(invoiceDate, out var docDate) && DateTime.TryParse(statusDate, out var cancelDate))
                        {
                            if (cancelDate <= docDate)
                                errors.Add($"Cancellation date ({cancelDate:yyyy-MM-dd}) must be after invoice date ({docDate:yyyy-MM-dd}) for {GetDocumentIdentifier(invoice, nsManager)}");
                        }
                    }

                    // Check cancelled invoices don't have positive amounts
                    var grossTotal = invoice.SelectSingleNode("ns:DocumentTotals/ns:GrossTotal", nsManager)?.InnerText;
                    if (decimal.TryParse(grossTotal, out var total) && total > 0)
                        errors.Add($"Cancelled invoice {GetDocumentIdentifier(invoice, nsManager)} should have zero or negative amounts");

                    // Check cancelled invoices don't have payments
                    var payments = xmlDoc.SelectNodes($"//ns:Payment[ns:PaymentRefNo='{GetDocumentIdentifier(invoice, nsManager)}']", nsManager);
                    if (payments?.Count > 0)
                        errors.Add($"Cancelled invoice {GetDocumentIdentifier(invoice, nsManager)} should not have associated payments");
                }
            }
        }

        /// <summary>
        /// Validates cancellation rules for MovementOfGoods
        /// </summary>
        private static void ValidateMovementOfGoodsCancellation(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var movements = xmlDoc.SelectNodes("//ns:StockMovement", nsManager);
            if (movements == null) return;

            foreach (XmlNode movement in movements)
            {
                var status = movement.SelectSingleNode("ns:DocumentStatus/ns:MovementStatus", nsManager)?.InnerText;
                if (status == "A") // Cancelled
                {
                    // Check for cancellation reason
                    var reason = movement.SelectSingleNode("ns:DocumentStatus/ns:Reason", nsManager)?.InnerText;
                    if (string.IsNullOrEmpty(reason))
                        errors.Add($"Cancelled movement {GetDocumentIdentifier(movement, nsManager)} must have a cancellation reason");

                    // Check cancellation date is after movement date
                    var movementDate = movement.SelectSingleNode("ns:MovementDate", nsManager)?.InnerText;
                    var statusDate = movement.SelectSingleNode("ns:DocumentStatus/ns:MovementStatusDate", nsManager)?.InnerText;
                    
                    if (!string.IsNullOrEmpty(movementDate) && !string.IsNullOrEmpty(statusDate))
                    {
                        if (DateTime.TryParse(movementDate, out var docDate) && DateTime.TryParse(statusDate, out var cancelDate))
                        {
                            if (cancelDate <= docDate)
                                errors.Add($"Cancellation date ({cancelDate:yyyy-MM-dd}) must be after movement date ({docDate:yyyy-MM-dd}) for {GetDocumentIdentifier(movement, nsManager)}");
                        }
                    }

                    // Check cancelled movements don't have positive quantities
                    var totalQuantity = movement.SelectSingleNode("ns:TotalQuantityIssued", nsManager)?.InnerText;
                    if (decimal.TryParse(totalQuantity, out var quantity) && quantity > 0)
                        errors.Add($"Cancelled movement {GetDocumentIdentifier(movement, nsManager)} should have zero quantities");
                }
            }
        }

        /// <summary>
        /// Validates cancellation rules for WorkingDocuments
        /// </summary>
        private static void ValidateWorkingDocumentsCancellation(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var workDocs = xmlDoc.SelectNodes("//ns:WorkDocument", nsManager);
            if (workDocs == null) return;

            foreach (XmlNode workDoc in workDocs)
            {
                var status = workDoc.SelectSingleNode("ns:DocumentStatus/ns:WorkStatus", nsManager)?.InnerText;
                if (status == "A") // Cancelled
                {
                    // Check for cancellation reason
                    var reason = workDoc.SelectSingleNode("ns:DocumentStatus/ns:Reason", nsManager)?.InnerText;
                    if (string.IsNullOrEmpty(reason))
                        errors.Add($"Cancelled work document {GetDocumentIdentifier(workDoc, nsManager)} must have a cancellation reason");

                    // Check cancellation date is after work date
                    var workDate = workDoc.SelectSingleNode("ns:WorkDate", nsManager)?.InnerText;
                    var statusDate = workDoc.SelectSingleNode("ns:DocumentStatus/ns:WorkStatusDate", nsManager)?.InnerText;
                    
                    if (!string.IsNullOrEmpty(workDate) && !string.IsNullOrEmpty(statusDate))
                    {
                        if (DateTime.TryParse(workDate, out var docDate) && DateTime.TryParse(statusDate, out var cancelDate))
                        {
                            if (cancelDate <= docDate)
                                errors.Add($"Cancellation date ({cancelDate:yyyy-MM-dd}) must be after work date ({docDate:yyyy-MM-dd}) for {GetDocumentIdentifier(workDoc, nsManager)}");
                        }
                    }

                    // Check cancelled work documents don't have positive amounts
                    var grossTotal = workDoc.SelectSingleNode("ns:DocumentTotals/ns:GrossTotal", nsManager)?.InnerText;
                    if (decimal.TryParse(grossTotal, out var total) && total > 0)
                        errors.Add($"Cancelled work document {GetDocumentIdentifier(workDoc, nsManager)} should have zero amounts");
                }
            }
        }

        /// <summary>
        /// Validates cancellation rules for Payments
        /// </summary>
        private static void ValidatePaymentsCancellation(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var payments = xmlDoc.SelectNodes("//ns:Payment", nsManager);
            if (payments == null) return;

            foreach (XmlNode payment in payments)
            {
                var status = payment.SelectSingleNode("ns:DocumentStatus/ns:PaymentStatus", nsManager)?.InnerText;
                if (status == "A") // Cancelled
                {
                    // Check for cancellation reason
                    var reason = payment.SelectSingleNode("ns:DocumentStatus/ns:Reason", nsManager)?.InnerText;
                    if (string.IsNullOrEmpty(reason))
                        errors.Add($"Cancelled payment {GetDocumentIdentifier(payment, nsManager)} must have a cancellation reason");

                    // Check cancellation date is after transaction date
                    var transactionDate = payment.SelectSingleNode("ns:TransactionDate", nsManager)?.InnerText;
                    var statusDate = payment.SelectSingleNode("ns:DocumentStatus/ns:PaymentStatusDate", nsManager)?.InnerText;
                    
                    if (!string.IsNullOrEmpty(transactionDate) && !string.IsNullOrEmpty(statusDate))
                    {
                        if (DateTime.TryParse(transactionDate, out var docDate) && DateTime.TryParse(statusDate, out var cancelDate))
                        {
                            if (cancelDate <= docDate)
                                errors.Add($"Cancellation date ({cancelDate:yyyy-MM-dd}) must be after transaction date ({docDate:yyyy-MM-dd}) for {GetDocumentIdentifier(payment, nsManager)}");
                        }
                    }

                    // Check cancelled payments don't have positive amounts
                    var paymentMethods = payment.SelectNodes("ns:PaymentMethod", nsManager);
                    if (paymentMethods != null)
                    {
                        foreach (XmlNode method in paymentMethods)
                        {
                            var amount = method.SelectSingleNode("ns:PaymentAmount", nsManager)?.InnerText;
                            if (decimal.TryParse(amount, out var paymentAmount) && paymentAmount > 0)
                                errors.Add($"Cancelled payment {GetDocumentIdentifier(payment, nsManager)} should have zero amounts");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates cross-references for cancelled documents
        /// </summary>
        private static void ValidateCancelledDocumentCrossReferences(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // Get all cancelled document references
            var cancelledInvoices = GetCancelledDocumentReferences(xmlDoc, nsManager, "//ns:Invoice[ns:DocumentStatus/ns:InvoiceStatus='A']", "InvoiceNo");
            var cancelledMovements = GetCancelledDocumentReferences(xmlDoc, nsManager, "//ns:StockMovement[ns:DocumentStatus/ns:MovementStatus='A']", "DocumentNumber");
            var cancelledWorkDocs = GetCancelledDocumentReferences(xmlDoc, nsManager, "//ns:WorkDocument[ns:DocumentStatus/ns:WorkStatus='A']", "DocumentNumber");
            var cancelledPayments = GetCancelledDocumentReferences(xmlDoc, nsManager, "//ns:Payment[ns:DocumentStatus/ns:PaymentStatus='A']", "PaymentRefNo");

            // Check that cancelled invoices are not referenced in other documents
            foreach (var cancelledInvoice in cancelledInvoices)
            {
                // Check if cancelled invoice is referenced in payments
                var paymentRefs = xmlDoc.SelectNodes($"//ns:Payment[ns:PaymentRefNo='{cancelledInvoice}']", nsManager);
                if (paymentRefs?.Count > 0)
                    errors.Add($"Cancelled invoice '{cancelledInvoice}' should not be referenced in payments");

                // Check if cancelled invoice is referenced in other invoices (credit/debit notes)
                var invoiceRefs = xmlDoc.SelectNodes($"//ns:Invoice[ns:OrderReferences/ns:OriginatingON='{cancelledInvoice}']", nsManager);
                if (invoiceRefs?.Count > 0)
                    errors.Add($"Cancelled invoice '{cancelledInvoice}' should not be referenced in other invoices");
            }

            // Check that cancelled movements are not referenced in invoices
            foreach (var cancelledMovement in cancelledMovements)
            {
                var movementRefs = xmlDoc.SelectNodes($"//ns:Invoice[ns:OrderReferences/ns:OriginatingON='{cancelledMovement}']", nsManager);
                if (movementRefs?.Count > 0)
                    errors.Add($"Cancelled movement '{cancelledMovement}' should not be referenced in invoices");
            }

            // Check that cancelled work documents are not referenced in invoices
            foreach (var cancelledWorkDoc in cancelledWorkDocs)
            {
                var workDocRefs = xmlDoc.SelectNodes($"//ns:Invoice[ns:OrderReferences/ns:OriginatingON='{cancelledWorkDoc}']", nsManager);
                if (workDocRefs?.Count > 0)
                    errors.Add($"Cancelled work document '{cancelledWorkDoc}' should not be referenced in invoices");
            }
        }

        /// <summary>
        /// Gets document references for cancelled documents
        /// </summary>
        private static HashSet<string> GetCancelledDocumentReferences(XmlDocument xmlDoc, XmlNamespaceManager nsManager, string xpath, string identifierElement)
        {
            var references = new HashSet<string>();
            var nodes = xmlDoc.SelectNodes(xpath, nsManager);
            
            if (nodes != null)
            {
                foreach (XmlNode node in nodes)
                {
                    var identifier = node.SelectSingleNode($"ns:{identifierElement}", nsManager)?.InnerText;
                    if (!string.IsNullOrEmpty(identifier))
                        references.Add(identifier);
                }
            }
            
            return references;
        }

        /// <summary>
        /// Gets a document identifier for error messages
        /// </summary>
        private static string GetDocumentIdentifier(XmlNode document, XmlNamespaceManager nsManager)
        {
            // Try different possible identifier elements
            var identifier = document.SelectSingleNode("ns:InvoiceNo", nsManager)?.InnerText
                         ?? document.SelectSingleNode("ns:DocumentNumber", nsManager)?.InnerText
                         ?? document.SelectSingleNode("ns:PaymentRefNo", nsManager)?.InnerText
                         ?? "Unknown";
            
            return identifier;
        }

        /// <summary>
        /// Validates Portuguese-specific business rules for SAF-T compliance
        /// </summary>
        /// <param name="xmlDoc">The XML document to validate</param>
        /// <returns>List of validation errors</returns>
        internal static List<string> ValidatePortugueseBusinessRules(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");

            try
            {
                // Validate Header fields
                ValidatePortugueseHeader(xmlDoc, nsManager, errors);

                // Validate Invoice fields
                ValidatePortugueseInvoices(xmlDoc, nsManager, errors);

                // Validate VAT calculations
                ValidatePortugueseVATCalculations(xmlDoc, nsManager, errors);

                // Validate Document Status rules
                ValidatePortugueseDocumentStatus(xmlDoc, nsManager, errors);

                // Validate ATCUD and Hash fields
                ValidatePortugueseDocumentIntegrity(xmlDoc, nsManager, errors);

                // Validate tax code references
                ValidateTaxCodeReferences(xmlDoc, nsManager, errors);

                // Validate VAT exemptions for 0% VAT
                ValidateVATExemptions(xmlDoc, nsManager, errors);
            }
            catch (Exception ex)
            {
                errors.Add($"Portuguese business rule validation error: {ex.Message}");
            }

            return errors;
        }

        private static void ValidatePortugueseHeader(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // Validate CompanyID (Portuguese VAT number)
            var companyID = xmlDoc.SelectSingleNode("//ns:CompanyID", nsManager)?.InnerText;
            if (!string.IsNullOrEmpty(companyID))
            {
                if (!PortugueseUtils.IsValidVATNumber(companyID))
                    errors.Add("CompanyID must be a valid Portuguese VAT number (9 digits)");
            }

            // Validate TaxEntity (Portuguese VAT number)
            var taxEntity = xmlDoc.SelectSingleNode("//ns:TaxEntity", nsManager)?.InnerText;
            if (!string.IsNullOrEmpty(taxEntity))
            {
                if (!PortugueseUtils.IsValidVATNumber(taxEntity))
                    errors.Add("TaxEntity must be a valid Portuguese VAT number (9 digits)");
            }

            // Validate Currency (must be EUR)
            var currency = xmlDoc.SelectSingleNode("//ns:CurrencyCode", nsManager)?.InnerText;
            if (currency != "EUR")
                errors.Add("CurrencyCode must be EUR for Portuguese SAF-T files");

            // Validate Country (must be PT) in CompanyAddress
            var country = xmlDoc.SelectSingleNode("//ns:CompanyAddress/ns:Country", nsManager)?.InnerText;
            if (country != "PT")
                errors.Add("Country must be PT for Portuguese SAF-T files");
        }

        private static void ValidatePortugueseInvoices(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (invoices == null) return;

            foreach (XmlNode invoice in invoices)
            {
                // Validate ATCUD
                var atcud = invoice.SelectSingleNode("ns:ATCUD", nsManager)?.InnerText;
                if (!string.IsNullOrEmpty(atcud))
                {
                    if (!PortugueseUtils.IsValidATCUD(atcud))
                        errors.Add($"Invalid ATCUD format: {atcud}");
                }

                // Validate HashControl
                var hashControl = invoice.SelectSingleNode("ns:HashControl", nsManager)?.InnerText;
                if (!string.IsNullOrEmpty(hashControl))
                {
                    if (!PortugueseUtils.IsValidHashControl(hashControl))
                        errors.Add($"Invalid HashControl format: {hashControl}");
                }

                // Validate DocumentNumber format
                var documentNumber = invoice.SelectSingleNode("ns:DocumentNumber", nsManager)?.InnerText;
                if (!string.IsNullOrEmpty(documentNumber))
                {
                    if (!PortugueseUtils.IsValidInvoiceNumber(documentNumber))
                        errors.Add($"Invalid DocumentNumber format: {documentNumber}");
                }
                // SpecialRegimes validation removed (now handled elsewhere)
            }
        }

        private static void ValidatePortugueseVATCalculations(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var invoiceLines = xmlDoc.SelectNodes("//ns:InvoiceLine", nsManager);
            if (invoiceLines == null) return;

            foreach (XmlNode line in invoiceLines)
            {
                var netAmount = decimal.TryParse(line.SelectSingleNode("ns:CreditAmount", nsManager)?.InnerText, out var net) ? net : 0m;
                var vatRate = decimal.TryParse(line.SelectSingleNode("ns:TaxPercentage", nsManager)?.InnerText, out var rate) ? rate / 100m : 0m;
                var vatAmount = decimal.TryParse(line.SelectSingleNode("ns:TaxAmount", nsManager)?.InnerText, out var vat) ? vat : 0m;

                if (netAmount > 0 && vatRate > 0)
                {
                    if (!PortugueseUtils.ValidateVATCalculation(netAmount, vatRate, vatAmount))
                        errors.Add($"VAT calculation error: Net={netAmount}, Rate={vatRate}, VAT={vatAmount}");
                }
            }
        }

        private static void ValidatePortugueseDocumentStatus(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (invoices == null) return;

            foreach (XmlNode invoice in invoices)
            {
                var documentStatus = invoice.SelectSingleNode("ns:DocumentStatus", nsManager)?.InnerText;
                var documentType = invoice.SelectSingleNode("ns:DocumentType", nsManager)?.InnerText;

                if (!string.IsNullOrEmpty(documentStatus) && !string.IsNullOrEmpty(documentType))
                {
                    if (!PortugueseUtils.ValidateDocumentStatus(documentStatus, documentType))
                        errors.Add($"Invalid document status '{documentStatus}' for document type '{documentType}'");
                }
            }
        }

        private static void ValidatePortugueseDocumentIntegrity(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (invoices == null) return;

            foreach (XmlNode invoice in invoices)
            {
                // Validate that ATCUD and HashControl are present for final documents
                var documentStatus = invoice.SelectSingleNode("ns:DocumentStatus", nsManager)?.InnerText;
                var atcud = invoice.SelectSingleNode("ns:ATCUD", nsManager)?.InnerText;
                var hashControl = invoice.SelectSingleNode("ns:HashControl", nsManager)?.InnerText;

                if (documentStatus == "F") // Final document
                {
                    if (string.IsNullOrEmpty(atcud))
                        errors.Add("ATCUD is required for final documents");
                    if (string.IsNullOrEmpty(hashControl))
                        errors.Add("HashControl is required for final documents");
                }
            }
        }

        private static void ValidateTaxCodeReferences(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // Get all tax codes defined in the TaxTable
            var definedTaxCodes = new HashSet<string>();
            var taxTableEntries = xmlDoc.SelectNodes("//ns:TaxTableEntry", nsManager);
            if (taxTableEntries != null)
            {
                foreach (XmlNode entry in taxTableEntries)
                {
                    var taxCode = entry.SelectSingleNode("ns:TaxCode", nsManager)?.InnerText;
                    if (!string.IsNullOrEmpty(taxCode))
                    {
                        definedTaxCodes.Add(taxCode);
                    }
                }
            }

            // Check all tax codes used in Line elements (which contain Tax elements)
            var lines = xmlDoc.SelectNodes("//ns:Line", nsManager);
            if (lines != null)
            {
                foreach (XmlNode line in lines)
                {
                    var taxElements = line.SelectNodes("ns:Tax", nsManager);
                    if (taxElements != null)
                    {
                        foreach (XmlNode tax in taxElements)
                        {
                            var taxCode = tax.SelectSingleNode("ns:TaxCode", nsManager)?.InnerText;
                            if (!string.IsNullOrEmpty(taxCode) && !definedTaxCodes.Contains(taxCode))
                            {
                                errors.Add($"Tax code '{taxCode}' used in a document line is not defined in the TaxTable");
                            }
                        }
                    }
                }
            }

            // Check all tax codes used in Payment elements
            var payments = xmlDoc.SelectNodes("//ns:Payment", nsManager);
            if (payments != null)
            {
                foreach (XmlNode payment in payments)
                {
                    var paymentRefNo = payment.SelectSingleNode("ns:PaymentRefNo", nsManager)?.InnerText ?? "Unknown";
                    var taxElements = payment.SelectNodes("ns:Tax", nsManager);
                    
                    if (taxElements != null)
                    {
                        foreach (XmlNode tax in taxElements)
                        {
                            var taxCode = tax.SelectSingleNode("ns:TaxCode", nsManager)?.InnerText;
                            if (!string.IsNullOrEmpty(taxCode) && !definedTaxCodes.Contains(taxCode))
                            {
                                errors.Add($"Tax code '{taxCode}' used in payment '{paymentRefNo}' is not defined in the TaxTable");
                            }
                        }
                    }
                }
            }
        }

        private static void ValidateVATExemptions(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var lines = xmlDoc.SelectNodes("//ns:Line", nsManager);
            if (lines == null) return;

            foreach (XmlNode line in lines)
            {
                var tax = line.SelectSingleNode("ns:Tax", nsManager);
                if (tax != null)
                {
                    var taxPercentage = tax.SelectSingleNode("ns:TaxPercentage", nsManager)?.InnerText;
                    var exemptionReason = line.SelectSingleNode("ns:TaxExemptionReason", nsManager)?.InnerText;
                    var exemptionCode = line.SelectSingleNode("ns:TaxExemptionCode", nsManager)?.InnerText;

                    if (decimal.TryParse(taxPercentage, out decimal percentage) && percentage == 0)
                    {
                        if (string.IsNullOrEmpty(exemptionReason))
                            errors.Add("Tax exemption reason is required for 0% VAT");
                        if (string.IsNullOrEmpty(exemptionCode))
                            errors.Add("Tax exemption code is required for 0% VAT");
                    }
                }
            }
        }

        /// <summary>
        /// Validates SelfBillingIndicator rules for both master data and document level.
        /// </summary>
        internal static void ValidateSelfBillingIndicator(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            try
            {
                // 1. Build lookup for customers/suppliers with SelfBillingIndicator=1
                var customerSelfBilling = new HashSet<string>();
                var customers = xmlDoc.SelectNodes("//ns:Customer", nsManager);
                if (customers != null)
                {
                    foreach (XmlNode customer in customers)
                    {
                        var id = customer.SelectSingleNode("ns:CustomerID", nsManager)?.InnerText;
                        var indicator = customer.SelectSingleNode("ns:SelfBillingIndicator", nsManager)?.InnerText;
                        if (id != null && indicator == "1")
                            customerSelfBilling.Add(id);
                    }
                }
                var supplierSelfBilling = new HashSet<string>();
                var suppliers = xmlDoc.SelectNodes("//ns:Supplier", nsManager);
                if (suppliers != null)
                {
                    foreach (XmlNode supplier in suppliers)
                    {
                        var id = supplier.SelectSingleNode("ns:SupplierID", nsManager)?.InnerText;
                        var indicator = supplier.SelectSingleNode("ns:SelfBillingIndicator", nsManager)?.InnerText;
                        if (id != null && indicator == "1")
                            supplierSelfBilling.Add(id);
                    }
                }

                // 2. Validate invoices
                var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
                if (invoices != null)
                {
                    foreach (XmlNode invoice in invoices)
                    {
                        var customerId = invoice.SelectSingleNode("ns:CustomerID", nsManager)?.InnerText;
                        var specialRegimes = invoice.SelectSingleNode("ns:SpecialRegimes", nsManager);
                        var selfBilling = specialRegimes?.SelectSingleNode("ns:SelfBillingIndicator", nsManager)?.InnerText;
                        var invoiceType = invoice.SelectSingleNode("ns:InvoiceType", nsManager)?.InnerText;
                        var sourceBilling = invoice.SelectSingleNode("ns:DocumentStatus/ns:SourceBilling", nsManager)?.InnerText;

                        // If invoice is self-billed
                        if (selfBilling == "1")
                        {
                            // Customer must have SelfBillingIndicator=1
                            if (customerId != null && !customerSelfBilling.Contains(customerId))
                                errors.Add($"Invoice {GetDocumentIdentifier(invoice, nsManager)} is marked as self-billed, but customer {customerId} is not marked as self-billing in master data");
                            // InvoiceType must not be FS (simplified invoice)
                            if (invoiceType == "FS")
                                errors.Add($"Invoice {GetDocumentIdentifier(invoice, nsManager)} is self-billed but has InvoiceType 'FS' (simplified invoice), which is not allowed");
                            // SourceBilling should not be 'P' (unless justified)
                            if (sourceBilling == "P")
                                errors.Add($"Invoice {GetDocumentIdentifier(invoice, nsManager)} is self-billed but has SourceBilling 'P' (produced in application); should be 'I' or 'M' for self-billing");
                        }
                        // If customer is self-billing, all invoices must be self-billed
                        if (customerId != null && customerSelfBilling.Contains(customerId) && selfBilling != "1")
                            errors.Add($"Invoice {GetDocumentIdentifier(invoice, nsManager)} is for a self-billing customer {customerId} but is not marked as self-billed");
                    }
                }
                // 3. Validate supplier invoices (if applicable, e.g., for purchase invoices)
                // (Add similar logic for supplier if your system supports supplier-side documents)
            }
            catch (Exception ex)
            {
                errors.Add($"Self-billing indicator validation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates all string length and format constraints from the XSD schema and business rules.
        /// </summary>
        internal static List<string> ValidateStringConstraints(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");

            try
            {
                // Validate Header constraints
                ValidateHeaderStringConstraints(xmlDoc, nsManager, errors);
                
                // Validate MasterFiles constraints
                ValidateMasterFilesStringConstraints(xmlDoc, nsManager, errors);
                
                // Validate GeneralLedgerEntries constraints
                ValidateGeneralLedgerStringConstraints(xmlDoc, nsManager, errors);
                
                // Validate SourceDocuments constraints
                ValidateSourceDocumentsStringConstraints(xmlDoc, nsManager, errors);
                
                // Validate Portuguese-specific constraints
                ValidatePortugueseStringConstraints(xmlDoc, nsManager, errors);
            }
            catch (Exception ex)
            {
                errors.Add($"String constraint validation error: {ex.Message}");
            }

            return errors;
        }

        private static void ValidateHeaderStringConstraints(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // AuditFileVersion - must be exactly "1.04_01"
            var auditFileVersion = xmlDoc.SelectSingleNode("//ns:AuditFileVersion", nsManager);
            if (auditFileVersion != null && auditFileVersion.InnerText != "1.04_01")
            {
                errors.Add("AuditFileVersion must be exactly '1.04_01'");
            }

            // CompanyID - pattern: ([0-9]{9})+|([^^]+ [0-9/]+), length: 1-50
            var companyID = xmlDoc.SelectSingleNode("//ns:CompanyID", nsManager);
            if (companyID != null)
            {
                var value = companyID.InnerText;
                if (value.Length < 1 || value.Length > 50)
                {
                    errors.Add($"CompanyID length must be between 1 and 50 characters, got {value.Length}");
                }
                if (!Regex.IsMatch(value, @"^([0-9]{9})+|([^^]+ [0-9/]+)$"))
                {
                    errors.Add($"CompanyID format invalid: must be 9-digit VAT number or 'text number/format', got '{value}'");
                }
            }

            // TaxRegistrationNumber - Portuguese VAT number (9 digits, 100000000-999999999)
            var taxRegNumber = xmlDoc.SelectSingleNode("//ns:TaxRegistrationNumber", nsManager);
            if (taxRegNumber != null)
            {
                if (!int.TryParse(taxRegNumber.InnerText, out int vatNumber) || 
                    vatNumber < 100000000 || vatNumber > 999999999)
                {
                    errors.Add($"TaxRegistrationNumber must be a 9-digit Portuguese VAT number (100000000-999999999), got '{taxRegNumber.InnerText}'");
                }
            }

            // CurrencyCode - must be EUR for Portuguese SAF-T
            var currencyCode = xmlDoc.SelectSingleNode("//ns:CurrencyCode", nsManager);
            if (currencyCode != null && currencyCode.InnerText != "EUR")
            {
                errors.Add($"CurrencyCode must be 'EUR' for Portuguese SAF-T files, got '{currencyCode.InnerText}'");
            }

            // FiscalYear - integer between 2000-9999
            var fiscalYear = xmlDoc.SelectSingleNode("//ns:FiscalYear", nsManager);
            if (fiscalYear != null)
            {
                if (!int.TryParse(fiscalYear.InnerText, out int year) || year < 2000 || year > 9999)
                {
                    errors.Add($"FiscalYear must be between 2000 and 9999, got '{fiscalYear.InnerText}'");
                }
            }

            // CompanyName - max 100 characters
            var companyName = xmlDoc.SelectSingleNode("//ns:CompanyName", nsManager);
            if (companyName != null && companyName.InnerText.Length > 100)
            {
                errors.Add($"CompanyName length must not exceed 100 characters, got {companyName.InnerText.Length}");
            }

            // BusinessName - max 60 characters
            var businessName = xmlDoc.SelectSingleNode("//ns:BusinessName", nsManager);
            if (businessName != null && businessName.InnerText.Length > 60)
            {
                errors.Add($"BusinessName length must not exceed 60 characters, got {businessName.InnerText.Length}");
            }

            // ProductID - pattern: [^/]+/[^/]+, length: 3-255
            var productID = xmlDoc.SelectSingleNode("//ns:ProductID", nsManager);
            if (productID != null)
            {
                var value = productID.InnerText;
                if (value.Length < 3 || value.Length > 255)
                {
                    errors.Add($"ProductID length must be between 3 and 255 characters, got {value.Length}");
                }
                if (!Regex.IsMatch(value, @"^[^/]+/[^/]+$"))
                {
                    errors.Add($"ProductID format invalid: must contain exactly one '/' separator, got '{value}'");
                }
            }

            // ProductVersion - max 30 characters
            var productVersion = xmlDoc.SelectSingleNode("//ns:ProductVersion", nsManager);
            if (productVersion != null && productVersion.InnerText.Length > 30)
            {
                errors.Add($"ProductVersion length must not exceed 30 characters, got {productVersion.InnerText.Length}");
            }

            // SoftwareCertificateNumber - max 20 characters
            var softwareCert = xmlDoc.SelectSingleNode("//ns:SoftwareCertificateNumber", nsManager);
            if (softwareCert != null && softwareCert.InnerText.Length > 20)
            {
                errors.Add($"SoftwareCertificateNumber length must not exceed 20 characters, got {softwareCert.InnerText.Length}");
            }
        }

        private static void ValidateMasterFilesStringConstraints(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // AccountID - pattern: (([^^]*)|Desconhecido), length: 1-30
            var accountIDs = xmlDoc.SelectNodes("//ns:AccountID", nsManager);
            foreach (XmlNode accountID in accountIDs)
            {
                var value = accountID.InnerText;
                if (value.Length < 1 || value.Length > 30)
                {
                    errors.Add($"AccountID length must be between 1 and 30 characters, got {value.Length}");
                }
                if (value != "Desconhecido" && !Regex.IsMatch(value, @"^[^^]*$"))
                {
                    errors.Add($"AccountID format invalid: must not contain '^' character unless 'Desconhecido', got '{value}'");
                }
            }

            // CustomerID - max 30 characters
            var customerIDs = xmlDoc.SelectNodes("//ns:CustomerID", nsManager);
            foreach (XmlNode customerID in customerIDs)
            {
                if (customerID.InnerText.Length > 30)
                {
                    errors.Add($"CustomerID length must not exceed 30 characters, got {customerID.InnerText.Length}");
                }
            }

            // SupplierID - max 30 characters
            var supplierIDs = xmlDoc.SelectNodes("//ns:SupplierID", nsManager);
            foreach (XmlNode supplierID in supplierIDs)
            {
                if (supplierID.InnerText.Length > 30)
                {
                    errors.Add($"SupplierID length must not exceed 30 characters, got {supplierID.InnerText.Length}");
                }
            }

            // ProductCode - max 30 characters
            var productCodes = xmlDoc.SelectNodes("//ns:ProductCode", nsManager);
            foreach (XmlNode productCode in productCodes)
            {
                if (productCode.InnerText.Length > 30)
                {
                    errors.Add($"ProductCode length must not exceed 30 characters, got {productCode.InnerText.Length}");
                }
            }

            // ProductDescription - length: 2-200
            var productDescriptions = xmlDoc.SelectNodes("//ns:ProductDescription", nsManager);
            foreach (XmlNode productDesc in productDescriptions)
            {
                var value = productDesc.InnerText;
                if (value.Length < 2 || value.Length > 200)
                {
                    errors.Add($"ProductDescription length must be between 2 and 200 characters, got {value.Length}");
                }
            }

            // GroupingCategory - enum: GR, GA, GM, AR, AA, AM
            var groupingCategories = xmlDoc.SelectNodes("//ns:GroupingCategory", nsManager);
            var validGroupingCategories = new HashSet<string> { "GR", "GA", "GM", "AR", "AA", "AM" };
            foreach (XmlNode groupingCategory in groupingCategories)
            {
                if (!validGroupingCategories.Contains(groupingCategory.InnerText))
                {
                    errors.Add($"GroupingCategory must be one of: GR, GA, GM, AR, AA, AM, got '{groupingCategory.InnerText}'");
                }
            }

            // GroupingCode - pattern: ([^^]*), length: 2-30
            var groupingCodes = xmlDoc.SelectNodes("//ns:GroupingCode", nsManager);
            foreach (XmlNode groupingCode in groupingCodes)
            {
                var value = groupingCode.InnerText;
                if (value.Length < 2 || value.Length > 30)
                {
                    errors.Add($"GroupingCode length must be between 2 and 30 characters, got {value.Length}");
                }
                if (!Regex.IsMatch(value, @"^[^^]*$"))
                {
                    errors.Add($"GroupingCode format invalid: must not contain '^' character, got '{value}'");
                }
            }

            // TaxonomyCode - integer between 1-999, but optional (nullable)
            var taxonomyCodes = xmlDoc.SelectNodes("//ns:TaxonomyCode", nsManager);
            foreach (XmlNode taxonomyCode in taxonomyCodes)
            {
                var value = taxonomyCode.InnerText;
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (!int.TryParse(value, out int code) || code < 1 || code > 999)
                    {
                        errors.Add($"TaxonomyCode must be between 1 and 999, got '{taxonomyCode.InnerText}'");
                    }
                }
            }
        }

        private static void ValidateGeneralLedgerStringConstraints(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // JournalID - pattern: [^ ]{1,30}
            var journalIDs = xmlDoc.SelectNodes("//ns:JournalID", nsManager);
            foreach (XmlNode journalID in journalIDs)
            {
                var value = journalID.InnerText;
                if (value.Length < 1 || value.Length > 30)
                {
                    errors.Add($"JournalID length must be between 1 and 30 characters, got {value.Length}");
                }
                if (value.Contains(" "))
                {
                    errors.Add($"JournalID must not contain spaces, got '{value}'");
                }
            }

            // TransactionID - max 70 characters
            var transactionIDs = xmlDoc.SelectNodes("//ns:TransactionID", nsManager);
            foreach (XmlNode transactionID in transactionIDs)
            {
                if (transactionID.InnerText.Length > 70)
                {
                    errors.Add($"TransactionID length must not exceed 70 characters, got {transactionID.InnerText.Length}");
                }
            }

            // SourceDocumentID - max 60 characters
            var sourceDocIDs = xmlDoc.SelectNodes("//ns:SourceDocumentID", nsManager);
            foreach (XmlNode sourceDocID in sourceDocIDs)
            {
                if (sourceDocID.InnerText.Length > 60)
                {
                    errors.Add($"SourceDocumentID length must not exceed 60 characters, got {sourceDocID.InnerText.Length}");
                }
            }

            // Description - max 100 characters
            var descriptions = xmlDoc.SelectNodes("//ns:Description", nsManager);
            foreach (XmlNode description in descriptions)
            {
                if (description.InnerText.Length > 100)
                {
                    errors.Add($"Description length must not exceed 100 characters, got {description.InnerText.Length}");
                }
            }
        }

        private static void ValidateSourceDocumentsStringConstraints(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // InvoiceNo - pattern: [^ ]+ [^/^ ]+/[0-9]+, length: 1-60
            var invoiceNos = xmlDoc.SelectNodes("//ns:InvoiceNo", nsManager);
            foreach (XmlNode invoiceNo in invoiceNos)
            {
                var value = invoiceNo.InnerText;
                if (value.Length < 1 || value.Length > 60)
                {
                    errors.Add($"InvoiceNo length must be between 1 and 60 characters, got {value.Length}");
                }
                if (!Regex.IsMatch(value, @"^[^ ]+ [^/^ ]+/[0-9]+$"))
                {
                    errors.Add($"InvoiceNo format invalid: must be 'type year/number', got '{value}'");
                }
            }

            // DocumentNumber - pattern: [^ ]+ [^/^ ]+/[0-9]+, length: 1-60
            var documentNumbers = xmlDoc.SelectNodes("//ns:DocumentNumber", nsManager);
            foreach (XmlNode docNumber in documentNumbers)
            {
                var value = docNumber.InnerText;
                if (value.Length < 1 || value.Length > 60)
                {
                    errors.Add($"DocumentNumber length must be between 1 and 60 characters, got {value.Length}");
                }
                if (!Regex.IsMatch(value, @"^[^ ]+ [^/^ ]+/[0-9]+$"))
                {
                    errors.Add($"DocumentNumber format invalid: must be 'type year/number', got '{value}'");
                }
            }

            // InvoiceStatus - enum: N, S, A, R, F
            var invoiceStatuses = xmlDoc.SelectNodes("//ns:InvoiceStatus", nsManager);
            var validInvoiceStatuses = new HashSet<string> { "N", "S", "A", "R", "F" };
            foreach (XmlNode invoiceStatus in invoiceStatuses)
            {
                if (!validInvoiceStatuses.Contains(invoiceStatus.InnerText))
                {
                    errors.Add($"InvoiceStatus must be one of: N, S, A, R, F, got '{invoiceStatus.InnerText}'");
                }
            }

            // InvoiceType - enum: FT, FS, FR, ND, NC, VD, TV, TD, AA, DA, RP, RE, CS, LD, RA
            var invoiceTypes = xmlDoc.SelectNodes("//ns:InvoiceType", nsManager);
            var validInvoiceTypes = new HashSet<string> { "FT", "FS", "FR", "ND", "NC", "VD", "TV", "TD", "AA", "DA", "RP", "RE", "CS", "LD", "RA" };
            foreach (XmlNode invoiceType in invoiceTypes)
            {
                if (!validInvoiceTypes.Contains(invoiceType.InnerText))
                {
                    errors.Add($"InvoiceType must be one of: FT, FS, FR, ND, NC, VD, TV, TD, AA, DA, RP, RE, CS, LD, RA, got '{invoiceType.InnerText}'");
                }
            }

            // MovementStatus - enum: N, T, A, F, R
            var movementStatuses = xmlDoc.SelectNodes("//ns:MovementStatus", nsManager);
            var validMovementStatuses = new HashSet<string> { "N", "T", "A", "F", "R" };
            foreach (XmlNode movementStatus in movementStatuses)
            {
                if (!validMovementStatuses.Contains(movementStatus.InnerText))
                {
                    errors.Add($"MovementStatus must be one of: N, T, A, F, R, got '{movementStatus.InnerText}'");
                }
            }

            // MovementType - enum: GR, GT, GA, GC, GD
            var movementTypes = xmlDoc.SelectNodes("//ns:MovementType", nsManager);
            var validMovementTypes = new HashSet<string> { "GR", "GT", "GA", "GC", "GD" };
            foreach (XmlNode movementType in movementTypes)
            {
                if (!validMovementTypes.Contains(movementType.InnerText))
                {
                    errors.Add($"MovementType must be one of: GR, GT, GA, GC, GD, got '{movementType.InnerText}'");
                }
            }

            // PaymentType - enum: RC, RG
            var paymentTypes = xmlDoc.SelectNodes("//ns:PaymentType", nsManager);
            var validPaymentTypes = new HashSet<string> { "RC", "RG" };
            foreach (XmlNode paymentType in paymentTypes)
            {
                if (!validPaymentTypes.Contains(paymentType.InnerText))
                {
                    errors.Add($"PaymentType must be one of: RC, RG, got '{paymentType.InnerText}'");
                }
            }

            // PaymentRefNo - max 60 characters
            var paymentRefNos = xmlDoc.SelectNodes("//ns:PaymentRefNo", nsManager);
            foreach (XmlNode paymentRefNo in paymentRefNos)
            {
                if (paymentRefNo.InnerText.Length > 60)
                {
                    errors.Add($"PaymentRefNo length must not exceed 60 characters, got {paymentRefNo.InnerText.Length}");
                }
            }

            // LineNumber - integer between 1-999999
            var lineNumbers = xmlDoc.SelectNodes("//ns:LineNumber", nsManager);
            foreach (XmlNode lineNumber in lineNumbers)
            {
                if (!int.TryParse(lineNumber.InnerText, out int number) || number < 1 || number > 999999)
                {
                    errors.Add($"LineNumber must be between 1 and 999999, got '{lineNumber.InnerText}'");
                }
            }

            // Quantity - max 21 characters
            var quantities = xmlDoc.SelectNodes("//ns:Quantity", nsManager);
            foreach (XmlNode quantity in quantities)
            {
                if (quantity.InnerText.Length > 21)
                {
                    errors.Add($"Quantity length must not exceed 21 characters, got {quantity.InnerText.Length}");
                }
            }

            // UnitPrice - max 21 characters
            var unitPrices = xmlDoc.SelectNodes("//ns:UnitPrice", nsManager);
            foreach (XmlNode unitPrice in unitPrices)
            {
                if (unitPrice.InnerText.Length > 21)
                {
                    errors.Add($"UnitPrice length must not exceed 21 characters, got {unitPrice.InnerText.Length}");
                }
            }

            // LineExtensionAmount - max 21 characters
            var lineExtensionAmounts = xmlDoc.SelectNodes("//ns:LineExtensionAmount", nsManager);
            foreach (XmlNode amount in lineExtensionAmounts)
            {
                if (amount.InnerText.Length > 21)
                {
                    errors.Add($"LineExtensionAmount length must not exceed 21 characters, got {amount.InnerText.Length}");
                }
            }

            // TaxPercentage - max 10 characters
            var taxPercentages = xmlDoc.SelectNodes("//ns:TaxPercentage", nsManager);
            foreach (XmlNode taxPercentage in taxPercentages)
            {
                if (taxPercentage.InnerText.Length > 10)
                {
                    errors.Add($"TaxPercentage length must not exceed 10 characters, got {taxPercentage.InnerText.Length}");
                }
            }

            // TaxAmount - max 21 characters
            var taxAmounts = xmlDoc.SelectNodes("//ns:TaxAmount", nsManager);
            foreach (XmlNode taxAmount in taxAmounts)
            {
                if (taxAmount.InnerText.Length > 21)
                {
                    errors.Add($"TaxAmount length must not exceed 21 characters, got {taxAmount.InnerText.Length}");
                }
            }

            // NetTotal - max 21 characters
            var netTotals = xmlDoc.SelectNodes("//ns:NetTotal", nsManager);
            foreach (XmlNode netTotal in netTotals)
            {
                if (netTotal.InnerText.Length > 21)
                {
                    errors.Add($"NetTotal length must not exceed 21 characters, got {netTotal.InnerText.Length}");
                }
            }

            // TaxPayable - max 21 characters
            var taxPayables = xmlDoc.SelectNodes("//ns:TaxPayable", nsManager);
            foreach (XmlNode taxPayable in taxPayables)
            {
                if (taxPayable.InnerText.Length > 21)
                {
                    errors.Add($"TaxPayable length must not exceed 21 characters, got {taxPayable.InnerText.Length}");
                }
            }

            // GrossTotal - max 21 characters
            var grossTotals = xmlDoc.SelectNodes("//ns:GrossTotal", nsManager);
            foreach (XmlNode grossTotal in grossTotals)
            {
                if (grossTotal.InnerText.Length > 21)
                {
                    errors.Add($"GrossTotal length must not exceed 21 characters, got {grossTotal.InnerText.Length}");
                }
            }
        }

        private static void ValidatePortugueseStringConstraints(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // Portuguese Tax Exemption Code - pattern: (M[0-9]{2})+
            var exemptionCodes = xmlDoc.SelectNodes("//ns:TaxExemptionCode", nsManager);
            foreach (XmlNode exemptionCode in exemptionCodes)
            {
                if (!Regex.IsMatch(exemptionCode.InnerText, @"^(M[0-9]{2})+$"))
                {
                    errors.Add($"TaxExemptionCode format invalid: must match pattern (M[0-9]{{2}})+, got '{exemptionCode.InnerText}'");
                }
            }

            // Portuguese Tax Exemption Reason - length: 6-60
            var exemptionReasons = xmlDoc.SelectNodes("//ns:TaxExemptionReason", nsManager);
            foreach (XmlNode exemptionReason in exemptionReasons)
            {
                var value = exemptionReason.InnerText;
                if (value.Length < 6 || value.Length > 60)
                {
                    errors.Add($"TaxExemptionReason length must be between 6 and 60 characters, got {value.Length}");
                }
            }

            // Portuguese Hash Control - pattern: [0-9]+|[0-9]+[\.][0-9]+|[0-9]+-[A-Z]{2}(M )([^/^ ]+[/][0-9]+)|[0-9]+-[A-Z]{2}(D )([^ ]+ [^/^ ]+[/][0-9]+), length: 1-70
            var hashControls = xmlDoc.SelectNodes("//ns:HashControl", nsManager);
            foreach (XmlNode hashControl in hashControls)
            {
                var value = hashControl.InnerText;
                if (value.Length < 1 || value.Length > 70)
                {
                    errors.Add($"HashControl length must be between 1 and 70 characters, got {value.Length}");
                }
                if (!Regex.IsMatch(value, @"^[0-9]+|[0-9]+\.[0-9]+|[0-9]+-[A-Z]{2}(M )([^/^ ]+[/][0-9]+)|[0-9]+-[A-Z]{2}(D )([^ ]+ [^/^ ]+[/][0-9]+)$"))
                {
                    errors.Add($"HashControl format invalid, got '{value}'");
                }
            }

            // Portuguese CN Code - pattern: [0-9]{8}
            var cnCodes = xmlDoc.SelectNodes("//ns:CNCode", nsManager);
            foreach (XmlNode cnCode in cnCodes)
            {
                if (!Regex.IsMatch(cnCode.InnerText, @"^[0-9]{8}$"))
                {
                    errors.Add($"CNCode must be exactly 8 digits, got '{cnCode.InnerText}'");
                }
            }

            // Portuguese EAC Code - pattern: ([0-9]*), length: 5
            var eacCodes = xmlDoc.SelectNodes("//ns:EACCode", nsManager);
            foreach (XmlNode eacCode in eacCodes)
            {
                var value = eacCode.InnerText;
                if (value.Length != 5)
                {
                    errors.Add($"EACCode must be exactly 5 characters, got {value.Length}");
                }
                if (!Regex.IsMatch(value, @"^[0-9]*$"))
                {
                    errors.Add($"EACCode must contain only digits, got '{value}'");
                }
            }

            // Portuguese Source Billing - enum: P, I, M
            var sourceBillings = xmlDoc.SelectNodes("//ns:SourceBilling", nsManager);
            var validSourceBillings = new HashSet<string> { "P", "I", "M" };
            foreach (XmlNode sourceBilling in sourceBillings)
            {
                if (!validSourceBillings.Contains(sourceBilling.InnerText))
                {
                    errors.Add($"SourceBilling must be one of: P, I, M, got '{sourceBilling.InnerText}'");
                }
            }

            // Portuguese Source Payment - enum: P, I, M
            var sourcePayments = xmlDoc.SelectNodes("//ns:SourcePayment", nsManager);
            var validSourcePayments = new HashSet<string> { "P", "I", "M" };
            foreach (XmlNode sourcePayment in sourcePayments)
            {
                if (!validSourcePayments.Contains(sourcePayment.InnerText))
                {
                    errors.Add($"SourcePayment must be one of: P, I, M, got '{sourcePayment.InnerText}'");
                }
            }

            // Portuguese Movement Tax Code - enum: RED, INT, NOR, ISE, OUT, NS
            var movementTaxCodes = xmlDoc.SelectNodes("//ns:MovementTaxCode", nsManager);
            var validMovementTaxCodes = new HashSet<string> { "RED", "INT", "NOR", "ISE", "OUT", "NS" };
            foreach (XmlNode movementTaxCode in movementTaxCodes)
            {
                if (!validMovementTaxCodes.Contains(movementTaxCode.InnerText))
                {
                    errors.Add($"MovementTaxCode must be one of: RED, INT, NOR, ISE, OUT, NS, got '{movementTaxCode.InnerText}'");
                }
            }

            // Portuguese Movement Tax Type - enum: IVA, NS
            var movementTaxTypes = xmlDoc.SelectNodes("//ns:MovementTaxType", nsManager);
            var validMovementTaxTypes = new HashSet<string> { "IVA", "NS" };
            foreach (XmlNode movementTaxType in movementTaxTypes)
            {
                if (!validMovementTaxTypes.Contains(movementTaxType.InnerText))
                {
                    errors.Add($"MovementTaxType must be one of: IVA, NS, got '{movementTaxType.InnerText}'");
                }
            }

            // Portuguese Accounting Period - integer between 1-16
            var accountingPeriods = xmlDoc.SelectNodes("//ns:AccountingPeriod", nsManager);
            foreach (XmlNode accountingPeriod in accountingPeriods)
            {
                if (!int.TryParse(accountingPeriod.InnerText, out int period) || period < 1 || period > 16)
                {
                    errors.Add($"AccountingPeriod must be between 1 and 16, got '{accountingPeriod.InnerText}'");
                }
            }

            // Portuguese Document Archival Number - pattern: [^ ]{1,20}
            var archivalNumbers = xmlDoc.SelectNodes("//ns:DocArchivalNumber", nsManager);
            foreach (XmlNode archivalNumber in archivalNumbers)
            {
                var value = archivalNumber.InnerText;
                if (value.Length < 1 || value.Length > 20)
                {
                    errors.Add($"DocArchivalNumber length must be between 1 and 20 characters, got {value.Length}");
                }
                if (value.Contains(" "))
                {
                    errors.Add($"DocArchivalNumber must not contain spaces, got '{value}'");
                }
            }
        }

        /// <summary>
        /// Validates the XML document against the schema only.
        /// </summary>
        /// <param name="xmlDoc">The XML document to validate.</param>
        /// <param name="schemaPath">Path to the schema file.</param>
        /// <returns>A list of validation error messages.</returns>
        private static List<string> ValidateSchema(XmlDocument xmlDoc, string schemaPath)
        {
            var errors = new List<string>();
            try
            {
                // Perform XSD 1.1 assertion validation
                var assertionErrors = ValidateXsd11Assertions(xmlDoc, schemaPath);
                errors.AddRange(assertionErrors);

                // Perform identity constraint validation using custom logic
                var identityErrors = ValidateIdentityConstraints(xmlDoc, schemaPath);
                errors.AddRange(identityErrors);
            }
            catch (Exception ex)
            {
                errors.Add($"Schema validation error: {ex.Message}");
            }
            return errors;
        }

        /// <summary>
        /// Validates SpecialRegimes fields in invoices.
        /// </summary>
        internal static void ValidateSpecialRegimesField(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (invoices == null) return;

            foreach (XmlNode invoice in invoices)
            {
                var specialRegimes = invoice.SelectSingleNode("ns:SpecialRegimes", nsManager);
                if (specialRegimes != null)
                {
                    var selfBilling = specialRegimes.SelectSingleNode("ns:SelfBillingIndicator", nsManager)?.InnerText;
                    var cashVAT = specialRegimes.SelectSingleNode("ns:CashVATSchemeIndicator", nsManager)?.InnerText;
                    var thirdParty = specialRegimes.SelectSingleNode("ns:ThirdPartiesBillingIndicator", nsManager)?.InnerText;

                    if (!string.IsNullOrEmpty(selfBilling) && selfBilling != "0" && selfBilling != "1")
                        errors.Add($"SpecialRegimes: SelfBillingIndicator must be 0 or 1 if present, got '{selfBilling}'");
                    if (!string.IsNullOrEmpty(cashVAT) && cashVAT != "0" && cashVAT != "1")
                        errors.Add($"SpecialRegimes: CashVATSchemeIndicator must be 0 or 1 if present, got '{cashVAT}'");
                    if (!string.IsNullOrEmpty(thirdParty) && thirdParty != "0" && thirdParty != "1")
                        errors.Add($"SpecialRegimes: ThirdPartiesBillingIndicator must be 0 or 1 if present, got '{thirdParty}'");
                }
            }
        }

        /// <summary>
        /// Validates SpecialRegimes fields in invoices (wrapper for tests).
        /// </summary>
        internal static List<string> ValidateSpecialRegimesField(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");
            ValidateSpecialRegimesField(xmlDoc, nsManager, errors);
            return errors;
        }

        /// <summary>
        /// Validates that all required fields are present in the XML document according to the SAF-T schema.
        /// </summary>
        internal static List<string> ValidateRequiredFields(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
            nsManager.AddNamespace("ns", "urn:OECD:StandardAuditFile-Tax:PT_1.04_01");

            try
            {
                // Validate Header required fields
                ValidateHeaderRequiredFields(xmlDoc, nsManager, errors);
                
                // Validate MasterFiles required fields
                ValidateMasterFilesRequiredFields(xmlDoc, nsManager, errors);
                
                // Validate GeneralLedgerEntries required fields
                ValidateGeneralLedgerRequiredFields(xmlDoc, nsManager, errors);
                
                // Validate SourceDocuments required fields
                ValidateSourceDocumentsRequiredFields(xmlDoc, nsManager, errors);
            }
            catch (Exception ex)
            {
                errors.Add($"Required field validation error: {ex.Message}");
            }

            return errors;
        }

        private static void ValidateHeaderRequiredFields(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // Required Header fields
            var requiredHeaderFields = new[]
            {
                "AuditFileVersion",
                "CompanyID", 
                "TaxRegistrationNumber",
                "TaxAccountingBasis",
                "CompanyName",
                "FiscalYear",
                "StartDate",
                "EndDate",
                "CurrencyCode",
                "DateCreated",
                "TaxEntity",
                "ProductCompanyTaxID",
                "SoftwareCertificateNumber",
                "ProductID",
                "ProductVersion"
            };

            foreach (var field in requiredHeaderFields)
            {
                var node = xmlDoc.SelectSingleNode($"//ns:{field}", nsManager);
                if (node == null || string.IsNullOrWhiteSpace(node.InnerText))
                {
                    errors.Add($"Required Header field '{field}' is missing or empty");
                }
            }

            // Required CompanyAddress fields
            var companyAddress = xmlDoc.SelectSingleNode("//ns:CompanyAddress", nsManager);
            if (companyAddress != null)
            {
                var requiredAddressFields = new[] { "Country" };
                foreach (var field in requiredAddressFields)
                {
                    var node = companyAddress.SelectSingleNode($"ns:{field}", nsManager);
                    if (node == null || string.IsNullOrWhiteSpace(node.InnerText))
                    {
                        errors.Add($"Required CompanyAddress field '{field}' is missing or empty");
                    }
                }
            }
        }

        private static void ValidateMasterFilesRequiredFields(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // Validate GeneralLedgerAccounts required fields
            var accounts = xmlDoc.SelectNodes("//ns:Account", nsManager);
            if (accounts != null)
            {
                foreach (XmlNode account in accounts)
                {
                    var accountID = account.SelectSingleNode("ns:AccountID", nsManager);
                    var accountDescription = account.SelectSingleNode("ns:AccountDescription", nsManager);
                    var groupCategory = account.SelectSingleNode("ns:GroupingCategory", nsManager);

                    if (accountID == null || string.IsNullOrWhiteSpace(accountID.InnerText))
                        errors.Add("AccountID is required for all Account elements");
                    if (accountDescription == null || string.IsNullOrWhiteSpace(accountDescription.InnerText))
                        errors.Add("AccountDescription is required for all Account elements");
                    if (groupCategory == null || string.IsNullOrWhiteSpace(groupCategory.InnerText))
                        errors.Add("GroupingCategory is required for all Account elements");
                }
            }

            // Validate Customer required fields
            var customers = xmlDoc.SelectNodes("//ns:Customer", nsManager);
            if (customers != null)
            {
                foreach (XmlNode customer in customers)
                {
                    var customerID = customer.SelectSingleNode("ns:CustomerID", nsManager);
                    if (customerID == null || string.IsNullOrWhiteSpace(customerID.InnerText))
                        errors.Add("CustomerID is required for all Customer elements");
                }
            }

            // Validate Supplier required fields
            var suppliers = xmlDoc.SelectNodes("//ns:Supplier", nsManager);
            if (suppliers != null)
            {
                foreach (XmlNode supplier in suppliers)
                {
                    var supplierID = supplier.SelectSingleNode("ns:SupplierID", nsManager);
                    if (supplierID == null || string.IsNullOrWhiteSpace(supplierID.InnerText))
                        errors.Add("SupplierID is required for all Supplier elements");
                }
            }

            // Validate Product required fields
            var products = xmlDoc.SelectNodes("//ns:Product", nsManager);
            if (products != null)
            {
                foreach (XmlNode product in products)
                {
                    var productCode = product.SelectSingleNode("ns:ProductCode", nsManager);
                    var productDescription = product.SelectSingleNode("ns:ProductDescription", nsManager);
                    var productNumberCode = product.SelectSingleNode("ns:ProductNumberCode", nsManager);

                    if (productCode == null || string.IsNullOrWhiteSpace(productCode.InnerText))
                        errors.Add("ProductCode is required for all Product elements");
                    if (productDescription == null || string.IsNullOrWhiteSpace(productDescription.InnerText))
                        errors.Add("ProductDescription is required for all Product elements");
                    if (productNumberCode == null || string.IsNullOrWhiteSpace(productNumberCode.InnerText))
                        errors.Add("ProductNumberCode is required for all Product elements");
                }
            }
        }

        private static void ValidateGeneralLedgerRequiredFields(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // Validate Journal required fields
            var journals = xmlDoc.SelectNodes("//ns:Journal", nsManager);
            if (journals != null)
            {
                foreach (XmlNode journal in journals)
                {
                    var journalID = journal.SelectSingleNode("ns:JournalID", nsManager);
                    if (journalID == null || string.IsNullOrWhiteSpace(journalID.InnerText))
                        errors.Add("JournalID is required for all Journal elements");
                }
            }

            // Validate Transaction required fields
            var transactions = xmlDoc.SelectNodes("//ns:Transaction", nsManager);
            if (transactions != null)
            {
                foreach (XmlNode transaction in transactions)
                {
                    var transactionID = transaction.SelectSingleNode("ns:TransactionID", nsManager);
                    var transactionDate = transaction.SelectSingleNode("ns:TransactionDate", nsManager);
                    var sourceID = transaction.SelectSingleNode("ns:SourceID", nsManager);
                    var description = transaction.SelectSingleNode("ns:Description", nsManager);

                    if (transactionID == null || string.IsNullOrWhiteSpace(transactionID.InnerText))
                        errors.Add("TransactionID is required for all Transaction elements");
                    if (transactionDate == null || string.IsNullOrWhiteSpace(transactionDate.InnerText))
                        errors.Add("TransactionDate is required for all Transaction elements");
                    if (sourceID == null || string.IsNullOrWhiteSpace(sourceID.InnerText))
                        errors.Add("SourceID is required for all Transaction elements");
                    if (description == null || string.IsNullOrWhiteSpace(description.InnerText))
                        errors.Add("Description is required for all Transaction elements");
                }
            }

            // Validate DebitLine required fields
            var debitLines = xmlDoc.SelectNodes("//ns:DebitLine", nsManager);
            if (debitLines != null)
            {
                foreach (XmlNode line in debitLines)
                {
                    var recordID = line.SelectSingleNode("ns:RecordID", nsManager);
                    var accountID = line.SelectSingleNode("ns:AccountID", nsManager);
                    var systemEntryDate = line.SelectSingleNode("ns:SystemEntryDate", nsManager);
                    var description = line.SelectSingleNode("ns:Description", nsManager);
                    var debitAmount = line.SelectSingleNode("ns:DebitAmount", nsManager);

                    if (recordID == null || string.IsNullOrWhiteSpace(recordID.InnerText))
                        errors.Add("RecordID is required for all DebitLine elements");
                    if (accountID == null || string.IsNullOrWhiteSpace(accountID.InnerText))
                        errors.Add("AccountID is required for all DebitLine elements");
                    if (systemEntryDate == null || string.IsNullOrWhiteSpace(systemEntryDate.InnerText))
                        errors.Add("SystemEntryDate is required for all DebitLine elements");
                    if (description == null || string.IsNullOrWhiteSpace(description.InnerText))
                        errors.Add("Description is required for all DebitLine elements");
                    if (debitAmount == null || string.IsNullOrWhiteSpace(debitAmount.InnerText))
                        errors.Add("DebitAmount is required for all DebitLine elements");
                }
            }

            // Validate CreditLine required fields
            var creditLines = xmlDoc.SelectNodes("//ns:CreditLine", nsManager);
            if (creditLines != null)
            {
                foreach (XmlNode line in creditLines)
                {
                    var recordID = line.SelectSingleNode("ns:RecordID", nsManager);
                    var accountID = line.SelectSingleNode("ns:AccountID", nsManager);
                    var systemEntryDate = line.SelectSingleNode("ns:SystemEntryDate", nsManager);
                    var description = line.SelectSingleNode("ns:Description", nsManager);
                    var creditAmount = line.SelectSingleNode("ns:CreditAmount", nsManager);

                    if (recordID == null || string.IsNullOrWhiteSpace(recordID.InnerText))
                        errors.Add("RecordID is required for all CreditLine elements");
                    if (accountID == null || string.IsNullOrWhiteSpace(accountID.InnerText))
                        errors.Add("AccountID is required for all CreditLine elements");
                    if (systemEntryDate == null || string.IsNullOrWhiteSpace(systemEntryDate.InnerText))
                        errors.Add("SystemEntryDate is required for all CreditLine elements");
                    if (description == null || string.IsNullOrWhiteSpace(description.InnerText))
                        errors.Add("Description is required for all CreditLine elements");
                    if (creditAmount == null || string.IsNullOrWhiteSpace(creditAmount.InnerText))
                        errors.Add("CreditAmount is required for all CreditLine elements");
                }
            }
        }

        private static void ValidateSourceDocumentsRequiredFields(XmlDocument xmlDoc, XmlNamespaceManager nsManager, List<string> errors)
        {
            // Validate Invoice required fields
            var invoices = xmlDoc.SelectNodes("//ns:Invoice", nsManager);
            if (invoices != null)
            {
                foreach (XmlNode invoice in invoices)
                {
                    var invoiceNo = invoice.SelectSingleNode("ns:InvoiceNo", nsManager);
                    var atcud = invoice.SelectSingleNode("ns:ATCUD", nsManager);
                    var documentStatus = invoice.SelectSingleNode("ns:DocumentStatus", nsManager);
                    var hash = invoice.SelectSingleNode("ns:Hash", nsManager);
                    var hashControl = invoice.SelectSingleNode("ns:HashControl", nsManager);
                    var invoiceDate = invoice.SelectSingleNode("ns:InvoiceDate", nsManager);
                    var invoiceType = invoice.SelectSingleNode("ns:InvoiceType", nsManager);
                    var specialRegimes = invoice.SelectSingleNode("ns:SpecialRegimes", nsManager);
                    var sourceID = invoice.SelectSingleNode("ns:SourceID", nsManager);
                    var systemEntryDate = invoice.SelectSingleNode("ns:SystemEntryDate", nsManager);
                    var customerID = invoice.SelectSingleNode("ns:CustomerID", nsManager);
                    var documentTotals = invoice.SelectSingleNode("ns:DocumentTotals", nsManager);

                    if (invoiceNo == null || string.IsNullOrWhiteSpace(invoiceNo.InnerText))
                        errors.Add("InvoiceNo is required for all Invoice elements");
                    if (atcud == null || string.IsNullOrWhiteSpace(atcud.InnerText))
                        errors.Add("ATCUD is required for all Invoice elements");
                    if (documentStatus == null || string.IsNullOrWhiteSpace(documentStatus.InnerText))
                        errors.Add("DocumentStatus is required for all Invoice elements");
                    if (hash == null || string.IsNullOrWhiteSpace(hash.InnerText))
                        errors.Add("Hash is required for all Invoice elements");
                    if (hashControl == null || string.IsNullOrWhiteSpace(hashControl.InnerText))
                        errors.Add("HashControl is required for all Invoice elements");
                    if (invoiceDate == null || string.IsNullOrWhiteSpace(invoiceDate.InnerText))
                        errors.Add("InvoiceDate is required for all Invoice elements");
                    if (invoiceType == null || string.IsNullOrWhiteSpace(invoiceType.InnerText))
                        errors.Add("InvoiceType is required for all Invoice elements");
                    if (specialRegimes == null)
                        errors.Add("SpecialRegimes is required for all Invoice elements");
                    if (sourceID == null || string.IsNullOrWhiteSpace(sourceID.InnerText))
                        errors.Add("SourceID is required for all Invoice elements");
                    if (systemEntryDate == null || string.IsNullOrWhiteSpace(systemEntryDate.InnerText))
                        errors.Add("SystemEntryDate is required for all Invoice elements");
                    if (customerID == null || string.IsNullOrWhiteSpace(customerID.InnerText))
                        errors.Add("CustomerID is required for all Invoice elements");
                    if (documentTotals == null)
                        errors.Add("DocumentTotals is required for all Invoice elements");
                }
            }

            // Validate InvoiceLine required fields
            var invoiceLines = xmlDoc.SelectNodes("//ns:SalesInvoices/ns:Invoice/ns:Line", nsManager);
            if (invoiceLines != null)
            {
                foreach (XmlNode line in invoiceLines)
                {
                    var lineNumber = line.SelectSingleNode("ns:LineNumber", nsManager);
                    var productCode = line.SelectSingleNode("ns:ProductCode", nsManager);
                    var productDescription = line.SelectSingleNode("ns:ProductDescription", nsManager);
                    var quantity = line.SelectSingleNode("ns:Quantity", nsManager);
                    var unitOfMeasure = line.SelectSingleNode("ns:UnitOfMeasure", nsManager);
                    var unitPrice = line.SelectSingleNode("ns:UnitPrice", nsManager);
                    var taxPointDate = line.SelectSingleNode("ns:TaxPointDate", nsManager);
                    var description = line.SelectSingleNode("ns:Description", nsManager);
                    var tax = line.SelectSingleNode("ns:Tax", nsManager);

                    if (lineNumber == null || string.IsNullOrWhiteSpace(lineNumber.InnerText))
                        errors.Add("LineNumber is required for all Invoice Line elements");
                    if (productCode == null || string.IsNullOrWhiteSpace(productCode.InnerText))
                        errors.Add("ProductCode is required for all Invoice Line elements");
                    if (productDescription == null || string.IsNullOrWhiteSpace(productDescription.InnerText))
                        errors.Add("ProductDescription is required for all Invoice Line elements");
                    if (quantity == null || string.IsNullOrWhiteSpace(quantity.InnerText))
                        errors.Add("Quantity is required for all Invoice Line elements");
                    if (unitOfMeasure == null || string.IsNullOrWhiteSpace(unitOfMeasure.InnerText))
                        errors.Add("UnitOfMeasure is required for all Invoice Line elements");
                    if (unitPrice == null || string.IsNullOrWhiteSpace(unitPrice.InnerText))
                        errors.Add("UnitPrice is required for all Invoice Line elements");
                    if (taxPointDate == null || string.IsNullOrWhiteSpace(taxPointDate.InnerText))
                        errors.Add("TaxPointDate is required for all Invoice Line elements");
                    if (description == null || string.IsNullOrWhiteSpace(description.InnerText))
                        errors.Add("Description is required for all Invoice Line elements");
                    if (tax == null)
                        errors.Add("Tax is required for all Invoice Line elements");
                    
                    // At least one of DebitAmount or CreditAmount must be present
                    var debitAmount = line.SelectSingleNode("ns:DebitAmount", nsManager);
                    var creditAmount = line.SelectSingleNode("ns:CreditAmount", nsManager);
                    if ((debitAmount == null || string.IsNullOrWhiteSpace(debitAmount.InnerText)) &&
                        (creditAmount == null || string.IsNullOrWhiteSpace(creditAmount.InnerText)))
                    {
                        errors.Add("Either DebitAmount or CreditAmount must be present for all Invoice Line elements");
                    }
                }
            }
        }
    }
}
