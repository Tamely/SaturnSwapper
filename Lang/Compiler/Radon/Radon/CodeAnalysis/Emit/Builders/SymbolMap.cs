using System.Collections.Generic;
using Radon.CodeAnalysis.Symbols;

namespace Radon.CodeAnalysis.Emit.Builders;

internal sealed class SymbolMap<TSymbol, T> : List<SymbolValue<TSymbol, T>>
    where TSymbol : Symbol
{
    public T this[TSymbol symbol]
    {
        get
        {
            foreach (var map in this)
            {
                if (map.Symbol == symbol)
                {
                    return map.Value;
                }
            }

            throw new KeyNotFoundException();
        }
        set
        {
            foreach (var map in this)
            {
                if (map.Symbol == symbol)
                {
                    map.Assign(value);
                    return;
                }
            }

            throw new KeyNotFoundException();
        }
    }
    
    public bool TryGetValue(TSymbol symbol, out SymbolValue<TSymbol, T> map)
    {
        foreach (var pair in this)
        {
            if (pair.Symbol == symbol)
            {
                map = pair;
                return true;
            }
        }

        map = default!;
        return false;
    }
}