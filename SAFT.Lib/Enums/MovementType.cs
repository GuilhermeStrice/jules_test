/*
<!-- Tipo de documento de movimentacao de mercadorias-->
<xs:element name="MovementType">
<xs:annotation>
<xs:documentation>
Restricao: Tipos de Documento (GR para Guia de remessa, GT para Guia de transporte incluindo as globais, GA para Guia de movimentacao de ativos fixos proprios, GC para Guia de consignacao, GD para Guia ou nota de devolucao
</xs:documentation>
</xs:annotation>
<xs:simpleType>
<xs:restriction base="xs:string">
<xs:enumeration value="GR"/>
<xs:enumeration value="GT"/>
<xs:enumeration value="GA"/>
<xs:enumeration value="GC"/>
<xs:enumeration value="GD"/>
</xs:restriction>
</xs:simpleType>
</xs:element>
*/

using System.Xml.Serialization;

namespace SAFT.Lib.Enums
{
    /// <summary>
    /// Tipo de documento de movimentacao de mercadorias
    /// GR para Guia de remessa, GT para Guia de transporte incluindo as globais, 
    /// GA para Guia de movimentacao de ativos fixos proprios, GC para Guia de consignacao, 
    /// GD para Guia ou nota de devolucao
    /// </summary>
    public enum MovementType
    {
        /// <summary>Guia de remessa</summary>
        [XmlEnum("GR")]
        GR,
        /// <summary>Guia de transporte incluindo as globais</summary>
        [XmlEnum("GT")]
        GT,
        /// <summary>Guia de movimentacao de ativos fixos proprios</summary>
        [XmlEnum("GA")]
        GA,
        /// <summary>Guia de consignacao</summary>
        [XmlEnum("GC")]
        GC,
        /// <summary>Guia ou nota de devolucao</summary>
        [XmlEnum("GD")]
        GD
    }
} 