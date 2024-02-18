using System.Collections.Generic;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Symbols;

namespace Radon.CodeAnalysis.Binding;

internal sealed class TypeParameterBuilder
{
    private List<TypeParameterSymbol> _typeParameters;
    public TypeParameterBuilder()
    {
        _typeParameters = new List<TypeParameterSymbol>();
    }

    public ImmutableArray<TypeParameterSymbol> Build()
    {
        if (_typeParameters.Count == 0)
        {
            return ImmutableArray<TypeParameterSymbol>.Empty;
        }

        var typeParameters = _typeParameters.ToImmutableArray();
        _typeParameters.Clear();
        return typeParameters;
    }

    public TypeParameterSymbol AddTypeParameter(string name)
    {
        var ordinal = _typeParameters.Count;
        var typeParameter = new TypeParameterSymbol(name, ordinal);
        _typeParameters.Add(typeParameter);
        return typeParameter;
    }
}
