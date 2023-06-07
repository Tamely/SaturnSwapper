using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundUnaryExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.UnaryExpression;
    public override TypeSymbol Type => Op.Type;
    public BoundUnaryOperator Op { get; }
    public BoundExpression Operand { get; }
    public override BoundConstant? ConstantValue { get; }
    
    public BoundUnaryExpression(SyntaxNode syntax, BoundUnaryOperator op, BoundExpression operand)
        : base(syntax)
    {
        Op = op;
        Operand = operand;
        ConstantValue = ConstantFolding.Fold(op, operand);
    }
}