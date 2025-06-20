/*
 * Original XSD Schema:
 * <xs:simpleType name="SAFTPTMovementTaxType">
 * <xs:restriction base="xs:string">
 * <xs:enumeration value="IVA"/>
 * <xs:enumeration value="NS"/>
 * </xs:restriction>
 * </xs:simpleType>
 */

using System.Xml.Serialization;

namespace SAFT.Lib.Enums
{
    /// <summary>
    /// SAFT Portugal Movement Tax Type
    /// Valid values: IVA, NS
    /// </summary>
    public enum MovementTaxType
    {
        /// <summary>VAT (IVA)</summary>
        [XmlEnum("IVA")]
        IVA,
        
        /// <summary>Not subject to tax</summary>
        [XmlEnum("NS")]
        NS
    }
} 