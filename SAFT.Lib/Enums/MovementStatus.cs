/*
<!-- Estado do documento MovementOfGoods -->
<xs:element name="MovementStatus">
<xs:annotation>
<xs:documentation>
N para Normal, T para Por conta de terceiros, A para Documento anulado, F para Documento faturado, quando para este documento tambem existe na tabela 4.1. para Documentos comerciais a clientes (SalesInvoices) o correspondente do tipo fatura ou fatura simplificada, R para Documento de resumo doutros documentos criados noutras aplicacoes e gerado nesta aplicacao
</xs:documentation>
</xs:annotation>
<xs:simpleType>
<xs:restriction base="xs:string">
<xs:enumeration value="N"/>
<xs:enumeration value="T"/>
<xs:enumeration value="A"/>
<xs:enumeration value="F"/>
<xs:enumeration value="R"/>
</xs:restriction>
</xs:simpleType>
</xs:element>
*/

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Estado do documento MovementOfGoods
    /// N para Normal, T para Por conta de terceiros, A para Documento anulado, 
    /// F para Documento faturado, quando para este documento tambem existe na tabela 4.1. 
    /// para Documentos comerciais a clientes (SalesInvoices) o correspondente do tipo fatura ou fatura simplificada, 
    /// R para Documento de resumo doutros documentos criados noutras aplicacoes e gerado nesta aplicacao
    /// </summary>
    public enum MovementStatus
    {
        /// <summary>Normal</summary>
        [XmlEnum("N")]
        N,
        /// <summary>Por conta de terceiros</summary>
        [XmlEnum("T")]
        T,
        /// <summary>Documento anulado</summary>
        [XmlEnum("A")]
        A,
        /// <summary>Documento faturado</summary>
        [XmlEnum("F")]
        F,
        /// <summary>Documento de resumo doutros documentos criados noutras aplicacoes e gerado nesta aplicacao</summary>
        [XmlEnum("R")]
        R
    }
} 