using System;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding;
using Radon.CodeAnalysis.Binding.Analyzers;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Symbols;

public sealed class TypeParameterSymbol : TypeSymbol
{
    public override string Name { get; }
    public override int Size { get; internal set; } = 0;
    internal override TypeBinder? TypeBinder { get; set; } = null;
    public override SymbolKind Kind => SymbolKind.TypeParameter;
    public override ImmutableArray<MemberSymbol> Members { get; private protected set; }
    public override AssemblySymbol? ParentAssembly => null;
    public override ImmutableArray<SyntaxKind> Modifiers => ImmutableArray<SyntaxKind>.Empty;
    public int Ordinal { get; }

    internal TypeParameterSymbol(string name, int ordinal)
    {
        Name = name;
        Ordinal = ordinal;
        Members = ImmutableArray<MemberSymbol>.Empty;
    }

    public override string ToString()
    {
        return Name;
    }
}