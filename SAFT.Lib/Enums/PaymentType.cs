using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// RC para Recibo emitido no ambito do regime de IVA de Caixa (incluindo os relativos a adiantamentos desse regime), RG para Outros recibos emitidos
    /// </summary>
    public enum PaymentType
    {
        [XmlEnum("RC")]
        CashVATReceipt,
        
        [XmlEnum("RG")]
        OtherReceipt
    }
} 