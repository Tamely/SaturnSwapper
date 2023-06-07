using Radon.CodeAnalysis.Syntax.Nodes.Expressions;

namespace Radon.CodeAnalysis.Syntax.Nodes.Members;

public sealed partial class EnumMemberDeclarationSyntax : MemberDeclarationSyntax
{
    public SyntaxToken Identifier { get; }
    public SyntaxToken? EqualsToken { get; }
    public ExpressionSyntax? Initializer { get; }

    public EnumMemberDeclarationSyntax(SyntaxTree syntaxTree, SyntaxToken identifier, SyntaxToken? equalsToken, 
                                       ExpressionSyntax? initializer)
        : base(syntaxTree)
    {
        Identifier = identifier;
        EqualsToken = equalsToken;
        Initializer = initializer;
    }
}