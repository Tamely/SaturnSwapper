using Radon.CodeAnalysis.Binding.Semantics.Operators;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundBinaryExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.BinaryExpression;
    public override TypeSymbol Type => Op.Type;
    public BoundExpression Left { get; }
    public BoundBinaryOperator Op { get; }
    public BoundExpression Right { get; }
    public override BoundConstant? ConstantValue { get; }
    
    public BoundBinaryExpression(SyntaxNode syntax, BoundExpression left, BoundBinaryOperator op, BoundExpression right)
        : base(syntax)
    {
        Left = left;
        Op = op;
        Right = right;
        ConstantValue = ConstantFolding.Fold(left, op, right);
    }
}