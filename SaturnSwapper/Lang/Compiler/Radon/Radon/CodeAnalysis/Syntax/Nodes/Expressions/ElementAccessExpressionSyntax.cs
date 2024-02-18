namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class ElementAccessExpressionSyntax : ExpressionSyntax
{
    public ExpressionSyntax Expression { get; }
    public SyntaxToken OpenBracketToken { get; }
    public ExpressionSyntax IndexExpression { get; }
    public SyntaxToken CloseBracketToken { get; }
    public override bool CanBeStatement => false;

    public ElementAccessExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression, SyntaxToken openBracketToken, 
                                   ExpressionSyntax indexExpression, SyntaxToken closeBracketToken) 
        : base(syntaxTree)
    {
        Expression = expression;
        OpenBracketToken = openBracketToken;
        IndexExpression = indexExpression;
        CloseBracketToken = closeBracketToken;
    }
}