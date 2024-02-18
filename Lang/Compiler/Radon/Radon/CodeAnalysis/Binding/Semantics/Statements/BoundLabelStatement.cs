using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Statements;

internal sealed class BoundLabelStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.LabelStatement;
    public BoundLabel Label { get; }
    public BoundLabelStatement(SyntaxNode syntax, BoundLabel label)
        : base(syntax)
    {
        Label = label;
    }
}