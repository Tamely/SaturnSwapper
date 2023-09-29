namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses;

public sealed partial class PointerTypeSyntax : TypeSyntax
{
    public TypeSyntax Type { get; }
    public SyntaxToken StarToken { get; }
    public PointerTypeSyntax(SyntaxTree syntaxTree, TypeSyntax type, SyntaxToken starToken)
        : base(syntaxTree, type.Identifier, type.TypeArgumentList)
    {
        Type = type;
        StarToken = starToken;
    }
}