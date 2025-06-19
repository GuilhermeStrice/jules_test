using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class MovementOfGoods
    {
        [XmlElement]
        public int NumberOfMovementLines { get; set; }
        [XmlElement]
        public decimal TotalQuantityIssued { get; set; }
        [XmlElement("StockMovement")]
        public List<StockMovement> StockMovements { get; set; }
    }
} 