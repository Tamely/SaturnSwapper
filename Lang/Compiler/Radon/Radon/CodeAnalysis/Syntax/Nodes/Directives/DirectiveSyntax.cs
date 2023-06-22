namespace Radon.CodeAnalysis.Syntax.Nodes.Directives;

public abstract class DirectiveSyntax : SyntaxNode
{
    protected DirectiveSyntax(SyntaxTree syntaxTree)
        : base(syntaxTree)
    {
    }
}