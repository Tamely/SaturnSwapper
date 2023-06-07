namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses;

public sealed partial class ParameterListSyntax : SyntaxNode
{
    public SyntaxToken OpenParenthesisToken { get; }
    public ImmutableSyntaxList<ParameterSyntax> Parameters { get; }
    public SyntaxToken CloseParenthesisToken { get; }
    
    public ParameterListSyntax(SyntaxTree syntaxTree, SyntaxToken openParenthesisToken, 
                               ImmutableSyntaxList<ParameterSyntax> parameters, SyntaxToken closeParenthesisToken) 
        : base(syntaxTree)
    {
        OpenParenthesisToken = openParenthesisToken;
        Parameters = parameters;
        CloseParenthesisToken = closeParenthesisToken;
    }
}