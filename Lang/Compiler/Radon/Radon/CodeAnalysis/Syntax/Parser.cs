using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Syntax.Nodes.Clauses;
using Radon.CodeAnalysis.Syntax.Nodes.Directives;
using Radon.CodeAnalysis.Syntax.Nodes.Expressions;
using Radon.CodeAnalysis.Syntax.Nodes.Members;
using Radon.CodeAnalysis.Syntax.Nodes.Statements;
using Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations;
using Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations.Bodies;

namespace Radon.CodeAnalysis.Syntax;

internal sealed class Parser
{
    private readonly SyntaxTree _syntaxTree;
    private ImmutableArray<SyntaxToken> _tokens;
    
    private int _position;
    
    public DiagnosticBag Diagnostics { get; }

    public Parser(SyntaxTree syntaxTree)
    {
        var tokens = new List<SyntaxToken>();
        var badTokens = new List<SyntaxToken>();
        var lexer = new Lexer(syntaxTree);
        SyntaxToken token;
        do
        {
            token = lexer.Lex();
            if (token.Kind == SyntaxKind.BadToken)
            {
                badTokens.Add(token);
            }
            else
            {
                if (badTokens.Count > 0)
                {
                    var leadingTrivia = token.LeadingTrivia.ToBuilder();
                    var index = 0;
                    foreach (var badToken in badTokens)
                    {
                        foreach (var lt in badToken.LeadingTrivia)
                        {
                            leadingTrivia.Insert(index++, lt);
                        }
                        
                        var trivia = new SyntaxTrivia(syntaxTree, SyntaxKind.SkippedTextTrivia, badToken.Position, badToken.Text);
                        leadingTrivia.Insert(index++, trivia);
                        foreach (var tt in badToken.TrailingTrivia)
                        {
                            leadingTrivia.Insert(index++, tt);
                        }
                    }
                    
                    badTokens.Clear();
                    token = new SyntaxToken(syntaxTree, token.Kind, token.Position, token.Text, token.Value, leadingTrivia.ToImmutable(), token.TrailingTrivia);
                }
                
                tokens.Add(token);
            }
        } while (token.Kind != SyntaxKind.EndOfFileToken);
        
        Diagnostics = lexer.Diagnostics;
        _syntaxTree = syntaxTree;
        _tokens = tokens.ToImmutableArray();
    }
    
    private SyntaxToken Peek(int offset)
    {
        var index = _position + offset;
        return index >= _tokens.Length ? _tokens[^1] : _tokens[index];
    }
    
    private SyntaxToken Current => Peek(0);
    
    private SyntaxToken NextToken()
    {
        var current = Current;
        _position++;
        return current;
    }
    
    private SyntaxToken MatchToken(SyntaxKind kind)
    {
        if (Current.Kind == kind)
        {
            return NextToken();
        }
        
        Diagnostics.ReportUnexpectedToken(Current.Location, Current.Text, kind);
        _position++;
        return kind.ManifestToken(_syntaxTree, Current.Position);
    }

    private SyntaxToken MatchToken(SyntaxKindAttribute attribute)
    {
        var matching = SyntaxKind.GetKinds(attribute);
        if (matching.Contains(Current.Kind))
        {
            return NextToken();
        }
        
        Diagnostics.ReportUnexpectedToken(Current.Location, Current.Kind, matching);
        return (matching.Length == 0 ? SyntaxKind.BadToken : matching[0]).ManifestToken(_syntaxTree, Current.Position);
    }
    
    public ImmutableSyntaxList<DirectiveSyntax> ParseDirectives()
    {
        var directives = ImmutableArray.CreateBuilder<DirectiveSyntax>();
        while (Current.Kind == SyntaxKind.HashToken)
        {
            var directive = ParseDirective();
            directives.Add(directive);
        }

        return new ImmutableSyntaxList<DirectiveSyntax>(directives.ToImmutable());
    }

