using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Statements;

internal sealed class BoundExpressionStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;
    public BoundExpression Expression { get; }
    
    public BoundExpressionStatement(SyntaxNode syntax, BoundExpression expression)
        : base(syntax)
    {
        Expression = expression;
    }
}