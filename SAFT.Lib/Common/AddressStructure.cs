using System.Xml.Serialization;

namespace SAFT.Lib
{
    public class AddressStructure
    {
        [XmlElement]
        public string BuildingNumber { get; set; }
        [XmlElement]
        public string StreetName { get; set; }
        [XmlElement]
        public string AddressDetail { get; set; }
        [XmlElement]
        public string City { get; set; }
        [XmlElement]
        public string PostalCode { get; set; }
        [XmlElement]
        public string Region { get; set; }
        [XmlElement]
        public string Country { get; set; }
    }
} 