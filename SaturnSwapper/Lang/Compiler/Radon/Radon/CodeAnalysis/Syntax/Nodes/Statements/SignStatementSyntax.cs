using Radon.CodeAnalysis.Syntax.Nodes.Expressions;

namespace Radon.CodeAnalysis.Syntax.Nodes.Statements;

public sealed partial class SignStatementSyntax : StatementSyntax
{
    public SyntaxToken SignKeyword { get; }
    public SyntaxToken ColonToken { get; }
    public ExpressionSyntax KeyExpression { get; }
    public SyntaxToken CommaToken { get; }
    public ExpressionSyntax ValueExpression { get; }
    public override SyntaxToken? SemicolonToken { get; }
    
    public SignStatementSyntax(SyntaxTree syntaxTree, SyntaxToken signKeyword, SyntaxToken colonToken, 
                               ExpressionSyntax keyExpression, SyntaxToken commaToken, ExpressionSyntax valueExpression) 
        : base(syntaxTree)
    {
        SignKeyword = signKeyword;
        ColonToken = colonToken;
        KeyExpression = keyExpression;
        CommaToken = commaToken;
        ValueExpression = valueExpression;
        SemicolonToken = null;
    }
}