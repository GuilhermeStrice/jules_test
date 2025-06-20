/*
<!-- Versao do estrutura do ficheiro -->
<xs:element name="AuditFileVersion">
<xs:simpleType>
<xs:restriction base="xs:string">
<xs:pattern value="1\.04_01"/>
</xs:restriction>
</xs:simpleType>
</xs:element>

<!-- Indicador de regime de IVA de Caixa -->
<xs:element name="CashVATSchemeIndicator">
<xs:simpleType>
<xs:restriction base="xs:integer">
<xs:minInclusive value="0"/>
<xs:maxInclusive value="1"/>
</xs:restriction>
</xs:simpleType>
</xs:element>

<!-- Ano Fiscal -->
<xs:element name="FiscalYear">
<xs:simpleType>
<xs:restriction base="xs:integer">
<xs:minInclusive value="2000"/>
<xs:maxInclusive value="9999"/>
</xs:restriction>
</xs:simpleType>
</xs:element>

<!-- Periodo contabilistico do documento -->
<xs:element name="Period">
<xs:simpleType>
<xs:restriction base="xs:integer">
<xs:minInclusive value="1"/>
<xs:maxInclusive value="12"/>
</xs:restriction>
</xs:simpleType>
</xs:element>

<!-- Indicador de Autofaturacao -->
<xs:element name="SelfBillingIndicator">
<xs:simpleType>
<xs:restriction base="xs:integer">
<xs:minInclusive value="0"/>
<xs:maxInclusive value="1"/>
</xs:restriction>
</xs:simpleType>
</xs:element>

<!--
 Indicador de faturacao emitida em nome e por conta de terceiros 
-->
<xs:element name="ThirdPartiesBillingIndicator">
<xs:simpleType>
<xs:restriction base="xs:integer">
<xs:minInclusive value="0"/>
<xs:maxInclusive value="1"/>
</xs:restriction>
</xs:simpleType>
</xs:element>
*/

namespace SAFT.Lib.Constants
{
    /// <summary>
    /// SAFTConstants - Constants for SAFT (Standard Audit File - Tax) Portugal
    /// Contains fixed values, validation patterns, and ranges from the XSD schema.
    /// This includes audit file version, fiscal year ranges, period ranges, and various indicators.
    /// </summary>
    /// <summary>
    /// Constants for SAFT (Standard Audit File - Tax) Portugal
    /// </summary>
    public static class SAFTConstants
    {
        /// <summary>
        /// Audit File Version - Fixed value from XSD schema
        /// </summary>
        public const string AuditFileVersion = "1.04_01";

        /// <summary>
        /// Portuguese currency code
        /// </summary>
        public const string CurrencyPT = "EUR";

        /// <summary>
        /// Portuguese country code
        /// </summary>
        public const string CountryPT = "PT";

        /// <summary>
        /// Portuguese tax country regions
        /// </summary>
        public static class TaxCountryRegions
        {
            public const string Portugal = "PT";
            public const string Azores = "PT-AC";
            public const string Madeira = "PT-MA";
        }

        /// <summary>
        /// Regular expressions for validation
        /// </summary>
        public static class Patterns
        {
            /// <summary>
            /// Pattern for AccountID: ((^[^]*)|Desconhecido)
            /// </summary>
            public const string AccountID = @"(([^^]*)|Desconhecido)";

            /// <summary>
            /// Pattern for CompanyID: ([0-9]{9})+|([^^]+ [0-9/]+)
            /// </summary>
            public const string CompanyID = @"([0-9]{9})+|([^^]+ [0-9/]+)";

            /// <summary>
            /// Pattern for DocumentNumber: [^ ]+ [^/^ ]+/[0-9]+
            /// </summary>
            public const string DocumentNumber = @"[^ ]+ [^/^ ]+/[0-9]+";

            /// <summary>
            /// Pattern for EACCode: ([0-9]*)
            /// </summary>
            public const string EACCode = @"(([0-9]*))";

            /// <summary>
            /// Pattern for InvoiceNo: [^ ]+ [^/^ ]+/[0-9]+
            /// </summary>
            public const string InvoiceNo = @"[^ ]+ [^/^ ]+/[0-9]+";

            /// <summary>
            /// Pattern for PaymentRefNo: [^ ]+ [^/^ ]+/[0-9]+
            /// </summary>
            public const string PaymentRefNo = @"[^ ]+ [^/^ ]+/[0-9]+";

