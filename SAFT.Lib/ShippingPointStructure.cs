/*
 * Original XSD Schema:
 * <!-- Estrutura de entregas de produtos -->
 * <xs:complexType name="ShippingPointStructure">
 * <xs:sequence>
 * <xs:element ref="DeliveryID" minOccurs="0" maxOccurs="unbounded"/>
 * <xs:element ref="DeliveryDate" minOccurs="0"/>
 * <xs:sequence minOccurs="0" maxOccurs="unbounded">
 *   <xs:element ref="WarehouseID" minOccurs="0"/>
 *   <xs:element ref="LocationID" minOccurs="0"/>
 * </xs:sequence>
 * <xs:element ref="Address" minOccurs="0"/>
 * </xs:sequence>
 * </xs:complexType>
 * 
 * Description: Shipping point structure for product deliveries.
 */

using System.Collections.Generic;
using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Represents shipping point structure for product deliveries.
    /// </summary>
    public class ShippingPointStructure
    {
        /// <summary>
        /// Collection of delivery IDs (optional, unbounded).
        /// </summary>
        [XmlElement("DeliveryID")]
        public List<string> DeliveryID { get; set; } = new List<string>();

        /// <summary>
        /// The delivery date (optional).
        /// </summary>
        [XmlElement("DeliveryDate")]
        public string? DeliveryDate { get; set; }

        /// <summary>
        /// Collection of warehouse IDs (optional, unbounded).
        /// </summary>
        [XmlElement("WarehouseID")]
        public List<string> WarehouseID { get; set; } = new List<string>();

        /// <summary>
        /// Collection of location IDs (optional, unbounded).
        /// </summary>
        [XmlElement("LocationID")]
        public List<string> LocationID { get; set; } = new List<string>();

        /// <summary>
        /// The address (optional).
        /// </summary>
        [XmlElement("Address")]
        public AddressStructure? Address { get; set; }
    }
} 