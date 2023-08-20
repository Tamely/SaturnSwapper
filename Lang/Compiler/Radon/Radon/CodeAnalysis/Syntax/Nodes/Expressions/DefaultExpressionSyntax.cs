using Radon.CodeAnalysis.Syntax.Nodes.Clauses;

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class DefaultExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken DefaultKeyword { get; }
    public SyntaxToken ColonToken { get; }
    public TypeSyntax Type { get; }
    public override bool CanBeStatement => false;

    internal DefaultExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken defaultKeyword, SyntaxToken colonToken, TypeSyntax type)
        : base(syntaxTree)
    {
        DefaultKeyword = defaultKeyword;
        ColonToken = colonToken;
        Type = type;
    }
}