using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// N para Normal, T para Por conta de terceiros, A para Documento anulado, F para Documento faturado, quando para este documento tambem existe na tabela 4.1. para Documentos comerciais a clientes (SalesInvoices) o correspondente do tipo fatura ou fatura simplificada, R para Documento de resumo doutros documentos criados noutras aplicacoes e gerado nesta aplicacao
    /// </summary>
    public enum WorkStatus
    {
        [XmlEnum("N")]
        Normal,
        
        [XmlEnum("T")]
        ThirdParty,
        
        [XmlEnum("A")]
        Cancelled,
        
        [XmlEnum("F")]
        Billed,
        
        [XmlEnum("R")]
        Summary
    }
} 