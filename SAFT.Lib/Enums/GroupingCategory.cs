namespace SAFT.Lib.Enums
{
    /// <summary>
    /// Categoria da conta
    /// GR para conta de 1. grau da contabilidade geral, 
    /// GA para conta agregadora ou integradora da contabilidade geral, 
    /// GM para conta de movimento da contabilidade geral, 
    /// AR para conta de 1. grau da contabilidade analitica, 
    /// AA para conta agregadora ou integradora da contabilidade analitica, 
    /// AM para conta de movimento da contabilidade analitica
    /// </summary>
    public enum GroupingCategory
    {
        /// <summary>Conta de 1. grau da contabilidade geral</summary>
        GR,
        /// <summary>Conta agregadora ou integradora da contabilidade geral</summary>
        GA,
        /// <summary>Conta de movimento da contabilidade geral</summary>
        GM,
        /// <summary>Conta de 1. grau da contabilidade analitica</summary>
        AR,
        /// <summary>Conta agregadora ou integradora da contabilidade analitica</summary>
        AA,
        /// <summary>Conta de movimento da contabilidade analitica</summary>
        AM
    }
} 