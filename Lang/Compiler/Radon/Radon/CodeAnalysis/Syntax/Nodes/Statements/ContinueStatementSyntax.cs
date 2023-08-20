namespace Radon.CodeAnalysis.Syntax.Nodes.Statements;

public sealed partial class ContinueStatementSyntax : StatementSyntax
{
    public SyntaxToken ContinueKeyword { get; }
    public override SyntaxToken SemicolonToken { get; }
    
    public ContinueStatementSyntax(SyntaxTree syntaxTree, SyntaxToken continueKeyword, SyntaxToken semicolonToken)
        : base(syntaxTree)
    {
        ContinueKeyword = continueKeyword;
        SemicolonToken = semicolonToken;
    }
}