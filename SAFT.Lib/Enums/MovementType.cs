namespace SAFT.Lib.Enums
{
    /// <summary>
    /// Tipo de documento de movimentacao de mercadorias
    /// GR para Guia de remessa, GT para Guia de transporte incluindo as globais, 
    /// GA para Guia de movimentacao de ativos fixos proprios, GC para Guia de consignacao, 
    /// GD para Guia ou nota de devolucao
    /// </summary>
    public enum MovementType
    {
        /// <summary>Guia de remessa</summary>
        GR,
        /// <summary>Guia de transporte incluindo as globais</summary>
        GT,
        /// <summary>Guia de movimentacao de ativos fixos proprios</summary>
        GA,
        /// <summary>Guia de consignacao</summary>
        GC,
        /// <summary>Guia ou nota de devolucao</summary>
        GD
    }
} 