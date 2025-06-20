/*
 * Original XSD Schema:
 * <!-- Estrutura de pagamentos-->
 * <xs:complexType name="PaymentMethod">
 * <xs:sequence>
 * <xs:element ref="PaymentMechanism" minOccurs="0"/>
 * <xs:element name="PaymentAmount" type="SAFmonetaryType"/>
 * <xs:element name="PaymentDate" type="SAFdateType"/>
 * </xs:sequence>
 * </xs:complexType>
 * 
 * Description: Payment structure for documents and payments.
 */

using System.Xml.Serialization;
using SAFT.Lib.Enums;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents payment structure for documents and payments.
    /// </summary>
    public class PaymentMethod
    {
        /// <summary>
        /// The payment mechanism (optional).
        /// </summary>
        [XmlElement("PaymentMechanism")]
        public PaymentMechanism? PaymentMechanism { get; set; }

        /// <summary>
        /// The payment amount.
        /// </summary>
        [XmlElement("PaymentAmount")]
        public decimal PaymentAmount { get; set; }

        /// <summary>
        /// The payment date.
        /// </summary>
        [XmlElement("PaymentDate")]
        public string PaymentDate { get; set; } = string.Empty;
    }
} 