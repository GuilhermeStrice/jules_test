/*
<!-- Tipo de Imposto -->
<xs:element name="TaxType">
<xs:simpleType>
<xs:restriction base="xs:string">
<xs:enumeration value="IVA"/>
<xs:enumeration value="IS"/>
<xs:enumeration value="NS"/>
</xs:restriction>
</xs:simpleType>
</xs:element>
*/

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Tipo de Imposto
    /// IVA para Imposto sobre o Valor Acrescentado, IS para Imposto do Selo, NS para Nao Sujeito
    /// </summary>
    public enum TaxType
    {
        /// <summary>Imposto sobre o Valor Acrescentado</summary>
        [XmlEnum("IVA")]
        IVA,
        /// <summary>Imposto do Selo</summary>
        [XmlEnum("IS")]
        IS,
        /// <summary>Nao Sujeito</summary>
        [XmlEnum("NS")]
        NS
    }
} 