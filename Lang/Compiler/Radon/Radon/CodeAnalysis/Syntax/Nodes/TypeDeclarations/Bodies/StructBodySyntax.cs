using Radon.CodeAnalysis.Syntax.Nodes.Members;

namespace Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations.Bodies;

public sealed partial class StructBodySyntax : SyntaxNode
{
    public SyntaxToken OpenBraceToken { get; }
    public ImmutableSyntaxList<MemberDeclarationSyntax> Members { get; }
    public SyntaxToken CloseBraceToken { get; }

    public StructBodySyntax(SyntaxTree syntaxTree, SyntaxToken openBraceToken, 
                            ImmutableSyntaxList<MemberDeclarationSyntax> members, SyntaxToken closeBraceToken)
        : base(syntaxTree)
    {
        OpenBraceToken = openBraceToken;
        Members = members;
        CloseBraceToken = closeBraceToken;
    }
}