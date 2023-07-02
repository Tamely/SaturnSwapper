using Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations.Bodies;

namespace Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations;

public sealed partial class StructDeclarationSyntax : TypeDeclarationSyntax
{
    public ImmutableSyntaxList<SyntaxToken> Modifiers { get; }
    public SyntaxToken StructKeyword { get; }
    public SyntaxToken Identifier { get; }
    public StructBodySyntax Body { get; }

    public StructDeclarationSyntax(SyntaxTree syntaxTree, ImmutableSyntaxList<SyntaxToken> modifiers,
                                   SyntaxToken structKeyword, SyntaxToken identifier,  StructBodySyntax body)
        : base(syntaxTree)
    {
        Modifiers = modifiers;
        StructKeyword = structKeyword;
        Identifier = identifier;
        Body = body;
    }
}
