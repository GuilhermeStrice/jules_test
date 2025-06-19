using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// N para Normal, R para Regularizacoes do periodo de tributacao, A para Apuramento de resultados, J para Movimentos de ajustamento
    /// </summary>
    public enum TransactionType
    {
        [XmlEnum("N")]
        Normal,
        
        [XmlEnum("R")]
        TaxPeriodRegularization,
        
        [XmlEnum("A")]
        ResultsDetermination,
        
        [XmlEnum("J")]
        AdjustmentMovements
    }
} 