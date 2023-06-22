using Radon.CodeAnalysis.Symbols;

namespace Radon.CodeAnalysis.Binding.Analyzers;

internal readonly record struct SymbolHash
{
    public readonly string Name;
    public readonly long Hash;
    private SymbolHash(string name, long hash)
    {
        Name = name;
        Hash = hash;
    }
    
    public static SymbolHash Create(string name)
    {
        return new SymbolHash(name, name.GetHashCode());
    }

    public static SymbolHash CreateMethod(AbstractMethodSymbol method)
    {
        var name = method.Name;
        var parameters = method.Parameters;

        long hash = 17;
        hash = hash * 31 + name.GetHashCode();
        foreach (var parameter in parameters)
        {
            hash = hash * 31 + parameter.Type.GetHashCode();
        }
        
        return new SymbolHash(name, hash);
    }
}