namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses;

public sealed partial class TypeArgumentListSyntax : SyntaxNode
{
    public SyntaxToken LessThanToken { get; }
    public SeparatedSyntaxList<TypeSyntax> Arguments { get; }
    public SyntaxToken GreaterThanToken { get; }
    
    public TypeArgumentListSyntax(SyntaxTree syntaxTree, SyntaxToken lessThanToken, SeparatedSyntaxList<TypeSyntax> arguments, 
                                    SyntaxToken greaterThanToken) 
        : base(syntaxTree)
    {
        LessThanToken = lessThanToken;
        Arguments = arguments;
        GreaterThanToken = greaterThanToken;
    }
}