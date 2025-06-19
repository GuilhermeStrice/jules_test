using System.Collections.Generic;
using System.Xml.Serialization;
using SAFT.Lib.Files;

namespace SAFT.Lib
{
    public class MasterFiles
    {
        [XmlArray("GeneralLedgerAccounts")]
        [XmlArrayItem("Account")]
        public List<Account> GeneralLedgerAccounts { get; set; }

        [XmlElement("Customer")]
        public List<Customer> Customers { get; set; }

        [XmlElement("Supplier")]
        public List<Supplier> Suppliers { get; set; }

        [XmlElement("Product")]
        public List<Product> Products { get; set; }

        [XmlElement("TaxTable")]
        public TaxTable TaxTable { get; set; }
    }
} 