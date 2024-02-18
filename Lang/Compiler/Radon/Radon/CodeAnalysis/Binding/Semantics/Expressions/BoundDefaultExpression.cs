using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundDefaultExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.DefaultExpression;
    public override TypeSymbol Type { get; }
    
    public BoundDefaultExpression(SyntaxNode syntax, TypeSymbol type)
        : base(syntax)
    {
        Type = type;
    }
}