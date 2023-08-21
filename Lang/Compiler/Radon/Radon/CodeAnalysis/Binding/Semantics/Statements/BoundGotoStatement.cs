using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Statements;

internal sealed class BoundGotoStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.GotoStatement;
    public BoundLabel Label { get; }
    public BoundGotoStatement(SyntaxNode syntax, BoundLabel label)
        : base(syntax)
    {
        Label = label;
    }
}