namespace Radon.CodeAnalysis.Syntax.Nodes.Members;

public sealed partial class InvalidMemberDeclarationSyntax : MemberDeclarationSyntax
{
    public ImmutableSyntaxList<SyntaxToken> Modifiers { get; }
    public SyntaxToken Keyword { get; }
    
    public InvalidMemberDeclarationSyntax(SyntaxTree syntaxTree, ImmutableSyntaxList<SyntaxToken> modifiers, SyntaxToken keyword) 
        : base(syntaxTree)
    {
        Modifiers = modifiers;
        Keyword = keyword;
    }
}