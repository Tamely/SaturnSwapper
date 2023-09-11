using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Radon.CodeAnalysis.Syntax.Nodes;
using static Radon.CodeAnalysis.Syntax.SKAttributes;

namespace Radon.CodeAnalysis.Syntax;

public sealed class SyntaxKind
{
#region Kinds
    
    // Special
    
#region Special Kinds
    
    public static readonly SyntaxKind BadToken =
        new(nameof(BadToken), Invalid, Token, Trivia);
    public static readonly SyntaxKind EndOfFileToken =
        new(nameof(EndOfFileToken), "\0", IsFixed, Token);
    
#endregion
    
    // Trivia
    
#region Trivia Kinds
    
    public static readonly SyntaxKind WhitespaceTrivia =
        new(nameof(WhitespaceTrivia), Trivia);
    public static readonly SyntaxKind LineBreakTrivia =
        new(nameof(LineBreakTrivia), Trivia);
    public static readonly SyntaxKind SingleLineCommentTrivia =
        new(nameof(SingleLineCommentTrivia), Trivia, Comment);
    public static readonly SyntaxKind MultiLineCommentTrivia =
        new(nameof(MultiLineCommentTrivia), Trivia, Comment);
    public static readonly SyntaxKind SkippedTextTrivia =
        new(nameof(SkippedTextTrivia), Trivia);
    public static readonly SyntaxKind DirectiveTrivia =
        new(nameof(DirectiveTrivia), Trivia);
    
#endregion
    
    // Operators

#region Operator Kinds

    // Assignment
    public static readonly SyntaxKind EqualsToken =
        new(nameof(EqualsToken), "=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.None, OperatorKind.Assignment), Operator);
    public static readonly SyntaxKind PlusEqualsToken =
        new(nameof(PlusEqualsToken), "+=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.None, OperatorKind.Assignment), CompoundAssignment, Operator);
    public static readonly SyntaxKind MinusEqualsToken =
        new(nameof(MinusEqualsToken), "-=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.None, OperatorKind.Assignment), CompoundAssignment, Operator);
    public static readonly SyntaxKind StarEqualsToken =
        new(nameof(StarEqualsToken), "*=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.None, OperatorKind.Assignment), CompoundAssignment, Operator);
    public static readonly SyntaxKind SlashEqualsToken =
        new(nameof(SlashEqualsToken), "/=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.None, OperatorKind.Assignment), CompoundAssignment, Operator);
    public static readonly SyntaxKind PercentEqualsToken =
        new(nameof(PercentEqualsToken), "%=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.None, OperatorKind.Assignment), CompoundAssignment, Operator);
    public static readonly SyntaxKind PipeEqualsToken =
        new(nameof(PipeEqualsToken), "|=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.None, OperatorKind.Assignment), CompoundAssignment, Operator);
    public static readonly SyntaxKind AmpersandEqualsToken =
        new(nameof(AmpersandEqualsToken), "&=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.None, OperatorKind.Assignment), CompoundAssignment, Operator);
    
    // Logical OR
    public static readonly SyntaxKind PipePipeToken =
        new(nameof(PipePipeToken), "||", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.LogicalOr, OperatorKind.Binary), Operator);
    
    // Logical AND
    public static readonly SyntaxKind AmpersandAmpersandToken =
        new(nameof(AmpersandAmpersandToken), "&&", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.LogicalAnd, OperatorKind.Binary), Operator);
    
    // Logical NOT
    public static readonly SyntaxKind BangToken =
        new(nameof(BangToken), "!", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.LogicalNot, OperatorKind.Unary), Operator);
    
    // Bitwise OR
    public static readonly SyntaxKind PipeToken =
        new(nameof(PipeToken), "|", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.BitwiseOr, OperatorKind.Binary), Operator);
    
    // Bitwise AND
    public static readonly SyntaxKind AmpersandToken =
        new(nameof(AmpersandToken), "&", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.BitwiseAnd, OperatorKind.Binary | OperatorKind.Unary), Operator);
    
    // Bitwise XOR
    public static readonly SyntaxKind HatToken =
        new(nameof(HatToken), "^", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.BitwiseXor, OperatorKind.Binary), Operator);

    // Equality
    public static readonly SyntaxKind EqualsEqualsToken =
        new(nameof(EqualsEqualsToken), "==", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Equality, OperatorKind.Binary), Operator);
    public static readonly SyntaxKind BangEqualsToken =
        new(nameof(BangEqualsToken), "!=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Equality, OperatorKind.Binary), Operator);
    
    // Relational
    public static readonly SyntaxKind LessThanToken =
        new(nameof(LessThanToken), "<", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Relational, OperatorKind.Binary), Operator);
    public static readonly SyntaxKind LessEqualsToken =
        new(nameof(LessEqualsToken), "<=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Relational, OperatorKind.Binary), Operator);
    public static readonly SyntaxKind GreaterThanToken =
        new(nameof(GreaterThanToken), ">", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Relational, OperatorKind.Binary), Operator);
    public static readonly SyntaxKind GreaterEqualsToken =
        new(nameof(GreaterEqualsToken), ">=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Relational, OperatorKind.Binary), Operator);
    
    // Shift
    public static readonly SyntaxKind LessLessToken =
        new(nameof(LessLessToken), "<<", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Shift, OperatorKind.Binary), Operator);
    public static readonly SyntaxKind GreaterGreaterToken =
        new(nameof(GreaterGreaterToken), ">>", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Shift, OperatorKind.Binary), Operator);
    
