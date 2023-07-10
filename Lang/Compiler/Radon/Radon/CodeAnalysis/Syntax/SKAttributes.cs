namespace Radon.CodeAnalysis.Syntax;

// ReSharper disable once InconsistentNaming
public static class SKAttributes
{
    public static readonly SyntaxKindAttribute Invalid = nameof(Invalid);
    public static readonly SyntaxKindAttribute Trivia = nameof(Trivia);
    public static readonly SyntaxKindAttribute Literal = nameof(Literal);
    public static readonly SyntaxKindAttribute Operator = nameof(Operator);
    public static readonly SyntaxKindAttribute Punctuation = nameof(Punctuation);
    public static readonly SyntaxKindAttribute Numeric = nameof(Numeric);
    public static readonly SyntaxKindAttribute Comment = nameof(Comment);
    public static readonly SyntaxKindAttribute Identifier = nameof(Identifier);
    public static readonly SyntaxKindAttribute Keyword = nameof(Keyword);
    public static readonly SyntaxKindAttribute TypeKeyword = nameof(TypeKeyword);
    public static readonly SyntaxKindAttribute VariableDeclarator = nameof(VariableDeclarator);
    public static readonly SyntaxKindAttribute Modifier = nameof(Modifier);
    public static readonly SyntaxKindAttribute TypeModifier = nameof(TypeModifier);
    public static readonly SyntaxKindAttribute IsFixed = nameof(IsFixed);
    public static readonly SyntaxKindAttribute Token = nameof(Token);
    public static readonly SyntaxKindAttribute Statement = nameof(Statement);
    public static readonly SyntaxKindAttribute Expression = nameof(Expression);
    public static readonly SyntaxKindAttribute Type = nameof(Type);
    public static readonly SyntaxKindAttribute Declaration = nameof(Declaration);
    public static readonly SyntaxKindAttribute Member = nameof(Member);
    public static readonly SyntaxKindAttribute Node = nameof(Node);
    public static readonly SyntaxKindAttribute Directive = nameof(Directive);
    public static readonly SyntaxKindAttribute MemberDeclaration = nameof(MemberDeclaration);
    public static readonly SyntaxKindAttribute TypeDeclaration = nameof(TypeDeclaration);
    public static readonly SyntaxKindAttribute DirectiveOperator = nameof(DirectiveOperator);
    public static readonly SyntaxKindAttribute DirectiveModifier = nameof(DirectiveModifier);
    public static readonly SyntaxKindAttribute CompilationUnit = nameof(CompilationUnit);
    public static readonly SyntaxKindAttribute FlowControl = nameof(FlowControl);
}