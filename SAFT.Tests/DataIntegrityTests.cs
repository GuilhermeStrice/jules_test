using Xunit;
using SAFT.Lib;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SAFT.Tests
{
    public class DataIntegrityTests
    {
        private string GetSchemaPath()
        {
            // Get the directory where the solution file is located (root directory)
            var solutionDir = Directory.GetCurrentDirectory();
            while (!File.Exists(Path.Combine(solutionDir, "SAFT.sln")))
            {
                solutionDir = Directory.GetParent(solutionDir)?.FullName;
                if (solutionDir == null)
                    throw new FileNotFoundException("Could not find SAFT.sln file");
            }
            return Path.Combine(solutionDir, "schema1_04_fixed.xsd");
        }

        [Fact]
        public void TestUniqueConstraintValidation()
        {
            // Test unique constraint validation
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""Root"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""Customer"" maxOccurs=""unbounded"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""CustomerID"" type=""xs:string""/>
                            <xs:element name=""Name"" type=""xs:string""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:sequence>
        </xs:complexType>
        <xs:unique name=""CustomerIDConstraint"">
            <xs:selector xpath=""Customer""/>
            <xs:field xpath=""CustomerID""/>
        </xs:unique>
    </xs:element>
</xs:schema>";
            
            var schemaPath = "test_unique_schema.xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid XML (unique CustomerIDs)
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
    <Customer>
        <CustomerID>CUST001</CustomerID>
        <Name>Customer 1</Name>
    </Customer>
    <Customer>
        <CustomerID>CUST002</CustomerID>
        <Name>Customer 2</Name>
    </Customer>
</Root>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("duplicate key sequence"));
                
                // Test invalid XML (duplicate CustomerID)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
    <Customer>
        <CustomerID>CUST001</CustomerID>
        <Name>Customer 1</Name>
    </Customer>
    <Customer>
        <CustomerID>CUST001</CustomerID>
        <Name>Customer 2</Name>
    </Customer>
</Root>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("duplicate key sequence") && error.Contains("CustomerIDConstraint"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestKeyConstraintValidation()
        {
            // Test key constraint validation (unique and required)
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""Root"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""Product"" maxOccurs=""unbounded"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""ProductCode"" type=""xs:string""/>
                            <xs:element name=""Description"" type=""xs:string""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:sequence>
        </xs:complexType>
        <xs:key name=""ProductCodeKey"">
            <xs:selector xpath=""Product""/>
            <xs:field xpath=""ProductCode""/>
        </xs:key>
    </xs:element>
</xs:schema>";
            
            var schemaPath = "test_key_schema.xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid XML (unique ProductCodes)
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
    <Product>
        <ProductCode>PROD001</ProductCode>
        <Description>Product 1</Description>
    </Product>
    <Product>
        <ProductCode>PROD002</ProductCode>
        <Description>Product 2</Description>
    </Product>
</Root>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("duplicate key sequence"));
                
                // Test invalid XML (duplicate ProductCode)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
    <Product>
        <ProductCode>PROD001</ProductCode>
        <Description>Product 1</Description>
    </Product>
    <Product>
        <ProductCode>PROD001</ProductCode>
        <Description>Product 2</Description>
    </Product>
</Root>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("duplicate key sequence") && error.Contains("ProductCodeKey"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestKeyRefConstraintValidation()
        {
            // Test keyref constraint validation (referential integrity)
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""Root"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""Customer"" maxOccurs=""unbounded"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""CustomerID"" type=""xs:string""/>
                            <xs:element name=""Name"" type=""xs:string""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
                <xs:element name=""Order"" maxOccurs=""unbounded"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""OrderID"" type=""xs:string""/>
                            <xs:element name=""CustomerID"" type=""xs:string""/>
                            <xs:element name=""Amount"" type=""xs:decimal""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:sequence>
        </xs:complexType>
        <xs:unique name=""CustomerIDConstraint"">
            <xs:selector xpath=""Customer""/>
            <xs:field xpath=""CustomerID""/>
        </xs:unique>
        <xs:keyref name=""OrderCustomerRef"" refer=""CustomerIDConstraint"">
            <xs:selector xpath=""Order""/>
            <xs:field xpath=""CustomerID""/>
        </xs:keyref>
    </xs:element>
</xs:schema>";
            
            var schemaPath = "test_keyref_schema.xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid XML (valid references)
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
    <Customer>
        <CustomerID>CUST001</CustomerID>
        <Name>Customer 1</Name>
    </Customer>
    <Customer>
        <CustomerID>CUST002</CustomerID>
        <Name>Customer 2</Name>
    </Customer>
    <Order>
        <OrderID>ORD001</OrderID>
        <CustomerID>CUST001</CustomerID>
        <Amount>100.00</Amount>
    </Order>
    <Order>
        <OrderID>ORD002</OrderID>
        <CustomerID>CUST002</CustomerID>
        <Amount>200.00</Amount>
    </Order>
</Root>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("Keyref fails to refer"));
                
                // Test invalid XML (invalid reference)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
    <Customer>
        <CustomerID>CUST001</CustomerID>
        <Name>Customer 1</Name>
    </Customer>
    <Order>
        <OrderID>ORD001</OrderID>
        <CustomerID>CUST999</CustomerID>
        <Amount>100.00</Amount>
    </Order>
</Root>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("Keyref fails to refer"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestComplexIdentityConstraints()
        {
            // Test complex identity constraints with multiple fields
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""Root"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""Invoice"" maxOccurs=""unbounded"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""InvoiceNo"" type=""xs:string""/>
                            <xs:element name=""InvoiceDate"" type=""xs:date""/>
                            <xs:element name=""CustomerID"" type=""xs:string""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:sequence>
        </xs:complexType>
        <xs:unique name=""InvoiceConstraint"">
            <xs:selector xpath=""Invoice""/>
            <xs:field xpath=""InvoiceNo""/>
            <xs:field xpath=""InvoiceDate""/>
        </xs:unique>
    </xs:element>
</xs:schema>";
            
            var schemaPath = "test_complex_schema.xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid XML (unique combinations)
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
    <Invoice>
        <InvoiceNo>INV001</InvoiceNo>
        <InvoiceDate>2024-01-01</InvoiceDate>
        <CustomerID>CUST001</CustomerID>
    </Invoice>
    <Invoice>
        <InvoiceNo>INV002</InvoiceNo>
        <InvoiceDate>2024-01-01</InvoiceDate>
        <CustomerID>CUST002</CustomerID>
    </Invoice>
</Root>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => error.Contains("duplicate key sequence"));
                
                // Test invalid XML (duplicate combination)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
    <Invoice>
        <InvoiceNo>INV001</InvoiceNo>
        <InvoiceDate>2024-01-01</InvoiceDate>
        <CustomerID>CUST001</CustomerID>
    </Invoice>
    <Invoice>
        <InvoiceNo>INV001</InvoiceNo>
        <InvoiceDate>2024-01-01</InvoiceDate>
        <CustomerID>CUST002</CustomerID>
    </Invoice>
</Root>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("duplicate key sequence") && error.Contains("InvoiceConstraint"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestSAFTDataIntegrity()
        {
            // Test data integrity with SAF-T specific scenarios
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""AuditFile"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""Header"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""AuditFileVersion"" type=""xs:string""/>
                            <xs:element name=""CompanyID"" type=""xs:string""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
                <xs:element name=""SourceDocuments"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""SalesInvoices"">
                                <xs:complexType>
                                    <xs:sequence>
                                        <xs:element name=""Invoice"" maxOccurs=""unbounded"">
                                            <xs:complexType>
                                                <xs:sequence>
                                                    <xs:element name=""InvoiceNo"" type=""xs:string""/>
                                                    <xs:element name=""InvoiceDate"" type=""xs:date""/>
                                                    <xs:element name=""CustomerID"" type=""xs:string""/>
                                                </xs:sequence>
                                            </xs:complexType>
                                        </xs:element>
                                    </xs:sequence>
                                </xs:complexType>
                            </xs:element>
                            <xs:element name=""Customers"">
                                <xs:complexType>
                                    <xs:sequence>
                                        <xs:element name=""Customer"" maxOccurs=""unbounded"">
                                            <xs:complexType>
                                                <xs:sequence>
                                                    <xs:element name=""CustomerID"" type=""xs:string""/>
                                                    <xs:element name=""CompanyName"" type=""xs:string""/>
                                                </xs:sequence>
                                            </xs:complexType>
                                        </xs:element>
                                    </xs:sequence>
                                </xs:complexType>
                            </xs:element>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:sequence>
        </xs:complexType>
        <xs:unique name=""CustomerIDConstraint"">
            <xs:selector xpath=""SourceDocuments/Customers/Customer""/>
            <xs:field xpath=""CustomerID""/>
        </xs:unique>
        <xs:unique name=""InvoiceNoConstraint"">
            <xs:selector xpath=""SourceDocuments/SalesInvoices/Invoice""/>
            <xs:field xpath=""InvoiceNo""/>
        </xs:unique>
        <xs:keyref name=""InvoiceCustomerRef"" refer=""CustomerIDConstraint"">
            <xs:selector xpath=""SourceDocuments/SalesInvoices/Invoice""/>
            <xs:field xpath=""CustomerID""/>
        </xs:keyref>
    </xs:element>
</xs:schema>";
            
            var schemaPath = "test_saft_schema.xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                // Test valid SAF-T XML
                var validXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
    <Header>
        <AuditFileVersion>1.04_01</AuditFileVersion>
        <CompanyID>123456789</CompanyID>
    </Header>
    <SourceDocuments>
        <SalesInvoices>
            <Invoice>
                <InvoiceNo>INV001</InvoiceNo>
                <InvoiceDate>2024-01-01</InvoiceDate>
                <CustomerID>CUST001</CustomerID>
            </Invoice>
            <Invoice>
                <InvoiceNo>INV002</InvoiceNo>
                <InvoiceDate>2024-01-02</InvoiceDate>
                <CustomerID>CUST002</CustomerID>
            </Invoice>
        </SalesInvoices>
        <Customers>
            <Customer>
                <CustomerID>CUST001</CustomerID>
                <CompanyName>Customer 1</CompanyName>
            </Customer>
            <Customer>
                <CustomerID>CUST002</CustomerID>
                <CompanyName>Customer 2</CompanyName>
            </Customer>
        </Customers>
    </SourceDocuments>
</AuditFile>";
                
                var validErrors = SchemaValidator.Validate(validXml, schemaPath);
                Assert.DoesNotContain(validErrors, error => 
                    error.Contains("duplicate key sequence") || 
                    error.Contains("Keyref fails to refer"));
                
                // Test invalid SAF-T XML (duplicate invoice numbers)
                var invalidXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<AuditFile>
    <Header>
        <AuditFileVersion>1.04_01</AuditFileVersion>
        <CompanyID>123456789</CompanyID>
    </Header>
    <SourceDocuments>
        <SalesInvoices>
            <Invoice>
                <InvoiceNo>INV001</InvoiceNo>
                <InvoiceDate>2024-01-01</InvoiceDate>
                <CustomerID>CUST001</CustomerID>
            </Invoice>
            <Invoice>
                <InvoiceNo>INV001</InvoiceNo>
                <InvoiceDate>2024-01-02</InvoiceDate>
                <CustomerID>CUST002</CustomerID>
            </Invoice>
        </SalesInvoices>
        <Customers>
            <Customer>
                <CustomerID>CUST001</CustomerID>
                <CompanyName>Customer 1</CompanyName>
            </Customer>
            <Customer>
                <CustomerID>CUST002</CustomerID>
                <CompanyName>Customer 2</CompanyName>
            </Customer>
        </Customers>
    </SourceDocuments>
</AuditFile>";
                
                var invalidErrors = SchemaValidator.Validate(invalidXml, schemaPath);
                Assert.Contains(invalidErrors, error => error.Contains("duplicate key sequence") && error.Contains("InvoiceNoConstraint"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }

        [Fact]
        public void TestDataIntegrityErrorMessages()
        {
            // Test that error messages are clear and informative
            var schema = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
    <xs:element name=""Root"">
        <xs:complexType>
            <xs:sequence>
                <xs:element name=""Item"" maxOccurs=""unbounded"">
                    <xs:complexType>
                        <xs:sequence>
                            <xs:element name=""ItemCode"" type=""xs:string""/>
                            <xs:element name=""Description"" type=""xs:string""/>
                        </xs:sequence>
                    </xs:complexType>
                </xs:element>
            </xs:sequence>
        </xs:complexType>
        <xs:unique name=""ItemCodeConstraint"">
            <xs:selector xpath=""Item""/>
            <xs:field xpath=""ItemCode""/>
        </xs:unique>
    </xs:element>
</xs:schema>";
            
            var schemaPath = "test_error_messages.xsd";
            File.WriteAllText(schemaPath, schema);
            
            try
            {
                var xml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<Root>
    <Item>
        <ItemCode>ITEM001</ItemCode>
        <Description>Item 1</Description>
    </Item>
    <Item>
        <ItemCode>ITEM001</ItemCode>
        <Description>Item 2</Description>
    </Item>
</Root>";
                
                var errors = SchemaValidator.Validate(xml, schemaPath);
                
                // Verify error message contains useful information
                Assert.Contains(errors, error => 
                    error.Contains("duplicate key sequence") && 
                    error.Contains("ITEM001") && 
                    error.Contains("ItemCodeConstraint"));
            }
            finally
            {
                if (File.Exists(schemaPath))
                    File.Delete(schemaPath);
            }
        }
    }
} 