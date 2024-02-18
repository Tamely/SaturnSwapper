namespace Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations;

public abstract class TypeDeclarationSyntax : SyntaxNode
{
    protected TypeDeclarationSyntax(SyntaxTree syntaxTree)
        : base(syntaxTree)
    {
    }
}