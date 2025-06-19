using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Categorias de contas segundo o plano de contas português
    /// </summary>
    public enum AccountCategory
    {
        [XmlEnum("1")]
        Assets,
        
        [XmlEnum("2")]
        Liabilities,
        
        [XmlEnum("3")]
        Equity,
        
        [XmlEnum("4")]
        Revenue,
        
        [XmlEnum("5")]
        Expenses,
        
        [XmlEnum("6")]
        CostOfGoodsSold,
        
        [XmlEnum("7")]
        OtherIncome,
        
        [XmlEnum("8")]
        OtherExpenses
    }
} 