    private DirectiveSyntax ParseDirective()
    {
        var hashToken = MatchToken(SyntaxKind.HashToken);
        if (!Current.Kind.TryGetAttribute(SKAttributes.DirectiveOperator, out _))
        {
            var expected = SyntaxKind.GetKinds(SKAttributes.DirectiveOperator);
            Diagnostics.ReportUnfinishedDirective(Current.Location, expected);
            return new InvalidDirectiveSyntax(_syntaxTree, hashToken, Current);
        }

        if (Current.Kind == SyntaxKind.IncludeKeyword)
        {
            var includeKeyword = NextToken();
            var stringToken = MatchToken(SyntaxKind.StringToken);
            return new IncludeDirectiveSyntax(_syntaxTree, hashToken, includeKeyword, stringToken);
        }

        Diagnostics.ReportUnexpectedToken(Current.Location, Current.Kind);
        return new InvalidDirectiveSyntax(_syntaxTree, hashToken, Current);
    }
    
    public CompilationUnitSyntax ParseCompilationUnit(ImmutableSyntaxList<DirectiveSyntax> directives)
    {
        var firstToken = Peek(0);
        var leadingTrivia = firstToken.LeadingTrivia.ToBuilder();
        if (directives.Count == 0)
        {
            goto skip;
        }
        
        foreach (var directive in directives)
        {
            leadingTrivia.Add(new SyntaxTrivia(_syntaxTree, SyntaxKind.DirectiveTrivia, directive.Position, directive.Text));
        }

        if (leadingTrivia.Count > 1)
        {
            var firstTrivia = leadingTrivia[0];
            // Move the first trivia to the end of the list.
            leadingTrivia.RemoveAt(0);
            leadingTrivia.Add(firstTrivia);
        }

        firstToken = new SyntaxToken(_syntaxTree, firstToken.Kind, firstToken.Position, firstToken.Text, 
                                     firstToken.Value, leadingTrivia.ToImmutable(), firstToken.TrailingTrivia);
        _tokens = _tokens.SetItem(_position, firstToken);

skip:
        var isCodeUnit = Current.Kind.TryGetAttribute(SKAttributes.TypeModifier, out _) ||
                           Current.Kind.TryGetAttribute(SKAttributes.TypeKeyword, out _);

        if (isCodeUnit)
        {
            var typeDeclarations = ParseTypeDeclarations();
            var endOfFileToken = MatchToken(SyntaxKind.EndOfFileToken);
            return new CodeCompilationUnitSyntax(_syntaxTree, typeDeclarations, endOfFileToken);
        }
        
        // Get the first token, and add the directives to the leading trivia.
        var statements = ParseStatements(SyntaxKind.EndOfFileToken);
        var eofToken = MatchToken(SyntaxKind.EndOfFileToken);
        return new PluginCompilationUnitSyntax(_syntaxTree, statements, eofToken);
    }

    private ImmutableSyntaxList<TypeDeclarationSyntax> ParseTypeDeclarations()
    {
        var types = ImmutableArray.CreateBuilder<TypeDeclarationSyntax>();
        if (Current.Kind.TryGetAttribute(SKAttributes.Modifier, out _) ||
            Current.Kind.TryGetAttribute(SKAttributes.TypeKeyword, out _))
        {
            while (Current.Kind.TryGetAttribute(SKAttributes.Modifier, out _) ||
                   Current.Kind.TryGetAttribute(SKAttributes.TypeKeyword, out _))
            {
                var current = Current;
                var type = ParseTypeDeclaration();
                types.Add(type);
                if (Current == current)
                {
                    NextToken();
                }
            }
        }
        else
        {
            Diagnostics.ReportInvalidTypeStart(Current.Location, Current.Text);
        }

        return new ImmutableSyntaxList<TypeDeclarationSyntax>(types.ToImmutable());
    }
    
    private TypeDeclarationSyntax ParseTypeDeclaration()
    {
        var modifiers = ParseModifiers();
        var keyword = MatchToken(SKAttributes.TypeKeyword);
        if (keyword.Kind == SyntaxKind.StructKeyword)
        {
            return ParseStructDeclaration(modifiers, keyword);
        }
        
        if (keyword.Kind == SyntaxKind.EnumKeyword)
        {
            return ParseEnumDeclaration(modifiers, keyword);
        }
        
        Diagnostics.ReportInvalidTypeDeclaration(keyword.Location, keyword.Text);
        return new InvalidTypeDeclarationSyntax(_syntaxTree, modifiers, keyword);
    }
    
