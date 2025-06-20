namespace SAFT.Lib.Enums
{
    /// <summary>
    /// Tipo de pagamento SAFT Portugal
    /// RC para Recibo emitido no ambito do regime de IVA de Caixa (incluindo os relativos a adiantamentos desse regime), 
    /// RG para Outros recibos emitidos
    /// </summary>
    public enum SAFTPTPaymentType
    {
        /// <summary>Recibo emitido no ambito do regime de IVA de Caixa</summary>
        RC,
        /// <summary>Outros recibos emitidos</summary>
        RG
    }
} 