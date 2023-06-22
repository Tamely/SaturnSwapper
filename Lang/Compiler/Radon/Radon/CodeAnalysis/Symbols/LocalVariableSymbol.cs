namespace Radon.CodeAnalysis.Symbols;

public sealed class LocalVariableSymbol : VariableSymbol
{
    public override SymbolKind Kind => SymbolKind.LocalVariable;
    public override string Name { get; }
    public override TypeSymbol Type { get; }

    internal LocalVariableSymbol(string name, TypeSymbol type)
    {
        Name = name;
        Type = type;
    }
    
    public override string ToString() => Name;
}