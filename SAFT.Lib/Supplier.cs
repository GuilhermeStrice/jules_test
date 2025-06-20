using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents a Supplier as defined in the SAF-T XSD schema.
    /// </summary>
    [XmlRoot("Supplier")]
    public class Supplier
    {
        [XmlElement("SupplierID")]
        public string SupplierID { get; set; }

        [XmlElement("AccountID")]
        public string AccountID { get; set; }

        [XmlElement("SupplierTaxID")]
        public string SupplierTaxID { get; set; }

        [XmlElement("CompanyName")]
        public string CompanyName { get; set; }

        [XmlElement("Contact")]
        public string? Contact { get; set; }

        [XmlElement("BillingAddress")]
        public AddressStructure BillingAddress { get; set; }

        [XmlElement("ShipFromAddress")]
        public List<AddressStructure>? ShipFromAddress { get; set; }

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
    /// Placeholder for AddressStructure. Replace with actual implementation.
    /// </summary>
    public class AddressStructure
    {
        // Define properties according to the XSD for AddressStructure
    }
} 