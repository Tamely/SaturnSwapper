namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class ThisExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken ThisKeyword { get; }
    public override bool CanBeStatement => false;
    
    public ThisExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken thisKeyword) 
        : base(syntaxTree)
    {
        ThisKeyword = thisKeyword;
    }
}