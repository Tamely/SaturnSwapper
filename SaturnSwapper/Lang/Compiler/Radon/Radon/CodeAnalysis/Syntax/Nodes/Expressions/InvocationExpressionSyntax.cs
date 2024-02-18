using Radon.CodeAnalysis.Syntax.Nodes.Clauses;

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class InvocationExpressionSyntax : ExpressionSyntax
{
    public ExpressionSyntax Expression { get; }
    public TypeArgumentListSyntax? TypeArgumentList { get; }
    public ArgumentListSyntax ArgumentList { get; }
    public override bool CanBeStatement => true;
    
    public InvocationExpressionSyntax(SyntaxTree syntaxTree, ExpressionSyntax expression, TypeArgumentListSyntax? typeArgumentList, 
                                      ArgumentListSyntax argumentList) 
        : base(syntaxTree)
    {
        Expression = expression;
        TypeArgumentList = typeArgumentList;
        ArgumentList = argumentList;
    }
}