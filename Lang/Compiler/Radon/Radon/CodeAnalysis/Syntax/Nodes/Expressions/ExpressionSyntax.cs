

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public abstract class ExpressionSyntax : SyntaxNode
{
    protected ExpressionSyntax(SyntaxTree syntaxTree) 
        : base(syntaxTree)
    {
    }
}