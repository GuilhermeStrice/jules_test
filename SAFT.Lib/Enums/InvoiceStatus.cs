/*
<!-- Estado do documento SalesInvoices -->
<xs:element name="InvoiceStatus">
<xs:annotation>
<xs:documentation>
N para Normal, S para Autofaturacao, A para Documento anulado, R para Documento de resumo doutros documentos criados noutras aplicacoes e gerado nesta aplicacao, F para Documento faturado
</xs:documentation>
</xs:annotation>
<xs:simpleType>
<xs:restriction base="xs:string">
<xs:enumeration value="N"/>
<xs:enumeration value="S"/>
<xs:enumeration value="A"/>
<xs:enumeration value="R"/>
<xs:enumeration value="F"/>
</xs:restriction>
</xs:simpleType>
</xs:element>
*/

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Estado do documento SalesInvoices
    /// N para Normal, S para Autofaturacao, A para Documento anulado, 
    /// R para Documento de resumo doutros documentos criados noutras aplicacoes e gerado nesta aplicacao, 
    /// F para Documento faturado
    /// </summary>
    public enum InvoiceStatus
    {
        /// <summary>Normal</summary>
        [XmlEnum("N")]
        N,
        /// <summary>Autofaturacao</summary>
        [XmlEnum("S")]
        S,
        /// <summary>Documento anulado</summary>
        [XmlEnum("A")]
        A,
        /// <summary>Documento de resumo doutros documentos criados noutras aplicacoes e gerado nesta aplicacao</summary>
        [XmlEnum("R")]
        R,
        /// <summary>Documento faturado</summary>
        [XmlEnum("F")]
        F
    }
} 