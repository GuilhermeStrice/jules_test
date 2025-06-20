namespace SAFT.Lib.Enums
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
        CM,
        /// <summary>Credito de consignacao</summary>
        CC,
        /// <summary>Fatura de consignacao nos termos do art.38 do CIVA</summary>
        FC,
        /// <summary>Folha de obra</summary>
        FO,
        /// <summary>Nota de encomenda</summary>
        NE,
        /// <summary>Outros documentos suscetiveis de apresentacao ao cliente</summary>
        OU,
        /// <summary>Orcamento</summary>
        OR,
        /// <summary>Fatura pro-forma</summary>
        PF,
        /// <summary>Documentos emitidos ate 2017-06-30</summary>
        DC,
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