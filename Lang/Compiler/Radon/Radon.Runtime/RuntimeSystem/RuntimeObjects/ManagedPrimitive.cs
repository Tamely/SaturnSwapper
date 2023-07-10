using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Radon.CodeAnalysis.Emit;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal abstract class ManagedPrimitive : RuntimeObject
{
    protected unsafe T GetValueAs<T>(byte[] bytes)
        where T : unmanaged
    {
        var size = sizeof(T);
        if (bytes.Length <= size)
        {
            fixed (byte* ptr = bytes)
            {
                var value = *(T*)ptr;
                return value;
            }
        }
        
        var remaining = bytes.Length - size;
        // Get the bytes that come after the first sizeof(T) bytes
        var remainingBytes = new byte[remaining];
        for (var i = 0; i < remaining; i++)
        {
            remainingBytes[i] = bytes[i + size];
        }
        
        // If any of the remaining bytes are not 0, then the value is too large to fit in T
        if (remainingBytes.Any(b => b != 0))
        {
            throw new OverflowException($"Value is too large to fit in {typeof(T).Name}");
        }
        
        fixed (byte* ptr = bytes)
        {
            var value = *(T*)ptr;
            return value;
        }
    }
    
    protected byte[] BitwiseAdd(ManagedPrimitive other)
    {
        var thisBytes = Serialize();
        var otherBytes = other.Serialize();
        MakeEqual(ref thisBytes, ref otherBytes);
        var result = new byte[thisBytes.Length];
        var carry = 0;
        for (var i = 0; i < thisBytes.Length; i++)
        {
            var sum = thisBytes[i] + otherBytes[i] + carry;
            result[i] = (byte)(sum & 0xFF);
            carry = sum >> 8;
        }
        
        return result;
    }

    protected byte[] BitwiseSubtract(ManagedPrimitive other)
    {
        var thisBytes = Serialize();
        var otherBytes = other.Serialize();
        MakeEqual(ref thisBytes, ref otherBytes);
        return BitwiseSubtract(thisBytes, otherBytes);
    }
    
    private byte[] BitwiseSubtract(byte[] value, byte[] other)
    {
        var result = new byte[value.Length];
        var borrow = 0;
        for (var i = 0; i < value.Length; i++)
        {
            var diff = value[i] - other[i] - borrow;
            result[i] = (byte)(diff & 0xFF);
            borrow = diff >> 8;
        }
        
        return result;
    }

    protected byte[] BitwiseMultiply(ManagedPrimitive other)
    {
        var thisBytes = Serialize();
        var otherBytes = other.Serialize();
        MakeEqual(ref thisBytes, ref otherBytes);
        var result = new byte[thisBytes.Length];
        for (var i = 0; i < thisBytes.Length; i++)
        {
            var carry = 0;
            for (var j = 0; j < thisBytes.Length; j++)
            {
                var product = thisBytes[i] * otherBytes[j] + result[i + j] + carry;
                result[i + j] = (byte)(product & 0xFF);
                carry = product >> 8;
            }
            
            result[i + thisBytes.Length] = (byte)carry;
        }
        
        return result;
    }
    
    protected byte[] BitwiseDivide(ManagedPrimitive other)
    {
        var thisBytes = Serialize();
        var otherBytes = other.Serialize();
        MakeEqual(ref thisBytes, ref otherBytes);
        return BitwiseDivide(thisBytes, otherBytes);
    }
    
    private byte[] BitwiseDivide(byte[] value, byte[] other) // Remainder will be used for modulus
    {
        var quotient = new byte[value.Length];
        var remainder = new byte[value.Length];
        Array.Copy(value, remainder, value.Length);
        
        var divisor = new byte[value.Length];
        Array.Copy(other, divisor, other.Length);
        
        var divisorShift = MostSignificantBit(value) - MostSignificantBit(other);
        if (divisorShift < 0)
        {
            return quotient;
        }
        
        divisor = ShiftLeft(divisor, divisorShift);
        for (var i = divisorShift; i >= 0; i--)
        {
            if (Compare(remainder, divisor) >= 0)
            {
                quotient[i / 8] |= (byte)(1 << (i % 8));
                remainder = BitwiseSubtract(remainder, divisor);
            }
            
            divisor = ShiftRight(divisor, 1);
        }
        
        return quotient;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void MakeEqual(ref byte[] bytes, ref byte[] compare)
    {
        if (bytes.Length < compare.Length)
        {
            var newArray = new byte[compare.Length];
            Array.Copy(bytes, newArray, bytes.Length);
            bytes = newArray;
        }
        else if (compare.Length < bytes.Length)
        {
            var newArray = new byte[bytes.Length];
            Array.Copy(compare, newArray, compare.Length);
            compare = newArray;
        }
    }
    
    private static int MostSignificantBit(byte[] bytes)
    {
        for (var i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] == 0)
            {
                continue;
            }
            
            for (var j = 7; j >= 0; j--)
            {
                if ((bytes[i] & (1 << j)) != 0)
                {
                    return (bytes.Length - i - 1) * 8 + j;
                }
            }
        }
        
        return -1;
    }
    
    private static int Compare(byte[] a, byte[] b)
    {
        for (var i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
            {
                return a[i] < b[i] ? -1 : 1;
            }
        }
        
        return 0;
    }
    
    private static byte[] ShiftLeft(byte[] a, int shift)
    {
        var result = new byte[a.Length];
        var byteShift = shift / 8;
        var bitShift = shift % 8;
        for (var i = 0; i < a.Length - byteShift - 1; i++)
        {
            result[i] = (byte)((a[i + byteShift] << bitShift) | (a[i + byteShift + 1] >> (8 - bitShift)));
        }
        
        result[a.Length - byteShift - 1] = (byte)(a[^1] << bitShift);
        return result;
    }
    
    private static byte[] ShiftRight(byte[] a, int shift)
    {
        var result = new byte[a.Length];
        var byteShift = shift / 8;
        var bitShift = shift % 8;
        for (var i = a.Length - 1; i > byteShift; i--)
        {
            result[i] = (byte)((a[i - byteShift] >> bitShift) | (a[i - byteShift - 1] << (8 - bitShift)));
        }
        
        result[byteShift] = (byte)(a[0] >> bitShift);
        return result;
    }
}

