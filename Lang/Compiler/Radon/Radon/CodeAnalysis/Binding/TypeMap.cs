using System.Collections.Generic;
using System.Linq;
using Radon.CodeAnalysis.Symbols;
using Radon.Utilities;

namespace Radon.CodeAnalysis.Binding;

internal sealed class TypeMap : Dictionary<TypeParameterSymbol, TypeSymbol?>
{
    public static TypeMap Empty => new();

    public TypeMap()
    {
    }
    
    public TypeMap(TypeMap typeMap)
    {
        foreach (var kvp in typeMap)
        {
            Add(kvp.Key, kvp.Value);
        }
    }

    public bool IsUnbound(TypeParameterSymbol typeParameter)
    {
        if (TryGetValue(typeParameter, out var type))
        {
            return type == null;
        }
        
        return true;
    }

    public bool AddBound(TypeParameterSymbol typeParameter, TypeSymbol? type)
    {
        return SafeAdd(typeParameter, type);
    }
    
    public bool AddUnbound(TypeParameterSymbol typeParameter)
    {
        return SafeAdd(typeParameter, null);
    }
    
    public TypeSymbol GetType(TypeParameterSymbol typeParameter)
    {
        if (!TryGetValue(typeParameter, out var type) ||
            type is null)
        {
            return TypeSymbol.Error;
        }

        return type;
    }
    
    private bool SafeAdd(TypeParameterSymbol key, TypeSymbol? value)
    {
        if (ContainsKey(key))
        {
            return false;
        }
        
        Add(key, value);
        return true;
    }
    
    public static TypeParameterSymbol CreateTypeParameter(string name, int ordinal, TypeMap typeMap)
    {
        return new TypeParameterSymbol(name, ordinal, typeMap);
    }
}