namespace Radon.CodeAnalysis.Syntax.Nodes.Members;

public abstract class MemberDeclarationSyntax : SyntaxNode
{
    protected MemberDeclarationSyntax(SyntaxTree syntaxTree)
        : base(syntaxTree)
    {
    }
}