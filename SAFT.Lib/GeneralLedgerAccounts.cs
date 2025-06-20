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
    }
} 