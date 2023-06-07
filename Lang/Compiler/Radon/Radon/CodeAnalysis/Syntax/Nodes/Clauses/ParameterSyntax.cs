namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses;

public sealed partial class ParameterSyntax : SyntaxNode
{
    public ImmutableSyntaxList<SyntaxToken> Modifiers { get; }
    public TypeSyntax Type { get; }
    public SyntaxToken Identifier { get; }
    
    public ParameterSyntax(SyntaxTree syntaxTree, ImmutableSyntaxList<SyntaxToken> modifiers, TypeSyntax type, SyntaxToken identifier) 
        : base(syntaxTree)
    {
        Modifiers = modifiers;
        Type = type;
        Identifier = identifier;
    }
}