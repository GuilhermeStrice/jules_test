namespace SAFT.Lib.Enums
{
    /// <summary>
    /// Estado do documento WorkingDocuments
    /// N para Normal, A para Anulado, F para faturado (quando para este documento tambem existe na tabela 4.1. 
    /// o correspondente do tipo fatura ou fatura simplificada)
    /// </summary>
    public enum WorkStatus
    {
        /// <summary>Normal</summary>
        N,
        /// <summary>Anulado</summary>
        A,
        /// <summary>Faturado</summary>
        F
    }
} 