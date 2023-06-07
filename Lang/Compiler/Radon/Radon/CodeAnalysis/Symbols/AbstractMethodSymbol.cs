using System;
using System.Collections.Immutable;

namespace Radon.CodeAnalysis.Symbols;

public abstract class AbstractMethodSymbol : MemberSymbol
{
    public abstract ImmutableArray<TypeParameterSymbol> TypeParameters { get; }
    public abstract ImmutableArray<ParameterSymbol> Parameters { get; }
    public abstract override TypeSymbol Type { get; }
    
    public static bool operator ==(AbstractMethodSymbol? left, AbstractMethodSymbol? right)
    {
        if (left is null && right is null)
        {
            return true;
        }
        
        if (left is null || right is null)
        {
            return false;
        }
        
        return left.Equals(right);
    }
    
    public static bool operator !=(AbstractMethodSymbol? left, AbstractMethodSymbol? right)
    {
        return !(left == right);
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not AbstractMethodSymbol method)
        {
            return false;
        }
        
        if (method.Name != Name)
        {
            return false;
        }
        
        if (method.TypeParameters.Length != TypeParameters.Length)
        {
            return false;
        }
        
        if (method.Parameters.Length != Parameters.Length)
        {
            return false;
        }

        return true;
    }
    
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(TypeParameters.Length);
        hash.Add(Parameters.Length);
        return hash.ToHashCode();
    }
}