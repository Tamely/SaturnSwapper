using System;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Symbols;

public sealed class TemplateMethodSymbol : AbstractMethodSymbol
{
    public override TypeSymbol ParentType { get; }
    public override string Name { get; }
    public override SymbolKind Kind => SymbolKind.TemplateMethod;
    public override ImmutableArray<ParameterSymbol> Parameters { get; }
    public override TypeSymbol Type { get; }
    public override ImmutableArray<SyntaxKind> Modifiers { get; }
    public ImmutableArray<TypeParameterSymbol> TypeParameters { get; }

    internal TemplateMethodSymbol(TypeSymbol parentType, string name, TypeSymbol type, ImmutableArray<ParameterSymbol> parameters, 
                                  ImmutableArray<SyntaxKind> modifiers, ImmutableArray<TypeParameterSymbol> typeParameters)
    {
        ParentType = parentType;
        Name = name;
        Parameters = parameters;
        Type = type;
        Modifiers = modifiers;
        TypeParameters = typeParameters;
    }
    
    public override MemberSymbol WithType(TypeSymbol type)
    {
        return new TemplateMethodSymbol(ParentType, Name, type, Parameters, Modifiers, TypeParameters);
    }

    public override MemberSymbol WithParentType(TypeSymbol parentType)
    {
        return new TemplateMethodSymbol(parentType, Name, Type, Parameters, Modifiers, TypeParameters);
    }

    public override string ToString() => $"{ParentType.Name}.{Name}";

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(ParentType);
        hash.Add(Type);
        hash.Add(Parameters);
        hash.Add(TypeParameters);
        return hash.ToHashCode();
    }
}
