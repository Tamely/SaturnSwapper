using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundDereferenceExpression : BoundExpression
{
    public override BoundNodeKind Kind { get; }
    public override TypeSymbol Type { get; }
    public BoundExpression Operand { get; }
    internal BoundDereferenceExpression(SyntaxNode syntax, TypeSymbol type, BoundExpression operand) 
        : base(syntax)
    {
        Kind = BoundNodeKind.DereferenceExpression;
        Operand = operand;
        Type = type;
    }
}