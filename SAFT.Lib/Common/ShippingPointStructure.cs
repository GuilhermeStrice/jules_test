using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib
{
    public class ShippingPointStructure
    {
        [XmlElement("DeliveryID")]
        public List<string> DeliveryIDs { get; set; }
        [XmlElement]
        public DateTime? DeliveryDate { get; set; }
        [XmlElement("WarehouseID")]
        public List<string> WarehouseIDs { get; set; }
        [XmlElement("LocationID")]
        public List<string> LocationIDs { get; set; }
        [XmlElement]
        public AddressStructure Address { get; set; }
    }
} 