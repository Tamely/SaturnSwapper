using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.VisualBasic;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Syntax;

public sealed class SyntaxKind
{
#region Kinds
    
    // Special
    
#region Special Kinds
    
    public static readonly SyntaxKind BadToken =
        new(nameof(BadToken), SKAttributes.Invalid, SKAttributes.Token, SKAttributes.Trivia);
    public static readonly SyntaxKind EndOfFileToken =
        new(nameof(EndOfFileToken), "\0", SKAttributes.IsFixed, SKAttributes.Token);
    
#endregion
    
    // Trivia
    
#region Trivia Kinds
    
    public static readonly SyntaxKind WhitespaceTrivia =
        new(nameof(WhitespaceTrivia), SKAttributes.Trivia);
    public static readonly SyntaxKind LineBreakTrivia =
        new(nameof(LineBreakTrivia), SKAttributes.Trivia);
    public static readonly SyntaxKind SingleLineCommentTrivia =
        new(nameof(SingleLineCommentTrivia), SKAttributes.Trivia);
    public static readonly SyntaxKind MultiLineCommentTrivia =
        new(nameof(MultiLineCommentTrivia), SKAttributes.Trivia);
    public static readonly SyntaxKind SkippedTextTrivia =
        new(nameof(SkippedTextTrivia), SKAttributes.Trivia);
    public static readonly SyntaxKind DirectiveTrivia =
        new(nameof(DirectiveTrivia), SKAttributes.Trivia);
    
#endregion
    
    // Operators

#region Operator Kinds

    // Assignment
    public static readonly SyntaxKind EqualsToken =
        new(nameof(EqualsToken), "=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Assignment, false, false, true));
    public static readonly SyntaxKind PlusEqualsToken =
        new(nameof(PlusEqualsToken), "+=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Assignment, false, false, true));
    public static readonly SyntaxKind MinusEqualsToken =
        new(nameof(MinusEqualsToken), "-=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Assignment, false, false, true));
    public static readonly SyntaxKind StarEqualsToken =
        new(nameof(StarEqualsToken), "*=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Assignment, false, false, true));
    public static readonly SyntaxKind SlashEqualsToken =
        new(nameof(SlashEqualsToken), "/=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Assignment, false, false, true));
    public static readonly SyntaxKind PercentEqualsToken =
        new(nameof(PercentEqualsToken), "%=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Assignment, false, false, true));
    
    // Logical OR
    public static readonly SyntaxKind PipePipeToken =
        new(nameof(PipePipeToken), "||", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.LogicalOr, true, false, false));
    
    // Logical AND
    public static readonly SyntaxKind AmpersandAmpersandToken =
        new(nameof(AmpersandAmpersandToken), "&&", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.LogicalAnd, true, false, false));
    
    // Logical NOT
    public static readonly SyntaxKind BangToken =
        new(nameof(BangToken), "!", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.LogicalNot, false, true, false));
    
    // Bitwise OR
    public static readonly SyntaxKind PipeToken =
        new(nameof(PipeToken), "|", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.BitwiseOr, true, false, false));
    
    // Bitwise AND
    public static readonly SyntaxKind AmpersandToken =
        new(nameof(AmpersandToken), "&", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.BitwiseAnd, true, false, false));

    // Equality
    public static readonly SyntaxKind EqualsEqualsToken =
        new(nameof(EqualsEqualsToken), "==", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Equality, true, false, false));
    public static readonly SyntaxKind BangEqualsToken =
        new(nameof(BangEqualsToken), "!=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Equality, true, false, false));
    
    // Relational
    public static readonly SyntaxKind LessToken =
        new(nameof(LessToken), "<", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Relational, true, false, false));
    public static readonly SyntaxKind LessEqualsToken =
        new(nameof(LessEqualsToken), "<=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Relational, true, false, false));
    public static readonly SyntaxKind GreaterToken =
        new(nameof(GreaterToken), ">", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Relational, true, false, false));
    public static readonly SyntaxKind GreaterEqualsToken =
        new(nameof(GreaterEqualsToken), ">=", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Relational, true, false, false));
    
    // Shift
    public static readonly SyntaxKind LessLessToken =
        new(nameof(LessLessToken), "<<", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Shift, true, false, false));
    public static readonly SyntaxKind GreaterGreaterToken =
        new(nameof(GreaterGreaterToken), ">>", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Shift, true, false, false));
    
    // Additive
    public static readonly SyntaxKind PlusToken =
        new(nameof(PlusToken), "+", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Additive, true, true, false));
    public static readonly SyntaxKind MinusToken =
        new(nameof(MinusToken), "-", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Additive, true, true, false));
    public static readonly SyntaxKind PlusPlusToken =
        new(nameof(PlusPlusToken), "++", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Additive, false, true, false));
    public static readonly SyntaxKind MinusMinusToken =
        new(nameof(MinusMinusToken), "--", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Additive, false, true, false));
    
    // Multiplicative
    public static readonly SyntaxKind StarToken =
        new(nameof(StarToken), "*", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Multiplicative, true, false, false));
    public static readonly SyntaxKind SlashToken =
        new(nameof(SlashToken), "/", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Multiplicative, true, false, false));
    public static readonly SyntaxKind PercentToken =
        new(nameof(PercentToken), "%", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Multiplicative, true, false, false));
    
    // Dot
    public static readonly SyntaxKind DotToken =
        new(nameof(DotToken), ".", 
            SyntaxKindAttribute.CreateOperator(OperatorPrecedence.Dot, false, false, false));
    
