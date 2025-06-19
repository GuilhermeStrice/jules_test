using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// N para Normal, S para Autofaturacao, A para Documento anulado, R para Documento de resumo doutros documentos criados noutras aplicacoes e gerado nesta aplicacao, F para Documento faturado
    /// </summary>
    public enum InvoiceStatus
    {
        [XmlEnum("N")]
        Normal,
        
        [XmlEnum("S")]
        SelfBilling,
        
        [XmlEnum("A")]
        Cancelled,
        
        [XmlEnum("R")]
        Summary,
        
        [XmlEnum("F")]
        Billed
    }
} 