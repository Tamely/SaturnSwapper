namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public abstract class ExpressionSyntax : SyntaxNode
{
    public abstract bool CanBeStatement { get; }
    protected ExpressionSyntax(SyntaxTree syntaxTree) 
        : base(syntaxTree)
    {
    }
}
