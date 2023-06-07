namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class ImportExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken ImportKeyword { get; }
    public LiteralExpressionSyntax Path { get; }
    
    public ImportExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken importKeyword, LiteralExpressionSyntax path)
        : base(syntaxTree)
    {
        ImportKeyword = importKeyword;
        Path = path;
    }
}