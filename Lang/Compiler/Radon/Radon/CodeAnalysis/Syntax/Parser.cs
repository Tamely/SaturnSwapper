using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Syntax.Nodes.Clauses;
using Radon.CodeAnalysis.Syntax.Nodes.Directives;
using Radon.CodeAnalysis.Syntax.Nodes.Expressions;
using Radon.CodeAnalysis.Syntax.Nodes.Members;
using Radon.CodeAnalysis.Syntax.Nodes.Statements;
using Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations;
using Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations.Bodies;
using Radon.CodeAnalysis.Text;

namespace Radon.CodeAnalysis.Syntax;

internal sealed class Parser
{
    private readonly SyntaxTree _syntaxTree;
    private ImmutableArray<SyntaxToken> _tokens;
    
    private int _position;
    private bool _shouldBreakRightShift;

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

    private SyntaxToken Current
    {
        get
        {
            var current = Peek(0);
            if (_shouldBreakRightShift && current.Kind == SyntaxKind.GreaterGreaterToken)
            {
                // we split the right shift into two tokens
                var first = new SyntaxToken(_syntaxTree, SyntaxKind.GreaterThanToken, current.Position, ">", null
                    , ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
                var second = new SyntaxToken(_syntaxTree, SyntaxKind.GreaterThanToken, current.Position + 1, ">", null
                    , ImmutableArray<SyntaxTrivia>.Empty, ImmutableArray<SyntaxTrivia>.Empty);
                    
                // Remove the right shift token and insert the two new tokens
                _tokens = _tokens.RemoveAt(_position).InsertRange(_position, new[] {first, second});
            }
                
            return current;
        }
    }
    
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
        return kind.ManifestToken(_syntaxTree, Current.Position);
    }
    
    private SyntaxToken ExpectToken(SyntaxKind kind)
    {
        if (Current.Kind == kind)
        {
            return NextToken();
        }
        
        // Get the location of the Current tokens leading trivia
        var previous = Peek(-1);
        var location = previous.Location;
        if (previous.TrailingTrivia.Length > 0)
        {
            var trivia = previous.TrailingTrivia[0];
            var start = trivia.Position;
            var length = trivia.Text.Length;
            var span = new TextSpan(start, length);
            location = new TextLocation(_syntaxTree.Text, span);
        }
        
        Diagnostics.ReportExpectedToken(location, kind);
        return kind.ManifestToken(_syntaxTree, Current.Position);
    }
    
    private void ResetPos(int pos)
    {
        _position = pos;
        Diagnostics.Unblock();
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
        if (!Current.Kind.HasAttribute(SKAttributes.DirectiveOperator))
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

        Diagnostics.ReportUnexpectedToken(Current.Location, Current.Text);
        return new InvalidDirectiveSyntax(_syntaxTree, hashToken, Current);
    }
    
