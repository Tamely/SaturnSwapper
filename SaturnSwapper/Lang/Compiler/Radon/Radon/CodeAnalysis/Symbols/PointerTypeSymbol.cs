using System;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Binders;
using Radon.CodeAnalysis.Binding.Semantics.Operators;

namespace Radon.CodeAnalysis.Symbols;

public sealed class PointerTypeSymbol : TypeSymbol
{
    public override string Name { get; }
    public override SymbolKind Kind { get; }
    public override ImmutableArray<MemberSymbol> Members { get; private protected set; }
    public override AssemblySymbol? ParentAssembly { get; }
    public TypeSymbol PointedType { get; }

    public override int Size
    {
        get => 8;
        internal set => throw new InvalidOperationException("Cannot set the size of a pointer.");
    }
    
    internal override TypeBinder? TypeBinder { get; set; }

    public PointerTypeSymbol(TypeSymbol type)
    {
        ParentAssembly = type.ParentAssembly;
        Name = type.Name + "*";
        Kind = SymbolKind.Pointer;
        Members = ImmutableArray<MemberSymbol>.Empty;
        PointedType = type;
        BoundBinaryOperator.CreateTypeOperators(this);
    }
    
    public override string ToString()
    {
        return Name;
    }
}