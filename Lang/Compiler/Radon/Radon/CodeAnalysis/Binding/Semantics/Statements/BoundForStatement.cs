using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Statements;

internal sealed class BoundForStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.ForStatement;
    public BoundStatement Initializer { get; }
    public BoundExpression Condition { get; }
    public BoundExpression Action { get; }
    public BoundStatement Body { get; }
    public BoundLabel BreakLabel { get; }
    public BoundLabel ContinueLabel { get; }
    
    public BoundForStatement(SyntaxNode syntax, BoundStatement initializer, BoundExpression condition, 
        BoundExpression action, BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel)
        : base(syntax)
    {
        Initializer = initializer;
        Condition = condition;
        Action = action;
        Body = body;
        BreakLabel = breakLabel;
        ContinueLabel = continueLabel;
    }
}