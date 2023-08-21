using System.Text;

namespace Radon.Common;

public static class StringExtensions
{
    public static string Encrypt(this string text, long key)
    {
        var array = new byte[text.Length];
        for (var i = 0; i < text.Length; i++)
        {
            array[i] = (byte)(text[i] ^ key);
        }
        
        return Encoding.UTF8.GetString(array);
    }
    
    public static string Decrypt(this string text, long key) => Encrypt(text, key);
}