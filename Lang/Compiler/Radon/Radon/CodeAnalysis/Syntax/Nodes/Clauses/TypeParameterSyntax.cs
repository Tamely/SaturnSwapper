namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses;

public sealed partial class TypeParameterSyntax : SyntaxNode
{
    public SyntaxToken Identifier { get; }

    public TypeParameterSyntax(SyntaxTree syntaxTree, SyntaxToken identifier) 
        : base(syntaxTree)
    {
        Identifier = identifier;
    }
}