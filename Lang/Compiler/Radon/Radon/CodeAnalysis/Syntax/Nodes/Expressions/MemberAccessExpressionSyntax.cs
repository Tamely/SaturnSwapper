namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class MemberAccessExpressionSyntax : ExpressionSyntax
{
    public ExpressionSyntax Expression { get; }
    public SyntaxToken DotToken { get; }
    public SyntaxToken Name { get; }
    public override bool CanBeStatement => false;
    
    public MemberAccessExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression, SyntaxToken dotToken, 
                                        SyntaxToken name) 
        : base(syntaxTree)
    {
        Expression = expression;
        DotToken = dotToken;
        Name = name;
    }
}