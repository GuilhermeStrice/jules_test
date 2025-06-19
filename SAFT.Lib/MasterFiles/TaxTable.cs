using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib.Files
{
    public class TaxTable
    {
        [XmlElement("TaxTableEntry")]
        public List<TaxTableEntry> TaxTableEntries { get; set; }
    }
} 