#endregion
    
    // Dynamic
    
#region Dynamic
    public static readonly SyntaxKind NumberToken =
        new(nameof(NumberToken), null, SKAttributes.Literal);
    public static readonly SyntaxKind StringToken =
        new(nameof(StringToken), null, SKAttributes.Literal);
    public static readonly SyntaxKind IdentifierToken =
        new(nameof(IdentifierToken), null, SKAttributes.Identifier);
    
#endregion
    
    // Punctuation

#region Punctuation

    public static readonly SyntaxKind OpenParenthesisToken =
        new(nameof(OpenParenthesisToken), "(", SKAttributes.Punctuation);
    public static readonly SyntaxKind CloseParenthesisToken =
        new(nameof(CloseParenthesisToken), ")", SKAttributes.Punctuation);
    public static readonly SyntaxKind OpenBraceToken =
        new(nameof(OpenBraceToken), "{", SKAttributes.Punctuation);
    public static readonly SyntaxKind CloseBraceToken =
        new(nameof(CloseBraceToken), "}", SKAttributes.Punctuation);
    public static readonly SyntaxKind OpenBracketToken =
        new(nameof(OpenBracketToken), "[", SKAttributes.Punctuation);
    public static readonly SyntaxKind CloseBracketToken =
        new(nameof(CloseBracketToken), "]", SKAttributes.Punctuation);
    public static readonly SyntaxKind CommaToken =
        new(nameof(CommaToken), ",", SKAttributes.Punctuation);
    public static readonly SyntaxKind HashToken =
        new(nameof(HashToken), "#", SKAttributes.Punctuation);
    public static readonly SyntaxKind ColonToken =
        new(nameof(ColonToken), ":", SKAttributes.Punctuation);
    public static readonly SyntaxKind SemicolonToken =
        new(nameof(SemicolonToken), ";", SKAttributes.Punctuation);
    
#endregion
    
    // Keywords
    
