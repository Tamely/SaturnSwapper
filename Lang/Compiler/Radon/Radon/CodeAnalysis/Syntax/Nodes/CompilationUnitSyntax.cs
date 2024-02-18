namespace Radon.CodeAnalysis.Syntax.Nodes;

public abstract class CompilationUnitSyntax : SyntaxNode
{
    public SyntaxToken EndOfFileToken { get; }
    protected CompilationUnitSyntax(SyntaxTree syntaxTree, SyntaxToken endOfFileToken)
        : base(syntaxTree)
    {
        EndOfFileToken = endOfFileToken;
    }
}