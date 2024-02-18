using Radon.CodeAnalysis.Syntax.Nodes.Clauses;
using Radon.CodeAnalysis.Syntax.Nodes.Statements;

namespace Radon.CodeAnalysis.Syntax.Nodes.Members;
public sealed partial class TemplateMethodDeclarationSyntax : MemberDeclarationSyntax
{
    public ImmutableSyntaxList<SyntaxToken> Modifiers { get; }
    public SyntaxToken TemplateKeyword { get; }
    public TypeSyntax ReturnType { get; }
    public SyntaxToken Identifier { get; }
    public TypeParameterListSyntax TypeParameterList { get; }
    public ParameterListSyntax ParameterList { get; }
    public BlockStatementSyntax Body { get; }

    public TemplateMethodDeclarationSyntax(SyntaxTree syntaxTree, ImmutableSyntaxList<SyntaxToken> modifiers, SyntaxToken templateKeyword,
                                           TypeSyntax returnType, SyntaxToken identifier, TypeParameterListSyntax typeParameterList,
                                           ParameterListSyntax parameterList, BlockStatementSyntax body)
        : base(syntaxTree)
    {
        Modifiers = modifiers;
        TemplateKeyword = templateKeyword;
        ReturnType = returnType;
        Identifier = identifier;
        TypeParameterList = typeParameterList;
        ParameterList = parameterList;
        Body = body;
    }
}