#region Keywords
    
    public static readonly SyntaxKind ImportKeyword =
        new(nameof(ImportKeyword), "import", SKAttributes.Keyword);
    public static readonly SyntaxKind SignKeyword =
        new(nameof(SignKeyword), "sign", SKAttributes.Keyword);
    public static readonly SyntaxKind StructKeyword =
        new(nameof(StructKeyword), "struct", SKAttributes.Keyword, SKAttributes.TypeKeyword);
    public static readonly SyntaxKind EnumKeyword =
        new(nameof(EnumKeyword), "enum", SKAttributes.Keyword, SKAttributes.TypeKeyword);
    public static readonly SyntaxKind TemplateKeyword =
        new(nameof(TemplateKeyword), "template", SKAttributes.Keyword, SKAttributes.TypeKeyword);
    public static readonly SyntaxKind NewKeyword =
        new(nameof(NewKeyword), "new", SKAttributes.Keyword);
    public static readonly SyntaxKind ThisKeyword =
        new(nameof(ThisKeyword), "this", SKAttributes.Keyword);
    public static readonly SyntaxKind TrueKeyword =
        new(nameof(TrueKeyword), "true", SKAttributes.Keyword,SKAttributes.Literal);
    public static readonly SyntaxKind FalseKeyword =
        new(nameof(FalseKeyword), "false", SKAttributes.Keyword, SKAttributes.Literal);
    public static readonly SyntaxKind DefaultKeyword =
        new(nameof(DefaultKeyword), "default", SKAttributes.Keyword);
    public static readonly SyntaxKind EncryptedKeyword =
        new(nameof(EncryptedKeyword), "encrypted", SKAttributes.Keyword, SKAttributes.Literal);
    
    // Modifiers
    public static readonly SyntaxKind StaticKeyword =
        new(nameof(StaticKeyword), "static", SKAttributes.Modifier, SKAttributes.TypeModifier);
    public static readonly SyntaxKind RuntimeInternalKeyword =
        new(nameof(RuntimeInternalKeyword), "__runtimeinternal", SKAttributes.Modifier, SKAttributes.TypeModifier); // Keywords for internal use will be prefixed with __
    public static readonly SyntaxKind PublicKeyword =
        new(nameof(PublicKeyword), "public", SKAttributes.Modifier, SKAttributes.TypeModifier);
    public static readonly SyntaxKind PrivateKeyword =
        new(nameof(PrivateKeyword), "private", SKAttributes.Modifier, SKAttributes.TypeModifier);

    // Directive Keywords
    public static readonly SyntaxKind IncludeKeyword =
        new(nameof(IncludeKeyword), "include", SKAttributes.Keyword, SKAttributes.DirectiveOperator);
    
    // Flow Control
    public static readonly SyntaxKind ReturnKeyword =
        new(nameof(ReturnKeyword), "return", SKAttributes.Keyword, SKAttributes.FlowControl);

#endregion
    
    // Nodes
    
#region Nodes

    public static readonly SyntaxKind Empty =
        new(nameof(Empty), null, SKAttributes.Node);
    public static readonly SyntaxKind PluginCompilationUnit =
        new(nameof(PluginCompilationUnit), null, SKAttributes.Node);
    public static readonly SyntaxKind CodeCompilationUnit =
        new(nameof(CodeCompilationUnit), null, SKAttributes.Node);
    public static readonly SyntaxKind Type =
        new(nameof(Type), null, SKAttributes.Node);
    public static readonly SyntaxKind VariableDeclarator =
        new(nameof(VariableDeclarator), null, SKAttributes.Node);
    public static readonly SyntaxKind StructBody =
        new(nameof(StructBody), null, SKAttributes.Node);
    public static readonly SyntaxKind EnumBody =
        new(nameof(EnumBody), null, SKAttributes.Node);
    public static readonly SyntaxKind ParameterList =
        new(nameof(ParameterList), null, SKAttributes.Node);
    public static readonly SyntaxKind Parameter =
        new(nameof(Parameter), null, SKAttributes.Node);
    public static readonly SyntaxKind TypeArgumentList =
        new(nameof(TypeArgumentList), null, SKAttributes.Node);
    public static readonly SyntaxKind ArgumentList =
        new(nameof(ArgumentList), null, SKAttributes.Node);
    public static readonly SyntaxKind TypeParameterList =
        new(nameof(TypeParameterList), null, SKAttributes.Node);
    public static readonly SyntaxKind TypeParameter =
        new(nameof(TypeParameter), null, SKAttributes.Node);
    public static readonly SyntaxKind ArrayType =
        new(nameof(ArrayType), null, SKAttributes.Node);

    #endregion
    
    // Directives
    
