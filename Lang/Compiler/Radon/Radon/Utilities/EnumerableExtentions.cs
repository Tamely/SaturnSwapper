using System.Collections.Generic;
using System.Numerics;
using Radon.CodeAnalysis.Symbols;

namespace Radon.Utilities;

public static class EnumerableExtentions
{
    public static int IndexOf<T>(this IEnumerable<T> enumerable, T element)
    {
        var index = 0;
        foreach (var item in enumerable)
        {
            if (EqualityComparer<T>.Default.Equals(item, element))
                return index;
            index++;
        }
        return -1;
    }
}