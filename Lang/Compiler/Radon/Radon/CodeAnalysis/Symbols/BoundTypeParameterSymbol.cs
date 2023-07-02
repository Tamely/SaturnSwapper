using System;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Analyzers;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Symbols;

internal sealed class BoundTypeParameterSymbol : TypeSymbol
{
    public override string Name { get; }
    public override int Size { get; internal set; } = 0;
    internal override TypeBinder? TypeBinder { get; set; } = null;
    public override SymbolKind Kind => SymbolKind.TypeParameter;
    public override ImmutableArray<MemberSymbol> Members { get; private protected set; }
    public override AssemblySymbol? ParentAssembly => null;
    public override ImmutableArray<SyntaxKind> Modifiers => ImmutableArray<SyntaxKind>.Empty;
    public int Ordinal { get; }
    public TypeSymbol BoundType { get; }
    public TypeParameterSymbol TypeParameter { get; }

    internal BoundTypeParameterSymbol(TypeParameterSymbol typeParameter, TypeSymbol boundType)
    {
        Name = typeParameter.Name;
        Ordinal = typeParameter.Ordinal;
        Members = ImmutableArray<MemberSymbol>.Empty;
        BoundType = boundType;
        TypeParameter = typeParameter;
    }

    public override string ToString()
    {
        return Name;
    }
}