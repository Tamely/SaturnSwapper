using Radon.CodeAnalysis.Syntax.Nodes.Expressions;

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements;

public sealed partial class ExpressionStatementSyntax : StatementSyntax
{
    public ExpressionSyntax Expression { get; }
    
    public ExpressionStatementSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression)
        : base(syntaxTree)
    {
        Expression = expression;
    }
}