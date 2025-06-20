using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace SAFT.Lib
{
    /// <summary>
    /// Portuguese VAT Number - 9-digit integer between 100000000 and 999999999
    /// </summary>
    [Serializable]
    [XmlType("SAFPTPortugueseVatNumber")]
    public class PortugueseVatNumber : IEquatable<PortugueseVatNumber>
    {
        private int _value;

        public PortugueseVatNumber() { }

        public PortugueseVatNumber(int value)
        {
            Value = value;
        }

        [XmlText]
        public int Value
        {
            get => _value;
            set
            {
                if (value < 100000000 || value > 999999999)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), 
                        "Portuguese VAT Number must be between 100000000 and 999999999");
                }
                _value = value;
            }
        }

        public static implicit operator int(PortugueseVatNumber vatNumber) => vatNumber.Value;
        public static implicit operator PortugueseVatNumber(int value) => new PortugueseVatNumber(value);

        public override string ToString() => _value.ToString();

        public bool Equals(PortugueseVatNumber other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PortugueseVatNumber);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static bool operator ==(PortugueseVatNumber left, PortugueseVatNumber right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PortugueseVatNumber left, PortugueseVatNumber right)
        {
            return !Equals(left, right);
        }
    }
} 