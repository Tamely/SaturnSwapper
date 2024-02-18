using Radon.CodeAnalysis.Syntax.Nodes.Expressions;

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements;

public sealed partial class ForStatementSyntax : StatementSyntax
{
    public SyntaxToken ForKeyword { get; }
    public SyntaxToken OpenParenthesisToken { get; }
    public StatementSyntax Initializer { get; }
    public ExpressionSyntax Condition { get; }
    public SyntaxToken ConditionSemicolonToken { get; }
    public ExpressionSyntax Incrementor { get; }
    public SyntaxToken CloseParenthesisToken { get; }
    public StatementSyntax Body { get; }
    public override SyntaxToken? SemicolonToken { get; }
    
    public ForStatementSyntax(SyntaxTree syntaxTree, SyntaxToken forKeyword, SyntaxToken openParenthesisToken, 
        StatementSyntax initializer, ExpressionSyntax condition, SyntaxToken conditionSemicolonToken,
        ExpressionSyntax incrementor, SyntaxToken closeParenthesisToken, StatementSyntax body)
        : base(syntaxTree)
    {
        ForKeyword = forKeyword;
        OpenParenthesisToken = openParenthesisToken;
        Initializer = initializer;
        Condition = condition;
        ConditionSemicolonToken = conditionSemicolonToken;
        Incrementor = incrementor;
        CloseParenthesisToken = closeParenthesisToken;
        Body = body;
        SemicolonToken = null;
    }
}