            /// <summary>
            /// Pattern for TaxCode: RED|INT|NOR|ISE|OUT|([a-zA-Z0-9\.])*|NS
            /// </summary>
            public const string TaxCode = @"RED|INT|NOR|ISE|OUT|([a-zA-Z0-9\.])*|NS";

            /// <summary>
            /// Pattern for SAFPTCNCode: [0-9]{8}
            /// </summary>
            public const string CNCode = @"[0-9]{8}";

            /// <summary>
            /// Pattern for SAFTPTDocArchivalNumber: [^ ]{1,20}
            /// </summary>
            public const string DocArchivalNumber = @"[^ ]{1,20}";

            /// <summary>
            /// Pattern for SAFPTGLAccountID: ([^^]*)
            /// </summary>
            public const string GLAccountID = @"([^^]*)";

            /// <summary>
            /// Pattern for SAFPTHashControl: [0-9]+|[0-9]+[\.][0-9]+|[0-9]+-[A-Z]{2}(M )([^/^ ]+[/][0-9]+)|[0-9]+-[A-Z]{2}(D )([^ ]+ [^/^ ]+[/][0-9]+)
            /// </summary>
            public const string HashControl = @"[0-9]+|[0-9]+[\.][0-9]+|[0-9]+-[A-Z]{2}(M )([^/^ ]+[/][0-9]+)|[0-9]+-[A-Z]{2}(D )([^ ]+ [^/^ ]+[/][0-9]+)";

            /// <summary>
            /// Pattern for SAFPTJournalID: [^ ]{1,30}
            /// </summary>
            public const string JournalID = @"[^ ]{1,30}";

            /// <summary>
            /// Pattern for SAFPTPortugueseTaxExemptionCode: (M[0-9]{2})+
            /// </summary>
            public const string PortugueseTaxExemptionCode = @"(M[0-9]{2})+";

            /// <summary>
            /// Pattern for SAFPTProductID: [^/]+/[^/]+
            /// </summary>
            public const string ProductID = @"[^/]+/[^/]+";

            /// <summary>
            /// Pattern for SAFPTTransactionID: [1-9][0-9]{3}-[01][0-9]-[0-3][0-9] [^ ]{1,30} [^ ]{1,20}
            /// </summary>
            public const string TransactionID = @"[1-9][0-9]{3}-[01][0-9]-[0-3][0-9] [^ ]{1,30} [^ ]{1,20}";

            /// <summary>
            /// Pattern for SAFPTUNNumber: [0-9]{4}
            /// </summary>
            public const string UNNumber = @"[0-9]{4}";
        }

        /// <summary>
        /// Validation ranges
        /// </summary>
        public static class Ranges
        {
            /// <summary>
            /// Fiscal year range: 2000-9999
            /// </summary>
            public const int FiscalYearMin = 2000;
            public const int FiscalYearMax = 9999;

            /// <summary>
            /// Period range: 1-12
            /// </summary>
            public const int PeriodMin = 1;
            public const int PeriodMax = 12;

            /// <summary>
            /// Accounting period range: 1-16
            /// </summary>
            public const int AccountingPeriodMin = 1;
            public const int AccountingPeriodMax = 16;

            /// <summary>
            /// Portuguese VAT number range: 100000000-999999999
            /// </summary>
            public const int PortugueseVatNumberMin = 100000000;
            public const int PortugueseVatNumberMax = 999999999;

            /// <summary>
            /// Taxonomy code range: 1-999
            /// </summary>
            public const int TaxonomyCodeMin = 1;
            public const int TaxonomyCodeMax = 999;

            /// <summary>
            /// Cash VAT scheme indicator range: 0-1
            /// </summary>
            public const int CashVATSchemeIndicatorMin = 0;
            public const int CashVATSchemeIndicatorMax = 1;

            /// <summary>
            /// Self billing indicator range: 0-1
            /// </summary>
            public const int SelfBillingIndicatorMin = 0;
            public const int SelfBillingIndicatorMax = 1;

            /// <summary>
            /// Third parties billing indicator range: 0-1
            /// </summary>
            public const int ThirdPartiesBillingIndicatorMin = 0;
            public const int ThirdPartiesBillingIndicatorMax = 1;
        }
    }
} 