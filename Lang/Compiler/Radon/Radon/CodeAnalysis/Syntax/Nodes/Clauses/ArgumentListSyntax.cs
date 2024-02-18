using Radon.CodeAnalysis.Syntax.Nodes.Expressions;

namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses;

public sealed partial class ArgumentListSyntax : SyntaxNode
{
    public SyntaxToken OpenParenthesisToken { get; }
    public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }
    public SyntaxToken CloseParenthesisToken { get; }
    
    public ArgumentListSyntax(SyntaxTree syntaxTree, SyntaxToken openParenthesisToken, SeparatedSyntaxList<ExpressionSyntax> arguments, 
                              SyntaxToken closeParenthesisToken) 
        : base(syntaxTree)
    {
        OpenParenthesisToken = openParenthesisToken;
        Arguments = arguments;
        CloseParenthesisToken = closeParenthesisToken;
    }
}