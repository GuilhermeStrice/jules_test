using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class SpecialRegimes
    {
        [XmlElement]
        public string SelfBillingIndicator { get; set; }
        
        [XmlElement]
        public string CashVATSchemeIndicator { get; set; }
        
        [XmlElement]
        public string ThirdPartiesBillingIndicator { get; set; }
    }
} 