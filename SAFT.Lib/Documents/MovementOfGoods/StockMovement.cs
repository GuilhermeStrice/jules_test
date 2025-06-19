using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class StockMovement
    {
        [XmlElement]
        public string DocumentNumber { get; set; }
        [XmlElement]
        public string ATCUD { get; set; }
        [XmlElement]
        public MovementDocumentStatus DocumentStatus { get; set; }
        [XmlElement]
        public string Hash { get; set; }
        [XmlElement]
        public string HashControl { get; set; }
        [XmlElement]
        public int? Period { get; set; }
        [XmlElement]
        public DateTime MovementDate { get; set; }
        [XmlElement]
        public string MovementType { get; set; }
        [XmlElement]
        public DateTime SystemEntryDate { get; set; }
        [XmlElement]
        public string TransactionID { get; set; }
        [XmlElement]
        public string CustomerID { get; set; }
        [XmlElement]
        public string SupplierID { get; set; }
        [XmlElement]
        public string SourceID { get; set; }
        [XmlElement]
        public string EACCode { get; set; }
        [XmlElement]
        public string MovementComments { get; set; }
        [XmlElement]
        public ShippingPointStructure ShipTo { get; set; }
        [XmlElement]
        public ShippingPointStructure ShipFrom { get; set; }
        [XmlElement]
        public DateTime? MovementEndTime { get; set; }
        [XmlElement]
        public DateTime MovementStartTime { get; set; }
        [XmlElement]
        public string ATDocCodeID { get; set; }
        [XmlElement("Line")]
        public List<StockMovementLine> Lines { get; set; }
        [XmlElement]
        public DocumentTotals DocumentTotals { get; set; }
    }
} 