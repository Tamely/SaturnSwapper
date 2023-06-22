namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class BinaryExpressionSyntax : ExpressionSyntax
{
    public ExpressionSyntax Left { get; }
    public SyntaxToken OperatorToken { get; }
    public ExpressionSyntax Right { get; }
    
    public BinaryExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken operatorToken, 
                                  ExpressionSyntax right) 
        : base(syntaxTree)
    {
        Left = left;
        OperatorToken = operatorToken;
        Right = right;
    }
}