    // Additive
    public static readonly SyntaxKind PlusToken =
        new(nameof(PlusToken), "+", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Additive, OperatorKind.Binary | OperatorKind.Unary), Operator);
    public static readonly SyntaxKind MinusToken =
        new(nameof(MinusToken), "-", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Additive, OperatorKind.Binary | OperatorKind.Unary), Operator);
    public static readonly SyntaxKind PlusPlusToken =
        new(nameof(PlusPlusToken), "++", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.None, OperatorKind.PostfixUnary), Operator);
    public static readonly SyntaxKind MinusMinusToken =
        new(nameof(MinusMinusToken), "--", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.None, OperatorKind.PostfixUnary), Operator);
    
    // Multiplicative
    public static readonly SyntaxKind StarToken =
        new(nameof(StarToken), "*", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Multiplicative, OperatorKind.Binary | OperatorKind.Unary), Operator);
    public static readonly SyntaxKind SlashToken =
        new(nameof(SlashToken), "/", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Multiplicative, OperatorKind.Binary), Operator);
    public static readonly SyntaxKind PercentToken =
        new(nameof(PercentToken), "%", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Multiplicative, OperatorKind.Binary), Operator);
    
    // Dot
    public static readonly SyntaxKind DotToken =
        new(nameof(DotToken), ".", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Dot, OperatorKind.None), Operator);
    // Arrow
    public static readonly SyntaxKind ArrowToken =
        new(nameof(ArrowToken), "->", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Dot, OperatorKind.None), Operator);
    
#endregion
    
    // Dynamic
    
#region Dynamic

    public static readonly SyntaxKind NumberToken =
        new(nameof(NumberToken), null, Literal, Numeric);
    public static readonly SyntaxKind StringToken =
        new(nameof(StringToken), null, Literal);
    public static readonly SyntaxKind IdentifierToken =
        new(nameof(IdentifierToken), null, Identifier);
    
#endregion
    
    // Punctuation

#region Punctuation

    public static readonly SyntaxKind OpenParenthesisToken =
        new(nameof(OpenParenthesisToken), "(", Punctuation);
    public static readonly SyntaxKind CloseParenthesisToken =
        new(nameof(CloseParenthesisToken), ")", Punctuation);
    public static readonly SyntaxKind OpenBraceToken =
        new(nameof(OpenBraceToken), "{", Punctuation);
    public static readonly SyntaxKind CloseBraceToken =
        new(nameof(CloseBraceToken), "}", Punctuation);
    public static readonly SyntaxKind OpenBracketToken =
        new(nameof(OpenBracketToken), "[", Punctuation);
    public static readonly SyntaxKind CloseBracketToken =
        new(nameof(CloseBracketToken), "]", Punctuation);
    public static readonly SyntaxKind CommaToken =
        new(nameof(CommaToken), ",", Punctuation);
    public static readonly SyntaxKind HashToken =
        new(nameof(HashToken), "#", Punctuation);
    public static readonly SyntaxKind ColonToken =
        new(nameof(ColonToken), ":", Punctuation);
    public static readonly SyntaxKind SemicolonToken =
        new(nameof(SemicolonToken), ";", Punctuation);
    
#endregion
    
    // Keywords
    
