using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundConversionExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.ConversionExpression;
    public override TypeSymbol Type { get; }
    public BoundExpression Expression { get; }
    
    public BoundConversionExpression(SyntaxNode syntax, TypeSymbol type, BoundExpression expression)
        : base(syntax)
    {
        Type = type;
        Expression = expression;
    }
}