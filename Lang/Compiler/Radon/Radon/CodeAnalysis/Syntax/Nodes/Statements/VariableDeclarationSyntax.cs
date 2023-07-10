using Radon.CodeAnalysis.Syntax.Nodes.Clauses;

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements;

public sealed partial class VariableDeclarationSyntax : StatementSyntax
{
    public TypeSyntax Type { get; }
    public VariableDeclaratorSyntax Declarator { get; }
    public override SyntaxToken SemicolonToken { get; }
    
    public VariableDeclarationSyntax(SyntaxTree syntaxTree, TypeSyntax type, VariableDeclaratorSyntax declarator, SyntaxToken semicolonToken)
        : base(syntaxTree)
    {
        Type = type;
        Declarator = declarator;
        SemicolonToken = semicolonToken;
    }
}