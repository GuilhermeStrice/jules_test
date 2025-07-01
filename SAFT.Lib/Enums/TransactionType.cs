/*
<!-- Tipos de Movimento Contabilistico -->
<xs:element name="TransactionType">
<xs:annotation>
<xs:documentation>
Restricao: N para Normal, R para Regularizacoes do periodo de tributacao, A para Apuramento de resultados, J para Movimentos de ajustamento
</xs:documentation>
</xs:annotation>
<xs:simpleType>
<xs:restriction base="xs:string">
<xs:enumeration value="N"/>
<xs:enumeration value="R"/>
<xs:enumeration value="A"/>
<xs:enumeration value="J"/>
</xs:restriction>
</xs:simpleType>
</xs:element>
*/

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Tipos de Movimento Contabilistico
    /// N para Normal, R para Regularizacoes do periodo de tributacao, 
    /// A para Apuramento de resultados, J para Movimentos de ajustamento
    /// </summary>
    public enum TransactionType
    {
        /// <summary>Normal</summary>
        [XmlEnum("N")]
        N,
        /// <summary>Regularizacoes do periodo de tributacao</summary>
        [XmlEnum("R")]
        R,
        /// <summary>Apuramento de resultados</summary>
        [XmlEnum("A")]
        A,
        /// <summary>Movimentos de ajustamento</summary>
        [XmlEnum("J")]
        J
    }
} 