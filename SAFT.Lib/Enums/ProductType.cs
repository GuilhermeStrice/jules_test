/*
<!-- Tipos de produto -->
<xs:element name="ProductType">
<xs:annotation>
<xs:documentation>
Restricao: P para Produtos, S para Servicos, O para Outros (Ex: portes debitados, adiantamentos recebidos ou alienacao de ativos), E para Impostos Especiais de Consumo (ex.:IABA, ISP, IT); I para impostos, taxas e encargos parafiscais exceto IVA e IS que deverao ser refletidos na tabela 2.5 Tabela de impostos (TaxTable)e Impostos Especiais de Consumo
</xs:documentation>
</xs:annotation>
<xs:simpleType>
<xs:restriction base="xs:string">
<xs:enumeration value="P"/>
<xs:enumeration value="S"/>
<xs:enumeration value="O"/>
<xs:enumeration value="E"/>
<xs:enumeration value="I"/>
</xs:restriction>
</xs:simpleType>
</xs:element>
*/

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Tipos de produto
    /// P para Produtos, S para Servicos, O para Outros (Ex: portes debitados, adiantamentos recebidos ou alienacao de ativos), 
    /// E para Impostos Especiais de Consumo (ex.:IABA, ISP, IT); 
    /// I para impostos, taxas e encargos parafiscais exceto IVA e IS que deverao ser refletidos na tabela 2.5 
    /// Tabela de impostos (TaxTable)e Impostos Especiais de Consumo
    /// </summary>
    public enum ProductType
    {
        /// <summary>Produtos</summary>
        [XmlEnum("P")]
        P,
        /// <summary>Servicos</summary>
        [XmlEnum("S")]
        S,
        /// <summary>Outros (Ex: portes debitados, adiantamentos recebidos ou alienacao de ativos)</summary>
        [XmlEnum("O")]
        O,
        /// <summary>Impostos Especiais de Consumo (ex.:IABA, ISP, IT)</summary>
        [XmlEnum("E")]
        E,
        /// <summary>Impostos, taxas e encargos parafiscais exceto IVA e IS</summary>
        [XmlEnum("I")]
        I
    }
} 