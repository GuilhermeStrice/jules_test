using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// GR para conta de 1. grau da contabilidade geral, GA para conta agregadora ou integradora da contabilidade geral, GM para conta de movimento da contabilidade geral, AR para conta de 1. grau da contabilidade analitica, AA para conta agregadora ou integradora da contabilidade analitica, AM para conta de movimento da contabilidade analitica
    /// </summary>
    public enum AccountType
    {
        [XmlEnum("GR")]
        GeneralLedgerFirstLevel,
        
        [XmlEnum("GA")]
        GeneralLedgerAggregator,
        
        [XmlEnum("GM")]
        GeneralLedgerMovement,
        
        [XmlEnum("AR")]
        AnalyticalFirstLevel,
        
        [XmlEnum("AA")]
        AnalyticalAggregator,
        
        [XmlEnum("AM")]
        AnalyticalMovement
    }
} 