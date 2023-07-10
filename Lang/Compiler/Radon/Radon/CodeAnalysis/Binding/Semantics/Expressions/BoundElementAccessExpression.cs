using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundElementAccessExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.ElementAccessExpression;
    public override TypeSymbol Type { get; }
    public BoundExpression Expression { get; }
    public BoundExpression IndexExpression { get; }

    public BoundElementAccessExpression(SyntaxNode syntax, TypeSymbol type, BoundExpression expression,
        BoundExpression indexExpression)
        : base(syntax)
    {
        Type = type;
        Expression = expression;
        IndexExpression = indexExpression;
    }
}