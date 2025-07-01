/*
 * Original XSD Schema:
 * <xs:simpleType name="SAFTPTMovementTaxCode">
 * <xs:restriction base="xs:string">
 * <xs:pattern value="RED|INT|NOR|ISE|OUT|NS"/>
 * </xs:restriction>
 * </xs:simpleType>
 */

using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// SAFT Portugal Movement Tax Code
    /// Valid values: RED, INT, NOR, ISE, OUT, NS
    /// </summary>
    public enum MovementTaxCode
    {
        /// <summary>Reduced rate</summary>
        [XmlEnum("RED")]
        RED,
        
        /// <summary>Intermediate rate</summary>
        [XmlEnum("INT")]
        INT,
        
        /// <summary>Normal rate</summary>
        [XmlEnum("NOR")]
        NOR,
        
        /// <summary>Exempt</summary>
        [XmlEnum("ISE")]
        ISE,
        
        /// <summary>Other</summary>
        [XmlEnum("OUT")]
        OUT,
        
        /// <summary>Not subject to tax</summary>
        [XmlEnum("NS")]
        NS
    }
} 