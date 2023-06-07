namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class UnaryExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken OperatorToken { get; }
    public ExpressionSyntax Operand { get; }
    
    public UnaryExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken operatorToken, ExpressionSyntax operand) 
        : base(syntaxTree)
    {
        OperatorToken = operatorToken;
        Operand = operand;
    }
}