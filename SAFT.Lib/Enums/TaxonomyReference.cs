/*
<!-- Categoria da conta -->
<xs:element name="TaxonomyReference">
<xs:annotation>
<xs:documentation>
S para SNC base (Taxonomia S), M para SNC microentidades (Taxonomia M), N para Normas Internacionais de Contabilidade (Taxonomia S), O para outros referenciais contabilisticos cuja taxonomia nao se encontra codificada
</xs:documentation>
</xs:annotation>
<xs:simpleType>
<xs:restriction base="xs:string">
<xs:enumeration value="S"/>
<xs:enumeration value="M"/>
<xs:enumeration value="N"/>
<xs:enumeration value="O"/>
</xs:restriction>
</xs:simpleType>
</xs:element>
*/

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Categoria da conta
    /// S para SNC base (Taxonomia S), M para SNC microentidades (Taxonomia M), 
    /// N para Normas Internacionais de Contabilidade (Taxonomia S), 
    /// O para outros referenciais contabilisticos cuja taxonomia nao se encontra codificada
    /// </summary>
    public enum TaxonomyReference
    {
        /// <summary>SNC base (Taxonomia S)</summary>
        [XmlEnum("S")]
        S,
        /// <summary>SNC microentidades (Taxonomia M)</summary>
        [XmlEnum("M")]
        M,
        /// <summary>Normas Internacionais de Contabilidade (Taxonomia S)</summary>
        [XmlEnum("N")]
        N,
        /// <summary>Outros referenciais contabilisticos cuja taxonomia nao se encontra codificada</summary>
        [XmlEnum("O")]
        O
    }
} 