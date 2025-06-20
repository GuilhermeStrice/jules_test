/*
<xs:simpleType name="SAFTPTPaymentType">
<xs:annotation>
<xs:documentation>
Restricao: RC para Recibo emitido no ambito do regime de IVA de Caixa (incluindo os relativos a adiantamentos desse regime), RG para Outros recibos emitidos
</xs:documentation>
</xs:annotation>
<xs:restriction base="xs:string">
<xs:enumeration value="RC"/>
<xs:enumeration value="RG"/>
</xs:restriction>
</xs:simpleType>
*/

using System.Xml.Serialization;

namespace SAFT.Lib.Enums
{
    /// <summary>
    /// Tipo de pagamento SAFT Portugal
    /// RC para Recibo emitido no ambito do regime de IVA de Caixa (incluindo os relativos a adiantamentos desse regime), 
    /// RG para Outros recibos emitidos
    /// </summary>
    public enum PaymentType
    {
        /// <summary>Recibo emitido no ambito do regime de IVA de Caixa</summary>
        [XmlEnum("RC")]
        RC,
        /// <summary>Outros recibos emitidos</summary>
        [XmlEnum("RG")]
        RG
    }
} 