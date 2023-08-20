namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class InvalidExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken Token { get; }
    public override bool CanBeStatement => true;
    
    public InvalidExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken token)
        : base(syntaxTree)
    {
        Token = token;
    }
}