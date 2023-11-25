using System;

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class UnaryExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken? PrefixOperator { get; }
    public ExpressionSyntax Operand { get; }
    public SyntaxToken? PostfixOperator { get; }
    public override bool CanBeStatement => GetOperatorToken().Kind == SyntaxKind.PlusPlusToken ||
                                           GetOperatorToken().Kind == SyntaxKind.MinusMinusToken;

    public UnaryExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken? prefixOperator, ExpressionSyntax operand, SyntaxToken? postfixOperator) 
        : base(syntaxTree)
    {
        PrefixOperator = prefixOperator;
        Operand = operand;
        PostfixOperator = postfixOperator;
    }
    
    public SyntaxToken GetOperatorToken()
    {
        if (PrefixOperator is not null)
        {
            return PrefixOperator;
        }

        if (PostfixOperator is not null)
        {
            return PostfixOperator;
        }

        return SyntaxToken.Empty;
    }
}