/*
<!-- Codigo da moeda (ISO 4217) -->
<xs:element name="CurrencyCode">
<!--
 Nao consta o EUR por nao existirem situacoes que requeiram este codigo de moeda 
-->
<xs:simpleType>
<xs:restriction base="xs:string">
<xs:pattern value="AED|AFN|ALL|AMD|ANG|AOA|ARS|AUD|AWG|AZN|BAM|BBD|BDT|BGN|BHD|BIF|BMD|BND|BOB|BOV|BRL|BSD|BTN|BWP|BYN|BYR|BZD|CAD|CDF|CHE|CHF|CHW|CLF|CLP|CNY|COP|COU|CRC|CUC|CUP|CVE|CZK|DJF|DKK|DOP|DZD|EGP|ERN|ETB|FJD|FKP|GBP|GEL|GHS|GIP|GMD|GNF|GTQ|GYD|HKD|HNL|HRK|HTG|HUF|IDR|ILS|INR|IQD|IRR|ISK|JMD|JOD|JPY|KES|KGS|KHR|KMF|KPW|KRW|KWD|KYD|KZT|LAK|LBP|LKR|LRD|LSL|LTL|LVL|LYD|MAD|MDL|MGA|MKD|MMK|MNT|MOP|MRO|MRU|MUR|MVR|MWK|MXN|MXV|MYR|MZN|NAD|NGN|NIO|NOK|NPR|NZD|OMR|PAB|PEN|PGK|PHP|PKR|PLN|PYG|QAR|RON|RSD|RUB|RWF|SAR|SBD|SCR|SDG|SEK|SGD|SHP|SLE|SLL|SOS|SRD|SSP|STD|STN|SVC|SYP|SZL|THB|TJS|TMT|TND|TOP|TRY|TTD|TWD|TZS|UAH|UGX|USD|USN|USS|UYI|UYU|UZS|VED|VEF|VES|VND|VUV|WST|XAF|XAG|XAU|XBA|XBB|XBC|XBD|XCD|XDR|XFU|XOF|XPD|XPF|XPT|XSU|XUA|YER|ZAR|ZMW|ZWL|EEK|SKK|TMM|ZMK|ZWD|ZWR"/>
</xs:restriction>
</xs:simpleType>
</xs:element>
*/

