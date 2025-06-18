# SAFT-PT Generator for C#

A complete C# generator for SAFT-PT (Portuguese Tax Audit File) XML documents based on the official schema version 1.04_01.

## Overview

This library provides a comprehensive solution for generating SAFT-PT XML files that comply with Portuguese tax authority requirements. It includes support for:

- Complete SAFT-PT structure generation
- Digital signature creation and verification
- All document types (invoices, movements, working documents, payments)
- Master files (customers, suppliers, products, tax tables)
- General ledger entries
- XML validation and formatting

## Features

- **Complete SAFT-PT Structure**: Implements all elements from the official schema
- **Digital Signatures**: RSA-based signature creation and verification
- **Type Safety**: Strongly typed classes for all SAFT-PT elements
- **XML Serialization**: Proper XML generation with correct namespaces
- **Validation**: Built-in validation for required fields and data types
- **Flexible**: Easy to extend and customize for specific business needs

## Installation

1. Add the `SaftPtGenerator.cs` file to your C# project
2. Ensure you have the following NuGet packages:
   - `System.Security.Cryptography`
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
    companyName: "Empresa Exemplo Lda",
    businessName: "Empresa Exemplo",
    companyAddress: new Address
    {
        AddressDetail = "Rua das Flores, 123",
        City = "Lisboa",
        PostalCode = "1000-001",
        Country = "PT"
    },
    fiscalYear: 2024,
    startDate: new DateTime(2024, 1, 1),
    endDate: new DateTime(2024, 12, 31),
    dateCreated: DateTime.Now,
    taxEntity: "AT",
    productCompanyTaxId: "123456789",
    softwareCertificateNumber: 12345,
    productId: "Software/1.0",
    productVersion: "1.0"
);

// Add customer
generator.AddCustomer(new Customer
{
    CustomerID = "CUST001",
    AccountID = "211",
    CustomerTaxID = "987654321",
    CompanyName = "Cliente Exemplo Lda",
    BillingAddress = new CustomerAddress
    {
        AddressDetail = "Rua do Cliente, 456",
        City = "Porto",
        PostalCode = "4000-001",
        Country = "PT"
    },
    SelfBillingIndicator = 0
});

// Add product
generator.AddProduct(new Product
{
    ProductType = "P", // Produtos
    ProductCode = "PROD001",
    ProductDescription = "Produto Exemplo",
    ProductNumberCode = "123456789"
});

// Add tax table
generator.SetTaxTable(new TaxTable
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
});

// Create invoice
var invoice = new Invoice
{
    InvoiceNo = "FT 2024/001",
    ATCUD = "ATCUD123456789",
    DocumentStatus = new InvoiceDocumentStatus
    {
        InvoiceStatus = "N", // Normal
        InvoiceStatusDate = DateTime.Now,
        SourceID = "SYS001",
        SourceBilling = "P" // Documento produzido na aplicação
    },
    Hash = "hash123456789",
    HashControl = "1-AT(FT) 2024/001",
    InvoiceDate = DateTime.Now,
    InvoiceType = "FT", // Fatura
    SpecialRegimes = new SpecialRegimes
    {
        SelfBillingIndicator = 0,
        CashVATSchemeIndicator = 0,
        ThirdPartiesBillingIndicator = 0
    },
    SourceID = "SYS001",
    SystemEntryDate = DateTime.Now,
    CustomerID = "CUST001",
    Line = new List<InvoiceLine>
    {
        new InvoiceLine
        {
            LineNumber = 1,
            ProductCode = "PROD001",
            ProductDescription = "Produto Exemplo",
            Quantity = 1,
            UnitOfMeasure = "UN",
            UnitPrice = 100.00m,
            TaxPointDate = DateTime.Now,
            Description = "Descrição da linha",
            Tax = new Tax
            {
                TaxType = "IVA",
                TaxCountryRegion = "PT",
                TaxCode = "NOR",
                TaxPercentage = 23.00m
            }
        }
    },
    DocumentTotals = new InvoiceDocumentTotals
    {
        TaxPayable = 23.00m,
        NetTotal = 100.00m,
        GrossTotal = 123.00m
    }
};

// Add source documents
generator.SetSourceDocuments(new SourceDocuments
{
    SalesInvoices = new SalesInvoices
    {
        NumberOfEntries = 1,
        TotalDebit = 123.00m,
        TotalCredit = 123.00m,
        Invoice = new List<Invoice> { invoice }
    }
});

// Generate the XML file
generator.GenerateFile("saft_pt_example.xml");
```

## Digital Signatures

To create digital signatures, you need RSA private and public keys:

```csharp
// Create generator with key paths
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

The generator includes validation for:

- Required fields
- Data type constraints
- Business rule validation
- XML schema compliance

## Error Handling

```csharp
try
{
    generator.GenerateFile("saft_pt_example.xml");
    Console.WriteLine("File generated successfully!");
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