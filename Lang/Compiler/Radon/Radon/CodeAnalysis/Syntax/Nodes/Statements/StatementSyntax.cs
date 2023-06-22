namespace Radon.CodeAnalysis.Syntax.Nodes.Statements;

public abstract class StatementSyntax : SyntaxNode
{
    protected StatementSyntax(SyntaxTree syntaxTree)
        : base(syntaxTree)
    {
    }
}

