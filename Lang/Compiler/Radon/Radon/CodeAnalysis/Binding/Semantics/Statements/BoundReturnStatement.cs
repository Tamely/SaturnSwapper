using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Statements;

internal sealed class BoundReturnStatement : BoundStatement
{
    public override BoundNodeKind Kind => BoundNodeKind.ReturnStatement;
    public BoundExpression? Expression { get; }
    public BoundReturnStatement(SyntaxNode syntax, BoundExpression? expression) 
        : base(syntax)
    {
        Expression = expression;
    }
}