namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses;

public sealed partial class TypeSyntax : SyntaxNode
{
    public SyntaxToken Identifier { get; }
    public TypeArgumentListSyntax? TypeArgumentList { get; }
    
    public TypeSyntax(SyntaxTree syntaxTree, SyntaxToken identifier, TypeArgumentListSyntax? typeArgumentList)
        : base(syntaxTree)
    {
        Identifier = identifier;
        TypeArgumentList = typeArgumentList;
    }
}