namespace Radon.CodeAnalysis.Binding.Semantics.Statements;

internal sealed class BoundLabel
{
    public string Name { get; }
    public BoundLabel(string name)
    {
        Name = name;
    }
    
    public override string ToString() => Name;
}