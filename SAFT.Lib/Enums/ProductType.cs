using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// P para Produtos, S para Servicos, O para Outros (Ex: portes debitados, adiantamentos recebidos ou alienacao de ativos), E para Impostos Especiais de Consumo (ex.:IABA, ISP, IT); I para impostos, taxas e encargos parafiscais exceto IVA e IS que deverao ser refletidos na tabela 2.5 Tabela de impostos (TaxTable)e Impostos Especiais de Consumo
    /// </summary>
    public enum ProductType
    {
        [XmlEnum("P")]
        Product,
        
        [XmlEnum("S")]
        Service,
        
        [XmlEnum("O")]
        Other,
        
        [XmlEnum("E")]
        SpecialConsumptionTax,
        
        [XmlEnum("I")]
        OtherTaxes
    }
} 