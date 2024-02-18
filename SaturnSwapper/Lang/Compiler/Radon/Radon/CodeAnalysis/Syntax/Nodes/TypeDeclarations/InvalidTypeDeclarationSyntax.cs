namespace Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations;

public sealed partial class InvalidTypeDeclarationSyntax : TypeDeclarationSyntax
{
    public ImmutableSyntaxList<SyntaxToken> Modifiers { get; }
    public SyntaxToken Keyword { get; }
    
    public InvalidTypeDeclarationSyntax(SyntaxTree syntaxTree, ImmutableSyntaxList<SyntaxToken> modifiers, SyntaxToken keyword) 
        : base(syntaxTree)
    {
        Modifiers = modifiers;
        Keyword = keyword;
    }
}