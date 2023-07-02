using System.Linq;
using Radon.CodeAnalysis.Syntax.Nodes.Clauses;
using Radon.CodeAnalysis.Syntax.Nodes.Statements;

namespace Radon.CodeAnalysis.Syntax.Nodes.Members;

public sealed partial class MethodDeclarationSyntax : MemberDeclarationSyntax
{
    public ImmutableSyntaxList<SyntaxToken> Modifiers { get; }
    public TypeSyntax ReturnType { get; }
    public SyntaxToken Identifier { get; }
    public ParameterListSyntax ParameterList { get; }
    public BlockStatementSyntax Body { get; }
    
    public MethodDeclarationSyntax(SyntaxTree syntaxTree, ImmutableSyntaxList<SyntaxToken> modifiers, TypeSyntax returnType, 
                                   SyntaxToken identifier, ParameterListSyntax parameterList, BlockStatementSyntax body) 
        : base(syntaxTree)
    {
        Modifiers = modifiers;
        ReturnType = returnType;
        Identifier = identifier;
        ParameterList = parameterList;
        Body = body;
    }
}
