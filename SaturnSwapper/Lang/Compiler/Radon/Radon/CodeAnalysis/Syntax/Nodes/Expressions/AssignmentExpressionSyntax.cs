namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class AssignmentExpressionSyntax : ExpressionSyntax
{
    public ExpressionSyntax Left { get; }
    public SyntaxToken OperatorToken { get; }
    public ExpressionSyntax Right { get; }
    public override bool CanBeStatement => true;

    public AssignmentExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax left, SyntaxToken operatorToken, 
                                      ExpressionSyntax right) 
        : base(syntaxTree)
    {
        Left = left;
        OperatorToken = operatorToken;
        Right = right;
    }
}