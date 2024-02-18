using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundLiteralExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
    public override TypeSymbol Type { get; }
    public object Value { get; }
    public override BoundConstant ConstantValue { get; }
    
    public BoundLiteralExpression(SyntaxNode syntax, TypeSymbol type, object value)
        : base(syntax)
    {
        Value = value;
        Type = type;
        ConstantValue = new BoundConstant(value);
    }
}