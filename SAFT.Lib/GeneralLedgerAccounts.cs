/*
 * Original XSD Schema:
 * <xs:element name="GeneralLedgerAccounts">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="TaxonomyReference"/>
 * <xs:element name="Account" maxOccurs="unbounded">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element name="AccountID" type="SAFPTGLAccountID"/>
 * <xs:element ref="AccountDescription"/>
 * <xs:element ref="OpeningDebitBalance"/>
 * <xs:element ref="OpeningCreditBalance"/>
 * <xs:element ref="ClosingDebitBalance"/>
 * <xs:element ref="ClosingCreditBalance"/>
 * <xs:element ref="GroupingCategory"/>
 * <xs:element name="GroupingCode" type="SAFPTGLAccountID" minOccurs="0"/>
 * <xs:element name="TaxonomyCode" type="SAFPTTaxonomyCode" minOccurs="0"/>
 * </xs:sequence>
 * <xs:assert test=" if ((ns:GroupingCategory != 'GM' and not(ns:TaxonomyCode)) or (ns:GroupingCategory eq 'GM' and ns:TaxonomyCode)) then true() else false()"/>
 * <xs:assert test=" if ((ns:GroupingCategory eq 'GR' and not(ns:GroupingCode)) or (ns:GroupingCategory eq 'AR' and not(ns:GroupingCode)) or (ns:GroupingCategory eq 'GA' and ns:GroupingCode) or (ns:GroupingCategory eq 'AA' and ns:GroupingCode) or (ns:GroupingCategory eq 'GM' and ns:GroupingCode) or (ns:GroupingCategory eq 'AM' and ns:GroupingCode)) then true() else false()"/>
 * </xs:complexType>
 * </xs:element>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * 
 * Description: General ledger accounts with taxonomy reference and account details.
 */

using System.Collections.Generic;
using System.Xml.Serialization;
using SAFT.Lib.Enums;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents the general ledger accounts with taxonomy reference and account details.
    /// </summary>
    [XmlRoot("GeneralLedgerAccounts")]
    public class GeneralLedgerAccounts
    {
        /// <summary>
        /// The taxonomy reference.
        /// </summary>
        [XmlElement("TaxonomyReference")]
        public TaxonomyReference TaxonomyReference { get; set; }

        /// <summary>
        /// The list of accounts.
        /// </summary>
        [XmlElement("Account")]
        public List<Account> Accounts { get; set; } = new List<Account>();

        /// <summary>
        /// Represents a general ledger account.
        /// </summary>
        public class Account
        {
            /// <summary>
            /// The account ID.
            /// </summary>
            [XmlElement("AccountID")]
            public string AccountID { get; set; } = string.Empty;

            /// <summary>
            /// The account description.
            /// </summary>
            [XmlElement("AccountDescription")]
            public string AccountDescription { get; set; } = string.Empty;

            /// <summary>
            /// The opening debit balance.
            /// </summary>
            [XmlElement("OpeningDebitBalance")]
            public decimal OpeningDebitBalance { get; set; }

            /// <summary>
            /// The opening credit balance.
            /// </summary>
            [XmlElement("OpeningCreditBalance")]
            public decimal OpeningCreditBalance { get; set; }

            /// <summary>
            /// The closing debit balance.
            /// </summary>
            [XmlElement("ClosingDebitBalance")]
            public decimal ClosingDebitBalance { get; set; }

            /// <summary>
            /// The closing credit balance.
            /// </summary>
            [XmlElement("ClosingCreditBalance")]
            public decimal ClosingCreditBalance { get; set; }

            /// <summary>
            /// The grouping category.
            /// </summary>
            [XmlElement("GroupingCategory")]
            public GroupingCategory GroupingCategory { get; set; }

            /// <summary>
            /// The grouping code (optional).
            /// </summary>
            [XmlElement("GroupingCode")]
            public string? GroupingCode { get; set; }

            /// <summary>
            /// The taxonomy code (optional).
            /// </summary>
            [XmlElement("TaxonomyCode")]
            public string? TaxonomyCode { get; set; }
        }
    }
} 