/*
 * Original XSD Schema:
 * <xs:simpleType name="SAFPTTaxonomyCode">
 * <xs:restriction base="xs:integer">
 * <xs:minInclusive value="1"/>
 * <xs:maxInclusive value="999"/>
 * </xs:restriction>
 * </xs:simpleType>
 */

using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace SAFT.Lib.Enums
{
    /// <summary>
    /// SAFT Portugal Taxonomy Code
    /// Range: 1-999 (integer)
    /// </summary>
    public class TaxonomyCode
    {
        private int _value;

        public TaxonomyCode() { }

        public TaxonomyCode(int value)
        {
            Value = value;
        }

        [XmlText]
        [Range(1, 999, ErrorMessage = "TaxonomyCode must be between 1 and 999")]
        public int Value
        {
            get => _value;
            set => _value = value;
        }

        public static implicit operator int(TaxonomyCode taxonomyCode) => taxonomyCode.Value;
        public static implicit operator TaxonomyCode(int value) => new TaxonomyCode(value);

        public override string ToString() => Value.ToString();
    }
} 