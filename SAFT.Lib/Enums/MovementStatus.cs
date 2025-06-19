using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// N para normal, A para Anulado
    /// </summary>
    public enum MovementStatus
    {
        [XmlEnum("N")]
        Normal,
        
        [XmlEnum("A")]
        Cancelled
    }
} 