    public CompilationUnitSyntax ParseCompilationUnit(ImmutableSyntaxList<DirectiveSyntax> directives)
    {
        var firstToken = Peek(0);
        var leadingTrivia = firstToken.LeadingTrivia.ToBuilder();
        if (directives.Count == 0)
        {
            goto Skip;
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

Skip:
        var isCodeUnit = Current.Kind.HasAttribute(SKAttributes.TypeModifier) ||
                           Current.Kind.HasAttribute(SKAttributes.TypeKeyword);

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
        if (Current.Kind.HasAttribute(SKAttributes.Modifier) ||
            Current.Kind.HasAttribute(SKAttributes.TypeKeyword))
        {
            while (Current.Kind.HasAttribute(SKAttributes.Modifier) ||
                   Current.Kind.HasAttribute(SKAttributes.TypeKeyword))
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

        if (keyword.Kind == SyntaxKind.TemplateKeyword)
        {
            return ParseTemplateDeclaration(modifiers, keyword);
        }
        
        Diagnostics.ReportInvalidTypeDeclaration(keyword.Location, keyword.Text);
        return new InvalidTypeDeclarationSyntax(_syntaxTree, modifiers, keyword);
    }
    
    private ImmutableSyntaxList<SyntaxToken> ParseModifiers()
    {
        var modifiers = ImmutableArray.CreateBuilder<SyntaxToken>();
        while (Current.Kind.HasAttribute(SKAttributes.Modifier))
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
        var closeBrace = ExpectToken(SyntaxKind.CloseBraceToken);
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
        if (Current.Kind == SyntaxKind.TemplateKeyword &&
            Peek(1).Kind == SyntaxKind.IdentifierToken)
        {
            var invalidTypeModifier = modifiers.FirstOrDefault(m => !m.Kind.HasAttribute(SKAttributes.TypeModifier));
            if (invalidTypeModifier is not null)
            {
                Diagnostics.ReportInvalidTypeModifier(invalidTypeModifier.Location, invalidTypeModifier.Text);
            }
            
            var templateKeyword = MatchToken(SyntaxKind.TemplateKeyword);
            var type = ParseTypeClause();
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var typeParameters = ParseTypeParameters();
            var parameters = ParseParameterList();
            var body = ParseBlockStatement();
            return new TemplateMethodDeclarationSyntax(_syntaxTree, modifiers, templateKeyword, type, identifier, typeParameters, parameters, body);
        }

        if (Current.Kind == SyntaxKind.IdentifierToken)
        {
            Diagnostics.Block();
            var pos = _position;
            if (!IsValidType())
            {
                goto Skip;
            }
            
            if (Current.Kind == SyntaxKind.IdentifierToken)
            {
                NextToken();
                if (Current.Kind == SyntaxKind.OpenParenthesisToken)
                {
                    ResetPos(pos);
                    return ParseMethod(modifiers);
                }
                    
                ResetPos(pos);
                return ParseField(modifiers);
            }
            
            if (Current.Kind == SyntaxKind.OpenParenthesisToken)
            {
                ResetPos(pos);
                var type = ParseTypeClause();
                var parameterList = ParseParameterList();
                var body = ParseBlockStatement();
                return new ConstructorDeclarationSyntax(_syntaxTree, modifiers, type, parameterList, body);
            }
        }

        Skip:
        Diagnostics.ReportUnexpectedToken(Current.Location, Current.Text);
        NextToken(); // Skip the unexpected token, otherwise we'll get stuck in an infinite loop
        return new InvalidMemberDeclarationSyntax(_syntaxTree, modifiers, Current);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MethodDeclarationSyntax ParseMethod(ImmutableSyntaxList<SyntaxToken> modifiers)
    {
        var invalidMethodModifier = modifiers.FirstOrDefault(m => !m.Kind.HasAttribute(SKAttributes.MethodModifier));
        if (invalidMethodModifier is not null)
        {
            Diagnostics.ReportInvalidMethodModifier(invalidMethodModifier.Location, invalidMethodModifier.Text);
        }
        
        var methodType = ParseTypeClause();
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var parameterList = ParseParameterList();
        var body = ParseBlockStatement();
        return new MethodDeclarationSyntax(_syntaxTree, modifiers, methodType, identifier, parameterList, body);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private FieldDeclarationSyntax ParseField(ImmutableSyntaxList<SyntaxToken> modifiers)
    {
        var invalidFieldModifier = modifiers.FirstOrDefault(m => !m.Kind.HasAttribute(SKAttributes.FieldModifier));
        if (invalidFieldModifier is not null)
        {
            Diagnostics.ReportInvalidFieldModifier(invalidFieldModifier.Location, invalidFieldModifier.Text);
        }
        
        var fieldType = ParseTypeClause();
        var initializer = ParseVariableDeclarator();
        var semicolon = ExpectToken(SyntaxKind.SemicolonToken);
        return new FieldDeclarationSyntax(_syntaxTree, modifiers, fieldType, initializer, semicolon);
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
        var members = ParseSeparatedSyntaxList(ParseEnumMember, SyntaxKind.CommaToken, SyntaxKind.CloseBraceToken);
        var closeBrace = MatchToken(SyntaxKind.CloseBraceToken);
        return new EnumBodySyntax(_syntaxTree, openBrace, members, closeBrace);
    }

    private TemplateDeclarationSyntax ParseTemplateDeclaration(ImmutableSyntaxList<SyntaxToken> modifiers, SyntaxToken keyword)
    {
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var typeParameters = ParseTypeParameters();
        var body = ParseStructBody();
        return new TemplateDeclarationSyntax(_syntaxTree, modifiers, keyword, identifier, typeParameters, body);
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

        if (Current.Kind == SyntaxKind.ReturnKeyword)
        {
            return ParseReturnStatement();
        }

        if (Current.Kind == SyntaxKind.IfKeyword)
        {
            return ParseIfStatement();
        }
        
        if (Current.Kind == SyntaxKind.WhileKeyword)
        {
            return ParseWhileStatement();
        }
        
        if (Current.Kind == SyntaxKind.ForKeyword)
        {
            return ParseForStatement();
        }
        
        if (Current.Kind == SyntaxKind.BreakKeyword)
        {
            return ParseBreakStatement();
        }
        
        if (Current.Kind == SyntaxKind.ContinueKeyword)
        {
            return ParseContinueStatement();
        }
        
        if (Current.Kind == SyntaxKind.IdentifierToken)
        {
            var pos = _position;
            Diagnostics.Block();
            if (!IsValidType())
            {
                goto Skip;
            }
            
            if (Current.Kind == SyntaxKind.IdentifierToken)
            {
                NextToken();
                if (Current.Kind == SyntaxKind.EqualsToken)
                {
                    ResetPos(pos);
                    return ParseVariableDeclarationStatement();
                }
            }

            Skip:
            ResetPos(pos);
        }
        
        return ParseExpressionStatement();
    }

    private bool IsValidType()
    {
        if (Current.Kind != SyntaxKind.IdentifierToken)
        {
            return false;
        }
        
        NextToken();
        if (Current.Kind == SyntaxKind.LessThanToken)
        {
            // We need to be able to differentiate between Foo < 10 and Foo<10> bar;
            // Foo<10> is not valid, but we still want to parse it as if it was, and report that there should be a name
            if (!IsValidTypeArgumentList(out var position))
            {
                return false;
            }
                
            _position = position;
        }
            
        if (Current.Kind == SyntaxKind.OpenBracketToken) // Array type
        {
            NextToken(); // Skip the open bracket
            NextToken(); // Skip the close bracket
        }
        
        return true;
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
        var key = ParseExpression();
        var comma = MatchToken(SyntaxKind.CommaToken);
        var value = ParseExpression();
        return new SignStatementSyntax(_syntaxTree, signKeyword, colon, key, comma, value);
    }
    
    private ReturnStatementSyntax ParseReturnStatement()
    {
        var returnKeyword = MatchToken(SyntaxKind.ReturnKeyword);
        if (Current.Kind == SyntaxKind.SemicolonToken)
        {
            var semicolon = NextToken();
            return new ReturnStatementSyntax(_syntaxTree, returnKeyword, null, semicolon);
        }
        
        var expression = ParseExpression();
        var exprSemicolon = ExpectToken(SyntaxKind.SemicolonToken);
        return new ReturnStatementSyntax(_syntaxTree, returnKeyword, expression, exprSemicolon);
    }

    private IfStatementSyntax ParseIfStatement()
    {
        var ifKeyword = MatchToken(SyntaxKind.IfKeyword);
        var openParenthesis = MatchToken(SyntaxKind.OpenParenthesisToken);
        var condition = ParseExpression();
        var closeParenthesis = MatchToken(SyntaxKind.CloseParenthesisToken);
        var body = ParseStatement();
        if (Current.Kind != SyntaxKind.ElseKeyword)
        {
            return new IfStatementSyntax(_syntaxTree, ifKeyword, openParenthesis, condition, closeParenthesis, body, null);
        }
        
        var elseClause = ParseElseClause();
        return new IfStatementSyntax(_syntaxTree, ifKeyword, openParenthesis, condition, closeParenthesis, body, elseClause);

    }

    private WhileStatementSyntax ParseWhileStatement()
    {
        var whileKeyword = MatchToken(SyntaxKind.WhileKeyword);
        var openParenthesis = MatchToken(SyntaxKind.OpenParenthesisToken);
        var condition = ParseExpression();
        var closeParenthesis = MatchToken(SyntaxKind.CloseParenthesisToken);
        var body = ParseStatement();
        return new WhileStatementSyntax(_syntaxTree, whileKeyword, openParenthesis, condition, closeParenthesis, body);
    }

    private ForStatementSyntax ParseForStatement()
    {
        var forKeyword = MatchToken(SyntaxKind.ForKeyword);
        var openParenthesis = MatchToken(SyntaxKind.OpenParenthesisToken);
        var initializer = ParseStatement();
        var condition = ParseExpression();
        var conditionSemicolon = MatchToken(SyntaxKind.SemicolonToken);
        var incrementor = ParseExpression();
        if (!incrementor.CanBeStatement)
        {
            Diagnostics.ReportExpressionCannotBeStatement(incrementor.Location, incrementor);
        }
        
        var closeParenthesis = MatchToken(SyntaxKind.CloseParenthesisToken);
        var body = ParseStatement();
        return new ForStatementSyntax(_syntaxTree, forKeyword, openParenthesis, initializer, condition, 
            conditionSemicolon, incrementor, closeParenthesis, body);
    }
    
    private BreakStatementSyntax ParseBreakStatement()
    {
        var breakKeyword = MatchToken(SyntaxKind.BreakKeyword);
        var semicolon = ExpectToken(SyntaxKind.SemicolonToken);
        return new BreakStatementSyntax(_syntaxTree, breakKeyword, semicolon);
    }
    
    private ContinueStatementSyntax ParseContinueStatement()
    {
        var continueKeyword = MatchToken(SyntaxKind.ContinueKeyword);
        var semicolon = ExpectToken(SyntaxKind.SemicolonToken);
        return new ContinueStatementSyntax(_syntaxTree, continueKeyword, semicolon);
    }

    private VariableDeclarationSyntax ParseVariableDeclarationStatement()
    {
        var type = ParseTypeClause();
        var declarator = ParseVariableDeclarator();
        var semicolon = ExpectToken(SyntaxKind.SemicolonToken);
        return new VariableDeclarationSyntax(_syntaxTree, type, declarator, semicolon);
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
        var semicolon = ExpectToken(SyntaxKind.SemicolonToken);
        if (!expression.CanBeStatement)
        {
            Diagnostics.ReportExpressionCannotBeStatement(expression.Location, expression);
        }
        
        return new ExpressionStatementSyntax(_syntaxTree, expression, semicolon);
    }
    
    private ExpressionSyntax ParseExpression()
    {
        return ParseAssignmentExpression();
    }
    
    private ExpressionSyntax ParseAssignmentExpression()
    {
        var left = ParseBinaryExpression();
        if (Current.Kind == SyntaxKind.EqualsToken || Current.Kind == SyntaxKind.PlusEqualsToken ||
            Current.Kind == SyntaxKind.MinusEqualsToken || Current.Kind == SyntaxKind.StarEqualsToken ||
            Current.Kind == SyntaxKind.SlashEqualsToken || Current.Kind == SyntaxKind.PercentEqualsToken ||
            Current.Kind == SyntaxKind.PipeEqualsToken || Current.Kind == SyntaxKind.AmpersandEqualsToken)
        {
            var equals = NextToken();
            var right = ParseExpression();
            return new AssignmentExpressionSyntax(_syntaxTree, left, equals, right);
        }
        
        return left;
    }

    private ExpressionSyntax ParseBinaryExpression(int parentPrecedence = 0)
    {
        ExpressionSyntax left;
        if (Current.Kind.TryGetAttribute(SKAttributes.Operator, out var attribute) &&
            attribute.Value is OperatorData { IsPrefixUnaryOperator: true } prefix &&
            prefix.Precedence >= parentPrecedence)
        {
            var operatorToken = NextToken();
            var operand = ParseBinaryExpression(prefix.Precedence);
            left = new UnaryExpressionSyntax(_syntaxTree, operatorToken, operand);
        }
        else
        {
            left = ParseCastExpression();
        }

        if (Current.Kind.TryGetAttribute(SKAttributes.Operator, out var attribute1) &&
            attribute1.Value is OperatorData { IsPostfixUnaryOperator: true } postfix &&
            postfix.Precedence >= parentPrecedence)
        {
            var operatorToken = NextToken();
            left = new UnaryExpressionSyntax(_syntaxTree, operatorToken, left);
        }
        
        while (true)
        {
            if (!Current.Kind.TryGetAttribute(SKAttributes.Operator, out var attr))
            {
                break;
            }

            var opData = (OperatorData)attr.Value!;
            var precedence = opData.Precedence;
            if (precedence == 0 || precedence <= parentPrecedence)
            {
                break;
            }
            
            var operatorToken = NextToken();
            var right = ParseBinaryExpression(precedence);
            left = new BinaryExpressionSyntax(_syntaxTree, left, operatorToken, right);
        }
        
        return left;
    }
    
    private ExpressionSyntax ParseCastExpression()
    {
        var pos = _position;
        // type: expression
        //
        // int x = 100;
        // byte b = byte: x;
        if (IsValidType() && Current.Kind == SyntaxKind.ColonToken)
        {
            ResetPos(pos);
            var type = ParseTypeClause();
            var colon = MatchToken(SyntaxKind.ColonToken);
            var expression = ParseCastExpression();
            return new CastExpressionSyntax(_syntaxTree, type, colon, expression);
        }
        
        ResetPos(pos);
        return ParseMemberAccessOrInvocation();
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
                var name = ExpectToken(SyntaxKind.IdentifierToken);
                left = new MemberAccessExpressionSyntax(_syntaxTree, left, dot, name);
            }
            else if (Current.Kind == SyntaxKind.OpenParenthesisToken ||
                     IsValidTypeArgumentList(out _))
            {
                var typeArguments = ParseTypeArguments();
                var arguments = ParseArgumentList();
                left = new InvocationExpressionSyntax(_syntaxTree, left, typeArguments, arguments);
            }
            else if (Current.Kind == SyntaxKind.OpenBracketToken)
            {
                var openBracket = NextToken();
                var argument = ParseExpression();
                var closeBracket = MatchToken(SyntaxKind.CloseBracketToken);
                left = new ElementAccessExpressionSyntax(_syntaxTree, left, openBracket, argument, closeBracket);
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
            if (type is ArrayTypeSyntax array)
            {
                return new NewArrayExpressionSyntax(_syntaxTree, newKeyword, array);
            }
            
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

        if (Current.Kind.HasAttribute(SKAttributes.Literal))
        {
            var literal = NextToken();
            return new LiteralExpressionSyntax(_syntaxTree, literal);
        }

        if (Current.Kind == SyntaxKind.ThisKeyword)
        {
            var thisKeyword = NextToken();
            return new ThisExpressionSyntax(_syntaxTree, thisKeyword);
        }

        var identifier = ExpectToken(SyntaxKind.IdentifierToken);
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
        if (Current.Kind != SyntaxKind.LessThanToken)
        {
            return null;
        }

        _shouldBreakRightShift = true;
        var less = NextToken();
        var arguments = ParseSeparatedSyntaxList(ParseTypeClause, SyntaxKind.CommaToken, SyntaxKind.GreaterThanToken);
        var greater = ExpectToken(SyntaxKind.GreaterThanToken);
        _shouldBreakRightShift = false;
        return new TypeArgumentListSyntax(_syntaxTree, less, arguments, greater);
    }

    private ArgumentListSyntax ParseArgumentList()
    {
        var openParenthesis = ExpectToken(SyntaxKind.OpenParenthesisToken);
        var arguments = ParseSeparatedSyntaxList(ParseExpression, SyntaxKind.CommaToken, SyntaxKind.CloseParenthesisToken);
        var closeParenthesis = ExpectToken(SyntaxKind.CloseParenthesisToken);
        return new ArgumentListSyntax(_syntaxTree, openParenthesis, arguments, closeParenthesis);
    }
    
    private TypeSyntax ParseTypeClause()
    {
        _shouldBreakRightShift = true;
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        var typeArguments = ParseTypeArguments();
        var type = new TypeSyntax(_syntaxTree, identifier, typeArguments);
        _shouldBreakRightShift = false;
        return Current.Kind == SyntaxKind.OpenBracketToken ? ParseArrayType(type) : type;
    }

    private ArrayTypeSyntax ParseArrayType(TypeSyntax? type)
    {
        _shouldBreakRightShift = true;
        type ??= ParseTypeClause();
        var openBracket = MatchToken(SyntaxKind.OpenBracketToken);
        ExpressionSyntax? size = null;
        if (Current.Kind != SyntaxKind.CloseBracketToken)
        {
            size = ParseExpression();
        }
        
        var closeBracket = MatchToken(SyntaxKind.CloseBracketToken);
        _shouldBreakRightShift = false;
        return new ArrayTypeSyntax(_syntaxTree, type, openBracket, size, closeBracket);
    }

    private TypeParameterListSyntax ParseTypeParameters()
    {
        var openAngle = MatchToken(SyntaxKind.LessThanToken);
        var parameters = ParseSeparatedSyntaxList(ParseTypeParameter, SyntaxKind.CommaToken, SyntaxKind.GreaterThanToken);
        var closeAngle = MatchToken(SyntaxKind.GreaterThanToken);
        return new TypeParameterListSyntax(_syntaxTree, openAngle, parameters, closeAngle);
    }
    
    private TypeParameterSyntax ParseTypeParameter()
    {
        var identifier = MatchToken(SyntaxKind.IdentifierToken);
        return new TypeParameterSyntax(_syntaxTree, identifier);
    }
    
    private ElseClauseSyntax ParseElseClause()
    {
        var elseKeyword = MatchToken(SyntaxKind.ElseKeyword);
        var statement = ParseStatement();
        return new ElseClauseSyntax(_syntaxTree, elseKeyword, statement);
    }

    private bool IsValidTypeArgumentList(out int position)
    {
        if (Current.Kind != SyntaxKind.LessThanToken)
        {
            position = _position;
            return false;
        }

        bool IsValidTypeArgumentToken(SyntaxKind kind) => kind == SyntaxKind.LessThanToken ||
                                                          kind == SyntaxKind.GreaterThanToken ||
                                                          kind == SyntaxKind.IdentifierToken ||
                                                          kind == SyntaxKind.CommaToken;
        
        var pos = _position;
        _shouldBreakRightShift = true;
        var isValidTypeArgumentList = true;
        var lastToken = Current;
        NextToken(); // Skip the less than token
        var angleBracketPairs = 1;
        while (angleBracketPairs != 0 && Current.Kind != SyntaxKind.EndOfFileToken)
        {
            if (Current.Kind == SyntaxKind.LessThanToken)
            {
                angleBracketPairs++;
            }
            else if (Current.Kind == SyntaxKind.GreaterThanToken)
            {
                angleBracketPairs--;
            }

            if (IsValidTypeArgumentToken(Current.Kind))
            {
                if (lastToken.Kind == SyntaxKind.IdentifierToken &&
                    Current.Kind != SyntaxKind.CommaToken &&
                    Current.Kind != SyntaxKind.GreaterThanToken)
                {
                    isValidTypeArgumentList = false;
                    break;
                }
                
                lastToken = Current;
                NextToken();
                continue;
            }
            
            isValidTypeArgumentList = false;
            break;
        }

        var p = _position;
        position = p;
        _position = pos;
        _shouldBreakRightShift = false;
        return isValidTypeArgumentList;
    }
    
    private SeparatedSyntaxList<T> ParseSeparatedSyntaxList<T>(Func<T> parseElement, SyntaxKind separatorTokenKind, SyntaxKind cancelTokenKind)
        where T : SyntaxNode
    {
        var nodesAndSeparators = ImmutableArray.CreateBuilder<SyntaxNode>();
        while (Current.Kind != cancelTokenKind)
        {
            var start = Current;
            var element = parseElement();
            nodesAndSeparators.Add(element);
            // If we reach the cancel token, we're done
            if (Current.Kind == cancelTokenKind ||
                Current.Kind == SyntaxKind.EndOfFileToken)
            {
                break;
            }
    
            // Otherwise, we need to parse the separator token
            var separator = ExpectToken(separatorTokenKind);
            nodesAndSeparators.Add(separator);
            
            // If parseElement() didn't consume any tokens,
            // we need to skip the current token and continue
            // in order to avoid an infinite loop
            if (Current == start)
            {
                NextToken();
            }
        }

        return new SeparatedSyntaxList<T>(nodesAndSeparators.ToImmutable());
    }
}