#region Keywords
    
    public static readonly SyntaxKind ImportKeyword =
        new(nameof(ImportKeyword), "import", Keyword);
    public static readonly SyntaxKind SignKeyword =
        new(nameof(SignKeyword), "sign", Keyword);
    public static readonly SyntaxKind StructKeyword =
        new(nameof(StructKeyword), "struct", Keyword, TypeKeyword);
    public static readonly SyntaxKind EnumKeyword =
        new(nameof(EnumKeyword), "enum", Keyword, TypeKeyword);
    public static readonly SyntaxKind TemplateKeyword =
        new(nameof(TemplateKeyword), "template", Keyword, TypeKeyword);
    public static readonly SyntaxKind NewKeyword =
        new(nameof(NewKeyword), "new", Keyword);
    public static readonly SyntaxKind ThisKeyword =
        new(nameof(ThisKeyword), "this", Keyword);
    public static readonly SyntaxKind TrueKeyword =
        new(nameof(TrueKeyword), "true", Keyword,Literal);
    public static readonly SyntaxKind FalseKeyword =
        new(nameof(FalseKeyword), "false", Keyword, Literal);
    public static readonly SyntaxKind DefaultKeyword =
        new(nameof(DefaultKeyword), "default", Keyword);
    public static readonly SyntaxKind EncryptedKeyword =
        new(nameof(EncryptedKeyword), "encrypted", Keyword, Literal);
    
    // Modifiers
    public static readonly SyntaxKind StaticKeyword =
        new(nameof(StaticKeyword), "static", TypeModifier, FieldModifier, MethodModifier, Modifier);
    public static readonly SyntaxKind RuntimeInternalKeyword =
        new(nameof(RuntimeInternalKeyword), "__runtimeinternal", TypeModifier, FieldModifier, MethodModifier, Modifier); // Keywords for internal use will be prefixed with __
    public static readonly SyntaxKind PublicKeyword =
        new(nameof(PublicKeyword), "public", TypeModifier, FieldModifier, MethodModifier, Modifier);
    public static readonly SyntaxKind PrivateKeyword =
        new(nameof(PrivateKeyword), "private", TypeModifier, FieldModifier, MethodModifier, Modifier);
    public static readonly SyntaxKind EntryKeyword =
        new(nameof(EntryKeyword), "entry", TypeModifier, MethodModifier, Modifier);
    public static readonly SyntaxKind RefKeyword =
        new(nameof(RefKeyword), "ref", TypeModifier, Modifier); // Can only be used on types and fields

    // Directive Keywords
    public static readonly SyntaxKind IncludeKeyword =
        new(nameof(IncludeKeyword), "include", Keyword, DirectiveOperator);
    
    // Flow Control
    public static readonly SyntaxKind ReturnKeyword =
        new(nameof(ReturnKeyword), "return", Keyword, FlowControl);
    public static readonly SyntaxKind IfKeyword =
        new(nameof(IfKeyword), "if", Keyword, FlowControl);
    public static readonly SyntaxKind ElseKeyword =
        new(nameof(ElseKeyword), "else", Keyword, FlowControl);
    public static readonly SyntaxKind WhileKeyword =
        new(nameof(WhileKeyword), "while", Keyword, FlowControl);
    public static readonly SyntaxKind ForKeyword =
        new(nameof(ForKeyword), "for", Keyword, FlowControl);
    public static readonly SyntaxKind BreakKeyword =
        new(nameof(BreakKeyword), "break", Keyword, FlowControl);
    public static readonly SyntaxKind ContinueKeyword =
        new(nameof(ContinueKeyword), "continue", Keyword, FlowControl);
    
#endregion
    
    // Nodes
    
#region Nodes

    public static readonly SyntaxKind Empty =
        new(nameof(Empty), null, Node);
    public static readonly SyntaxKind PluginCompilationUnit =
        new(nameof(PluginCompilationUnit), null, Node);
    public static readonly SyntaxKind CodeCompilationUnit =
        new(nameof(CodeCompilationUnit), null, Node);
    public static readonly SyntaxKind Type =
        new(nameof(Type), null, Node);
    public static readonly SyntaxKind VariableDeclarator =
        new(nameof(VariableDeclarator), null, Node);
    public static readonly SyntaxKind StructBody =
        new(nameof(StructBody), null, Node);
    public static readonly SyntaxKind EnumBody =
        new(nameof(EnumBody), null, Node);
    public static readonly SyntaxKind ParameterList =
        new(nameof(ParameterList), null, Node);
    public static readonly SyntaxKind Parameter =
        new(nameof(Parameter), null, Node);
    public static readonly SyntaxKind TypeArgumentList =
        new(nameof(TypeArgumentList), null, Node);
    public static readonly SyntaxKind ArgumentList =
        new(nameof(ArgumentList), null, Node);
    public static readonly SyntaxKind TypeParameterList =
        new(nameof(TypeParameterList), null, Node);
    public static readonly SyntaxKind TypeParameter =
        new(nameof(TypeParameter), null, Node);
    public static readonly SyntaxKind ArrayType =
        new(nameof(ArrayType), null, Node);
    public static readonly SyntaxKind PointerType =
        new(nameof(PointerType), null, Node);
    public static readonly SyntaxKind ElseClause =
        new(nameof(ElseClause), null, Node);

    #endregion
    
    // Directives
    
