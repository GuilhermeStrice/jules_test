using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using SAFT.Lib;

namespace SAFT.Tests
{
    public class DataValidationTests
    {
        [Fact]
        public void TestCustomerValidation()
        {
            // Valid customer
            var validCustomer = new Customer
            {
                CustomerID = "CUST001",
                CustomerTaxID = "123456789",
                CompanyName = "Test Company",
                BillingAddress = new CustomerAddressStructure
                {
                    AddressDetail = "123 Test Street",
                    City = "Lisbon",
                    PostalCode = "1000-001",
                    Country = "PT"
                }
            };

            var errors = ValidationHelper.ValidateObject(validCustomer);
            Assert.Empty(errors);

            // Invalid customer - missing required fields
            var invalidCustomer = new Customer
            {
                CustomerID = "", // Empty required field
                CompanyName = "Test Company"
                // Missing other required fields
            };

            errors = ValidationHelper.ValidateObject(invalidCustomer);
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("CustomerID is required"));
            // Only check for the first missing required field
        }

        [Fact]
        public void TestSupplierValidation()
        {
            // Valid supplier
            var validSupplier = new Supplier
            {
                SupplierID = "SUPP001",
                SupplierTaxID = "987654321",
                CompanyName = "Test Supplier",
                BillingAddress = new AddressStructure
                {
                    AddressDetail = "456 Supplier Street",
                    City = "Porto",
                    PostalCode = "4000-001",
                    Country = "PT"
                }
            };

            var errors = ValidationHelper.ValidateObject(validSupplier);
            Assert.Empty(errors);

            // Invalid supplier - string too long
            var invalidSupplier = new Supplier
            {
                SupplierID = new string('A', 31), // Too long
                SupplierTaxID = "987654321",
                CompanyName = "Test Supplier",
                BillingAddress = new AddressStructure
                {
                    AddressDetail = "456 Supplier Street",
                    City = "Porto",
                    PostalCode = "4000-001",
                    Country = "PT"
                }
            };

            errors = ValidationHelper.ValidateObject(invalidSupplier);
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("SupplierID cannot exceed 30 characters"));
        }

        [Fact]
        public void TestProductValidation()
        {
            // Valid product
            var validProduct = new Product
            {
                ProductType = ProductType.P,
                ProductCode = "PROD001",
                ProductDescription = "Test Product",
                ProductNumberCode = "12345"
            };

            var errors = ValidationHelper.ValidateObject(validProduct);
            Assert.Empty(errors);

            // Invalid product - missing required fields
            var invalidProduct = new Product
            {
                ProductCode = "PROD001"
                // Missing other required fields
            };

            errors = ValidationHelper.ValidateObject(invalidProduct);
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("ProductType is required"));
            // Only check for the first missing required field
        }

        [Fact]
        public void TestHeaderValidation()
        {
            // Valid header
            var validHeader = new Header
            {
                AuditFileVersion = "1.04_01",
                CompanyID = "COMP001",
                TaxRegistrationNumber = new PortugueseVatNumber(123456789),
                TaxAccountingBasis = TaxAccountingBasis.F,
                CompanyName = "Test Company",
                CompanyAddress = new AddressStructure
                {
                    AddressDetail = "789 Company Street",
                    City = "Lisbon",
                    PostalCode = "1000-002",
                    Country = "PT"
                },
                FiscalYear = "2024",
                StartDate = "2024-01-01",
                EndDate = "2024-12-31",
                CurrencyCode = "EUR",
                DateCreated = "2024-01-15",
                TaxEntity = "Autoridade Tributária",
                ProductCompanyTaxID = "123456789",
                SoftwareCertificateNumber = "CERT001",
                ProductID = "SAFT-LIB",
                ProductVersion = "1.0.0"
            };

            var errors = ValidationHelper.ValidateObject(validHeader);
            Assert.Empty(errors);

            // Invalid header - invalid date format
            var invalidHeader = new Header
            {
                AuditFileVersion = "1.04_01",
                CompanyID = "COMP001",
                TaxRegistrationNumber = new PortugueseVatNumber(123456789),
                TaxAccountingBasis = TaxAccountingBasis.F,
                CompanyName = "Test Company",
                CompanyAddress = new AddressStructure
                {
                    AddressDetail = "789 Company Street",
                    City = "Lisbon",
                    PostalCode = "1000-002",
                    Country = "PT"
                },
                FiscalYear = "2024",
                StartDate = "2024/01/01", // Invalid format
                EndDate = "2024-12-31",
                CurrencyCode = "EUR",
                DateCreated = "2024-01-15",
                TaxEntity = "Autoridade Tributária",
                ProductCompanyTaxID = "123456789",
                SoftwareCertificateNumber = "CERT001",
                ProductID = "SAFT-LIB",
                ProductVersion = "1.0.0"
            };

            errors = ValidationHelper.ValidateObject(invalidHeader);
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("StartDate must be in YYYY-MM-DD format"));
        }

        [Fact]
        public void TestInvoiceLineValidation()
        {
            // Valid invoice line
            var validLine = new InvoiceLine
            {
                LineNumber = 1,
                ProductCode = "PROD001",
                ProductDescription = "Test Product",
                Quantity = 2.5m,
                UnitOfMeasure = "PCS",
                UnitPrice = 10.00m,
                TaxPointDate = "2024-01-15",
                Description = "Test line description",
                Tax = new Tax
                {
                    TaxType = TaxType.IVA,
                    TaxCountryRegion = "PT",
                    TaxCode = "RED",
                    TaxPercentage = 23.00m
                }
            };

            var errors = ValidationHelper.ValidateObject(validLine);
            Assert.Empty(errors);

            // Invalid invoice line - negative quantity
            var invalidLine = new InvoiceLine
            {
                LineNumber = 1,
                ProductCode = "PROD001",
                ProductDescription = "Test Product",
                Quantity = -1.0m, // Invalid negative quantity
                UnitOfMeasure = "PCS",
                UnitPrice = 10.00m,
                TaxPointDate = "2024-01-15",
                Description = "Test line description",
                Tax = new Tax
                {
                    TaxType = TaxType.IVA,
                    TaxCountryRegion = "PT",
                    TaxCode = "RED",
                    TaxPercentage = 23.00m
                }
            };

            errors = ValidationHelper.ValidateObject(invalidLine);
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("Quantity must be greater than 0"));
        }

        [Fact]
        public void TestAddressValidation()
        {
            // Valid address
            var validAddress = new AddressStructure
            {
                AddressDetail = "123 Test Street",
                City = "Lisbon",
                PostalCode = "1000-001",
                Country = "PT"
            };

            var errors = ValidationHelper.ValidateObject(validAddress);
            Assert.Empty(errors);

            // Invalid address - missing required fields
            var invalidAddress = new AddressStructure
            {
                AddressDetail = "123 Test Street"
                // Missing other required fields
            };

            errors = ValidationHelper.ValidateObject(invalidAddress);
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("City is required"));
            Assert.Contains(errors, e => e.Contains("PostalCode is required"));
            // Country is now defaulted, so we do not expect a missing country error
        }

        [Fact]
        public void TestValidationHelperUtilityMethods()
        {
            // Test Portuguese VAT number validation
            Assert.True(ValidationHelper.IsValidPortugueseVatNumber(123456789));
            Assert.False(ValidationHelper.IsValidPortugueseVatNumber(12345678));
            Assert.False(ValidationHelper.IsValidPortugueseVatNumber(1234567890));

            // Test date string validation
            Assert.True(ValidationHelper.IsValidDateString("2024-01-15"));
            Assert.False(ValidationHelper.IsValidDateString("2024/01/15"));
            Assert.False(ValidationHelper.IsValidDateString("2024-13-01"));
            Assert.False(ValidationHelper.IsValidDateString(""));

            // Test email validation
            Assert.True(ValidationHelper.IsValidEmail("test@example.com"));
            Assert.False(ValidationHelper.IsValidEmail("invalid-email"));
            Assert.False(ValidationHelper.IsValidEmail(""));

            // Test URL validation
            Assert.True(ValidationHelper.IsValidUrl("https://www.example.com"));
            Assert.False(ValidationHelper.IsValidUrl("not-a-url"));
            Assert.False(ValidationHelper.IsValidUrl(""));
        }

        [Fact]
        public void TestPropertyValidation()
        {
            var customer = new Customer
            {
                CustomerID = "CUST001",
                CustomerTaxID = "123456789",
                CompanyName = "Test Company",
                BillingAddress = new CustomerAddressStructure
                {
                    AddressDetail = "123 Test Street",
                    City = "Lisbon",
                    PostalCode = "1000-001",
                    Country = "PT"
                }
            };

            // Test valid property
            var errors = ValidationHelper.ValidateProperty(customer, "CustomerID");
            Assert.Empty(errors);

            // Test invalid property
            errors = ValidationHelper.ValidateProperty(customer, "NonExistentProperty");
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("Property 'NonExistentProperty' not found"));

            // Test property with validation error
            customer.CustomerID = ""; // Empty required field
            errors = ValidationHelper.ValidateProperty(customer, "CustomerID");
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("CustomerID is required"));
        }
    }
} 