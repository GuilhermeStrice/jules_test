namespace SAFT.Lib.Enums
{
    /// <summary>
    /// Origem do documento de faturacao
    /// P para documento produzido na aplicacao, 
    /// I para documento integrado e produzido noutra aplicacao, 
    /// M para documento proveniente de recuperacao ou de emissao manual
    /// </summary>
    public enum SAFTPTSourceBilling
    {
        /// <summary>Documento produzido na aplicacao</summary>
        P,
        /// <summary>Documento integrado e produzido noutra aplicacao</summary>
        I,
        /// <summary>Documento proveniente de recuperacao ou de emissao manual</summary>
        M
    }
} 