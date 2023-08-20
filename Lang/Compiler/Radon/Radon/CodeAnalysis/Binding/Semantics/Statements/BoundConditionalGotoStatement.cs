using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Statements;

internal sealed class BoundConditionalGotoStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.ConditionalGotoStatement;
    public BoundLabel Label { get; }
    public BoundExpression Condition { get; }
    public bool JumpIfTrue { get; }
    public BoundConditionalGotoStatement(SyntaxNode syntax, BoundLabel label, BoundExpression condition, bool jumpIfTrue)
        : base(syntax)
    {
        Label = label;
        Condition = condition;
        JumpIfTrue = jumpIfTrue;
    }
}