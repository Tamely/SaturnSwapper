using System;
using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Binding.Binders;
using Radon.CodeAnalysis.Binding.Semantics.Operators;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Symbols;

public sealed class StructSymbol : TypeSymbol
{
    public override string Name { get; }
    internal override TypeBinder? TypeBinder { get; set; }
    public override SymbolKind Kind => SymbolKind.Struct;
    public override ImmutableArray<MemberSymbol> Members { get; private protected set; }
    public override AssemblySymbol? ParentAssembly { get; }
    public override ImmutableArray<SyntaxKind> Modifiers { get; }
    public override int Size { get; internal set; }

    internal StructSymbol(string name, ImmutableArray<MemberSymbol> members, AssemblySymbol? parentAssembly, 
                          ImmutableArray<SyntaxKind> modifiers, TypeBinder? typeBinder = null)
    {
        Name = name;
        Members = ImmutableArray<MemberSymbol>.Empty;
        ParentAssembly = parentAssembly;
        Modifiers = modifiers;
        foreach (var member in members)
        {
            AddMember(member);
        }
        
        var fields = Members.OfType<FieldSymbol>().ToArray();
        var size = 0;
        foreach (var field in fields)
        {
            size += field.Type.Size;
        }
        
        Size = size;
        TypeBinder = typeBinder;
    }
    
    internal StructSymbol(string name, int defaultSize, ImmutableArray<MemberSymbol> members, AssemblySymbol? parentAssembly, 
        ImmutableArray<SyntaxKind> modifiers)
        : this(name, members, parentAssembly, modifiers)
    {
        Size = defaultSize;
    }

    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode());
    }
    
    public override string ToString()
    {
        return Name;
    }
}
