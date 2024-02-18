using Radon.CodeAnalysis.Syntax.Nodes.Members;

namespace Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations.Bodies;

public sealed partial class EnumBodySyntax : SyntaxNode
{
    public SyntaxToken OpenBraceToken { get; }
    public SeparatedSyntaxList<EnumMemberDeclarationSyntax> Members { get; }
    public SyntaxToken CloseBraceToken { get; }

    public EnumBodySyntax(SyntaxTree syntaxTree, SyntaxToken openBraceToken, 
                          SeparatedSyntaxList<EnumMemberDeclarationSyntax> members, SyntaxToken closeBraceToken)
        : base(syntaxTree)
    {
        OpenBraceToken = openBraceToken;
        Members = members;
        CloseBraceToken = closeBraceToken;
    }
}