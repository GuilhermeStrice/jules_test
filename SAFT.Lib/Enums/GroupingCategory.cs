/*
<!-- Categoria da conta -->
<xs:element name="GroupingCategory">
<xs:annotation>
<xs:documentation>
GR para conta de 1. grau da contabilidade geral, GA para conta agregadora ou integradora da contabilidade geral, GM para conta de movimento da contabilidade geral, AR para conta de 1. grau da contabilidade analitica, AA para conta agregadora ou integradora da contabilidade analitica, AM para conta de movimento da contabilidade analitica
</xs:documentation>
</xs:annotation>
<xs:simpleType>
<xs:restriction base="xs:string">
<xs:enumeration value="GR"/>
<xs:enumeration value="GA"/>
<xs:enumeration value="GM"/>
<xs:enumeration value="AR"/>
<xs:enumeration value="AA"/>
<xs:enumeration value="AM"/>
</xs:restriction>
</xs:simpleType>
</xs:element>
*/

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Categoria da conta
    /// GR para conta de 1. grau da contabilidade geral, 
    /// GA para conta agregadora ou integradora da contabilidade geral, 
    /// GM para conta de movimento da contabilidade geral, 
    /// AR para conta de 1. grau da contabilidade analitica, 
    /// AA para conta agregadora ou integradora da contabilidade analitica, 
    /// AM para conta de movimento da contabilidade analitica
    /// </summary>
    public enum GroupingCategory
    {
        /// <summary>Conta de 1. grau da contabilidade geral</summary>
        [XmlEnum("GR")]
        GR,
        /// <summary>Conta agregadora ou integradora da contabilidade geral</summary>
        [XmlEnum("GA")]
        GA,
        /// <summary>Conta de movimento da contabilidade geral</summary>
        [XmlEnum("GM")]
        GM,
        /// <summary>Conta de 1. grau da contabilidade analitica</summary>
        [XmlEnum("AR")]
        AR,
        /// <summary>Conta agregadora ou integradora da contabilidade analitica</summary>
        [XmlEnum("AA")]
        AA,
        /// <summary>Conta de movimento da contabilidade analitica</summary>
        [XmlEnum("AM")]
        AM
    }
} 