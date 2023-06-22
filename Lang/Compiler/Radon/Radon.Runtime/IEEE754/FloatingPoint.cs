using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Radon.Runtime.IEEE754;

// TODO: Implement the IEEE 754 standard. https://en.wikipedia.org/wiki/IEEE_754
// I know I just linked to Wikipedia, but it's a good starting point.
internal readonly struct FloatingPoint
{
    private readonly byte[] _value;
    private readonly ImmutableArray<Bit> _bits;
    public FloatingPoint(byte[] value)
    {
        // Make sure the value's length in bits is divisible by 8.
        if (value.Length * 8 % 8 != 0)
        {
            throw new ArgumentException("The value's length in bits must be divisible by 8.", nameof(value));
        }
        
        _value = value;
        _bits = Bit.FromBytes(value);
    }
    
    public Bit Sign => _bits[0];

    public ImmutableArray<Bit> Exponent
    {
        get
        {
            // Convert the 0 and 1 bits to a byte.
            var exponentBits = new Bit[_bits.Length == 32 ? 8 : 11]; // 32-bit float has 8 exponent bits,
                                                                     // 64-bit float has 11 exponent bits.
            for (var i = 1; i < 8; i++)
            {
                exponentBits[i] = _bits[i];
            }

            return exponentBits.ToImmutableArray();
        }
    }
    
    public ImmutableArray<Bit> Mantissa
    {
        get
        {
            // Convert the 0 and 1 bits to a byte.
            var mantissaBits = new Bit[_bits.Length == 32 ? 23 : 53];
            for (var i = 9; i < 32; i++)
            {
                mantissaBits[i] = _bits[i];
            }

            return mantissaBits.ToImmutableArray();
        }
    }

    public byte[] ConvertToDecimal(int decimalSize)
    {
        if (decimalSize < _value.Length)
        {
            throw new ArgumentException("The decimal size must be greater than or equal to the value's length.", nameof(decimalSize));
        }

        var decimalValue = new Bit[decimalSize * 8];
        return Array.Empty<byte>();
        // TODO: Figure out a way to convert the value to a decimal.
    }
}

internal readonly struct Bit
{
    private readonly bool _value;

    public Bit()
    {
        _value = false;
    }

    private Bit(bool value)
    {
        _value = value;
    }
    
    public static implicit operator bool(Bit value) => value._value;
    public static implicit operator byte(Bit value) => value._value ? (byte)1 : (byte)0;
    public static implicit operator sbyte(Bit value) => value._value ? (sbyte)1 : (sbyte)0;
    public static implicit operator short(Bit value) => value._value ? (short)1 : (short)0;
    public static implicit operator ushort(Bit value) => value._value ? (ushort)1 : (ushort)0;
    public static implicit operator int(Bit value) => value._value ? 1 : 0;
    public static implicit operator uint(Bit value) => value._value ? 1U : 0;
    public static implicit operator long(Bit value) => value._value ? 1L : 0;
    public static implicit operator ulong(Bit value) => value._value ? 1UL : 0;
    public static implicit operator Bit(bool value) => new(value);
    public static implicit operator Bit(int value)
    {
        return value switch
        {
            0 => new Bit(false),
            1 => new Bit(true),
            _ => throw new ArgumentException("The value must be either 0 or 1.", nameof(value))
        };
    }

    public static Bit operator &(Bit left, Bit right) => left._value & right._value;
    public static Bit operator |(Bit left, Bit right) => left._value | right._value;
    public static Bit operator ^(Bit left, Bit right) => left._value ^ right._value;
    public static Bit operator ~(Bit value) => !value._value;
    public static bool operator ==(Bit left, Bit right) => left._value == right._value;
    public static bool operator !=(Bit left, Bit right) => left._value != right._value;
    
    public static ImmutableArray<Bit> FromBytes(byte[] bytes)
    {
        var bits = new Bit[bytes.Length * 8];
        for (var i = 0; i < bytes.Length; i++)
        {
            var b = bytes[i];
            for (var j = 0; j < 8; j++)
            {
                bits[i * 8 + j] = (b & (1 << j)) != 0;
            }
        }

        return bits.ToImmutableArray();
    }

    public override bool Equals(object? obj) => obj is Bit bit && _value == bit._value;

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString()
    {
        return _value ? "1" : "0";
    }
}
