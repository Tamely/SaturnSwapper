namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses;

public sealed partial class TypeParameterListSyntax : SyntaxNode
{
    public SyntaxToken LessThanToken { get; }
    public SeparatedSyntaxList<TypeParameterSyntax> Parameters { get; }
    public SyntaxToken GreaterThanToken { get; }
    
    public TypeParameterListSyntax(SyntaxTree syntaxTree, SyntaxToken lessThanToken, SeparatedSyntaxList<TypeParameterSyntax> parameters, 
        SyntaxToken greaterThanToken) 
        : base(syntaxTree)
    {
        LessThanToken = lessThanToken;
        Parameters = parameters;
        GreaterThanToken = greaterThanToken;
    }
}