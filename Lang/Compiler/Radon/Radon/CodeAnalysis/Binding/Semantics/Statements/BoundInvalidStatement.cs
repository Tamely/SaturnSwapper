using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Statements;

internal sealed class BoundInvalidStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.InvalidStatement;
    
    public BoundInvalidStatement(SyntaxNode syntax)
        : base(syntax)
    {
    }
}