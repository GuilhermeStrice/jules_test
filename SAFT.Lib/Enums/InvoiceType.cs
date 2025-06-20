namespace SAFT.Lib.Enums
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
        FT,
        /// <summary>Fatura simplificada, emitida nos termos do artigo 40. do Codigo do IVA</summary>
        FS,
        /// <summary>Fatura-recibo</summary>
        FR,
        /// <summary>Nota de debito</summary>
        ND,
        /// <summary>Nota de credito</summary>
        NC,
        /// <summary>Venda a dinheiro e factura/recibo (para dados ate 2012-12-31)</summary>
        VD,
        /// <summary>Talao de venda (para dados ate 2012-12-31)</summary>
        TV,
        /// <summary>Talao de devolucao (para dados ate 2012-12-31)</summary>
        TD,
        /// <summary>Alienacao de ativos (para dados ate 2012-12-31)</summary>
        AA,
        /// <summary>Devolucao de ativos (para dados ate 2012-12-31)</summary>
        DA,
        /// <summary>Premio ou recibo de premio (setor Segurador)</summary>
        RP,
        /// <summary>Estorno ou recibo de estorno (setor Segurador)</summary>
        RE,
        /// <summary>Imputacao a co-seguradoras (setor Segurador)</summary>
        CS,
        /// <summary>Imputacao a co-seguradora lider (setor Segurador)</summary>
        LD,
        /// <summary>Resseguro aceite (setor Segurador)</summary>
        RA
    }
} 