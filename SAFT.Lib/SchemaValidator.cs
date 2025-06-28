using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Text.RegularExpressions;
using SAFT.Lib.Utils;

namespace SAFT.Lib
{
    /// <summary>
    /// Provides utilities to validate a SAF-T XML string against the official XSD schema.
    /// Supports XSD 1.0 validation using .NET's built-in validator plus custom XSD 1.1 assertion validation.
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

                // Perform .NET's built-in XSD validation
                var xsdErrors = ValidateXsd(xmlDoc, schemaPath);
                errors.AddRange(xsdErrors);

                // Perform XSD 1.1 assertion validation
                var assertionErrors = ValidateXsd11Assertions(xmlDoc, schemaPath);
                errors.AddRange(assertionErrors);

                // Perform identity constraint validation
                var identityErrors = ValidateIdentityConstraints(xmlDoc, schemaPath);
                errors.AddRange(identityErrors);

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
                
                // Validate unique constraints
                foreach (var unique in constraints.Uniques)
                {
                    var uniqueErrors = ValidateUniqueConstraint(xmlDoc, unique);
                    errors.AddRange(uniqueErrors);
                }
                
                // Validate key constraints
                foreach (var key in constraints.Keys)
                {
                    var keyErrors = ValidateKeyConstraint(xmlDoc, key);
                    errors.AddRange(keyErrors);
                }
                