#region Directives
    
    public static readonly SyntaxKind FileDirective =
        new(nameof(FileDirective), null, SKAttributes.Node, SKAttributes.Directive);
    public static readonly SyntaxKind IncludeDirective =
        new(nameof(IncludeDirective), null, SKAttributes.Node, SKAttributes.Directive);
    public static readonly SyntaxKind InvalidDirective =
        new(nameof(InvalidDirective), null, SKAttributes.Node, SKAttributes.Directive);

#endregion
    
    // Expressions
    
#region Expressions
    
    public static readonly SyntaxKind InvalidExpression =
        new(nameof(InvalidExpression), null, SKAttributes.Node, SKAttributes.Expression);
    public static readonly SyntaxKind LiteralExpression =
        new(nameof(LiteralExpression), null, SKAttributes.Node, SKAttributes.Expression);
    public static readonly SyntaxKind ImportExpression =
        new(nameof(ImportExpression), null, SKAttributes.Node, SKAttributes.Expression);
    public static readonly SyntaxKind NameExpression =
        new(nameof(NameExpression), null, SKAttributes.Node, SKAttributes.Expression);
    public static readonly SyntaxKind BinaryExpression =
        new(nameof(BinaryExpression), null, SKAttributes.Node, SKAttributes.Expression);
    public static readonly SyntaxKind ParenthesizedExpression =
        new(nameof(ParenthesizedExpression), null, SKAttributes.Node, SKAttributes.Expression);
    public static readonly SyntaxKind UnaryExpression =
        new(nameof(UnaryExpression), null, SKAttributes.Node, SKAttributes.Expression);
    public static readonly SyntaxKind MemberAccessExpression =
        new(nameof(MemberAccessExpression), null, SKAttributes.Node, SKAttributes.Expression);
    public static readonly SyntaxKind AssignmentExpression =
        new(nameof(AssignmentExpression), null, SKAttributes.Node, SKAttributes.Expression);
    public static readonly SyntaxKind InvocationExpression =
        new(nameof(InvocationExpression), null, SKAttributes.Node, SKAttributes.Expression);
    public static readonly SyntaxKind NewExpression =
        new(nameof(NewExpression), null, SKAttributes.Node, SKAttributes.Expression);
    public static readonly SyntaxKind ThisExpression =
        new(nameof(ThisExpression), null, SKAttributes.Node, SKAttributes.Expression);
    public static readonly SyntaxKind DefaultExpression =
        new(nameof(DefaultExpression), null, SKAttributes.Node, SKAttributes.Expression);
    public static readonly SyntaxKind NewArrayExpression =
        new(nameof(NewArrayExpression), null, SKAttributes.Node, SKAttributes.Expression);
    public static readonly SyntaxKind ElementAccessExpression =
        new(nameof(ElementAccessExpression), null, SKAttributes.Node, SKAttributes.Expression);

#endregion
    
    // Statements
    
#region Statements
    
    public static readonly SyntaxKind VariableDeclaration =
        new(nameof(VariableDeclaration), null, SKAttributes.Node, SKAttributes.Statement);
    public static readonly SyntaxKind ExpressionStatement =
        new(nameof(ExpressionStatement), null, SKAttributes.Node, SKAttributes.Statement);
    public static readonly SyntaxKind BlockStatement =
        new(nameof(BlockStatement), null, SKAttributes.Node, SKAttributes.Statement);
    public static readonly SyntaxKind SignStatement =
        new(nameof(SignStatement), null, SKAttributes.Node, SKAttributes.Statement);
    public static readonly SyntaxKind ReturnStatement =
        new(nameof(ReturnStatement), null, SKAttributes.Node, SKAttributes.Statement);
    public static readonly SyntaxKind InvalidStatement =
        new(nameof(InvalidStatement), null, SKAttributes.Node, SKAttributes.Statement);
    
