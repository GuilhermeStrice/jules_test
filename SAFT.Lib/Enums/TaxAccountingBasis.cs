using System.Xml.Serialization;

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
        [XmlEnum("C")]
        C,
        /// <summary>Faturacao emitida por terceiros</summary>
        [XmlEnum("E")]
        E,
        /// <summary>Faturacao</summary>
        [XmlEnum("F")]
        F,
        /// <summary>Contabilidade integrada com a faturacao</summary>
        [XmlEnum("I")]
        I,
        /// <summary>Faturacao parcial</summary>
        [XmlEnum("P")]
        P,
        /// <summary>Recibos (para dados ate 2012-12-31)</summary>
        [XmlEnum("R")]
        R,
        /// <summary>Autofaturacao</summary>
        [XmlEnum("S")]
        S,
        /// <summary>Documentos de transporte (para dados ate 2012-12-31)</summary>
        [XmlEnum("T")]
        T
    }
} 