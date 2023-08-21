using Radon.CodeAnalysis.Syntax.Nodes.Clauses;

namespace Radon.CodeAnalysis.Syntax.Nodes.Expressions;

public sealed partial class NewExpressionSyntax : ExpressionSyntax
{
    public SyntaxToken NewKeyword { get; }
    public TypeSyntax Type { get; }
    public TypeArgumentListSyntax? TypeArgumentList { get; }
    public ArgumentListSyntax ArgumentList { get; }
    public override bool CanBeStatement => true;
    
    public NewExpressionSyntax(SyntaxTree syntaxTree, SyntaxToken newKeyword, TypeSyntax type, TypeArgumentListSyntax? typeArgumentList, ArgumentListSyntax argumentList) 
        : base(syntaxTree)
    {
        NewKeyword = newKeyword;
        Type = type;
        TypeArgumentList = typeArgumentList;
        ArgumentList = argumentList;
    }
}