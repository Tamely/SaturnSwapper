using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Analyzers;

namespace Radon.CodeAnalysis.Symbols;

public sealed class ArrayTypeSymbol : TypeSymbol
{
    public override string Name { get; }
    public override SymbolKind Kind => SymbolKind.Array;
    public override ImmutableArray<MemberSymbol> Members { get; private protected set; } = ImmutableArray<MemberSymbol>.Empty;
    public override AssemblySymbol? ParentAssembly { get; }
    public override int Size { get; internal set; } = 0;
    internal override TypeBinder? TypeBinder { get; set; }
    public TypeSymbol ElementType { get; }

    public ArrayTypeSymbol(TypeSymbol elementType)
    {
        ParentAssembly = elementType.ParentAssembly;
        Name = elementType.Name + "[]";
        ElementType = elementType;
    }
    
    public override string ToString()
    {
        return Name;
    }
}