namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class NameExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken IdentifierToken { get; }
    public override bool CanBeStatement => false;
    
    public NameExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifierToken) 
        : base(syntaxTree)
    {
        IdentifierToken = identifierToken;
    }
}