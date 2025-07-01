/*
<!-- Estado do documento WorkingDocuments -->
<xs:element name="WorkStatus">
<xs:annotation>
<xs:documentation>
N para Normal, A para Anulado, F para faturado (quando para este documento tambem existe na tabela 4.1. o correspondente do tipo fatura ou fatura simplificada)
</xs:documentation>
</xs:annotation>
<xs:simpleType>
<xs:restriction base="xs:string">
<xs:enumeration value="N"/>
<xs:enumeration value="A"/>
<xs:enumeration value="F"/>
</xs:restriction>
</xs:simpleType>
</xs:element>
*/

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Estado do documento WorkingDocuments
    /// N para Normal, A para Anulado, F para faturado (quando para este documento tambem existe na tabela 4.1. 
    /// o correspondente do tipo fatura ou fatura simplificada)
    /// </summary>
    public enum WorkStatus
    {
        /// <summary>Normal</summary>
        [XmlEnum("N")]
        N,
        /// <summary>Anulado</summary>
        [XmlEnum("A")]
        A,
        /// <summary>Faturado</summary>
        [XmlEnum("F")]
        F
    }
} 