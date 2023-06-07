using System;
using System.Collections.Generic;

namespace Radon.Runtime.Utilities;

public static class LinqExtensions
{
    // IndexOf method for IEnumerable<T>. i.e IEnumerable<T>.IndexOf(x => x.Property == value)
    public static int IndexOf<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var index = 0;
        foreach (var item in source)
        {
            if (predicate(item))
            {
                return index;
            }
            
            index++;
        }
        
        return -1;
    }
}

public static class ObjectExtensions
{
    public static bool IsNumber(this object value)
    {
        return value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal;
    }
}
