namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class BinaryExpressionSyntax : ExpressionSyntax
{
    /*
     * 1 + 2 * 3
     * 
     * 1 // Literal expression
     * + // Operator Token
     * (2 * 3) // Binary expression
     *
     * 
     * MyMethod(1) + 2 * 3
     * 
     */
    
    public ExpressionSyntax Left { get; }
    public SyntaxToken OperatorToken { get; }
    public ExpressionSyntax Right { get; }
    public override bool CanBeStatement => false;
    
    public BinaryExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken operatorToken, 
                                  ExpressionSyntax right) 
        : base(syntaxTree)
    {
        Left = left;
        OperatorToken = operatorToken;
        Right = right;
    }
}