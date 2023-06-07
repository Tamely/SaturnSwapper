using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Symbols;

public sealed class MethodSymbol : AbstractMethodSymbol
{
    public override TypeSymbol ParentType { get; }
    public override string Name { get; }
    public override SymbolKind Kind => SymbolKind.Method;
    public override ImmutableArray<TypeParameterSymbol> TypeParameters { get; }
    public override ImmutableArray<ParameterSymbol> Parameters { get; }
    public override TypeSymbol Type { get; }
    public override ImmutableArray<SyntaxKind> Modifiers { get; }
    
    internal MethodSymbol(TypeSymbol parentType, string name, TypeSymbol type, ImmutableArray<TypeParameterSymbol> typeParameters, 
        ImmutableArray<ParameterSymbol> parameters, ImmutableArray<SyntaxKind> modifiers)
    {
        ParentType = parentType;
        Name = name;
        TypeParameters = typeParameters;
        Parameters = parameters;
        Type = type;
        Modifiers = modifiers;
    }
    
    public override MemberSymbol WithType(TypeSymbol type)
    {
        return new MethodSymbol(ParentType, Name, type, TypeParameters, Parameters, Modifiers);
    }

    public override MemberSymbol WithParentType(TypeSymbol parentType)
    {
        return new MethodSymbol(parentType, Name, Type, TypeParameters, Parameters, Modifiers);
    }
    
    internal MethodSymbol WithTypeParameters(TypeMap map)
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

        var parentType = (StructSymbol)ParentType;
        var parentTypeParameters = parentType.TypeParameters;
        foreach (var parentTypeParameter in parentTypeParameters)
        {
            newTypeParameters.Add(parentTypeParameter);
            oldNewPairs.Add(parentTypeParameter, parentTypeParameter);
        }
        
        var type = Type;
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

        return new MethodSymbol(ParentType, Name, type, newTypeParameters.ToImmutable(), 
            newParameters.ToImmutable(), Modifiers);
    }

    public override string ToString() => $"{ParentType.Name}.{Name}";
}