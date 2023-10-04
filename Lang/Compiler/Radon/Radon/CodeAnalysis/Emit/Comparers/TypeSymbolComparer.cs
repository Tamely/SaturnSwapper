using System.Collections.Generic;
using Radon.CodeAnalysis.Symbols;

namespace Radon.CodeAnalysis.Emit.Comparers;

internal sealed class TypeSymbolComparer : IEqualityComparer<TypeSymbol>
{
    public bool Equals(TypeSymbol? x, TypeSymbol? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }
        
        if (ReferenceEquals(x, null) || ReferenceEquals(y, null) || x.GetType() != y.GetType())
        {
            return false;
        }

        return x == y;
    }

    public int GetHashCode(TypeSymbol obj)
    {
        return obj.GetHashCode();
    }
}