using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Semantics.Expressions;

internal sealed class BoundAssignmentExpression : BoundExpression
{
    public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
    public override TypeSymbol Type => Left.Type;
    public BoundExpression Left { get; }
    public BoundExpression Right { get; }
    public SyntaxKind AssignmentOperatorKind { get; }
    
    public BoundAssignmentExpression(SyntaxNode syntax, BoundExpression left, BoundExpression right, SyntaxKind assignmentOperatorKind)
        : base(syntax)
    {
        Left = left;
        Right = right;
        AssignmentOperatorKind = assignmentOperatorKind;
    }
}