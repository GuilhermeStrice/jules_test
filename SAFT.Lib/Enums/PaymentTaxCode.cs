/*
 * Original XSD Schema:
 * <xs:simpleType name="PaymentTaxCode">
 * <xs:restriction base="xs:string">
 * <xs:minLength value="1"/>
 * <xs:maxLength value="10"/>
 * <xs:pattern value="RED|INT|NOR|ISE|OUT|([a-zA-Z0-9\.])*|NA"/>
 * </xs:restriction>
 * </xs:simpleType>
 */

using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace SAFT.Lib.Enums
{
    /// <summary>
    /// Payment Tax Code
    /// Valid values: RED, INT, NOR, ISE, OUT, NA, or alphanumeric with dots (1-10 chars)
    /// </summary>
    public class PaymentTaxCode
    {
        private string _value = string.Empty;

        public PaymentTaxCode() { }

        public PaymentTaxCode(string value)
        {
            Value = value;
        }

        [XmlText]
        [MinLength(1, ErrorMessage = "PaymentTaxCode must be at least 1 character long")]
        [MaxLength(10, ErrorMessage = "PaymentTaxCode must be at most 10 characters long")]
        [RegularExpression(@"^(RED|INT|NOR|ISE|OUT|NA|[a-zA-Z0-9\.]+)$", ErrorMessage = "PaymentTaxCode must match pattern: RED|INT|NOR|ISE|OUT|NA|([a-zA-Z0-9\\.])*")]
        public string Value
        {
            get => _value;
            set => _value = value ?? string.Empty;
        }

        public static implicit operator string(PaymentTaxCode paymentTaxCode) => paymentTaxCode.Value;
        public static implicit operator PaymentTaxCode(string value) => new PaymentTaxCode(value);

        public override string ToString() => Value;
    }
} 