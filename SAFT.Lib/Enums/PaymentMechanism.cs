/*
<!-- Meio de pagamento-->
<xs:element name="PaymentMechanism">
<xs:annotation>
<xs:documentation>
Restricao:CC para Cartao credito, CD para Cartao debito, CH para Cheque bancario, CI para credito documentario internacional, CO para Cheque ou cartao oferta, CS para Compensacao de saldos em conta corrente, DE para Dinheiro eletronico, por exemplo em cartoes de fidelidade ou de pontos, LC para Letra comercial, MB para Referencias de pagamento para Multibanco, NU para Numerario, OU para Outros meios aqui nao assinalados, PR para Permuta de bens, TB para Transferencia bancaria ou debito direto autorizado, TR para titulos de compensacao extrassalarial independentemente do seu suporte, por exemplo, titulos de refeicao, educacao, etc.
</xs:documentation>
</xs:annotation>
<xs:simpleType>
<xs:restriction base="xs:string">
<xs:enumeration value="CC"/>
<xs:enumeration value="CD"/>
<xs:enumeration value="CH"/>
<xs:enumeration value="CI"/>
<xs:enumeration value="CO"/>
<xs:enumeration value="CS"/>
<xs:enumeration value="DE"/>
<xs:enumeration value="LC"/>
<xs:enumeration value="MB"/>
<xs:enumeration value="NU"/>
<xs:enumeration value="OU"/>
<xs:enumeration value="PR"/>
<xs:enumeration value="TB"/>
<xs:enumeration value="TR"/>
</xs:restriction>
</xs:simpleType>
</xs:element>
*/

using System.Xml.Serialization;

namespace SAFT.Lib.Enums
{
    /// <summary>
    /// Meio de pagamento
    /// CC para Cartao credito, CD para Cartao debito, CH para Cheque bancario, 
    /// CI para credito documentario internacional, CO para Cheque ou cartao oferta, 
    /// CS para Compensacao de saldos em conta corrente, DE para Dinheiro eletronico, 
    /// por exemplo em cartoes de fidelidade ou de pontos, LC para Letra comercial, 
    /// MB para Referencias de pagamento para Multibanco, NU para Numerario, 
    /// OU para Outros meios aqui nao assinalados, PR para Permuta de bens, 
    /// TB para Transferencia bancaria ou debito direto autorizado, 
    /// TR para titulos de compensacao extrassalarial independentemente do seu suporte, 
    /// por exemplo, titulos de refeicao, educacao, etc.
    /// </summary>
    public enum PaymentMechanism
    {
        /// <summary>Cartao credito</summary>
        [XmlEnum("CC")]
        CC,
        /// <summary>Cartao debito</summary>
        [XmlEnum("CD")]
        CD,
        /// <summary>Cheque bancario</summary>
        [XmlEnum("CH")]
        CH,
        /// <summary>Credito documentario internacional</summary>
        [XmlEnum("CI")]
        CI,
        /// <summary>Cheque ou cartao oferta</summary>
        [XmlEnum("CO")]
        CO,
        /// <summary>Compensacao de saldos em conta corrente</summary>
        [XmlEnum("CS")]
        CS,
        /// <summary>Dinheiro eletronico</summary>
        [XmlEnum("DE")]
        DE,
        /// <summary>Letra comercial</summary>
        [XmlEnum("LC")]
        LC,
        /// <summary>Referencias de pagamento para Multibanco</summary>
        [XmlEnum("MB")]
        MB,
        /// <summary>Numerario</summary>
        [XmlEnum("NU")]
        NU,
        /// <summary>Outros meios aqui nao assinalados</summary>
        [XmlEnum("OU")]
        OU,
        /// <summary>Permuta de bens</summary>
        [XmlEnum("PR")]
        PR,
        /// <summary>Transferencia bancaria ou debito direto autorizado</summary>
        [XmlEnum("TB")]
        TB,
        /// <summary>Titulos de compensacao extrassalarial</summary>
        [XmlEnum("TR")]
        TR
    }
} 