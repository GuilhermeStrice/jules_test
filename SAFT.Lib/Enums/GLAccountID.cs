/*
 * Original XSD Schema:
 * <xs:simpleType name="SAFPTGLAccountID">
 * <xs:restriction base="xs:string">
 * <xs:pattern value="([^^]*)"/>
 * <xs:minLength value="2"/>
 * <xs:maxLength value="30"/>
 * </xs:restriction>
 * </xs:simpleType>
 */

using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// SAFT Portugal General Ledger Account ID
    /// Pattern: Any string not containing the caret character (^)
    /// Length: 2-30 characters
    /// </summary>
    public class GLAccountID
    {
        private string _value = string.Empty;

        public GLAccountID() { }

        public GLAccountID(string value)
        {
            Value = value;
        }

        [XmlText]
        [MinLength(2, ErrorMessage = "GLAccountID must be at least 2 characters long")]
        [MaxLength(30, ErrorMessage = "GLAccountID must be at most 30 characters long")]
        [RegularExpression(@"^[^^]*$", ErrorMessage = "GLAccountID cannot contain the caret character (^)")]
        public string Value
        {
            get => _value;
            set => _value = value ?? string.Empty;
        }

        public static implicit operator string(GLAccountID glAccountID) => glAccountID.Value;
        public static implicit operator GLAccountID(string value) => new GLAccountID(value);

        public override string ToString() => Value;
    }
} 