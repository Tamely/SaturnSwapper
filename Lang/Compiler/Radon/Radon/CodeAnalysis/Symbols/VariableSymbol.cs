namespace Radon.CodeAnalysis.Symbols;

public abstract class VariableSymbol : Symbol
{
    public abstract override TypeSymbol Type { get; }
    public sealed override bool HasType => true;
}