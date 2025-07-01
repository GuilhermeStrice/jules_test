/*
 * Original XSD Schema:
 * <xs:element name="DocumentTotals">
 * <xs:complexType>
 * <xs:sequence>
 * <xs:element ref="TaxPayable"/>
 * <xs:element ref="NetTotal"/>
 * <xs:element ref="GrossTotal"/>
 * <xs:element name="Currency" type="Currency" minOccurs="0"/>
 * <xs:element name="Settlement" type="Settlement" minOccurs="0" maxOccurs="unbounded"/>
 * <xs:element name="Payment" type="PaymentMethod" minOccurs="0" maxOccurs="unbounded"/>
 * </xs:sequence>
 * </xs:complexType>
 * </xs:element>
 * 
 * Description: Document totals including tax, net, and gross amounts with optional currency and payment information.
 */

using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents document totals including tax, net, and gross amounts.
    /// </summary>
    public class DocumentTotals
    {
        /// <summary>
        /// The tax payable amount.
        /// </summary>
        [XmlElement("TaxPayable")]
        public decimal TaxPayable { get; set; }

        /// <summary>
        /// The net total amount.
        /// </summary>
        [XmlElement("NetTotal")]
        public decimal NetTotal { get; set; }

        /// <summary>
        /// The gross total amount.
        /// </summary>
        [XmlElement("GrossTotal")]
        public decimal GrossTotal { get; set; }

        /// <summary>
        /// Currency information (optional).
        /// </summary>
        [XmlElement("Currency")]
        public Currency? Currency { get; set; }

        /// <summary>
        /// Collection of settlements (optional, unbounded).
        /// </summary>
        [XmlElement("Settlement")]
        public List<Settlement> Settlements { get; set; } = new List<Settlement>();

        /// <summary>
        /// Collection of payment methods (optional, unbounded).
        /// </summary>
        [XmlElement("Payment")]
        public List<PaymentMethod> Payments { get; set; } = new List<PaymentMethod>();
    }
} 