using Radon.CodeAnalysis.Syntax.Nodes.Clauses;

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class NewArrayExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken NewKeyword { get; }
    public ArrayTypeSyntax Type { get; }

    public NewArrayExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken newKeyword, ArrayTypeSyntax type) 
        : base(syntaxTree)
    {
        NewKeyword = newKeyword;
        Type = type; // Contains the size of the array
    }
}