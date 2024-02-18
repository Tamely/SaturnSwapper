using Radon.CodeAnalysis.Syntax.Nodes.Expressions;

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements;

public sealed partial class ExpressionStatementSyntax : StatementSyntax
{
    public ExpressionSyntax Expression { get; }
    public override SyntaxToken SemicolonToken { get; }
    
    public ExpressionStatementSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression, SyntaxToken semicolonToken)
        : base(syntaxTree)
    {
        Expression = expression;
        SemicolonToken = semicolonToken;
    }
}