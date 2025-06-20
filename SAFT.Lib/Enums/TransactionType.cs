namespace SAFT.Lib.Enums
{
    /// <summary>
    /// Tipos de Movimento Contabilistico
    /// N para Normal, R para Regularizacoes do periodo de tributacao, 
    /// A para Apuramento de resultados, J para Movimentos de ajustamento
    /// </summary>
    public enum TransactionType
    {
        /// <summary>Normal</summary>
        N,
        /// <summary>Regularizacoes do periodo de tributacao</summary>
        R,
        /// <summary>Apuramento de resultados</summary>
        A,
        /// <summary>Movimentos de ajustamento</summary>
        J
    }
} 