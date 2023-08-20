namespace Radon.CodeAnalysis.Syntax.Nodes.Statements;

public sealed partial class BreakStatementSyntax : StatementSyntax
{
    public SyntaxToken BreakKeyword { get; }
    public override SyntaxToken SemicolonToken { get; }
    
    public BreakStatementSyntax(SyntaxTree syntaxTree, SyntaxToken breakKeyword, SyntaxToken semicolonToken)
        : base(syntaxTree)
    {
        BreakKeyword = breakKeyword;
        SemicolonToken = semicolonToken;
    }
}