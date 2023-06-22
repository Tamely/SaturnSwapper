using Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations.Bodies;

namespace Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations;

public sealed partial class EnumDeclarationSyntax : TypeDeclarationSyntax
{
    public ImmutableSyntaxList<SyntaxToken> Modifiers { get; }
    public SyntaxToken EnumKeyword { get; }
    public SyntaxToken Identifier { get; }
    public EnumBodySyntax Body { get; }

    public EnumDeclarationSyntax(SyntaxTree syntaxTree, ImmutableSyntaxList<SyntaxToken> modifiers, 
                                 SyntaxToken enumKeyword, SyntaxToken identifier, EnumBodySyntax body)
        : base(syntaxTree)
    {
        Modifiers = modifiers;
        EnumKeyword = enumKeyword;
        Identifier = identifier;
        Body = body;
    }
}