    private ImmutableSyntaxList<SyntaxToken> ParseModifiers()
    {
        var modifiers = ImmutableArray.CreateBuilder<SyntaxToken>();
        while (Current.Kind.TryGetAttribute(SKAttributes.Modifier, out _))
        {
            var modifier = NextToken();
            modifiers.Add(modifier);
        }
        
        return new ImmutableSyntaxList<SyntaxToken>(modifiers.ToImmutable());
    }
    
    private StructDeclarationSyntax ParseStructDeclaration(ImmutableSyntaxList<SyntaxToken> modifiers, SyntaxToken keyword)
    {
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var body = ParseStructBody();
        return new StructDeclarationSyntax(_syntaxTree, modifiers, keyword, identifier, body);
    }
    
    private StructBodySyntax ParseStructBody()
    {
        var openBrace = MatchToken(SyntaxKind.OpenBraceToken);
        var members = ParseMembers();
        var closeBrace = MatchToken(SyntaxKind.CloseBraceToken);
        return new StructBodySyntax(_syntaxTree, openBrace, members, closeBrace);
    }
    
    private ImmutableSyntaxList<MemberDeclarationSyntax> ParseMembers()
    {
        var members = ImmutableArray.CreateBuilder<MemberDeclarationSyntax>();
        while (Current.Kind != SyntaxKind.CloseBraceToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            var startToken = Current;
            var member = ParseMember();
            members.Add(member);
            if (Current == startToken)
            {
                NextToken();
            }
        }
        
        return new ImmutableSyntaxList<MemberDeclarationSyntax>(members.ToImmutable());
    }

    private MemberDeclarationSyntax ParseMember()
    {
        var modifiers = ParseModifiers();
        if (Current.Kind == SyntaxKind.IdentifierToken /* Type */ &&
            (Peek(1).Kind == SyntaxKind.IdentifierToken ||
             Peek(1).Kind.TryGetAttribute(SKAttributes.Operator, out _)) /* Name or operator */ &&
            Peek(2).Kind == SyntaxKind.OpenParenthesisToken)
        {
            var type = ParseTypeClause();
            var isOperator = false;
            foreach (var modifier in modifiers)
            {
                if (modifier.Kind == SyntaxKind.OperatorKeyword)
                {
                    isOperator = true;
                    break;
                }
            }
            
            SyntaxToken identifier;
            if (isOperator)
            {
                identifier = MatchToken(SKAttributes.Operator);
                if (identifier.Kind.TryGetAttribute(SKAttributes.Operator, out var op))
                {
                    var opData = op.GetValue<OperatorData>();
                    if (opData!.Precedence is (int)OperatorPrecedence.Assignment &&
                        !opData.IsBinaryOperator)
                    {
                        Diagnostics.ReportCannotOverloadAssignmentOperator(identifier.Location, identifier.Text);
                    }
                }
            }
            else
            {
                identifier = MatchToken(SyntaxKind.IdentifierToken);
            }
            
            var parameterList = ParseParameterList();
            var parseBody = true;
            foreach (var modifier in modifiers)
            {
                if (modifier.Kind == SyntaxKind.RuntimeInternalKeyword)
                {
                    parseBody = false;
                    break;
                }
            }

            var body = ParseBlockStatement();
            if (parseBody)
            {
            }
            else if (Current.Kind == SyntaxKind.OpenBraceToken)
            {
                var unexpected = ParseBlockStatement();
                Diagnostics.ReportRuntimeInternalMethodWithBody(unexpected.Location);
            }
            
            return new MethodDeclarationSyntax(_syntaxTree, modifiers, type, identifier, parameterList, body);
        }

        if (Current.Kind == SyntaxKind.IdentifierToken &&
            Peek(1).Kind == SyntaxKind.OpenParenthesisToken)
        {
            var type = ParseTypeClause();
            var parameterList = ParseParameterList();
            var body = ParseBlockStatement();
            return new ConstructorDeclarationSyntax(_syntaxTree, modifiers, type, parameterList, body);
        }

        if (Current.Kind == SyntaxKind.IdentifierToken &&
            Peek(1).Kind == SyntaxKind.IdentifierToken)
        {
            // It's most likely a field declaration
            var type = ParseTypeClause();
            var initializer = ParseVariableDeclarator();
            return new FieldDeclarationSyntax(_syntaxTree, modifiers, type, initializer);
        }

        Diagnostics.ReportUnexpectedToken(Current.Location, Current.Kind);
        NextToken(); // Skip the unexpected token, otherwise we'll get stuck in an infinite loop
        return new InvalidMemberDeclarationSyntax(_syntaxTree, modifiers, Current);
    }
    
