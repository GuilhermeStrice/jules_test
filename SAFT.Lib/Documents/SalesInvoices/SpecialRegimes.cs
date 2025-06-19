using System.Xml.Serialization;

namespace SAFT.Lib.Documents
{
    public class SpecialRegimes
    {
        [XmlElement]
        public int SelfBillingIndicator { get; set; }

        [XmlElement]
        public int CashVATSchemeIndicator { get; set; }

        [XmlElement]
        public int ThirdPartiesBillingIndicator { get; set; }
    }
} 