namespace SAFT.Lib.Constants
{
    /// <summary>
    /// CurrencyCodes - ISO 4217 currency codes
    /// Contains all valid ISO 4217 currency codes as defined in the SAFT XSD schema.
    /// Note: EUR is not included as per XSD comment - "Nao consta o EUR por nao existirem situacoes que requeiram este codigo de moeda"
    /// These codes are used for identifying currencies in various SAFT documents and structures.
    /// </summary>
    /// <summary>
    /// ISO 4217 currency codes as defined in the SAFT XSD schema
    /// Note: EUR is not included as per XSD comment - "Nao consta o EUR por nao existirem situacoes que requeiram este codigo de moeda"
    /// </summary>
    public static class CurrencyCodes
    {
        public const string UAE_Dirham = "AED";
        public const string Afghan_Afghani = "AFN";
        public const string Albanian_Lek = "ALL";
        public const string Armenian_Dram = "AMD";
        public const string Netherlands_Antillean_Guilder = "ANG";
        public const string Angolan_Kwanza = "AOA";
        public const string Argentine_Peso = "ARS";
        public const string Australian_Dollar = "AUD";
        public const string Aruban_Florin = "AWG";
        public const string Azerbaijani_Manat = "AZN";
        public const string Bosnia_Herzegovina_Convertible_Mark = "BAM";
        public const string Barbados_Dollar = "BBD";
        public const string Bangladeshi_Taka = "BDT";
        public const string Bulgarian_Lev = "BGN";
        public const string Bahraini_Dinar = "BHD";
        public const string Burundian_Franc = "BIF";
        public const string Bermudian_Dollar = "BMD";
        public const string Brunei_Dollar = "BND";
        public const string Bolivian_Boliviano = "BOB";
        public const string Bolivian_Mvdol = "BOV";
        public const string Brazilian_Real = "BRL";
        public const string Bahamian_Dollar = "BSD";
        public const string Bhutanese_Ngultrum = "BTN";
        public const string Botswana_Pula = "BWP";
        public const string Belarusian_Ruble = "BYN";
        public const string Belarusian_Ruble_Old = "BYR";
        public const string Belize_Dollar = "BZD";
        public const string Canadian_Dollar = "CAD";
        public const string Congolese_Franc = "CDF";
        public const string WIR_Euro = "CHE";
        public const string Swiss_Franc = "CHF";
        public const string WIR_Franc = "CHW";
        public const string Unidad_de_Fomento = "CLF";
        public const string Chilean_Peso = "CLP";
        public const string Chinese_Yuan = "CNY";
        public const string Colombian_Peso = "COP";
        public const string Unidad_de_Valor_Real = "COU";
        public const string Costa_Rican_Colon = "CRC";
        public const string Cuban_Convertible_Peso = "CUC";
        public const string Cuban_Peso = "CUP";
        public const string Cape_Verde_Escudo = "CVE";
        public const string Czech_Koruna = "CZK";
        public const string Djiboutian_Franc = "DJF";
        public const string Danish_Krone = "DKK";
        public const string Dominican_Peso = "DOP";
        public const string Algerian_Dinar = "DZD";
        public const string Egyptian_Pound = "EGP";
        public const string Eritrean_Nakfa = "ERN";
        public const string Ethiopian_Birr = "ETB";
        public const string Fijian_Dollar = "FJD";
        public const string Falkland_Islands_Pound = "FKP";
        public const string Pound_Sterling = "GBP";
        public const string Georgian_Lari = "GEL";
        public const string Ghanaian_Cedi = "GHS";
        public const string Gibraltar_Pound = "GIP";
        public const string Gambian_Dalasi = "GMD";
        public const string Guinean_Franc = "GNF";
        public const string Guatemalan_Quetzal = "GTQ";
        public const string Guyana_Dollar = "GYD";
        public const string Hong_Kong_Dollar = "HKD";
        public const string Honduran_Lempira = "HNL";
        public const string Croatian_Kuna = "HRK";
        public const string Haitian_Gourde = "HTG";
        public const string Hungarian_Forint = "HUF";
        public const string Indonesian_Rupiah = "IDR";
        public const string Israeli_New_Shekel = "ILS";
        public const string Indian_Rupee = "INR";
        public const string Iraqi_Dinar = "IQD";
        public const string Iranian_Rial = "IRR";
        public const string Icelandic_Krona = "ISK";
        public const string Jamaican_Dollar = "JMD";
        public const string Jordanian_Dinar = "JOD";
        public const string Japanese_Yen = "JPY";
        public const string Kenyan_Shilling = "KES";
        public const string Kyrgyzstani_Som = "KGS";
        public const string Cambodian_Riel = "KHR";
        public const string Comorian_Franc = "KMF";
        public const string North_Korean_Won = "KPW";
        public const string South_Korean_Won = "KRW";
        public const string Kuwaiti_Dinar = "KWD";
        public const string Cayman_Islands_Dollar = "KYD";
        public const string Kazakhstani_Tenge = "KZT";
        public const string Lao_Kip = "LAK";
        public const string Lebanese_Pound = "LBP";
        public const string Sri_Lankan_Rupee = "LKR";
        public const string Liberian_Dollar = "LRD";
        public const string Lesotho_Loti = "LSL";
        public const string Lithuanian_Litas = "LTL";
        public const string Latvian_Lats = "LVL";
        public const string Libyan_Dinar = "LYD";
        public const string Moroccan_Dirham = "MAD";
        public const string Moldovan_Leu = "MDL";
        public const string Malagasy_Ariary = "MGA";
        public const string Macedonian_Denar = "MKD";
        public const string Myanmar_Kyat = "MMK";
        public const string Mongolian_Tugrik = "MNT";
        public const string Macanese_Pataca = "MOP";
        public const string Mauritanian_Ouguiya = "MRO";
        public const string Mauritanian_Ouguiya_New = "MRU";
        public const string Mauritian_Rupee = "MUR";
        public const string Maldivian_Rufiyaa = "MVR";
        public const string Malawian_Kwacha = "MWK";
        public const string Mexican_Peso = "MXN";
        public const string Mexican_Unidad_de_Inversion = "MXV";
        public const string Malaysian_Ringgit = "MYR";
        public const string Mozambican_Metical = "MZN";
        public const string Namibian_Dollar = "NAD";
        public const string Nigerian_Naira = "NGN";
        public const string Nicaraguan_Cordoba = "NIO";
        public const string Norwegian_Krone = "NOK";
        public const string Nepalese_Rupee = "NPR";
        public const string New_Zealand_Dollar = "NZD";
        public const string Omani_Rial = "OMR";
        public const string Panamanian_Balboa = "PAB";
        public const string Peruvian_Sol = "PEN";
        public const string Papua_New_Guinean_Kina = "PGK";
        public const string Philippine_Peso = "PHP";
        public const string Pakistani_Rupee = "PKR";
        public const string Polish_Zloty = "PLN";
        public const string Paraguayan_Guarani = "PYG";
        public const string Qatari_Riyal = "QAR";
        public const string Romanian_Leu = "RON";
        public const string Serbian_Dinar = "RSD";
        public const string Russian_Ruble = "RUB";
        public const string Rwandan_Franc = "RWF";
        public const string Saudi_Riyal = "SAR";
        public const string Solomon_Islands_Dollar = "SBD";
        public const string Seychelles_Rupee = "SCR";
        public const string Sudanese_Pound = "SDG";
        public const string Swedish_Krona = "SEK";
        public const string Singapore_Dollar = "SGD";
        public const string Saint_Helena_Pound = "SHP";
        public const string Sierra_Leone_Leone = "SLE";
        public const string Sierra_Leone_Leone_Old = "SLL";
        public const string Somali_Shilling = "SOS";
        public const string Surinamese_Dollar = "SRD";
        public const string South_Sudanese_Pound = "SSP";
        public const string Sao_Tome_and_Principe_Dobra = "STD";
        public const string Sao_Tome_and_Principe_Dobra_New = "STN";
        public const string Salvadoran_Colon = "SVC";
        public const string Syrian_Pound = "SYP";
        public const string Eswatini_Lilangeni = "SZL";
        public const string Thai_Baht = "THB";
        public const string Tajikistani_Somoni = "TJS";
        public const string Turkmenistan_Manat = "TMT";
        public const string Tunisian_Dinar = "TND";
        public const string Tongan_Paanga = "TOP";
        public const string Turkish_Lira = "TRY";
        public const string Trinidad_and_Tobago_Dollar = "TTD";
        public const string New_Taiwan_Dollar = "TWD";
        public const string Tanzanian_Shilling = "TZS";
        public const string Ukrainian_Hryvnia = "UAH";
        public const string Ugandan_Shilling = "UGX";
        public const string US_Dollar = "USD";
        public const string US_Dollar_Next_Day = "USN";
        public const string US_Dollar_Same_Day = "USS";
        public const string Uruguay_Peso_en_Unidades_Indexadas = "UYI";
        public const string Uruguayan_Peso = "UYU";
        public const string Uzbekistan_Som = "UZS";
        public const string Venezuelan_Bolivar_Digital = "VED";
        public const string Venezuelan_Bolivar_Old = "VEF";
        public const string Venezuelan_Bolivar_Soberano = "VES";
        public const string Vietnamese_Dong = "VND";
        public const string Vanuatu_Vatu = "VUV";
        public const string Samoan_Tala = "WST";
        public const string CFA_Franc_BEAC = "XAF";
        public const string Silver = "XAG";
        public const string Gold = "XAU";
        public const string European_Composite_Unit = "XBA";
        public const string European_Monetary_Unit = "XBB";
        public const string European_Unit_of_Account_9 = "XBC";
        public const string European_Unit_of_Account_17 = "XBD";
        public const string East_Caribbean_Dollar = "XCD";
        public const string Special_Drawing_Rights = "XDR";
        public const string UIC_Franc = "XFU";
        public const string CFA_Franc_BCEAO = "XOF";
        public const string Palladium = "XPD";
        public const string CFP_Franc = "XPF";
        public const string Platinum = "XPT";
        public const string SUCRE = "XSU";
        public const string ADB_Unit_of_Account = "XUA";
        public const string Yemeni_Rial = "YER";
        public const string South_African_Rand = "ZAR";
        public const string Zambian_Kwacha = "ZMW";
        public const string Zimbabwean_Dollar = "ZWL";
        public const string Estonian_Kroon = "EEK";
        public const string Slovak_Koruna = "SKK";
        public const string Turkmenistani_Manat_Old = "TMM";
        public const string Zambian_Kwacha_Old = "ZMK";
        public const string Zimbabwean_Dollar_Old = "ZWD";
        public const string Zimbabwean_Dollar_New = "ZWR";
    }
} 