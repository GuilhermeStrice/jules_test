using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// FT para Fatura, emitida nos termos do artigo 36. do Codigo do IVA, FS para Fatura simplificada, emitida nos termos do artigo 40. do Codigo do IVA, FR para Fatura-recibo, ND para Nota de debito, NC para Nota de credito, VD para Venda a dinheiro e factura/recibo (a), TV para Talao de venda (a), TD para Talao de devolucao (a), AA para Alienacao de ativos (a), DA para Devolucao de ativos (a). Para o setor Segurador, ainda pode ser preenchido com: RP para Premio ou recibo de premio, RE para Estorno ou recibo de estorno, CS para Imputacao a co-seguradoras, LD para Imputacao a co-seguradora lider, RA para Resseguro aceite. (a) Para os dados ate 2012-12-31
    /// </summary>
    public enum InvoiceType
    {
        [XmlEnum("FT")]
        Invoice,
        
        [XmlEnum("FS")]
        SimplifiedInvoice,
        
        [XmlEnum("FR")]
        InvoiceReceipt,
        
        [XmlEnum("ND")]
        DebitNote,
        
        [XmlEnum("NC")]
        CreditNote,
        
        [XmlEnum("VD")]
        CashSaleInvoice,
        
        [XmlEnum("TV")]
        SalesTicket,
        
        [XmlEnum("TD")]
        ReturnTicket,
        
        [XmlEnum("AA")]
        AssetSale,
        
        [XmlEnum("DA")]
        AssetReturn,
        
        // Insurance sector
        [XmlEnum("RP")]
        PremiumReceipt,
        
        [XmlEnum("RE")]
        ReversalReceipt,
        
        [XmlEnum("CS")]
        CoInsuranceAllocation,
        
        [XmlEnum("LD")]
        LeadCoInsuranceAllocation,
        
        [XmlEnum("RA")]
        ReinsuranceAccepted
    }
} 