internal abstract class ManagedPrimitive<T> : ManagedPrimitive
    where T : unmanaged
{
    public override unsafe int Size => Type.TypeInfo.Size == -1 ? sizeof(T) : Type.TypeInfo.Size;
    public abstract override RuntimeType Type { get; }
    public abstract T PrimValue { get; protected set; }
    public sealed override object Value => PrimValue;
    
    public sealed override int ResolveSize()
    {
        return Size;
    }

    public sealed override unsafe byte[] Serialize()
    {
        var bytes = new byte[Size];
        fixed (byte* ptr = bytes)
        {
            *(T*)ptr = PrimValue;
        }
        
        return bytes;
    }

    public sealed override IRuntimeObject? ConvertTo(RuntimeType type)
    {
        var thisSize = ResolveSize();
        var size = IRuntimeObject.CreateDefault(type).ResolveSize();

        // We can fit smaller types into larger types, but not the other way around
        // There is an exception for this however; if the number can fit in the smaller type, then we can convert it
        // First, we must get our value as a byte array
        var bytes = Serialize();
        var newBytes = new byte[size];
        if (size >= thisSize)
        {
            for (var i = 0; i < thisSize; i++)
            {
                newBytes[i] = bytes[i];
            }
        }
        else
        {
            // If the size is smaller, then we must check if the number can fit in the smaller type
            // If it can, then we can convert it
            var endOfBytes = new byte[bytes.Length - newBytes.Length];
            for (var i = 0; i < endOfBytes.Length; i++)
            {
                endOfBytes[i] = bytes[i + newBytes.Length];
            }

            // If any of the bytes are not 0, then the number is too big to fit in the type
            if (endOfBytes.Any(b => b != 0))
            {
                return null;
            }
            
            for (var i = 0; i < bytes.Length; i++)
            {
                newBytes[i] = bytes[i];
            }
        }
        
        return IRuntimeObject.Deserialize(type, newBytes);
    }

    public abstract override IRuntimeObject ComputeOperation(OpCode operation, IRuntimeObject? other);
}