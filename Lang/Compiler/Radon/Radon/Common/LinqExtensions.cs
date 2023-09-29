using System;
using System.Collections.Generic;

namespace Radon.Common;

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
    
    public static int IndexOf<T>(this IEnumerable<T> enumerable, T element, IEqualityComparer<T>? comparer)
    {
        comparer ??= EqualityComparer<T>.Default;
        var index = 0;
        foreach (var item in enumerable)
        {
            if (comparer.Equals(item, element))
            {
                return index;
            }
            
            index++;
        }
        
        return -1;
    }
}