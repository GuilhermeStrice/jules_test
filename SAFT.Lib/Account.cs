using System.Xml.Serialization;
using SAFT.Lib.Enums;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents a general ledger account.
    /// </summary>
    public class Account
    {
        /// <summary>
        /// The account ID.
        /// </summary>
        [XmlElement("AccountID")]
        public GLAccountID AccountID { get; set; } = new GLAccountID();

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
        public GLAccountID? GroupingCode { get; set; }

        /// <summary>
        /// The taxonomy code (optional).
        /// </summary>
        [XmlElement("TaxonomyCode")]
        public TaxonomyCode? TaxonomyCode { get; set; }
    }
}