/*
 * Original XSD Schema:
 * <!-- Estrutura de Regimes especiais de faturacao-->
 * <xs:complexType name="SpecialRegimes">
 * <xs:sequence>
 * <xs:element ref="SelfBillingIndicator"/>
 * <xs:element ref="CashVATSchemeIndicator"/>
 * <xs:element ref="ThirdPartiesBillingIndicator"/>
 * </xs:sequence>
 * </xs:complexType>
 * 
 * Description: Special regimes structure for invoicing.
 */

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents special regimes structure for invoicing.
    /// </summary>
    public class SpecialRegimes
    {
        /// <summary>
        /// The self-billing indicator (0 or 1).
        /// </summary>
        [XmlElement("SelfBillingIndicator")]
        public int SelfBillingIndicator { get; set; }

        /// <summary>
        /// The cash VAT scheme indicator (0 or 1).
        /// </summary>
        [XmlElement("CashVATSchemeIndicator")]
        public int CashVATSchemeIndicator { get; set; }

        /// <summary>
        /// The third parties billing indicator (0 or 1).
        /// </summary>
        [XmlElement("ThirdPartiesBillingIndicator")]
        public int ThirdPartiesBillingIndicator { get; set; }
    }
} 