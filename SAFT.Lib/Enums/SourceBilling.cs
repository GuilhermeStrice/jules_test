/*
<!-- Origem do documento -->
<xs:simpleType name="SAFTPTSourceBilling">
<xs:annotation>
<xs:documentation>
P para documento produzido na aplicacao, I para documento integrado e produzido noutra aplicacao, M para documento proveniente de recuperacao ou de emissao manual
</xs:documentation>
</xs:annotation>
<xs:restriction base="xs:string">
<xs:enumeration value="P"/>
<xs:enumeration value="I"/>
<xs:enumeration value="M"/>
</xs:restriction>
</xs:simpleType>
*/

using System.Xml.Serialization;

namespace SAFT.Lib.Enums
{
    /// <summary>
    /// Origem do documento de faturacao
    /// P para documento produzido na aplicacao, 
    /// I para documento integrado e produzido noutra aplicacao, 
    /// M para documento proveniente de recuperacao ou de emissao manual
    /// </summary>
    public enum SourceBilling
    {
        /// <summary>Documento produzido na aplicacao</summary>
        [XmlEnum("P")]
        P,
        /// <summary>Documento integrado e produzido noutra aplicacao</summary>
        [XmlEnum("I")]
        I,
        /// <summary>Documento proveniente de recuperacao ou de emissao manual</summary>
        [XmlEnum("M")]
        M
    }
} 