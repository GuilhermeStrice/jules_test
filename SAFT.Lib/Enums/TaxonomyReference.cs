namespace SAFT.Lib.Enums
{
    /// <summary>
    /// Categoria da conta
    /// S para SNC base (Taxonomia S), M para SNC microentidades (Taxonomia M), 
    /// N para Normas Internacionais de Contabilidade (Taxonomia S), 
    /// O para outros referenciais contabilisticos cuja taxonomia nao se encontra codificada
    /// </summary>
    public enum TaxonomyReference
    {
        /// <summary>SNC base (Taxonomia S)</summary>
        S,
        /// <summary>SNC microentidades (Taxonomia M)</summary>
        M,
        /// <summary>Normas Internacionais de Contabilidade (Taxonomia S)</summary>
        N,
        /// <summary>Outros referenciais contabilisticos cuja taxonomia nao se encontra codificada</summary>
        O
    }
} 