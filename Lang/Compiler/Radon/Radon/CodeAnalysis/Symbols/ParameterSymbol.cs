namespace Radon.CodeAnalysis.Symbols;

public sealed class ParameterSymbol : VariableSymbol
{
    public override SymbolKind Kind => SymbolKind.Parameter;
    public override string Name { get; }
    public override TypeSymbol Type { get; }
    public int Ordinal { get; }

    internal ParameterSymbol(string name, TypeSymbol type, int ordinal)
    {
        Name = name;
        Type = type;
        Ordinal = ordinal;
    }
    
    public override string ToString() => Name;
}