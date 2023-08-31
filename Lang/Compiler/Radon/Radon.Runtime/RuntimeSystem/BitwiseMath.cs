using System;

namespace Radon.Runtime.RuntimeSystem;

internal static class BitwiseMath
{
    public static byte[] BitwiseAdd(byte[] left, byte[] right)
    {
        MakeEqual(ref left, ref right);
        var result = new byte[left.Length];
        var carry = 0;
        for (var i = 0; i < left.Length; i++)
        {
            var sum = left[i] + right[i] + carry;
            result[i] = (byte)(sum & 0xFF);
            carry = sum >> 8;
        }
        
        return result;
    }

    public static byte[] BitwiseSubtract(byte[] left, byte[] right)
    {
        MakeEqual(ref left, ref right);
        var result = new byte[left.Length];
        var borrow = 0;
        for (var i = 0; i < left.Length; i++)
        {
            var diff = left[i] - right[i] - borrow;
            result[i] = (byte)(diff & 0xFF);
            borrow = diff >> 8;
        }
        
        return result;
    }

    public static byte[] BitwiseMultiply(byte[] left, byte[] right)
    {
        MakeEqual(ref left, ref right);
        var result = new byte[left.Length * 2];
        var overflow = false;
        for (var i = 0; i < left.Length; i++)
        {
            for (var j = 0; j < right.Length; j++)
            {
                var k = i + j;
                if (k >= left.Length)
                {
                    overflow = true;
                    continue;
                }
                
                var carry = result[k] + left[i] * right[j];
                result[k] = (byte)carry;
                k++;
                carry >>= 8;
                while (carry > 0)
                {
                    if (k >= left.Length)
                    {
                        overflow = true;
                        break;
                    }
                    carry += result[k];
                    result[k] = (byte)carry;
                    carry >>= 8;
                    k++;
                }
            }
        }
        
        if (overflow)
        {
            throw new OverflowException("The result of the multiplication cannot be represented in an array of the same size as the input arrays.");
        }
        
        return result;
    }

    public static byte[] BitwiseDivide(byte[] left, byte[] right)
    {
        MakeEqual(ref left, ref right);
        return BitwiseDivideAndRemainder(left, right).Quotient;
    }
    
    public static byte[] BitwiseModulo(byte[] left, byte[] right)
    {
        MakeEqual(ref left, ref right);
        return BitwiseDivideAndRemainder(left, right).Remainder;
    }
    
    public static byte[] BitwiseOr(byte[] left, byte[] right)
    {
        MakeEqual(ref left, ref right);
        var result = new byte[left.Length];
        for (var i = 0; i < left.Length; i++)
        {
            result[i] = (byte)(left[i] | right[i]);
        }
        
        return result;
    }
    
    public static byte[] BitwiseAnd(byte[] left, byte[] right)
    {
        MakeEqual(ref left, ref right);
        var result = new byte[left.Length];
        for (var i = 0; i < left.Length; i++)
        {
            result[i] = (byte)(left[i] & right[i]);
        }
        
        return result;
    }
    
    public static byte[] BitwiseXor(byte[] left, byte[] right)
    {
        MakeEqual(ref left, ref right);
        var result = new byte[left.Length];
        for (var i = 0; i < left.Length; i++)
        {
            result[i] = (byte)(left[i] ^ right[i]);
        }
        
        return result;
    }

    public static unsafe byte[] BitwiseShiftLeft(byte[] left, byte[] right)
    {
        MakeEqual(ref left, ref right);
        fixed (byte* ptr = right)
        {
            var shift = *(int*)ptr;
            return ShiftLeft(left, shift);
        }
    }
    
    public static unsafe byte[] BitwiseShiftRight(byte[] left, byte[] right)
    {
        MakeEqual(ref left, ref right);
        fixed (byte* ptr = right)
        {
            var shift = *(int*)ptr;
            return ShiftRight(left, shift);
        }
    }
    
    public static byte[] BitwiseNegate(byte[] value)
    {
        var result = new byte[value.Length];
        for (var i = 0; i < value.Length; i++)
        {
            result[i] = (byte)~value[i];
        }
        
        return result;
    }
    
    public static bool BitwiseGreaterThan(byte[] left, byte[] right)
    {
        MakeEqual(ref left, ref right);
        return Compare(left, right) > 0;
    }
    
    public static bool BitwiseGreaterThanOrEqual(byte[] left, byte[] right)
    {
        MakeEqual(ref left, ref right);
        return Compare(left, right) >= 0;
    }
    
    public static bool BitwiseLessThan(byte[] left, byte[] right)
    {
        MakeEqual(ref left, ref right);
        return Compare(left, right) < 0;
    }
    
    public static bool BitwiseLessThanOrEqual(byte[] left, byte[] right)
    {
        MakeEqual(ref left, ref right);
        return Compare(left, right) <= 0;
    }
    
    public static int Compare(byte[] left, byte[] right)
    {
        MakeEqual(ref left, ref right);
        for (var i = left.Length - 1; i >= 0; i--)
        {
            if (left[i] < right[i])
            {
                return -1;
            }
            
            if (left[i] > right[i])
            {
                return 1;
            }
        }
        
        return 0;
    }
    
    public static void MakeEqual(ref byte[] left, ref byte[] right)
    {
        if (left.Length < right.Length)
        {
            var newLeft = new byte[right.Length];
            Array.Copy(left, newLeft, left.Length);
            left = newLeft;
        }
        else if (left.Length > right.Length)
        {
            var newRight = new byte[left.Length];
            Array.Copy(right, newRight, right.Length);
            right = newRight;
        }
    }

    private static (byte[] Quotient, byte[] Remainder) BitwiseDivideAndRemainder(byte[] left, byte[] right)
    {
        var quotient = new byte[left.Length];
        var remainder = new byte[left.Length];
        Array.Copy(left, remainder, left.Length);
        
        var divisor = new byte[left.Length];
        Array.Copy(right, divisor, right.Length);
        
        var divisorShift = MostSignificantBit(left) - MostSignificantBit(right);
        if (divisorShift < 0)
        {
            return (quotient, remainder);
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
        
        return (quotient, remainder);
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

    private static byte[] ShiftLeft(byte[] bytes, int shift)
    {
        var result = new byte[bytes.Length];
        var byteShift = shift / 8;
        var bitShift = shift % 8;
        for (var i = 0; i < bytes.Length; i++)
        {
            result[i] = (byte)((bytes[i + byteShift] << bitShift) | (bytes[i + byteShift + 1] >> (8 - bitShift)));
        }
        
        result[bytes.Length - byteShift - 1] = (byte)(bytes[^1] << bitShift);
        return result;
    }
    
    private static byte[] ShiftRight(byte[] bytes, int shift)
    {
        var result = new byte[bytes.Length];
        var byteShift = shift / 8;
        var bitShift = shift % 8;
        for (var i = 0; i < bytes.Length; i++)
        {
            result[i] = (byte)((bytes[i + byteShift] >> bitShift) | (bytes[i + byteShift + 1] << (8 - bitShift)));
        }
        
        result[bytes.Length - byteShift - 1] = (byte)(bytes[^1] >> bitShift);
        return result;
    }
}