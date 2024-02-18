namespace Radon.CodeAnalysis.Syntax.Nodes.Statements;

public abstract class StatementSyntax : SyntaxNode
{
    public abstract SyntaxToken SemicolonToken { get; }
    protected StatementSyntax(SyntaxTree syntaxTree)
        : base(syntaxTree)
    {
    }
}
