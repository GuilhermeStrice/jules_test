using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace SAFT.Lib.Validation
{
    public static partial class SchemaValidator
    {
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
    }
}
