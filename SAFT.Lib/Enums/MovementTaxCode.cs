/*
 * Original XSD Schema:
 * <xs:simpleType name="SAFTPTMovementTaxCode">
 * <xs:restriction base="xs:string">
 * <xs:pattern value="RED|INT|NOR|ISE|OUT|NS"/>
 * </xs:restriction>
 * </xs:simpleType>
 */

using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace SAFT.Lib.Enums
{
    /// <summary>
    /// SAFT Portugal Movement Tax Code
    /// Valid values: RED, INT, NOR, ISE, OUT, NS
    /// </summary>
    public class MovementTaxCode
    {
        private string _value = string.Empty;

        public MovementTaxCode() { }

        public MovementTaxCode(string value)
        {
            Value = value;
        }

        [XmlText]
        [RegularExpression(@"^(RED|INT|NOR|ISE|OUT|NS)$", ErrorMessage = "MovementTaxCode must match pattern: RED|INT|NOR|ISE|OUT|NS")]
        public string Value
        {
            get => _value;
            set => _value = value ?? string.Empty;
        }

        public static implicit operator string(MovementTaxCode movementTaxCode) => movementTaxCode.Value;
        public static implicit operator MovementTaxCode(string value) => new MovementTaxCode(value);

        public override string ToString() => Value;
    }
} 