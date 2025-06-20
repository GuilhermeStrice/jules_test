namespace SAFT.Lib.Enums
{
    /// <summary>
    /// Tipo de sistema que exportou o SAFT
    /// C para Contabilidade, E para Faturacao emitida por terceiros, F para Faturacao, 
    /// I para Contabilidade integrada com a faturacao, P para Faturacao parcial, 
    /// R para Recibos (a), S para Autofaturacao, T para Documentos de transporte (a). 
    /// (a) Deve ser indicado este tipo, se o programa apenas este emitir este tipo de documento. 
    /// Caso contrario, devera ser utilizado o tipo C, F ou I
    /// </summary>
    public enum TaxAccountingBasis
    {
        /// <summary>Contabilidade</summary>
        C,
        /// <summary>Faturacao emitida por terceiros</summary>
        E,
        /// <summary>Faturacao</summary>
        F,
        /// <summary>Contabilidade integrada com a faturacao</summary>
        I,
        /// <summary>Faturacao parcial</summary>
        P,
        /// <summary>Recibos (para dados ate 2012-12-31)</summary>
        R,
        /// <summary>Autofaturacao</summary>
        S,
        /// <summary>Documentos de transporte (para dados ate 2012-12-31)</summary>
        T
    }
} 