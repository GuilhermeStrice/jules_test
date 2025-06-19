using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// IRS para Imposto Sobre o Rendimento das Pessoas Singulares, IRC para Imposto Sobre o Rendimento das Pessoas colectivas, IS para Imposto do selo
    /// </summary>
    public enum TaxType
    {
        [XmlEnum("IRS")]
        PersonalIncomeTax,
        
        [XmlEnum("IRC")]
        CorporateIncomeTax,
        
        [XmlEnum("IS")]
        StampDuty
    }
} 