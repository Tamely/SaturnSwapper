using Radon.CodeAnalysis.Syntax.Nodes.Clauses;

namespace Radon.CodeAnalysis.Syntax.Nodes.Members;

public sealed partial class FieldDeclarationSyntax : MemberDeclarationSyntax
{
    public ImmutableSyntaxList<SyntaxToken> Modifiers { get; }
    public TypeSyntax Type { get; }
    public VariableDeclaratorSyntax VariableDeclarator { get; }
    public SyntaxToken SemicolonToken { get; }
    
    public FieldDeclarationSyntax(SyntaxTree syntaxTree, ImmutableSyntaxList<SyntaxToken> modifiers, TypeSyntax type, 
        VariableDeclaratorSyntax variableDeclarator, SyntaxToken semicolonToken) 
        : base(syntaxTree)
    {
        Modifiers = modifiers;
        Type = type;
        VariableDeclarator = variableDeclarator;
        SemicolonToken = semicolonToken;
    }
}