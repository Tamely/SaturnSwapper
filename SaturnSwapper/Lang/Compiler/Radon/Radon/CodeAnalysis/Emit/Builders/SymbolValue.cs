using System;
using System.Collections.Generic;
using Radon.CodeAnalysis.Symbols;

namespace Radon.CodeAnalysis.Emit.Builders;

internal sealed class SymbolValue<TSymbol, T>
    where TSymbol : Symbol
{
    public TSymbol Symbol { get; }
    public T Value => List[Index];
    public int Index { get; }
    public List<T> List { get; }
    
    public SymbolValue(TSymbol symbol, T value, int index, List<T> list)
    {
        Symbol = symbol;
        Index = index;
        if (index == list.Count)
        {
            list.Add(value);
        }
        
        List = list;
    }

    public void Assign(T value)
    {
        List[Index] = value;
    }
    
    public SymbolValue<TSymbolNew, T> Cast<TSymbolNew>()
        where TSymbolNew : Symbol
    {
        return new(Symbol as TSymbolNew ?? throw new InvalidCastException(), Value, Index, List);
    }
}