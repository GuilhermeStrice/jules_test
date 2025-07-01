/*
<!-- Tipo de documento de venda-->
<xs:element name="InvoiceType">
<xs:annotation>
<xs:documentation>
Restricao:FT para Fatura, emitida nos termos do artigo 36. do Codigo do IVA, FS para Fatura simplificada, emitida nos termos do artigo 40. do Codigo do IVA, FR para Fatura-recibo, ND para Nota de debito, NC para Nota de credito, VD para Venda a dinheiro e factura/recibo (a), TV para Talao de venda (a), TD para Talao de devolucao (a), AA para Alienacao de ativos (a), DA para Devolucao de ativos (a). Para o setor Segurador, ainda pode ser preenchido com: RP para Premio ou recibo de premio, RE para Estorno ou recibo de estorno, CS para Imputacao a co-seguradoras, LD para Imputacao a co-seguradora lider, RA para Resseguro aceite. (a) Para os dados ate 2012-12-31
</xs:documentation>
</xs:annotation>
<xs:simpleType>
<xs:restriction base="xs:string">
<xs:enumeration value="FT"/>
<xs:enumeration value="FS"/>
<xs:enumeration value="FR"/>
<xs:enumeration value="ND"/>
<xs:enumeration value="NC"/>
<xs:enumeration value="VD"/>
<xs:enumeration value="TV"/>
<xs:enumeration value="TD"/>
<xs:enumeration value="AA"/>
<xs:enumeration value="DA"/>
<!-- Para o sector segurador-->
<xs:enumeration value="RP"/>
<xs:enumeration value="RE"/>
<xs:enumeration value="CS"/>
<xs:enumeration value="LD"/>
<xs:enumeration value="RA"/>
</xs:restriction>
</xs:simpleType>
</xs:element>
*/

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Tipo de documento de venda
    /// FT para Fatura, emitida nos termos do artigo 36. do Codigo do IVA, 
    /// FS para Fatura simplificada, emitida nos termos do artigo 40. do Codigo do IVA, 
    /// FR para Fatura-recibo, ND para Nota de debito, NC para Nota de credito, 
    /// VD para Venda a dinheiro e factura/recibo (a), TV para Talao de venda (a), 
    /// TD para Talao de devolucao (a), AA para Alienacao de ativos (a), 
    /// DA para Devolucao de ativos (a). Para o setor Segurador, ainda pode ser preenchido com: 
    /// RP para Premio ou recibo de premio, RE para Estorno ou recibo de estorno, 
    /// CS para Imputacao a co-seguradoras, LD para Imputacao a co-seguradora lider, 
    /// RA para Resseguro aceite. (a) Para os dados ate 2012-12-31
    /// </summary>
    public enum InvoiceType
    {
        /// <summary>Fatura, emitida nos termos do artigo 36. do Codigo do IVA</summary>
        [XmlEnum("FT")]
        FT,
        /// <summary>Fatura simplificada, emitida nos termos do artigo 40. do Codigo do IVA</summary>
        [XmlEnum("FS")]
        FS,
        /// <summary>Fatura-recibo</summary>
        [XmlEnum("FR")]
        FR,
        /// <summary>Nota de debito</summary>
        [XmlEnum("ND")]
        ND,
        /// <summary>Nota de credito</summary>
        [XmlEnum("NC")]
        NC,
        /// <summary>Venda a dinheiro e factura/recibo (para dados ate 2012-12-31)</summary>
        [XmlEnum("VD")]
        VD,
        /// <summary>Talao de venda (para dados ate 2012-12-31)</summary>
        [XmlEnum("TV")]
        TV,
        /// <summary>Talao de devolucao (para dados ate 2012-12-31)</summary>
        [XmlEnum("TD")]
        TD,
        /// <summary>Alienacao de ativos (para dados ate 2012-12-31)</summary>
        [XmlEnum("AA")]
        AA,
        /// <summary>Devolucao de ativos (para dados ate 2012-12-31)</summary>
        [XmlEnum("DA")]
        DA,
        /// <summary>Premio ou recibo de premio (setor Segurador)</summary>
        [XmlEnum("RP")]
        RP,
        /// <summary>Estorno ou recibo de estorno (setor Segurador)</summary>
        [XmlEnum("RE")]
        RE,
        /// <summary>Imputacao a co-seguradoras (setor Segurador)</summary>
        [XmlEnum("CS")]
        CS,
        /// <summary>Imputacao a co-seguradora lider (setor Segurador)</summary>
        [XmlEnum("LD")]
        LD,
        /// <summary>Resseguro aceite (setor Segurador)</summary>
        [XmlEnum("RA")]
        RA
    }
} 