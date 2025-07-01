/*
<!-- Codigo do tipo de imposto retido -->
<xs:element name="WithholdingTaxType">
<xs:annotation>
<xs:documentation>
Restricao: IRS para Imposto Sobre o Rendimento das Pessoas Singulares, IRC para Imposto Sobre o Rendimento das Pessoas colectivas, IS para Imposto do selo
</xs:documentation>
</xs:annotation>
<xs:simpleType>
<xs:restriction base="xs:string">
<xs:enumeration value="IRS"/>
<xs:enumeration value="IRC"/>
<xs:enumeration value="IS"/>
</xs:restriction>
</xs:simpleType>
</xs:element>
*/

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Codigo do tipo de imposto retido
    /// IRS para Imposto Sobre o Rendimento das Pessoas Singulares, 
    /// IRC para Imposto Sobre o Rendimento das Pessoas colectivas, 
    /// IS para Imposto do selo
    /// </summary>
    public enum WithholdingTaxType
    {
        /// <summary>Imposto Sobre o Rendimento das Pessoas Singulares</summary>
        [XmlEnum("IRS")]
        IRS,
        /// <summary>Imposto Sobre o Rendimento das Pessoas colectivas</summary>
        [XmlEnum("IRC")]
        IRC,
        /// <summary>Imposto do selo</summary>
        [XmlEnum("IS")]
        IS
    }
} 