namespace SAFT.Lib.Enums
{
    /// <summary>
    /// Estado do documento SalesInvoices
    /// N para Normal, S para Autofaturacao, A para Documento anulado, 
    /// R para Documento de resumo doutros documentos criados noutras aplicacoes e gerado nesta aplicacao, 
    /// F para Documento faturado
    /// </summary>
    public enum InvoiceStatus
    {
        /// <summary>Normal</summary>
        N,
        /// <summary>Autofaturacao</summary>
        S,
        /// <summary>Documento anulado</summary>
        A,
        /// <summary>Documento de resumo doutros documentos criados noutras aplicacoes e gerado nesta aplicacao</summary>
        R,
        /// <summary>Documento faturado</summary>
        F
    }
} 