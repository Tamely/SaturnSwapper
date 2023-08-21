namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class UnaryExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken OperatorToken { get; }
    public ExpressionSyntax Operand { get; }
    public override bool CanBeStatement => OperatorToken.Kind == SyntaxKind.PlusPlusToken ||
                                           OperatorToken.Kind == SyntaxKind.MinusMinusToken;

    public UnaryExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken operatorToken, ExpressionSyntax operand) 
        : base(syntaxTree)
    {
        OperatorToken = operatorToken;
        Operand = operand;
    }
}