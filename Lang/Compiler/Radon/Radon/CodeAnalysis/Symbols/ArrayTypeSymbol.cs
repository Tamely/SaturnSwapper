using System;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Binders;
using Radon.CodeAnalysis.Binding.Semantics.Operators;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Symbols;

public sealed class ArrayTypeSymbol : TypeSymbol
{
    public override string Name { get; }
    public override SymbolKind Kind => SymbolKind.Array;
    public override ImmutableArray<MemberSymbol> Members { get; private protected set; }
    public override AssemblySymbol? ParentAssembly { get; }
    public override int Size
    {
        get => 8;
        internal set => throw new InvalidOperationException("Cannot set the size of an array.");
    }
    internal override TypeBinder? TypeBinder { get; set; }
    public override ImmutableArray<SyntaxKind> Modifiers { get; }
    public TypeSymbol ElementType { get; }

    public ArrayTypeSymbol(TypeSymbol elementType)
    {
        ParentAssembly = elementType.ParentAssembly;
        Name = elementType.Name + "[]";
        ElementType = elementType;
        var lengthMethod = new MethodSymbol(this, "Length", Int, 
            ImmutableArray<ParameterSymbol>.Empty, ImmutableArray.Create(SyntaxKind.PublicKeyword));
        Members = ImmutableArray.Create<MemberSymbol>(lengthMethod);
        Modifiers = ImmutableArray.Create(SyntaxKind.PublicKeyword, SyntaxKind.RefKeyword);
        BoundBinaryOperator.CreateTypeOperators(this);
    }
    
    public override string ToString()
    {
        return Name;
    }
}