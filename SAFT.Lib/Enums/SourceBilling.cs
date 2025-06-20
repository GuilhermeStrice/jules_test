/*
 * Original XSD Schema:
 * <xs:simpleType name="SAFTPTSourceBilling">
 * <xs:annotation>
 * <xs:documentation>
 * P para documento produzido na aplicacao, I para documento integrado e produzido noutra aplicacao, M para documento proveniente de recuperacao ou de emissao manual
 * </xs:documentation>
 * </xs:annotation>
 * <xs:restriction base="xs:string">
 * <xs:enumeration value="P"/>
 * <xs:enumeration value="I"/>
 * <xs:enumeration value="M"/>
 * </xs:restriction>
 * </xs:simpleType>
 */

using System.Xml.Serialization;

namespace SAFT.Lib.Enums
{
    /// <summary>
    /// Source Billing - indicates the source of billing documents
    /// P: Document produced in the application
    /// I: Document integrated and produced in another application  
    /// M: Document from recovery or manual issuance
    /// </summary>
    public enum SourceBilling
    {
        /// <summary>Document produced in the application</summary>
        [XmlEnum("P")]
        Produced,
        
        /// <summary>Document integrated and produced in another application</summary>
        [XmlEnum("I")]
        Integrated,
        
        /// <summary>Document from recovery or manual issuance</summary>
        [XmlEnum("M")]
        Manual
    }
} 