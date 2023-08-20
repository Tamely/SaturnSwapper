

using Radon.CodeAnalysis.Syntax.Nodes.Clauses;

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class CastExpressionSyntax : ExpressionSyntax
{
    public TypeSyntax Type { get; }
    public SyntaxToken ColonToken { get; }
    public ExpressionSyntax Expression { get; }
    public override bool CanBeStatement => false;
    
    public CastExpressionSyntax(SyntaxTree syntaxTree, TypeSyntax type, SyntaxToken colonToken, ExpressionSyntax expression) 
        : base(syntaxTree)
    {
        Type = type;
        ColonToken = colonToken;
        Expression = expression;
    }
}