using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents a Customer as defined in the SAF-T XSD schema.
    /// </summary>
    [XmlRoot("Customer")]
    public class Customer
    {
        [XmlElement("CustomerID")]
        public string CustomerID { get; set; }

        [XmlElement("AccountID")]
        public string AccountID { get; set; }

        [XmlElement("CustomerTaxID")]
        public string CustomerTaxID { get; set; }

        [XmlElement("CompanyName")]
        public string CompanyName { get; set; }

        [XmlElement("Contact")]
        public string? Contact { get; set; }

        [XmlElement("BillingAddress")]
        public CustomerAddressStructure BillingAddress { get; set; }

        [XmlElement("ShipToAddress")]
        public List<CustomerAddressStructure>? ShipToAddress { get; set; }

        [XmlElement("Telephone")]
        public string? Telephone { get; set; }

        [XmlElement("Fax")]
        public string? Fax { get; set; }

        [XmlElement("Email")]
        public string? Email { get; set; }

        [XmlElement("Website")]
        public string? Website { get; set; }

        [XmlElement("SelfBillingIndicator")]
        public int SelfBillingIndicator { get; set; }
    }

    /// <summary>
    /// Placeholder for CustomerAddressStructure. Replace with actual implementation.
    /// </summary>
    public class CustomerAddressStructure
    {
        // Define properties according to the XSD for CustomerAddressStructure
    }
} 