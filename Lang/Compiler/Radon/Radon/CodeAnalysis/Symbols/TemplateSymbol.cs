using System;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Binders;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Symbols;

public sealed class TemplateSymbol : TypeSymbol
{
    public override string Name { get; }

    public override int Size
    {
        get => throw new InvalidOperationException("Templates have no size.");
        internal set => throw new InvalidOperationException("Templates have no size.");
    }
    internal override TypeBinder? TypeBinder { get; set; }
    public override SymbolKind Kind => SymbolKind.Template;
    public override ImmutableArray<MemberSymbol> Members { get; private protected set; }
    public override AssemblySymbol? ParentAssembly { get; }
    public override ImmutableArray<SyntaxKind> Modifiers { get; }
    public ImmutableArray<TypeParameterSymbol> TypeParameters { get; }

    internal TemplateSymbol(string name, ImmutableArray<MemberSymbol> members, AssemblySymbol? parentAssembly,
                            ImmutableArray<SyntaxKind> modifiers, ImmutableArray<TypeParameterSymbol> typeParameters, TypeBinder? typeBinder = null)
    {
        Name = name;
        Members = ImmutableArray<MemberSymbol>.Empty;
        ParentAssembly = parentAssembly;
        Modifiers = modifiers;
        TypeParameters = typeParameters;
        foreach (var member in members)
        {
            AddMember(member);
        }
        
        TypeBinder = typeBinder;
    }


    public override string ToString()
    {
        return Name;
    }
}