using Radon.CodeAnalysis.Syntax.Nodes.Expressions;

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements;

public sealed partial class IfStatementSyntax : StatementSyntax
{
    public SyntaxToken IfKeyword { get; }
    public SyntaxToken OpenParenthesisToken { get; }
    public ExpressionSyntax Condition { get; }
    public SyntaxToken CloseParenthesisToken { get; }
    public StatementSyntax ThenStatement { get; }
    public Clauses.ElseClauseSyntax? ElseClause { get; }
    public override SyntaxToken? SemicolonToken { get; }
    
    public IfStatementSyntax(SyntaxTree syntaxTree, SyntaxToken ifKeyword, SyntaxToken openParenthesisToken, 
        ExpressionSyntax condition, SyntaxToken closeParenthesisToken, StatementSyntax thenStatement, Clauses.ElseClauseSyntax? elseClause)
        : base(syntaxTree)
    {
        IfKeyword = ifKeyword;
        OpenParenthesisToken = openParenthesisToken;
        Condition = condition;
        CloseParenthesisToken = closeParenthesisToken;
        ThenStatement = thenStatement;
        ElseClause = elseClause;
        SemicolonToken = null;
    }
}