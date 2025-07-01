/*
<!-- Estado do documento Payments -->
<xs:element name="PaymentStatus">
<xs:annotation>
<xs:documentation>N para normal, A para Anulado </xs:documentation>
</xs:annotation>
<xs:simpleType>
<xs:restriction base="xs:string">
<xs:enumeration value="N"/>
<xs:enumeration value="A"/>
</xs:restriction>
</xs:simpleType>
</xs:element>
*/

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Estado do documento Payments
    /// N para normal, A para Anulado
    /// </summary>
    public enum PaymentStatus
    {
        /// <summary>Normal</summary>
        [XmlEnum("N")]
        N,
        /// <summary>Anulado</summary>
        [XmlEnum("A")]
        A
    }
} 