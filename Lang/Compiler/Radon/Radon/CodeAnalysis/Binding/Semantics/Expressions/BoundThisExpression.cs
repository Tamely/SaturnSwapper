using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundThisExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.ThisExpression;
    public override TypeSymbol Type { get; }
    
    public BoundThisExpression(SyntaxNode syntax, TypeSymbol type)
        : base(syntax)
    {
        Type = type;
    }
}