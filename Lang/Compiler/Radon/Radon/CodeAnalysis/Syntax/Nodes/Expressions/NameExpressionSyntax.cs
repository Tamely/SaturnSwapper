namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class NameExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken IdentifierToken { get; }
    
    public NameExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken identifierToken) 
        : base(syntaxTree)
    {
        IdentifierToken = identifierToken;
    }
}