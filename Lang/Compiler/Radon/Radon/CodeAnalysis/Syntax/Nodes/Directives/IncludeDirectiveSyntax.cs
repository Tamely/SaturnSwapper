namespace Radon.CodeAnalysis.Syntax.Nodes.Directives;

public sealed partial class IncludeDirectiveSyntax : DirectiveSyntax
{
    public SyntaxToken HashToken { get; }
    public SyntaxToken IncludeKeyword { get; }
    public SyntaxToken Path { get; }

    internal IncludeDirectiveSyntax(SyntaxTree syntaxTree, SyntaxToken hashToken, SyntaxToken includeKeyword,
                              SyntaxToken path)
        : base(syntaxTree)
    {
        HashToken = hashToken;
        IncludeKeyword = includeKeyword;
        Path = path;
    }
}