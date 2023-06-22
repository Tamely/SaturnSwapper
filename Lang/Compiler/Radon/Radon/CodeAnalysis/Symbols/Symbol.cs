using System;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Symbols;

public abstract class Symbol
{
    public abstract string Name { get; }
    public abstract SymbolKind Kind { get; }
    public virtual bool HasType => false;

    public virtual TypeSymbol Type => throw new InvalidOperationException("Only symbols with types have types.");

    public virtual ImmutableArray<SyntaxKind> Modifiers => ImmutableArray<SyntaxKind>.Empty;
    
    public bool HasModifier(SyntaxKind kind)
    {
        foreach (var modifier in Modifiers)
        {
            if (modifier == kind)
            {
                return true;
            }
        }

        return false;
    }
    
    public abstract override string ToString();
}