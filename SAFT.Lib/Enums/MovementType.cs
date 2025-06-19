using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// GR para Guia de remessa, GT para Guia de transporte incluindo as globais, GA para Guia de movimentacao de ativos fixos proprios, GC para Guia de consignacao, GD para Guia ou nota de devolucao
    /// </summary>
    public enum MovementType
    {
        [XmlEnum("GR")]
        DeliveryNote,
        
        [XmlEnum("GT")]
        TransportGuide,
        
        [XmlEnum("GA")]
        FixedAssetMovement,
        
        [XmlEnum("GC")]
        ConsignmentGuide,
        
        [XmlEnum("GD")]
        ReturnGuide
    }
} 