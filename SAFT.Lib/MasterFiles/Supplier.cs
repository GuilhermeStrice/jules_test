using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib.Files
{
    public class Supplier
    {
        [XmlElement]
        public string SupplierID { get; set; }
        [XmlElement]
        public string AccountID { get; set; }
        [XmlElement]
        public string SupplierTaxID { get; set; }
        [XmlElement]
        public string CompanyName { get; set; }
        [XmlElement]
        public string Contact { get; set; }
        [XmlElement]
        public AddressStructure BillingAddress { get; set; }
        [XmlElement("ShipFromAddress")]
        public List<AddressStructure> ShipFromAddresses { get; set; }
        [XmlElement]
        public string Telephone { get; set; }
        [XmlElement]
        public string Fax { get; set; }
        [XmlElement]
        public string Email { get; set; }
        [XmlElement]
        public string Website { get; set; }
        [XmlElement]
        public int SelfBillingIndicator { get; set; }
    }
} 