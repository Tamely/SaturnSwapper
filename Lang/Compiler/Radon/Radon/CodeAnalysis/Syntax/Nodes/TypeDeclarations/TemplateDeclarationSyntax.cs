using Radon.CodeAnalysis.Syntax.Nodes.Clauses;
using Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations.Bodies;

namespace Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations;

public sealed partial class TemplateDeclarationSyntax : TypeDeclarationSyntax
{
    public ImmutableSyntaxList<SyntaxToken> Modifiers { get; }
    public SyntaxToken TemplateKeyword { get; }
    public SyntaxToken Identifier { get; }
    public TypeParameterListSyntax TypeParameterList { get; }
    public StructBodySyntax Body { get; }

    public TemplateDeclarationSyntax(SyntaxTree syntaxTree, ImmutableSyntaxList<SyntaxToken> modifiers,
                                     SyntaxToken templateKeyword, SyntaxToken identifier, 
                                     TypeParameterListSyntax typeParameterList, StructBodySyntax body)
        : base(syntaxTree)
    {
        Modifiers = modifiers;
        TemplateKeyword = templateKeyword;
        Identifier = identifier;
        TypeParameterList = typeParameterList;
        Body = body;
    }
}
