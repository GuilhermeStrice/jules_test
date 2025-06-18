# SAFT-PT Generator for C#

A complete C# implementation of the SAFT-PT (Standard Audit File for Tax Purposes - Portugal) generator based on schema version 1.04_01. This library provides comprehensive support for generating Portuguese tax audit files in XML format.

## Features

- **Complete Schema Support**: Implements all elements from SAFT-PT schema version 1.04_01
- **String-based Implementation**: Uses strings for all schema elements as requested
- **Digital Signatures**: RSA-based digital signature creation and verification
- **Data Validation**: Comprehensive validation of audit file structure and data
- **Multiple Document Types**: Support for invoices, movements, working documents, and payments
- **Flexible API**: Easy-to-use methods for building audit files
- **Error Handling**: Robust error handling and validation

## Installation

1. Add the `SaftPtGenerator.cs` file to your C# project
2. Ensure you have the required .NET dependencies:
   - `System.Security.Cryptography`
   - `System.Xml`
   - `System.Xml.Serialization`

## Quick Start

```csharp
using SaftPtGenerator;

// Create generator instance
var generator = new SaftPtGenerator();

// Initialize header
generator.InitializeHeader(
    companyId: "123456789",
    taxRegistrationNumber: 123456789,
    companyName: "Example Company Lda",
    businessName: "Example Business",
    companyAddress: new Address
    {
        BuildingNumber = "123",
        StreetName = "Example Street",
        City = "Lisbon",
        PostalCode = "1000-000",
        Country = "PT"
    },
    fiscalYear: 2024,
    startDate: new DateTime(2024, 1, 1),
    endDate: new DateTime(2024, 12, 31),
    dateCreated: DateTime.Now,
    taxEntity: "Lisboa",
    productCompanyTaxId: "123456789",
    softwareCertificateNumber: 123456,
    productId: "Example Software",
    productVersion: "1.0.0"
);

// Add tax table
var taxTable = new TaxTable
{
    TaxTableEntry = new List<TaxTableEntry>
    {
        new TaxTableEntry
        {
            TaxType = "IVA",
            TaxCountryRegion = "PT",
            TaxCode = "NOR",
            Description = "IVA Normal",
            TaxPercentage = 23.00m
        }
    }
};
generator.SetTaxTable(taxTable);

// Add customer
var customer = new Customer
{
    CustomerID = "CUST001",
    CustomerTaxID = "123456789",
    CompanyName = "Customer Company Lda",
    BillingAddress = new CustomerAddress
    {
        BuildingNumber = "456",
        StreetName = "Customer Street",
        City = "Porto",
        PostalCode = "4000-000",
        Country = "PT"
    }
};
generator.AddCustomer(customer);

// Generate file
generator.GenerateFile("saft_pt_example.xml");
```

## Validation

The generator includes comprehensive validation to ensure your SAFT-PT file meets all requirements:

```csharp
// Validate before generating
var validationErrors = generator.ValidateAuditFile();

if (validationErrors.Count > 0)
{
    Console.WriteLine("Validation errors found:");
    foreach (var error in validationErrors)
    {
        Console.WriteLine($"- {error}");
    }
    return;
}

// Generate with validation (default)
generator.GenerateFile("saft_pt_example.xml", validate: true);

// Generate without validation
generator.GenerateFile("saft_pt_example.xml", validate: false);
```

## Digital Signatures

Create and verify digital signatures for documents:

```csharp
var generator = new SaftPtGenerator(
    privateKeyPath: "private_key.pem",
    publicKeyPath: "public_key.pem"
);

// Create signature
string signature = generator.CreateSignature(
    docDate: DateTime.Now,
    systemEntryDate: DateTime.Now,
    docNumber: "FT 2024/001",
    grossTotal: 123.00m
);

// Verify signature
bool isValid = generator.VerifySignature(
    signature: signature,
    docDate: DateTime.Now,
    systemEntryDate: DateTime.Now,
    docNumber: "FT 2024/001",
    grossTotal: 123.00m
);
```

## Supported Document Types

### Invoice Types
- `FT` - Fatura
- `FS` - Fatura simplificada
- `FR` - Fatura-recibo
- `ND` - Nota de débito
- `NC` - Nota de crédito
- `VD` - Venda a dinheiro
- `TV` - Talão de venda
- `TD` - Talão de devolução
- `AA` - Alienação de ativos
- `DA` - Devolução de ativos

### Movement Types
- `GR` - Guia de remessa
- `GT` - Guia de transporte
- `GA` - Guia de movimentação de ativos fixos próprios
- `GC` - Guia de consignação
- `GD` - Guia ou nota de devolução

### Work Document Types
- `CM` - Consulta de mesa
- `CC` - Crédito de consignação
- `FC` - Fatura de consignação
- `FO` - Folha de obra
- `NE` - Nota de encomenda
- `OU` - Outros documentos
- `OR` - Orçamento
- `PF` - Fatura pro-forma

### Tax Types
- `IVA` - Imposto sobre o Valor Acrescentado
- `IS` - Imposto do Selo
- `NS` - Não sujeito

### Tax Codes
- `NOR` - Normal
- `RED` - Reduzido
- `INT` - Intermédio
- `ISE` - Isento
- `OUT` - Outros

## Data Validation

The generator validates:

- **Required Fields**: All mandatory elements are present
- **Data Types**: Proper data types for all fields
- **Business Rules**: Logical validation (e.g., start date before end date)
- **Relationships**: Valid references between entities
- **Schema Compliance**: XML structure matches SAFT-PT schema

### Validation Rules

- Header must contain all required company information
- Tax table must have at least one entry
- Customers and suppliers must have valid tax IDs
- Products must have product codes
- Invoices must have at least one line
- Document totals must match line totals
- All referenced entities must exist in master files

## Error Handling

```csharp
try
{
    // Validate first
    var errors = generator.ValidateAuditFile();
    if (errors.Count > 0)
    {
        throw new InvalidOperationException($"Validation failed: {string.Join(", ", errors)}");
    }

    // Generate file
    generator.GenerateFile("saft_pt_example.xml");
    Console.WriteLine("File generated successfully!");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error generating file: {ex.Message}");
}
```

## File Structure

The generated XML file follows the official SAFT-PT structure:

```xml
<?xml version="1.0" encoding="utf-8"?>
<AuditFile xmlns="urn:OECD:StandardAuditFile-Tax:PT_1.04_01">
  <Header>
    <AuditFileVersion>1.04_01</AuditFileVersion>
    <CompanyID>123456789</CompanyID>
    <TaxRegistrationNumber>123456789</TaxRegistrationNumber>
    <!-- ... other header elements ... -->
  </Header>
  <MasterFiles>
    <!-- Customers, Suppliers, Products, Tax Table -->
  </MasterFiles>
  <GeneralLedgerEntries>
    <!-- Accounting entries -->
  </GeneralLedgerEntries>
  <SourceDocuments>
    <!-- Invoices, Movements, Working Documents, Payments -->
  </SourceDocuments>
</AuditFile>
```

## Requirements

- .NET Framework 4.7.2 or later / .NET Core 3.1 or later
- C# 8.0 or later
- System.Security.Cryptography package

## License

This project is licensed under the MIT License.

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

## References

- [SAFT-PT Schema Documentation](https://github.com/joaomfrebelo/Saft-PT_4_PHP)
- [Portuguese Tax Authority](https://www.portaldasfinancas.gov.pt/)
- [SAFT-PT Official Documentation](https://www.portaldasfinancas.gov.pt/at/html/index.html) 