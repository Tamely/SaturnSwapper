using System;

namespace Radon.Common;

public static class StringExtensions
{
    public static string Encrypt(this string str, long key)
    {
        var inputBytes = StringToByteArray(str);
        var keyBytes = LongToBytes(key);
        var result = new byte[inputBytes.Length];

        for (var i = 0; i < inputBytes.Length; i++)
        {
            result[i] = (byte)(inputBytes[i] ^ keyBytes[i % keyBytes.Length]);
        }
        
        return ByteArrayToString(result);
    }
    
    public static string Decrypt(this string str, long key) => str.Encrypt(key);
    
    private static byte[] StringToByteArray(string str)
    {
        var bytes = new byte[str.Length];
        for (var i = 0; i < str.Length; i++)
        {
            bytes[i] = (byte)str[i];
        }
        
        return bytes;
    }
    
    private static string ByteArrayToString(byte[] bytes)
    {
        var chars = new char[bytes.Length];
        for (var i = 0; i < bytes.Length; i++)
        {
            chars[i] = (char)bytes[i];
        }
        
        return new string(chars);
    }

    private static unsafe byte[] LongToBytes(long l)
    {
        var bytes = stackalloc byte[sizeof(long)];
        *(long*)bytes = l;
        return new ReadOnlySpan<byte>(bytes, sizeof(long)).ToArray();
    }
}