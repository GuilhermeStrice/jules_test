using System.Collections.Generic;
using System.Xml.Serialization;
using SAFT.Lib;

namespace SAFT.Lib.Files
{
    public class Customer
    {
        [XmlElement]
        public string CustomerID { get; set; }
        [XmlElement]
        public string AccountID { get; set; }
        [XmlElement]
        public string CustomerTaxID { get; set; }
        [XmlElement]
        public string CompanyName { get; set; }
        [XmlElement]
        public string Contact { get; set; }
        [XmlElement]
        public CustomerAddressStructure BillingAddress { get; set; }
        [XmlElement("ShipToAddress")]
        public List<CustomerAddressStructure> ShipToAddresses { get; set; }
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