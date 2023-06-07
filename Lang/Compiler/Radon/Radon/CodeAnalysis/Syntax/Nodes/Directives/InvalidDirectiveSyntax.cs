namespace Radon.CodeAnalysis.Syntax.Nodes.Directives;

public sealed partial class InvalidDirectiveSyntax : DirectiveSyntax
{
    public SyntaxToken HashToken { get; }
    public SyntaxToken Directive { get; }

    public InvalidDirectiveSyntax(SyntaxTree syntaxTree, SyntaxToken hashToken, SyntaxToken directive) 
        : base(syntaxTree)
    {
        HashToken = hashToken;
        Directive = directive;
    }
}