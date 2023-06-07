using System;

namespace Radon.CodeAnalysis.Symbols;

public sealed class AssemblySymbol : Symbol
{
    public override string Name { get; }
    public override SymbolKind Kind => SymbolKind.Assembly;
    public Guid AssemblyId { get; }
    
    internal AssemblySymbol(string name)
    {
        Name = name;
        AssemblyId = Guid.NewGuid();
    }
    
    public override string ToString() => Name;
}