using System.Linq;
using Radon.CodeAnalysis.Syntax.Nodes.Clauses;
using Radon.CodeAnalysis.Syntax.Nodes.Statements;

namespace Radon.CodeAnalysis.Syntax.Nodes.Members;

public sealed partial class ConstructorDeclarationSyntax : MemberDeclarationSyntax
{
    public ImmutableSyntaxList<SyntaxToken> Modifiers { get; }
    public TypeSyntax Type { get; }
    public ParameterListSyntax ParameterList { get; }
    public BlockStatementSyntax Body { get; }
    public bool IsStatic => Modifiers.Any(m => m.Kind == SyntaxKind.StaticKeyword);
    
    public ConstructorDeclarationSyntax(SyntaxTree syntaxTree, ImmutableSyntaxList<SyntaxToken> modifiers, TypeSyntax type, 
                                        ParameterListSyntax parameterList, BlockStatementSyntax body) 
        : base(syntaxTree)
    {
        Modifiers = modifiers;
        Type = type;
        ParameterList = parameterList;
        Body = body;
    }
}