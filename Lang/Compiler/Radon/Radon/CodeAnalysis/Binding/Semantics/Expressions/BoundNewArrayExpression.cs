using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundNewArrayExpression : BoundExpression, ISymbolExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.NewArrayExpression;
    public override ArrayTypeSymbol Type { get; }
    public BoundExpression SizeExpression { get; }
    public Symbol Symbol => Type;

    public BoundNewArrayExpression(SyntaxNode syntax, ArrayTypeSymbol type, BoundExpression sizeExpression)
        : base(syntax)
    {
        Type = type;
        SizeExpression = sizeExpression;
    }
}