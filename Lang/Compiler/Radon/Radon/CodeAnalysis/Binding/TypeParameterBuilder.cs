using System.Collections.Immutable;
using Radon.CodeAnalysis.Symbols;

namespace Radon.CodeAnalysis.Binding;

internal sealed class TypeParameterBuilder
{
    private TypeMap _typeMap;

    public TypeParameterBuilder()
    {
        _typeMap = TypeMap.Empty;
    }
        
    public ImmutableArray<TypeParameterSymbol> Build()
    {
        if (_typeMap.Count == 0)
        {
            return ImmutableArray<TypeParameterSymbol>.Empty;
        }
        
        var result = _typeMap.Keys.ToImmutableArray();
        _typeMap = new TypeMap();
        return result;
    }

    public TypeParameterSymbol AddTypeParameter(string name)
    {
        var typeParameter = TypeMap.CreateTypeParameter(name, _typeMap.Count, _typeMap); 
        _typeMap.AddUnbound(typeParameter);
        return typeParameter;
    }
}