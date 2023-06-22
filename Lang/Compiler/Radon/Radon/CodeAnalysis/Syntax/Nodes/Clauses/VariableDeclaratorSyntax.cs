using Radon.CodeAnalysis.Syntax.Nodes.Expressions;

namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses;

public sealed partial class VariableDeclaratorSyntax : SyntaxNode
{
    public SyntaxToken Identifier { get; }
    public SyntaxToken? EqualsToken { get; }
    public ExpressionSyntax? Initializer { get; }
    
    public VariableDeclaratorSyntax(SyntaxTree syntaxTree, SyntaxToken identifier, SyntaxToken? equalsToken, ExpressionSyntax? initializer)
        : base(syntaxTree)
    {
        Identifier = identifier;
        EqualsToken = equalsToken;
        Initializer = initializer;
    }
}