    private EnumMemberDeclarationSyntax ParseEnumMember()
    {
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        if (Current.Kind == SyntaxKind.EqualsToken)
        {
            var equals = NextToken();
            var value = ParseExpression();
            return new EnumMemberDeclarationSyntax(_syntaxTree, identifier, equals, value);
        }
        
        return new EnumMemberDeclarationSyntax(_syntaxTree, identifier, null, null);
    }

    private EnumDeclarationSyntax ParseEnumDeclaration(ImmutableSyntaxList<SyntaxToken> modifiers, SyntaxToken keyword)
    {
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var body = ParseEnumBody();
        return new EnumDeclarationSyntax(_syntaxTree, modifiers, keyword, identifier, body);
    }
    
    private EnumBodySyntax ParseEnumBody()
    {
        var openBrace = MatchToken(SyntaxKind.OpenBraceToken);
        var members = 
            ParseSeparatedSyntaxList(ParseEnumMember, SyntaxKind.CommaToken, SyntaxKind.CloseBraceToken);
        var closeBrace = MatchToken(SyntaxKind.CloseBraceToken);
        return new EnumBodySyntax(_syntaxTree, openBrace, members, closeBrace);
    }
    
    private ParameterListSyntax ParseParameterList()
    {
        var openParenthesis = MatchToken(SyntaxKind.OpenParenthesisToken);
        var parameters = ParseParameters();
        var closeParenthesis = MatchToken(SyntaxKind.CloseParenthesisToken);
        return new ParameterListSyntax(_syntaxTree, openParenthesis, parameters, closeParenthesis);
    }
    
    private ImmutableSyntaxList<ParameterSyntax> ParseParameters()
    {
        var parameters = ImmutableArray.CreateBuilder<ParameterSyntax>();
        while (Current.Kind != SyntaxKind.CloseParenthesisToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            var parameter = ParseParameter();
            parameters.Add(parameter);
            if (Current.Kind == SyntaxKind.CommaToken)
            {
                NextToken();
            }
        }
        
        return new ImmutableSyntaxList<ParameterSyntax>(parameters.ToImmutable());
    }

    private StatementSyntax ParseStatement()
    {
        if (Current.Kind == SyntaxKind.OpenBraceToken)
        {
            return ParseBlockStatement();
        }

        if (Current.Kind == SyntaxKind.SignKeyword)
        {
            return ParseSignStatement();
        }
        
        if (Current.Kind == SyntaxKind.IdentifierToken)
        {
            if (Peek(1).Kind == SyntaxKind.IdentifierToken ||
                (Peek(1).Kind == SyntaxKind.LessToken && Peek(2).Kind == SyntaxKind.IdentifierToken))
            {
                return ParseVariableDeclarationStatement();
            }
        }
        
        return ParseExpressionStatement();
    }
    
    private BlockStatementSyntax ParseBlockStatement()
    {
        var openBrace = MatchToken(SyntaxKind.OpenBraceToken);
        var statements = ParseStatements(SyntaxKind.CloseBraceToken);
        var closeBrace = MatchToken(SyntaxKind.CloseBraceToken);
        return new BlockStatementSyntax(_syntaxTree, openBrace, statements, closeBrace);
    }

    private SignStatementSyntax ParseSignStatement()
    {
        var signKeyword = MatchToken(SyntaxKind.SignKeyword);
        var colon = MatchToken(SyntaxKind.ColonToken);
        var keyString = MatchToken(SyntaxKind.StringToken);
        var comma = MatchToken(SyntaxKind.CommaToken);
        var valueString = MatchToken(SyntaxKind.StringToken);
        return new SignStatementSyntax(_syntaxTree, signKeyword, colon, keyString, comma, valueString);
    }
    
