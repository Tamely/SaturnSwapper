using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Statements;

internal sealed class BoundWhileStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;
    public BoundExpression Condition { get; }
    public BoundStatement Body { get; }
    public BoundLabel BreakLabel { get; }
    public BoundLabel ContinueLabel { get; }
    
    public BoundWhileStatement(SyntaxNode syntax, BoundExpression condition, BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel)
        : base(syntax)
    {
        Condition = condition;
        Body = body;
        BreakLabel = breakLabel;
        ContinueLabel = continueLabel;
    }
}