namespace Radon.CodeAnalysis.Syntax.Nodes.Statements;

public sealed partial class InvalidStatementSyntax : StatementSyntax
{
    public InvalidStatementSyntax(SyntaxTree syntaxTree, SyntaxToken token)
        : base(syntaxTree)
    {
        Token = token;
    }
    
    public SyntaxToken Token { get; }
}