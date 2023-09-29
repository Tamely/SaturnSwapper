using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundAddressOfExpression : BoundExpression
{
    public override BoundNodeKind Kind { get; }
    public override TypeSymbol Type { get; }
    public PointerTypeSymbol Pointer { get; }
    public BoundExpression Operand { get; }
    internal BoundAddressOfExpression(SyntaxNode syntax, PointerTypeSymbol pointer, BoundExpression operand) 
        : base(syntax)
    {
        Kind = BoundNodeKind.AddressOfExpression;
        Pointer = pointer;
        Operand = operand;
        Type = pointer;
    }
}