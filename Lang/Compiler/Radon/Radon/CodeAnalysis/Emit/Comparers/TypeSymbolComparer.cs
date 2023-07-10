using System.Collections.Generic;
using Radon.CodeAnalysis.Symbols;

namespace Radon.CodeAnalysis.Emit.Comparers;

internal sealed class MethodSymbolComparer : IEqualityComparer<AbstractMethodSymbol>
{
    public bool Equals(AbstractMethodSymbol? x, AbstractMethodSymbol? y)
    {
        return x == y;
    }

    public int GetHashCode(AbstractMethodSymbol obj)
    {
        return obj.GetHashCode();
    }
}