#region Directives
    
    public static readonly SyntaxKind FileDirective =
        new(nameof(FileDirective), null, Node, Directive);
    public static readonly SyntaxKind IncludeDirective =
        new(nameof(IncludeDirective), null, Node, Directive);
    public static readonly SyntaxKind InvalidDirective =
        new(nameof(InvalidDirective), null, Node, Directive);

#endregion
    
    // Expressions
    
#region Expressions
    
    public static readonly SyntaxKind InvalidExpression =
        new(nameof(InvalidExpression), null, Node, Expression);
    public static readonly SyntaxKind LiteralExpression =
        new(nameof(LiteralExpression), null, Node, Expression);
    public static readonly SyntaxKind ImportExpression =
        new(nameof(ImportExpression), null, Node, Expression);
    public static readonly SyntaxKind NameExpression =
        new(nameof(NameExpression), null, Node, Expression);
    public static readonly SyntaxKind BinaryExpression =
        new(nameof(BinaryExpression), null, Node, Expression);
    public static readonly SyntaxKind ParenthesizedExpression =
        new(nameof(ParenthesizedExpression), null, Node, Expression);
    public static readonly SyntaxKind UnaryExpression =
        new(nameof(UnaryExpression), null, Node, Expression);
    public static readonly SyntaxKind MemberAccessExpression =
        new(nameof(MemberAccessExpression), null, Node, Expression);
    public static readonly SyntaxKind AssignmentExpression =
        new(nameof(AssignmentExpression), null, Node, Expression);
    public static readonly SyntaxKind InvocationExpression =
        new(nameof(InvocationExpression), null, Node, Expression);
    public static readonly SyntaxKind NewExpression =
        new(nameof(NewExpression), null, Node, Expression);
    public static readonly SyntaxKind ThisExpression =
        new(nameof(ThisExpression), null, Node, Expression);
    public static readonly SyntaxKind DefaultExpression =
        new(nameof(DefaultExpression), null, Node, Expression);
    public static readonly SyntaxKind NewArrayExpression =
        new(nameof(NewArrayExpression), null, Node, Expression);
    public static readonly SyntaxKind ElementAccessExpression =
        new(nameof(ElementAccessExpression), null, Node, Expression);
    public static readonly SyntaxKind CastExpression =
        new(nameof(CastExpression), null, Node, Expression);

    #endregion
    
    // Statements
    
#region Statements
    
    public static readonly SyntaxKind VariableDeclaration =
        new(nameof(VariableDeclaration), null, Node, Statement);
    public static readonly SyntaxKind ExpressionStatement =
        new(nameof(ExpressionStatement), null, Node, Statement);
    public static readonly SyntaxKind BlockStatement =
        new(nameof(BlockStatement), null, Node, Statement);
    public static readonly SyntaxKind SignStatement =
        new(nameof(SignStatement), null, Node, Statement);
    public static readonly SyntaxKind ReturnStatement =
        new(nameof(ReturnStatement), null, Node, Statement);
    public static readonly SyntaxKind IfStatement =
        new(nameof(IfStatement), null, Node, Statement);
    public static readonly SyntaxKind WhileStatement =
        new(nameof(WhileStatement), null, Node, Statement);
    public static readonly SyntaxKind ForStatement =
        new(nameof(ForStatement), null, Node, Statement);
    public static readonly SyntaxKind BreakStatement =
        new(nameof(BreakStatement), null, Node, Statement);
    public static readonly SyntaxKind ContinueStatement =
        new(nameof(ContinueStatement), null, Node, Statement);
    public static readonly SyntaxKind InvalidStatement =
        new(nameof(InvalidStatement), null, Node, Statement);

    #endregion

    // Member Declarations
    
