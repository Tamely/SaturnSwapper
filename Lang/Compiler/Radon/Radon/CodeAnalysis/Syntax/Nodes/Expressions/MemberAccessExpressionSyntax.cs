namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class MemberAccessExpressionSyntax : ExpressionSyntax
{
    public ExpressionSyntax Expression { get; }
    public SyntaxToken AccessToken { get; }
    public SyntaxToken Name { get; }
    public override bool CanBeStatement => false;
    
    public MemberAccessExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression, SyntaxToken accessToken, 
                                        SyntaxToken name) 
        : base(syntaxTree)
    {
        Expression = expression;
        AccessToken = accessToken;
        Name = name;
    }
}