using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Symbols;

public sealed class ConstructorSymbol : AbstractMethodSymbol
{
    public new static readonly ConstructorSymbol Error = new(TypeSymbol.Error, ImmutableArray<ParameterSymbol>.Empty, ImmutableArray<SyntaxKind>.Empty);
    
    public override TypeSymbol ParentType { get; }
    public override string Name => Modifiers.Contains(SyntaxKind.StaticKeyword) ? ".cctor" : ".ctor";
    public override SymbolKind Kind => SymbolKind.Constructor;
    public override ImmutableArray<ParameterSymbol> Parameters { get; }
    public override TypeSymbol Type => ParentType;
    public override ImmutableArray<SyntaxKind> Modifiers { get; }
    
    internal ConstructorSymbol(TypeSymbol parentType, ImmutableArray<ParameterSymbol> parameters, ImmutableArray<SyntaxKind> modifiers)
    {
        ParentType = parentType;
        Parameters = parameters;
        Modifiers = modifiers;
    }

    public override MemberSymbol WithType(TypeSymbol type)
    {
        return new ConstructorSymbol(type, Parameters, Modifiers);
    }

    public override MemberSymbol WithParentType(TypeSymbol parentType)
    {
        return new ConstructorSymbol(parentType, Parameters, Modifiers);
    }
    
    public override string ToString() => $"{ParentType.Name}.{Name}";
}