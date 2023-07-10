using Radon.CodeAnalysis.Syntax.Nodes.Expressions;

namespace Radon.CodeAnalysis.Syntax.Nodes.Clauses;

public sealed partial class ArrayTypeSyntax : TypeSyntax
{
    public TypeSyntax TypeSyntax { get; }
    public SyntaxToken OpenBracketToken { get; }
    public ExpressionSyntax? SizeExpression { get; }
    public SyntaxToken CloseBracketToken { get; }
    
    public ArrayTypeSyntax(SyntaxTree syntaxTree, TypeSyntax typeSyntax, SyntaxToken openBracketToken, ExpressionSyntax? sizeExpression, SyntaxToken closeBracketToken) 
        : base(syntaxTree, typeSyntax.Identifier, typeSyntax.TypeArgumentList)
    {
        TypeSyntax = typeSyntax;
        OpenBracketToken = openBracketToken;
        SizeExpression = sizeExpression;
        CloseBracketToken = closeBracketToken;
    }
}