namespace Radon.CodeAnalysis.Syntax.Nodes.Statements;

public sealed partial class BlockStatementSyntax : StatementSyntax
{
    public SyntaxToken OpenBraceToken { get; }
    public ImmutableSyntaxList<StatementSyntax> Statements { get; }
    public SyntaxToken CloseBraceToken { get; }
    public override SyntaxToken? SemicolonToken { get; }
    
    public BlockStatementSyntax(SyntaxTree syntaxTree, SyntaxToken openBraceToken, 
                                ImmutableSyntaxList<StatementSyntax> statements, SyntaxToken closeBraceToken) 
        : base(syntaxTree)
    {
        OpenBraceToken = openBraceToken;
        Statements = statements;
        CloseBraceToken = closeBraceToken;
        SemicolonToken = null;
    }
}