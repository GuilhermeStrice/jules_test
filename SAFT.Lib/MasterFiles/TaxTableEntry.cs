using System;
using System.Xml.Serialization;

namespace SAFT.Lib.Files
{
    public class TaxTableEntry
    {
        [XmlElement]
        public TaxType TaxType { get; set; }
        [XmlElement]
        public string TaxCountryRegion { get; set; }
        [XmlElement]
        public string TaxCode { get; set; }
        [XmlElement]
        public string Description { get; set; }
        [XmlElement]
        public decimal TaxPercentage { get; set; }
        [XmlElement]
        public decimal TaxAmount { get; set; }
        [XmlElement]
        public string TaxExpirationDate { get; set; }
    }
} 