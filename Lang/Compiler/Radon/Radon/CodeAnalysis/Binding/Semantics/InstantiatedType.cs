using System.Collections.Immutable;
using Radon.CodeAnalysis.Symbols;

namespace Radon.CodeAnalysis.Binding.Semantics;

internal sealed class InstantiatedType
{
    public TypeSymbol Type { get; }
    public ImmutableArray<TypeSymbol> TypeArguments { get; }

    public InstantiatedType(TypeSymbol type, ImmutableArray<TypeSymbol> typeArguments)
    {
        Type = type;
        TypeArguments = typeArguments;
    }
    
    public static bool operator ==(InstantiatedType left, InstantiatedType right)
    {
        return left.Equals(right);
    }
    
    public static bool operator !=(InstantiatedType left, InstantiatedType right)
    {
        return !left.Equals(right);
    }
    
    /*public static bool operator ==(InstantiatedType left, TypeSymbol right)
    {
        return left.Type == right;
    }
    
    public static bool operator !=(InstantiatedType left, TypeSymbol right)
    {
        return left.Type != right;
    }*/
    
    public override bool Equals(object? obj)
    {
        if (obj is not InstantiatedType other)
        {
            return false;
        }
        
        return Type == other.Type && TypeArguments.Length == other.TypeArguments.Length;
    }
}