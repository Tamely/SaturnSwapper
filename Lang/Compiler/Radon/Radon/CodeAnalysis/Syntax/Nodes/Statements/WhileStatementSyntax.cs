using Radon.CodeAnalysis.Syntax.Nodes.Expressions;

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements;

public sealed partial class WhileStatementSyntax : StatementSyntax
{
    public SyntaxToken WhileKeyword { get; }
    public SyntaxToken OpenParenthesisToken { get; }
    public ExpressionSyntax Condition { get; }
    public SyntaxToken CloseParenthesisToken { get; }
    public StatementSyntax Body { get; }
    public override SyntaxToken? SemicolonToken { get; }
    
    public WhileStatementSyntax(SyntaxTree syntaxTree, SyntaxToken whileKeyword, SyntaxToken openParenthesisToken, 
        ExpressionSyntax condition, SyntaxToken closeParenthesisToken, StatementSyntax body)
        : base(syntaxTree)
    {
        WhileKeyword = whileKeyword;
        OpenParenthesisToken = openParenthesisToken;
        Condition = condition;
        CloseParenthesisToken = closeParenthesisToken;
        Body = body;
        SemicolonToken = null;
    }
}