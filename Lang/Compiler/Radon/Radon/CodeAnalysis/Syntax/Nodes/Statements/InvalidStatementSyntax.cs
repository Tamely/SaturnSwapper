namespace Radon.CodeAnalysis.Syntax.Nodes.Statements;

public sealed partial class InvalidStatementSyntax : StatementSyntax
{
    public SyntaxToken Token { get; }
    public override SyntaxToken SemicolonToken { get; }
    
    public InvalidStatementSyntax(SyntaxTree syntaxTree, SyntaxToken token, SyntaxToken semicolonToken)
        : base(syntaxTree)
    {
        Token = token;
        SemicolonToken = semicolonToken;
    }
}