#region Member Declarations

    public static readonly SyntaxKind EnumMemberDeclaration =
        new(nameof(EnumMemberDeclaration), null, Node, MemberDeclaration);
    public static readonly SyntaxKind InvalidMemberDeclaration =
        new(nameof(InvalidMemberDeclaration), null, Node, MemberDeclaration);
    public static readonly SyntaxKind MethodDeclaration =
        new(nameof(MethodDeclaration), null, Node, MemberDeclaration);
    public static readonly SyntaxKind TemplateMethodDeclaration =
        new(nameof(TemplateMethodDeclaration), null, Node, MemberDeclaration);
    public static readonly SyntaxKind FieldDeclaration =
        new(nameof(FieldDeclaration), null, Node, MemberDeclaration);
    public static readonly SyntaxKind ConstructorDeclaration =
        new(nameof(ConstructorDeclaration), null, Node, MemberDeclaration);

#endregion
    
    // Type Declarations
    
#region Type Declarations
    
    public static readonly SyntaxKind StructDeclaration =
        new(nameof(StructDeclaration), null, Node, TypeDeclaration);
    public static readonly SyntaxKind EnumDeclaration =
        new(nameof(EnumDeclaration), null, Node, TypeDeclaration);
    public static readonly SyntaxKind TemplateDeclaration =
        new(nameof(TemplateDeclaration), null, Node, TypeDeclaration);
    public static readonly SyntaxKind InvalidTypeDeclaration =
        new(nameof(InvalidTypeDeclaration), null, Node, TypeDeclaration);
    
#endregion

#endregion

    public string Name { get; }
    public string? Text { get; }
    public SyntaxKindAttribute[] Attributes { get; }

    private SyntaxKind(string name, string? text = null)
        : this(name, text, Array.Empty<SyntaxKindAttribute>())
    {
    }
    
    private SyntaxKind(string name, params SyntaxKindAttribute[] attributes)
        : this(name, null, attributes)
    {
    }

    private SyntaxKind(string name, string? text, params SyntaxKindAttribute[] attributes)
    {
        Name = name;
        Text = text;
        if (Text != null && 
            !attributes.Contains(IsFixed))
        {
            attributes = attributes.Append(IsFixed).ToArray();
        }

        // Only doing this for tokens, because I'm too lazy to add the 'Token' attribute to all tokens
        if ((Name.Contains("Token") ||
             Name.Contains("Keyword")) &&
            !attributes.Contains(Token))
        {
            attributes = attributes.Append(Token).ToArray();
        }

        Attributes = attributes;
    }
    
    public bool TryGetAttribute(in SyntaxKindAttribute attribute, out SyntaxKindAttribute result)
    {
        foreach (var attr in Attributes)
        {
            if (attr.Name == attribute.Name)
            {
                result = attr;
                return true;
            }
        }

        result = default!;
        return false;
    }
    
    public bool HasAttribute(in SyntaxKindAttribute attribute)
    {
        foreach (var attr in Attributes)
        {
            if (attr.Name == attribute.Name)
            {
                return true;
            }
        }

        return false;
    }

    public static ImmutableArray<SyntaxKind> GetKinds(SyntaxKindAttribute attribute)
    {
        var kinds = ImmutableArray.CreateBuilder<SyntaxKind>();
        foreach (var field in typeof(SyntaxKind).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType == typeof(SyntaxKind))
            {
                var kind = (SyntaxKind)field.GetValue(null)!;
                if (kind.HasAttribute(attribute))
                {
                    kinds.Add(kind);
                }
            }
        }
        
        return kinds.ToImmutable();
    }

    public static SyntaxKind GetKind(string text)
    {
        foreach (var field in typeof(SyntaxKind).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType == typeof(SyntaxKind))
            {
                var kind = (SyntaxKind)field.GetValue(null)!;
                if (kind.Text == text)
                {
                    return kind;
                }
            }
        }

        throw new ArgumentException($"SyntaxKind '{text}' does not exist.", nameof(text));
    }
    
    public static ImmutableArray<SyntaxKind> GetKinds()
    {
        var kinds = ImmutableArray.CreateBuilder<SyntaxKind>();
        foreach (var field in typeof(SyntaxKind).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType == typeof(SyntaxKind))
            {
                kinds.Add((SyntaxKind)field.GetValue(null)!);
            }
        }

        return kinds.ToImmutable();
    }

    public SyntaxToken ManifestToken(SyntaxTree syntaxTree, int position = 0, string text = "", object? value = null)
    {
        if (!HasAttribute(Token))
        {
            throw new NotATokenException(this);
        }

        var leadingTrivia = ImmutableArray<SyntaxTrivia>.Empty;
        var trailingTrivia = ImmutableArray<SyntaxTrivia>.Empty;
        return new SyntaxToken(syntaxTree, this, position, text, value, leadingTrivia, trailingTrivia);
    }

    public override string ToString() => Name;
}