    private ImmutableSyntaxList<StatementSyntax> ParseStatements(SyntaxKind cancellationToken)
    {
        var statements = ImmutableArray.CreateBuilder<StatementSyntax>();
        while (Current.Kind != cancellationToken && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            var startToken = Current;
            var statement = ParseStatement();
            statements.Add(statement);
            
            // If ParseStatement() consumed no tokens, skip the current token
            if (Current == startToken)
            {
                NextToken();
            }
        }
        
        return new ImmutableSyntaxList<StatementSyntax>(statements.ToImmutable());
    }
    
    private VariableDeclarationSyntax ParseVariableDeclarationStatement()
    {
        var type = ParseTypeClause();
        var declarator = ParseVariableDeclarator();
        return new VariableDeclarationSyntax(_syntaxTree, type, declarator);
    }
    
    private VariableDeclaratorSyntax ParseVariableDeclarator()
    {
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        if (Current.Kind == SyntaxKind.EqualsToken)
        {
            var equals = NextToken();
            var initializer = ParseExpression();
            return new VariableDeclaratorSyntax(_syntaxTree, identifier, equals, initializer);
        }
        
        return new VariableDeclaratorSyntax(_syntaxTree, identifier, null, null);
    }
    
    private ExpressionStatementSyntax ParseExpressionStatement()
    {
        var expression = ParseExpression();
        return new ExpressionStatementSyntax(_syntaxTree, expression);
    }
    
    private ExpressionSyntax ParseExpression()
    {
        return ParseAssignmentExpression();
    }
    
    private ExpressionSyntax ParseAssignmentExpression()
    {
        var left = ParseBinaryExpression();
        if (Current.Kind == SyntaxKind.EqualsToken)
        {
            var equals = NextToken();
            var right = ParseExpression();
            return new AssignmentExpressionSyntax(_syntaxTree, left, equals, right);
        }
        
        return left;
    }
    
    private ExpressionSyntax ParseBinaryExpression(int precedence = 0)
    {
        ExpressionSyntax left;
        if (Current.Kind.TryGetAttribute(SKAttributes.Operator, out var attribute) &&
            attribute.Value is OperatorData { IsUnaryOperator: true } op &&
            op.Precedence >= precedence)
        {
            var operatorToken = NextToken();
            var operand = ParseBinaryExpression(op.Precedence);
            left = new UnaryExpressionSyntax(_syntaxTree, operatorToken, operand);
        }
        else
        {
            left = ParseMemberAccessOrInvocation();
        }

        while (Current.Kind.TryGetAttribute(SKAttributes.Operator, out var attr) &&
               attr.Value is OperatorData { IsBinaryOperator: true } binOp &&
               binOp.Precedence != 0 &&
               binOp.Precedence <= precedence)
        {
            var operatorToken = NextToken();
            var right = ParseBinaryExpression(binOp.Precedence);
            left = new BinaryExpressionSyntax(_syntaxTree, left, operatorToken, right);
        }
        
        return left;
    }
    
    private ExpressionSyntax ParseMemberAccessOrInvocation()
    {
        // We do this because MemberAccess and Invocation are left-associative and have the same precedence
        // left-associative means that we parse the left side first, then the right side
        
        var left = ParsePrimaryExpression();
        while (true)
        {
            if (Current.Kind == SyntaxKind.DotToken)
            {
                var dot = NextToken();
                var name = MatchToken(SyntaxKind.IdentifierToken);
                left = new MemberAccessExpressionSyntax(_syntaxTree, left, dot, name);
            }
            else if (Current.Kind == SyntaxKind.OpenParenthesisToken ||
                     Current.Kind == SyntaxKind.LessToken)
            {
                var typeArguments = ParseTypeArguments();
                var arguments = ParseArgumentList();
                left = new InvocationExpressionSyntax(_syntaxTree, left, typeArguments, arguments);
            }
            else
            {
                break;
            }
        }
        
        return left;
    }

