/*
<!--
 Tipo de documento suscetivel de apresentacao ao cliente para conferencia de mercadorias ou de prestacao de servicos e fatura de consignacao nos termos do art. 38. do codigo do IVA 
-->
<xs:element name="WorkType">
<xs:annotation>
<xs:documentation>
Restricao: DC para documentos emitidos ate 2017-06-30, CM para consulta de mesa, CC para credito de consignacao, FC para fatura de consignacao nos termos do art.38 do CIVA, FO para folha de obra, NE para nota de encomenda, OU para outros documentos suscetiveis de apresentacao ao cliente para conferencia de mercadorias ou de prestacao de servicos que nao se encontrem aqui devidamente identificados (ou seus equivalentes), OR para orcamento, PF para fatura pro-forma. Para o setor Segurador quando para os tipos de documentos a seguir identificados tambem deva existir na tabela 4.1 - Documentos comerciais a clientes (SalesInvoices) a correspondente fatura ou documento rectificativo de fatura, ainda pode ser preenchido com RP para premio ou recibo de premio, RE para estorno ou recibo de estorno, CS para imputacao a co-seguradoras, LD para imputacao a co-seguradora lider, RA para resseguro aceite.
</xs:documentation>
</xs:annotation>
<xs:simpleType>
<xs:restriction base="xs:string">
<xs:enumeration value="CM"/>
<xs:enumeration value="CC"/>
<xs:enumeration value="FC"/>
<xs:enumeration value="FO"/>
<xs:enumeration value="NE"/>
<xs:enumeration value="OU"/>
<xs:enumeration value="OR"/>
<xs:enumeration value="PF"/>
<!-- Para para dados ate 2017-06-30-->
<xs:enumeration value="DC"/>
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
    /// Tipo de documento suscetivel de apresentacao ao cliente para conferencia de mercadorias 
    /// ou de prestacao de servicos e fatura de consignacao nos termos do art. 38. do codigo do IVA
    /// CM para consulta de mesa, CC para credito de consignacao, 
    /// FC para fatura de consignacao nos termos do art.38 do CIVA, FO para folha de obra, 
    /// NE para nota de encomenda, OU para outros documentos suscetiveis de apresentacao ao cliente 
    /// para conferencia de mercadorias ou de prestacao de servicos que nao se encontrem aqui devidamente identificados, 
    /// OR para orcamento, PF para fatura pro-forma. Para o setor Segurador quando para os tipos de documentos 
    /// a seguir identificados tambem deva existir na tabela 4.1 - Documentos comerciais a clientes (SalesInvoices) 
    /// a correspondente fatura ou documento rectificativo de fatura, ainda pode ser preenchido com 
    /// RP para premio ou recibo de premio, RE para estorno ou recibo de estorno, 
    /// CS para imputacao a co-seguradoras, LD para imputacao a co-seguradora lider, 
    /// RA para resseguro aceite.
    /// </summary>
    public enum WorkType
    {
        /// <summary>Consulta de mesa</summary>
        [XmlEnum("CM")]
        CM,
        /// <summary>Credito de consignacao</summary>
        [XmlEnum("CC")]
        CC,
        /// <summary>Fatura de consignacao nos termos do art.38 do CIVA</summary>
        [XmlEnum("FC")]
        FC,
        /// <summary>Folha de obra</summary>
        [XmlEnum("FO")]
        FO,
        /// <summary>Nota de encomenda</summary>
        [XmlEnum("NE")]
        NE,
        /// <summary>Outros documentos suscetiveis de apresentacao ao cliente</summary>
        [XmlEnum("OU")]
        OU,
        /// <summary>Orcamento</summary>
        [XmlEnum("OR")]
        OR,
        /// <summary>Fatura pro-forma</summary>
        [XmlEnum("PF")]
        PF,
        /// <summary>Documentos emitidos ate 2017-06-30</summary>
        [XmlEnum("DC")]
        DC,
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