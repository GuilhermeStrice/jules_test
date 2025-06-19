using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// S para SNC base (Taxonomia S), M para SNC microentidades (Taxonomia M), N para Normas Internacionais de Contabilidade (Taxonomia S), O para outros referenciais contabilisticos cuja taxonomia nao se encontra codificada
    /// </summary>
    public enum TaxAccountingBasis
    {
        [XmlEnum("S")]
        SNCBase,
        
        [XmlEnum("M")]
        SNCMicroEntities,
        
        [XmlEnum("N")]
        InternationalAccountingStandards,
        
        [XmlEnum("O")]
        OtherAccountingStandards
    }
} 