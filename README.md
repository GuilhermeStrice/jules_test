# SAFT Library - Portuguese Standard Audit File for Tax

A comprehensive C# library for generating and validating Portuguese SAF-T (Standard Audit File for Tax) documents according to version 1.04_01 of the official schema.

## Features

- **Complete SAF-T Model**: Full implementation of all Portuguese SAF-T entities
- **XML Serialization**: Generate compliant SAF-T XML files
- **Schema Validation**: Validate against official Portuguese XSD schema
- **Data Integrity**: Cross-reference validation and business rule compliance
- **Configuration Management**: Flexible configuration system
- **Comprehensive Testing**: 45+ unit tests covering all major functionality

## Quick Start

### Installation

```bash
git clone <repository-url>
cd jules_test-1
dotnet restore
dotnet build
```

### Basic Usage

```csharp
using SAFT.Lib;
using SAFT.Lib.Utils;

// Create a default audit file
var auditFile = AuditFile.CreateDefault();

// Configure company details
auditFile.Header.CompanyName = "My Company Ltd";
auditFile.Header.TaxRegistrationNumber = new PortugueseVatNumber { Value = 123456789 };

// Add customers
auditFile.MasterFiles.Customers.Add(new Customer
{
    CustomerID = "CUST001",
    AccountID = "1101",
    CustomerName = "John Doe",
    CustomerAddress = new AddressStructure
    {
        AddressDetail = "123 Main St",
        City = "Lisbon",
        PostalCode = "1000-001",
        Country = "PT"
    }
});

// Generate SAF-T XML file
auditFile.SaveToOutputDirectory("my_saft_file.xml");
```

### Validation

```csharp
// Validate against schema
var validationResult = SchemaValidator.Validate(xmlContent, "schema1_04.xsd");
if (!validationResult.IsValid)
{
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"Validation Error: {error}");
    }
}
```

## Project Structure

```
SAFT.Lib/                    # Main library
├── AuditFile.cs            # Main SAF-T file structure
├── Header.cs               # File header with company info
├── Customer.cs             # Customer entity
├── Supplier.cs             # Supplier entity
├── Product.cs              # Product entity
├── SalesInvoices.cs        # Invoice management
├── GeneralLedgerEntries.cs # GL entries
├── MovementOfGoods.cs      # Stock movements
├── Payments.cs             # Payment records
├── Enums/                  # SAF-T enumerations
├── Utils/                  # Utility classes
│   ├── XmlUtils.cs         # XML operations
│   ├── SAFTConfiguration.cs # Configuration management
│   └── ValidationHelper.cs # Validation utilities
└── Constants/              # SAF-T constants

SAFT.Tests/                 # Test suite
├── DataValidationTests.cs  # Data validation tests
├── SchemaValidationTests.cs # Schema validation tests
├── BusinessLogicValidationTests.cs # Business rule tests
└── PortugueseTaxRuleValidationTests.cs # Portuguese tax compliance
```

## Configuration

The library uses a JSON-based configuration system:

```json
{
  "SchemaPath": "schema1_04.xsd",
  "OutputDirectory": "./output",
  "StrictValidation": true,
  "DefaultCountryCode": "PT",
  "DefaultCurrencyCode": "EUR",
  "DefaultFiscalYear": 2024
}
```

## Running Tests

```bash
# Run all tests
./run-tests.sh

# Or using dotnet directly
dotnet test SAFT.sln
```

## Portuguese Tax Compliance

This library implements all Portuguese tax requirements including:

- Portuguese VAT number validation (9 digits)
- Portuguese tax codes (RED, INT, NOR, ISE, OUT, NS)
- Currency validation (EUR only)
- Invoice numbering sequence validation
- Tax calculation accuracy
- Document status validation

## Schema Files

- `schema1_04.xsd` - Original Portuguese SAF-T schema
- `schema1_04_fixed.xsd` - Fixed version with corrections

## Dependencies

- .NET 9.0
- System.Text.Json
- System.ComponentModel.Annotations
- xUnit (for testing)

## License

[Add your license information here]

## Contributing

[Add contribution guidelines here] 