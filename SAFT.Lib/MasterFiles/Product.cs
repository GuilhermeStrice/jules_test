using System.Xml.Serialization;

namespace SAFT.Lib.Files
{
    public class Product
    {
        [XmlElement]
        public string ProductType { get; set; }
        [XmlElement]
        public string ProductCode { get; set; }
        [XmlElement]
        public string ProductGroup { get; set; }
        [XmlElement]
        public string ProductDescription { get; set; }
        [XmlElement]
        public string ProductNumberCode { get; set; }
        [XmlElement]
        public CustomsDetails CustomsDetails { get; set; }
        // CustomsDetails omitted for brevity
    }
} 