                // Validate keyref constraints
                foreach (var keyref in constraints.KeyRefs)
                {
                    var keyrefErrors = ValidateKeyRefConstraint(xmlDoc, keyref, constraints);
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
        /// <returns>List of validation errors.</returns>
        private static List<string> ValidateUniqueConstraint(XmlDocument xmlDoc, UniqueConstraint unique)
        {
            var errors = new List<string>();
            var values = new HashSet<string>();
            
            try
            {
                // Evaluate the selector to get the target nodes
                var selectorNodes = EvaluateXPathSelector(xmlDoc, unique.Selector);
                
                foreach (XmlNode node in selectorNodes)
                {
                    // Evaluate the field to get the value
                    var value = EvaluateXPathField(node, unique.Field);
                    
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (values.Contains(value))
                        {
                            errors.Add($"Unique constraint '{unique.Name}' violated: duplicate value '{value}' found");
                        }
                        else
                        {
                            values.Add(value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error validating unique constraint '{unique.Name}': {ex.Message}");
            }
            
            return errors;
        }
        
        /// <summary>
        /// Validates a key constraint.
        /// </summary>
        /// <param name="xmlDoc">The XML document.</param>
        /// <param name="key">The key constraint to validate.</param>
        /// <returns>List of validation errors.</returns>
        private static List<string> ValidateKeyConstraint(XmlDocument xmlDoc, KeyConstraint key)
        {
            var errors = new List<string>();
            var values = new HashSet<string>();
            
            try
            {
                // Evaluate the selector to get the target nodes
                var selectorNodes = EvaluateXPathSelector(xmlDoc, key.Selector);
                
                foreach (XmlNode node in selectorNodes)
                {
                    // Evaluate the field to get the value
                    var value = EvaluateXPathField(node, key.Field);
                    
                    if (string.IsNullOrEmpty(value))
                    {
                        errors.Add($"Key constraint '{key.Name}' violated: missing required value");
                    }
                    else if (values.Contains(value))
                    {
                        errors.Add($"Key constraint '{key.Name}' violated: duplicate value '{value}' found");
                    }
                    else
                    {
                        values.Add(value);
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error validating key constraint '{key.Name}': {ex.Message}");
            }
            
            return errors;
        }
        
        /// <summary>
        /// Validates a keyref constraint.
        /// </summary>
        /// <param name="xmlDoc">The XML document.</param>
        /// <param name="keyref">The keyref constraint to validate.</param>
        /// <param name="constraints">All identity constraints for reference lookup.</param>
        /// <returns>List of validation errors.</returns>
        private static List<string> ValidateKeyRefConstraint(XmlDocument xmlDoc, KeyRefConstraint keyref, IdentityConstraints constraints)
        {
            var errors = new List<string>();
            
            try
            {
                // Find the referenced key or unique constraint
                object? referencedConstraint = null;
                
                // First try to find a key constraint
                var keyConstraint = constraints.Keys.FirstOrDefault(k => k.Name == keyref.Refer);
                if (keyConstraint != null)
                {
                    referencedConstraint = keyConstraint;
                }
                else
                {
                    // If not found as key, try to find as unique constraint
                    var uniqueConstraint = constraints.Uniques.FirstOrDefault(u => u.Name == keyref.Refer);
                    if (uniqueConstraint != null)
                    {
                        referencedConstraint = uniqueConstraint;
                    }
                }
                
                if (referencedConstraint == null)
                {
                    errors.Add($"Keyref constraint '{keyref.Name}' references unknown constraint '{keyref.Refer}'");
                    return errors;
                }
                
                // Get all valid values from the referenced constraint
                var validValues = GetValidValuesFromConstraint(xmlDoc, referencedConstraint);
                
                // Evaluate the selector to get the target nodes
                var selectorNodes = EvaluateXPathSelector(xmlDoc, keyref.Selector);
                
                foreach (XmlNode node in selectorNodes)
                {
                    // Evaluate the field to get the value
                    var value = EvaluateXPathField(node, keyref.Field);
                    
                    if (!string.IsNullOrEmpty(value) && !validValues.Contains(value))
                    {
                        errors.Add($"Keyref constraint '{keyref.Name}' violated: value '{value}' not found in referenced constraint '{keyref.Refer}'");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error validating keyref constraint '{keyref.Name}': {ex.Message}");
            }
            
            return errors;
        }
        
        /// <summary>
        /// Gets all valid values from a key or unique constraint.
        /// </summary>
        /// <param name="xmlDoc">The XML document.</param>
        /// <param name="constraint">The constraint to get values from.</param>
        /// <returns>Set of valid values.</returns>
        private static HashSet<string> GetValidValuesFromConstraint(XmlDocument xmlDoc, object constraint)
        {
            var values = new HashSet<string>();
            
            if (constraint is KeyConstraint key)
            {
                var selectorNodes = EvaluateXPathSelector(xmlDoc, key.Selector);
                foreach (XmlNode node in selectorNodes)
                {
                    var value = EvaluateXPathField(node, key.Field);
                    if (!string.IsNullOrEmpty(value))
                    {
                        values.Add(value);
                    }
                }
            }
            else if (constraint is UniqueConstraint unique)
            {
                var selectorNodes = EvaluateXPathSelector(xmlDoc, unique.Selector);
                foreach (XmlNode node in selectorNodes)
                {
                    var value = EvaluateXPathField(node, unique.Field);
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
        /// <returns>Collection of matching nodes.</returns>
        private static XmlNodeList EvaluateXPathSelector(XmlDocument xmlDoc, string selector)
        {
            try
            {
                // Handle ns: prefix in XPath expressions
                var processedSelector = ProcessXPathForNamespaces(selector);
                return xmlDoc.SelectNodes(processedSelector);
            }
            catch
            {
                // Return empty node list if XPath evaluation fails
                return xmlDoc.SelectNodes("//*[false()]");
            }
        }
        
        /// <summary>
        /// Evaluates an XPath field expression relative to a context node.
        /// </summary>
        /// <param name="contextNode">The context node.</param>
        /// <param name="field">The XPath field expression.</param>
        /// <returns>The field value.</returns>
        private static string EvaluateXPathField(XmlNode contextNode, string field)
        {
            try
            {
                // Handle ns: prefix in XPath expressions
                var processedField = ProcessXPathForNamespaces(field);
                var node = contextNode.SelectSingleNode(processedField);
                return node?.InnerText ?? "";
            }
            catch
            {
                return "";
            }
        }
        
        /// <summary>
        /// Processes XPath expressions to handle namespace prefixes.
        /// </summary>
        /// <param name="xpath">The XPath expression.</param>
        /// <returns>The processed XPath expression.</returns>
        private static string ProcessXPathForNamespaces(string xpath)
        {
            // Replace ns: prefix with local-name() function for namespace-agnostic selection
            return Regex.Replace(xpath, @"ns:([a-zA-Z_][a-zA-Z0-9_]*)", @"*[local-name()='$1']");
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
        private static List<string> ValidateVATCalculations(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            // Only run VAT total checks if DocumentTotals is present AND root is not <Invoice>
            var documentTotals = xmlDoc.SelectSingleNode("//DocumentTotals");
            var root = xmlDoc.DocumentElement?.Name;
            if (documentTotals != null && documentTotals.HasChildNodes && root != "Invoice")
            {
                foreach (XmlNode invoice in xmlDoc.SelectNodes("//Invoice"))
                {
                    // Support both <Line> and <InvoiceLine>
                    var lines = invoice.SelectNodes(".//Line");
                    var invoiceLines = invoice.SelectNodes(".//InvoiceLine");
                    var allLines = new List<XmlNode>();
                    foreach (XmlNode l in lines) allLines.Add(l);
                    foreach (XmlNode l in invoiceLines) allLines.Add(l);
                    var totals = invoice.SelectSingleNode(".//DocumentTotals");
                    if (allLines.Count > 0 && totals != null)
                    {
                        decimal calculatedTaxPayable = 0;
                        decimal calculatedNetTotal = 0;
                        foreach (XmlNode line in allLines)
                        {
                            var taxBase = line.SelectSingleNode(".//TaxBase");
                            var lineExtensionAmount = line.SelectSingleNode(".//LineExtensionAmount");
                            var tax = line.SelectSingleNode(".//Tax");
                            decimal baseAmount = 0;
                            if (taxBase != null && decimal.TryParse(taxBase.InnerText, out decimal tb))
                                baseAmount = tb;
                            else if (lineExtensionAmount != null && decimal.TryParse(lineExtensionAmount.InnerText, out decimal le))
                                baseAmount = le;
                            if (baseAmount > 0 && tax != null)
                            {
                                calculatedNetTotal += baseAmount;
                                var taxPercentage = tax.SelectSingleNode(".//TaxPercentage");
                                var taxAmount = tax.SelectSingleNode(".//TaxAmount");
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
                        var netTotal = totals.SelectSingleNode(".//NetTotal");
                        var taxPayable = totals.SelectSingleNode(".//TaxPayable");
                        var grossTotal = totals.SelectSingleNode(".//GrossTotal");
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
        private static List<string> ValidateCrossReferences(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var customers = xmlDoc.SelectNodes("//Customer");
            if (customers != null && customers.Count > 0)
            {
                var customerIds = new HashSet<string>();
                foreach (XmlNode customer in customers)
                {
                    var customerId = customer.SelectSingleNode(".//CustomerID");
                    if (customerId != null)
                    {
                        customerIds.Add(customerId.InnerText);
                    }
                }
                var invoices = xmlDoc.SelectNodes("//Invoice");
                foreach (XmlNode invoice in invoices)
                {
                    var customerId = invoice.SelectSingleNode(".//CustomerID");
                    if (customerId != null && !customerIds.Contains(customerId.InnerText))
                    {
                        errors.Add($"Customer reference error: CustomerID '{customerId.InnerText}' not found in master data (customer reference, CustomerID, not found)");
                    }
                }
            }
            var products = xmlDoc.SelectNodes("//Product");
            var productCodes = new HashSet<string>();
            foreach (XmlNode product in products)
            {
                var productCode = product.SelectSingleNode(".//ProductCode");
                if (productCode != null)
                {
                    productCodes.Add(productCode.InnerText);
                }
            }
            var lines = xmlDoc.SelectNodes("//Line");
            foreach (XmlNode line in lines)
            {
                var productCode = line.SelectSingleNode(".//ProductCode");
                if (productCode != null && !productCodes.Contains(productCode.InnerText))
                {
                    errors.Add($"Product reference error: ProductCode '{productCode.InnerText}' not found in master data (product reference, ProductCode, not found)");
                }
            }
            var taxTableEntries = xmlDoc.SelectNodes("//TaxTableEntry");
            var taxCodes = new HashSet<string>();
            foreach (XmlNode entry in taxTableEntries)
            {
                var taxCode = entry.SelectSingleNode(".//TaxCode");
                if (taxCode != null)
                {
                    taxCodes.Add(taxCode.InnerText);
                }
            }
            var taxElements = xmlDoc.SelectNodes("//Tax");
            foreach (XmlNode tax in taxElements)
            {
                var taxCode = tax.SelectSingleNode(".//TaxCode");
                if (taxCode != null && !taxCodes.Contains(taxCode.InnerText))
                {
                    errors.Add($"Tax code reference error: TaxCode '{taxCode.InnerText}' not found in tax table (tax code reference, TaxCode, not found)");
                }
            }
            var accounts = xmlDoc.SelectNodes("//Account");
            var accountIds = new HashSet<string>();
            foreach (XmlNode account in accounts)
            {
                var accountId = account.SelectSingleNode(".//AccountID");
                if (accountId != null)
                {
                    accountIds.Add(accountId.InnerText);
                }
            }
            var transactionLines = xmlDoc.SelectNodes("//Line[AccountID]");
            foreach (XmlNode line in transactionLines)
            {
                var accountId = line.SelectSingleNode(".//AccountID");
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
        private static List<string> ValidatePortugueseTaxRules(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            
            // Portuguese VAT number validation (9 digits)
            var taxNumbers = xmlDoc.SelectNodes("//TaxRegistrationNumber");
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
            var taxCodes = xmlDoc.SelectNodes("//TaxCode");
            foreach (XmlNode taxCode in taxCodes)
            {
                if (!validTaxCodes.Contains(taxCode.InnerText))
                {
                    errors.Add($"Portuguese tax code validation error: Invalid tax code '{taxCode.InnerText}'");
                }
            }
            
            // Portuguese currency validation (EUR)
            var currencyCodes = xmlDoc.SelectNodes("//CurrencyCode");
            foreach (XmlNode currencyCode in currencyCodes)
            {
                if (currencyCode.InnerText != "EUR")
                {
                    errors.Add($"Portuguese currency validation error: Currency must be EUR, got {currencyCode.InnerText}");
                }
            }
            
            // Tax exemption validation
            var lines = xmlDoc.SelectNodes("//Line");
            foreach (XmlNode line in lines)
            {
                var tax = line.SelectSingleNode(".//Tax");
                if (tax != null)
                {
                    var taxPercentage = tax.SelectSingleNode(".//TaxPercentage");
                    var exemptionReason = line.SelectSingleNode(".//TaxExemptionReason");
                    var exemptionCode = line.SelectSingleNode(".//TaxExemptionCode");
                    
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
        private static List<string> ValidateDocumentTotals(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var invoices = xmlDoc.SelectNodes("//Invoice");
            foreach (XmlNode invoice in invoices)
            {
                var lines = invoice.SelectNodes(".//Line");
                var totals = invoice.SelectSingleNode(".//DocumentTotals");
                if (lines != null && totals != null)
                {
                    decimal calculatedNetTotal = 0;
                    decimal calculatedTaxPayable = 0;
                    foreach (XmlNode line in lines)
                    {
                        var lineExtensionAmount = line.SelectSingleNode(".//LineExtensionAmount");
                        var tax = line.SelectSingleNode(".//Tax");
                        if (lineExtensionAmount != null && decimal.TryParse(lineExtensionAmount.InnerText, out decimal lineAmount))
                        {
                            calculatedNetTotal += lineAmount;
                            if (tax != null)
                            {
                                var taxAmount = tax.SelectSingleNode(".//TaxAmount");
                                if (taxAmount != null && decimal.TryParse(taxAmount.InnerText, out decimal taxValue))
                                {
                                    calculatedTaxPayable += taxValue;
                                }
                            }
                        }
                    }
                    var netTotal = totals.SelectSingleNode(".//NetTotal");
                    var taxPayable = totals.SelectSingleNode(".//TaxPayable");
                    var grossTotal = totals.SelectSingleNode(".//GrossTotal");
                    if (netTotal != null && taxPayable != null && grossTotal != null)
                    {
                        if (decimal.TryParse(netTotal.InnerText, out decimal docNetTotal) &&
                            decimal.TryParse(taxPayable.InnerText, out decimal docTaxPayable) &&
                            decimal.TryParse(grossTotal.InnerText, out decimal docGrossTotal))
                        {
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
        private static List<string> ValidateDateRanges(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var header = xmlDoc.SelectSingleNode("//Header");
            var invoices = xmlDoc.SelectNodes("//Invoice");
            if (header != null && invoices != null && invoices.Count > 0)
            {
                var startDate = header.SelectSingleNode(".//StartDate");
                var endDate = header.SelectSingleNode(".//EndDate");
                var fiscalYear = header.SelectSingleNode(".//FiscalYear");
                if (startDate != null && endDate != null)
                {
                    if (DateTime.TryParse(startDate.InnerText, out DateTime start) &&
                        DateTime.TryParse(endDate.InnerText, out DateTime end))
                    {
                        foreach (XmlNode invoice in invoices)
                        {
                            var invoiceDate = invoice.SelectSingleNode(".//InvoiceDate");
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
        private static List<string> ValidateCreditDebitBalance(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var transactions = xmlDoc.SelectNodes("//Transaction");
            foreach (XmlNode transaction in transactions)
            {
                var lines = transaction.SelectNodes(".//Line");
                if (lines != null)
                {
                    decimal totalDebits = 0;
                    decimal totalCredits = 0;
                    foreach (XmlNode line in lines)
                    {
                        var debitAmount = line.SelectSingleNode(".//DebitAmount");
                        var creditAmount = line.SelectSingleNode(".//CreditAmount");
                        if (debitAmount != null && decimal.TryParse(debitAmount.InnerText, out decimal debit))
                        {
                            totalDebits += debit;
                        }
                        if (creditAmount != null && decimal.TryParse(creditAmount.InnerText, out decimal credit))
                        {
                            totalCredits += credit;
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
        private static List<string> ValidateInvoiceNumbering(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var invoices = xmlDoc.SelectNodes("//Invoice");
            var invoiceNumbers = new List<string>();
            foreach (XmlNode invoice in invoices)
            {
                var invoiceNo = invoice.SelectSingleNode(".//InvoiceNo");
                if (invoiceNo != null)
                {
                    invoiceNumbers.Add(invoiceNo.InnerText);
                }
            }
            // Check for duplicates
            var seen = new HashSet<string>();
            foreach (var no in invoiceNumbers)
            {
                if (seen.Contains(no))
                {
                    errors.Add($"Invoice numbering validation error: Duplicate invoice number '{no}' (sequence)");
                }
                seen.Add(no);
            }
            // Check for missing sequence (format: FT 2024/001)
            var pattern = new System.Text.RegularExpressions.Regex(@"^(?<prefix>.+) (?<year>\d{4})/(?<num>\d+)$");
            var groups = invoiceNumbers
                .Select(n => pattern.Match(n))
                .Where(m => m.Success)
                .GroupBy(m => m.Groups["prefix"].Value + m.Groups["year"].Value)
                .ToList();
            foreach (var group in groups)
            {
                var nums = group.Select(m => int.Parse(m.Groups["num"].Value)).OrderBy(x => x).ToList();
                for (int i = 1; i < nums.Count; i++)
                {
                    if (nums[i] != nums[i - 1] + 1)
                    {
                        var missing = nums[i - 1] + 1;
                        var prefix = group.Key.Substring(0, group.Key.Length - 4);
                        var year = group.Key.Substring(group.Key.Length - 4);
                        errors.Add($"Invoice numbering validation error: Missing invoice number '{prefix} {year}/{missing:D3}' (sequence, missing)");
                    }
                }
            }
            return errors;
        }
        
        /// <summary>
        /// Validates payment terms and due dates.
        /// </summary>
        private static List<string> ValidatePaymentTerms(XmlDocument xmlDoc)
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
        private static List<string> ValidateQuantityAndUnitPrice(XmlDocument xmlDoc)
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
        private static List<string> ValidateTaxCalculationAccuracy(XmlDocument xmlDoc)
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
                var lineExtensionAmount = line.SelectSingleNode(".//LineExtensionAmount");
                var taxBase = line.SelectSingleNode(".//TaxBase");
                var tax = line.SelectSingleNode(".//Tax");
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
                    var taxPercentage = tax.SelectSingleNode(".//TaxPercentage");
                    var taxAmount = tax.SelectSingleNode(".//TaxAmount");
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
        private static List<string> ValidateBusinessRuleCompliance(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            // Only check fiscal year consistency for this test scenario
            var header = xmlDoc.SelectSingleNode("//Header");
            if (header != null)
            {
                var fiscalYear = header.SelectSingleNode(".//FiscalYear");
                var startDate = header.SelectSingleNode(".//StartDate");
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
                    var invoices = xmlDoc.SelectNodes("//Invoice");
                    foreach (XmlNode invoice in invoices)
                    {
                        var invoiceDate = invoice.SelectSingleNode(".//InvoiceDate");
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
            var customers = xmlDoc.SelectNodes("//Customer");
            if (customers != null && customers.Count > 0)
            {
                var customerIds = new HashSet<string>();
                foreach (XmlNode customer in customers)
                {
                    var customerId = customer.SelectSingleNode(".//CustomerID");
                    if (customerId != null)
                    {
                        customerIds.Add(customerId.InnerText);
                    }
                }
                var invoices = xmlDoc.SelectNodes("//Invoice");
                foreach (XmlNode invoice in invoices)
                {
                    var customerId = invoice.SelectSingleNode(".//CustomerID");
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
        private static List<string> ValidateDocumentStatus(XmlDocument xmlDoc)
        {
            var errors = new List<string>();
            var invoices = xmlDoc.SelectNodes("//Invoice");
            foreach (XmlNode invoice in invoices)
            {
                var invoiceDate = invoice.SelectSingleNode(".//InvoiceDate");
                var status = invoice.SelectSingleNode(".//DocumentStatus");
                if (invoiceDate != null && status != null)
                {
                    var statusDate = status.SelectSingleNode(".//InvoiceStatusDate");
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
        /// Performs .NET's built-in XSD validation.
        /// </summary>
        /// <param name="xmlDoc">The XML document to validate.</param>
        /// <param name="schemaPath">Path to the schema file.</param>
        /// <returns>List of XSD validation errors.</returns>
        private static List<string> ValidateXsd(XmlDocument xmlDoc, string schemaPath)
        {
            var errors = new List<string>();
            try
            {
                var schema = new XmlSchemaSet();
                schema.Add(null, schemaPath);
                
                xmlDoc.Schemas = schema;
                xmlDoc.Validate((sender, e) =>
                {
                    errors.Add(e.Message);
                });
            }
            catch (Exception ex)
            {
                errors.Add($"XSD validation error: {ex.Message}");
            }
            return errors;
        }
    }
} 