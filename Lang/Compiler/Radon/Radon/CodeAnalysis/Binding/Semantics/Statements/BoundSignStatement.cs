using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Statements;

internal sealed class BoundSignStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.SignStatement;
    public string Key { get; }
    public string Value { get; }
    
    public BoundSignStatement(SyntaxNode syntax, string key, string value)
        : base(syntax)
    {
        Key = key;
        Value = value;
    }
}