namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class LiteralExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken LiteralToken { get; }

    internal LiteralExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken literalToken)
        : base(syntaxTree)
    {
        LiteralToken = literalToken;
    }
}