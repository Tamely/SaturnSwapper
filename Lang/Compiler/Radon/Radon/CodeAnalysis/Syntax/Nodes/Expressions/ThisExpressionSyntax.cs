namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class ThisExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken ThisKeyword { get; }
    
    public ThisExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken thisKeyword) 
        : base(syntaxTree)
    {
        ThisKeyword = thisKeyword;
    }
}