    private ExpressionSyntax ParsePrimaryExpression()
    {
        if (Current.Kind == SyntaxKind.OpenParenthesisToken)
        {
            var left = NextToken();
            var expression = ParseExpression();
            var right = MatchToken(SyntaxKind.CloseParenthesisToken);
            return new ParenthesizedExpressionSyntax(_syntaxTree, left, expression, right);
        }

        if (Current.Kind == SyntaxKind.ImportKeyword)
        {
            var importKeyword = NextToken();
            var path = MatchToken(SyntaxKind.StringToken);
            var stringLiteral = new LiteralExpressionSyntax(_syntaxTree, path);
            return new ImportExpressionSyntax(_syntaxTree, importKeyword, stringLiteral);
        }

        if (Current.Kind == SyntaxKind.NewKeyword)
        {
            var newKeyword = NextToken();
            var type = ParseTypeClause();
            var arguments = ParseArgumentList();
            // Type arguments is null because they would've been parsed as part of the type clause
            return new NewExpressionSyntax(_syntaxTree, newKeyword, type, null, arguments);
        }

        if (Current.Kind == SyntaxKind.DefaultKeyword)
        {
            var defaultKeyword = NextToken();
            var colon = MatchToken(SyntaxKind.ColonToken);
            var type = ParseTypeClause();
            return new DefaultExpressionSyntax(_syntaxTree, defaultKeyword, colon, type);
        }

        if (Current.Kind == SyntaxKind.StringToken || 
            Current.Kind == SyntaxKind.NumberToken ||
            Current.Kind == SyntaxKind.TrueKeyword ||
            Current.Kind == SyntaxKind.FalseKeyword)
        {
            var literal = NextToken();
            return new LiteralExpressionSyntax(_syntaxTree, literal);
        }

        if (Current.Kind == SyntaxKind.ThisKeyword)
        {
            var thisKeyword = NextToken();
            return new ThisExpressionSyntax(_syntaxTree, thisKeyword);
        }

        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        return new NameExpressionSyntax(_syntaxTree, identifier);
    }

    private ParameterSyntax ParseParameter()
    {
        var modifiers = ParseModifiers();
        var type = ParseTypeClause();
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        return new ParameterSyntax(_syntaxTree, modifiers, type, identifier);
    }
    
    private TypeArgumentListSyntax? ParseTypeArguments()
    {
        if (Current.Kind != SyntaxKind.LessToken)
        {
            return null;
        }

        var less = NextToken();
        var arguments = ParseSeparatedSyntaxList(ParseTypeClause, SyntaxKind.CommaToken, SyntaxKind.GreaterToken);
        var greater = MatchToken(SyntaxKind.GreaterToken);
        return new TypeArgumentListSyntax(_syntaxTree, less, arguments, greater);
    }
    
    private ArgumentListSyntax ParseArgumentList()
    {
        var openParenthesis = MatchToken(SyntaxKind.OpenParenthesisToken);
        var arguments = ParseSeparatedSyntaxList(ParseExpression, SyntaxKind.CommaToken, SyntaxKind.CloseParenthesisToken);
        var closeParenthesis = MatchToken(SyntaxKind.CloseParenthesisToken);
        return new ArgumentListSyntax(_syntaxTree, openParenthesis, arguments, closeParenthesis);
    }
    
    private TypeSyntax ParseTypeClause()
    {
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var typeArguments = ParseTypeArguments();
        return new TypeSyntax(_syntaxTree, identifier, typeArguments);
    }
    
    private SeparatedSyntaxList<T> ParseSeparatedSyntaxList<T>(Func<T> parseElement, SyntaxKind separatorTokenKind, SyntaxKind cancelTokenKind)
        where T : SyntaxNode
    {
        var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
        
        // We need to copy the following variables because they are captured by the lambda and can cause memory leaks
        // if the closure's lifetime is longer than the local scope
        var parseElementCopy = parseElement;
        var separatorTokenKindCopy = separatorTokenKind;
        var cancelTokenKindCopy = cancelTokenKind;

        while (Current.Kind != cancelTokenKindCopy)
        {
            var current = Current;
            var element = parseElementCopy();
            nodesAndSeparators.Add(element);
            // If we reach the cancel token, we're done
            if (Current.Kind == cancelTokenKindCopy ||
                Current.Kind == SyntaxKind.EndOfFileToken)
            {
                break;
            }
    
            // Otherwise, we need to parse the separator token
            var separator = MatchToken(separatorTokenKindCopy);
            nodesAndSeparators.Add(separator);
            
            // If parseElement() didn't consume any tokens,
            // we need to skip the current token and continue
            // in order to avoid an infinite loop
            if (Current == current)
            {
                NextToken();
            }
        }

        return new SeparatedSyntaxList<T>(nodesAndSeparators.ToImmutable());
    }
}