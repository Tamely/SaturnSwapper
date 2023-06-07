using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Symbols;

public sealed class ConstructorSymbol : AbstractMethodSymbol
{
    public new static readonly ConstructorSymbol Error = new(TypeSymbol.Error, ImmutableArray<ParameterSymbol>.Empty, ImmutableArray<SyntaxKind>.Empty);
    
    public override TypeSymbol ParentType { get; }
    public override string Name => Modifiers.Contains(SyntaxKind.StaticKeyword) ? ".cctor" : ".ctor";
    public override SymbolKind Kind => SymbolKind.Constructor;
    public override ImmutableArray<TypeParameterSymbol> TypeParameters { get; }
    public override ImmutableArray<ParameterSymbol> Parameters { get; }
    public override TypeSymbol Type => ParentType;
    public override ImmutableArray<SyntaxKind> Modifiers { get; }
    
    internal ConstructorSymbol(TypeSymbol parentType, ImmutableArray<ParameterSymbol> parameters, ImmutableArray<SyntaxKind> modifiers)
    {
        ParentType = parentType;
        TypeParameters = ImmutableArray<TypeParameterSymbol>.Empty;
        Parameters = parameters;
        Modifiers = modifiers;
    }

    public override MemberSymbol WithType(TypeSymbol type)
    {
        return new ConstructorSymbol(type, Parameters, Modifiers);
    }

    public override MemberSymbol WithParentType(TypeSymbol parentType)
    {
        return new ConstructorSymbol(parentType, Parameters, Modifiers);
    }

    internal ConstructorSymbol WithTypeParameters(TypeMap map)
    {
        var newTypeParameters = ImmutableArray.CreateBuilder<TypeParameterSymbol>(TypeParameters.Length);
        var oldNewPairs = new Dictionary<TypeParameterSymbol, TypeParameterSymbol>();
        // Create new type parameters with the type map applied to them.
        // Make sure to take into account that the return type or parameter types may be type parameters themselves.
        foreach (var oldTypeParameter in TypeParameters)
        {
            var newTypeParameter = new TypeParameterSymbol(oldTypeParameter.Name, oldTypeParameter.Ordinal, map);
            newTypeParameters.Add(newTypeParameter);
            oldNewPairs.Add(oldTypeParameter, newTypeParameter);
        }
        
        var type = ParentType;
        if (type is TypeParameterSymbol typeParameter)
        {
            type = oldNewPairs[typeParameter];
        }
        
        var newParameters = ImmutableArray.CreateBuilder<ParameterSymbol>(Parameters.Length);
        foreach (var parameter in Parameters)
        {
            var newType = parameter.Type;
            if (newType is TypeParameterSymbol typeParam)
            {
                newType = oldNewPairs[typeParam];
            }
            
            newParameters.Add(new ParameterSymbol(parameter.Name, newType, parameter.Ordinal));
        }

        return new ConstructorSymbol(type, newParameters.ToImmutable(), Modifiers);
    }
    
    public override string ToString() => $"{ParentType.Name}.{Name}";
}