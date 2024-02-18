using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Symbols;

public sealed class MethodSymbol : AbstractMethodSymbol
{
    public new static readonly MethodSymbol Error = new(TypeSymbol.Error, "m?", TypeSymbol.Error, ImmutableArray<ParameterSymbol>.Empty, ImmutableArray<SyntaxKind>.Empty);
    
    public override TypeSymbol ParentType { get; }
    public override string Name { get; }
    public override SymbolKind Kind => SymbolKind.Method;
    public override ImmutableArray<ParameterSymbol> Parameters { get; }
    public override TypeSymbol Type { get; }
    public override ImmutableArray<SyntaxKind> Modifiers { get; }
    
    internal MethodSymbol(TypeSymbol parentType, string name, TypeSymbol type, ImmutableArray<ParameterSymbol> parameters, 
                          ImmutableArray<SyntaxKind> modifiers)
    {
        ParentType = parentType;
        Name = name;
        Type = type;
        Parameters = parameters;
        Modifiers = modifiers;
    }
    
    public override MemberSymbol WithType(TypeSymbol type)
    {
        return new MethodSymbol(ParentType, Name, type, Parameters, Modifiers);
    }

    public override MemberSymbol WithParentType(TypeSymbol parentType)
    {
        return new MethodSymbol(parentType, Name, Type, Parameters, Modifiers);
    }

    public override string ToString() => $"{ParentType.Name}.{Name}";
}
