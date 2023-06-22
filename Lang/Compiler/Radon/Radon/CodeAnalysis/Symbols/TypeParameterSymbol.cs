using System;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding;

namespace Radon.CodeAnalysis.Symbols;

public sealed class TypeParameterSymbol : TypeSymbol
{
    public override string Name { get; }
    public override SymbolKind Kind => SymbolKind.TypeParameter;
    public override int Size => -1;

    public override ImmutableArray<MemberSymbol> Members
    {
        get => ImmutableArray<MemberSymbol>.Empty;
        private protected set => throw new InvalidOperationException("Type parameter does not have members.");
    }
    
    public override AssemblySymbol ParentAssembly => throw new InvalidOperationException("Type parameter does not have a parent assembly.");
    public int Ordinal { get; }
    internal TypeMap TypeMap { get; }
    internal TypeParameterSymbol(string name, int ordinal, TypeMap typeMap)
    {
        Name = name;
        Ordinal = ordinal;
        TypeMap = typeMap;
    }
    
    public override string ToString() => Name;
}