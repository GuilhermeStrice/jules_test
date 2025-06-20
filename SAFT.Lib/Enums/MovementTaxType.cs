/*
 * Original XSD Schema:
 * <xs:simpleType name="SAFTPTMovementTaxType">
 * <xs:restriction base="xs:string">
 * <xs:enumeration value="IVA"/>
 * <xs:enumeration value="NS"/>
 * </xs:restriction>
 * </xs:simpleType>
 */

using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace SAFT.Lib.Enums
{
    /// <summary>
    /// SAFT Portugal Movement Tax Type
    /// Valid values: IVA, NS
    /// </summary>
    public class MovementTaxType
    {
        private string _value = string.Empty;

        public MovementTaxType() { }

        public MovementTaxType(string value)
        {
            Value = value;
        }

        [XmlText]
        [RegularExpression(@"^(IVA|NS)$", ErrorMessage = "MovementTaxType must be either 'IVA' or 'NS'")]
        public string Value
        {
            get => _value;
            set => _value = value ?? string.Empty;
        }

        public static implicit operator string(MovementTaxType movementTaxType) => movementTaxType.Value;
        public static implicit operator MovementTaxType(string value) => new MovementTaxType(value);

        public override string ToString() => Value;
    }
} 