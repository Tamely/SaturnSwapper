using System;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Symbols;

public sealed class ErrorMemberSymbol : MemberSymbol
{
    public override TypeSymbol ParentType => TypeSymbol.Error;
    public override string Name => string.Empty;
    public override SymbolKind Kind => SymbolKind.Error;
    public override TypeSymbol Type => TypeSymbol.Error;
    public override ImmutableArray<SyntaxKind> Modifiers => ImmutableArray<SyntaxKind>.Empty;
    
    internal ErrorMemberSymbol()
    {
    }
    
    public override MemberSymbol WithType(TypeSymbol type)
    {
        return this;
    }

    public override MemberSymbol WithParentType(TypeSymbol parentType)
    {
        return this;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(ParentType);
        hash.Add(Type);
        return hash.ToHashCode();
    }

    public override string ToString() => Name;
}