#endregion

    // Member Declarations
    
#region Member Declarations

    public static readonly SyntaxKind EnumMemberDeclaration =
        new(nameof(EnumMemberDeclaration), null, SKAttributes.Node, SKAttributes.MemberDeclaration);
    public static readonly SyntaxKind InvalidMemberDeclaration =
        new(nameof(InvalidMemberDeclaration), null, SKAttributes.Node, SKAttributes.MemberDeclaration);
    public static readonly SyntaxKind MethodDeclaration =
        new(nameof(MethodDeclaration), null, SKAttributes.Node, SKAttributes.MemberDeclaration);
    public static readonly SyntaxKind TemplateMethodDeclaration =
        new(nameof(TemplateMethodDeclaration), null, SKAttributes.Node, SKAttributes.MemberDeclaration);
    public static readonly SyntaxKind FieldDeclaration =
        new(nameof(FieldDeclaration), null, SKAttributes.Node, SKAttributes.MemberDeclaration);
    public static readonly SyntaxKind ConstructorDeclaration =
        new(nameof(ConstructorDeclaration), null, SKAttributes.Node, SKAttributes.MemberDeclaration);

#endregion
    
    // Type Declarations
    
#region Type Declarations
    
    public static readonly SyntaxKind StructDeclaration =
        new(nameof(StructDeclaration), null, SKAttributes.Node, SKAttributes.TypeDeclaration);
    public static readonly SyntaxKind EnumDeclaration =
        new(nameof(EnumDeclaration), null, SKAttributes.Node, SKAttributes.TypeDeclaration);
    public static readonly SyntaxKind TemplateDeclaration =
        new(nameof(TemplateDeclaration), null, SKAttributes.Node, SKAttributes.TypeDeclaration);
    public static readonly SyntaxKind InvalidTypeDeclaration =
        new(nameof(InvalidTypeDeclaration), null, SKAttributes.Node, SKAttributes.TypeDeclaration);
    
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
            !attributes.Contains(SKAttributes.IsFixed))
        {
            attributes = attributes.Append(SKAttributes.IsFixed).ToArray();
        }

        // Only doing this for tokens, because I'm too lazy to add the 'Token' attribute to all tokens
        if (Name.Contains("Token") &&
            !attributes.Contains(SKAttributes.Token))
        {
            attributes = attributes.Append(SKAttributes.Token).ToArray();
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

    public static ImmutableArray<SyntaxKind> GetKinds(SyntaxKindAttribute attribute)
    {
        var kinds = ImmutableArray.CreateBuilder<SyntaxKind>();
        foreach (var field in typeof(SyntaxKind).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType == typeof(SyntaxKind))
            {
                var kind = (SyntaxKind)field.GetValue(null)!;
                if (kind.TryGetAttribute(attribute, out _))
                {
                    kinds.Add(kind);
                }
            }
        }
        
        return kinds.ToImmutable();
    }

    public static SyntaxKind GetKind(string name)
    {
        foreach (var field in typeof(SyntaxKind).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.FieldType == typeof(SyntaxKind))
            {
                var kind = (SyntaxKind)field.GetValue(null)!;
                if (kind.Name == name)
                {
                    return kind;
                }
            }
        }

        throw new ArgumentException($"SyntaxKind '{name}' does not exist.", nameof(name));
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
        if (!TryGetAttribute(SKAttributes.Token, out _))
        {
            throw new NotATokenException(this);
        }

        var leadingTrivia = ImmutableArray<SyntaxTrivia>.Empty;
        var trailingTrivia = ImmutableArray<SyntaxTrivia>.Empty;
        return new SyntaxToken(syntaxTree, this, position, text, value, leadingTrivia, trailingTrivia);
    }

    public override string ToString() => Name;
}

