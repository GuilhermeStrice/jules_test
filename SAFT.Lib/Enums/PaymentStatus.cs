using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// N para Normal, A para Anulado, F para faturado (quando para este documento tambem existe na tabela 4.1. o correspondente do tipo fatura ou fatura simplificada)
    /// </summary>
    public enum PaymentStatus
    {
        [XmlEnum("N")]
        Normal,
        
        [XmlEnum("A")]
        Cancelled,
        
        [XmlEnum("F")]
        Billed
    }
} 