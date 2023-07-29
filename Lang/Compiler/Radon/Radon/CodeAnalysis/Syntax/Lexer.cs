using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Linq;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Text;

namespace Radon.CodeAnalysis.Syntax;

internal sealed class Lexer
{
    private readonly SyntaxTree _syntaxTree;
    private readonly SourceText _text;

    private int _position;
    private SyntaxKind _kind;
    private int _start;
    private object? _value;
    private readonly ImmutableArray<SyntaxTrivia>.Builder _triviaBuilder;
    
    public DiagnosticBag Diagnostics { get; }

    public Lexer(SyntaxTree syntaxTree)
    {
        Diagnostics = new DiagnosticBag();
        _syntaxTree = syntaxTree;
        _text = syntaxTree.Text;
        
        _position = 0;
        _kind = SyntaxKind.BadToken;
        _start = 0;
        _value = null;
        _triviaBuilder = ImmutableArray.CreateBuilder<SyntaxTrivia>();
    }
    
    private char Current => Peek(0);
    private char Lookahead => Peek(1);
    
    private char Peek(int offset)
    {
        var index = _position + offset;
        return index >= _text.Length ? '\0' : _text[index];
    }

    public SyntaxToken Lex()
    {
        ReadTrivia(true);
        
        var leadingTrivia = _triviaBuilder.ToImmutable();
        var tokenStart = _position;
        
        ReadToken();
        
        var tokenKind = _kind;
        var tokenValue = _value;
        if (tokenKind == SyntaxKind.TrueKeyword)
        {
            tokenValue = true;
        }
        else if (tokenKind == SyntaxKind.FalseKeyword)
        {
            tokenValue = false;
        }
        
        var tokenLength = _position - tokenStart;
        
        ReadTrivia(false);
        
        var trailingTrivia = _triviaBuilder.ToImmutable();
        var tokenText = _kind.TryGetAttribute(SKAttributes.IsFixed, out _) 
                            ? _kind.Text! : _text.ToString(tokenStart, tokenLength);
        
        return new SyntaxToken(_syntaxTree, tokenKind, tokenStart, tokenText, tokenValue, leadingTrivia, trailingTrivia);
    }
    
    private void ReadTrivia(bool leading)
    {
        _triviaBuilder.Clear();
        var done = false;
        while (!done)
        {
            _start = _position;
            _kind = SyntaxKind.BadToken;
            _value = null;

            switch (Current)
            {
                case '\0':
                    done = true;
                    break;
                case '/':
                    switch (Lookahead)
                    {
                        case '/':
                            ReadSingleLineComment();
                            break;
                        case '*':
                            ReadMultiLineComment();
                            break;
                        default:
                            done = true;
                            break;
                    }
                    break;
                case '\n':
                case '\r':
                    if (!leading)
                    {
                        done = true;
                    }

                    ReadLineBreak();
                    break;
                case ' ':
                case '\t':
                    ReadWhiteSpace();
                    break;
                default:
                    if (char.IsWhiteSpace(Current))
                    {
                        ReadWhiteSpace();
                    }
                    else
                    {
                        done = true;
                    }

                    break;
            }

            var length = _position - _start;
            if (length > 0)
            {
                var text = _text.ToString(_start, length);
                var trivia = new SyntaxTrivia(_syntaxTree, _kind, _start, text);
                _triviaBuilder.Add(trivia);
            }
        }
    }

    private void ReadWhiteSpace()
    {
        var done = false;
        while (!done)
        {
            switch (Current)
            {
                case '\0':
                case '\r':
                case '\n':
                    done = true;
                    break;
                default:
                    if (!char.IsWhiteSpace(Current))
                    {
                        done = true;
                    }
                    else
                    {
                        _position++;
                    }

                    break;
            }
        }

        _kind = SyntaxKind.WhitespaceTrivia;
    }

    private void ReadLineBreak()
    {
        if (Current == '\r' && Lookahead == '\n')
        {
            _position += 2;
        }
        else
        {
            _position++;
        }
        
        _kind = SyntaxKind.LineBreakTrivia;
    }

    private void ReadMultiLineComment()
    {
        _position += 2;
        var done = false;
        while (!done)
        {
            switch (Current)
            {
                case '\0':
                    var span = new TextSpan(_start, 2);
                    var location = new TextLocation(_text, span);
                    Diagnostics.ReportUnterminatedMultiLineComment(location);
                    done = true;
                    break;
                case '*':
                    if (Lookahead == '/')
                    {
                        _position++;
                        done = true;
                    }
                    _position++;
                    break;
                default:
                    _position++;
                    break;
            }
        }

        _kind = SyntaxKind.MultiLineCommentTrivia;
    }

    private void ReadSingleLineComment()
    {
        _position += 2;
        var done = false;
        while (!done)
        {
            switch (Current)
            {
                case '\0':
                case '\r':
                case '\n':
                    done = true;
                    break;
                default:
                    _position++;
                    break;
            }
        }

        _kind = SyntaxKind.SingleLineCommentTrivia;
    }

    private void ReadToken()
    {
        if (Current == '\0')
        {
            _kind = SyntaxKind.EndOfFileToken;
            return;
        }
        
        var kinds = SyntaxKind.GetKinds()
                              .Where(k => k.TryGetAttribute(SKAttributes.IsFixed, out _))
                              .OrderByDescending(k => k.Text!.Length);
        foreach (var kind in kinds)
        {
            // We need this because say we had a variable call "enum1", it would parse the enum keyword,
            // then the number 1, instead of the "enum1" identifier
            var mustBeSeparated = kind.TryGetAttribute(SKAttributes.Keyword, out _) ||
                                      kind.TryGetAttribute(SKAttributes.Literal, out _);
            var text = kind.Text!;
            if (text.Length == 0)
            {
                continue;
            }
                
            if (Current == text[0])
            {
                var done = true;
                for (var i = 1; i < text.Length; i++)
                {
                    if (Peek(i) != text[i])
                    {
                        done = false;
                        break;
                    }
                }

                if (done)
                {
                    var isSeparated = true;
                    if (mustBeSeparated)
                    {
                        var pos = _position;
                        _position = _start + text.Length;
                        ReadTrivia(false);
                        var trivia = _triviaBuilder.ToImmutable();
                        if (trivia.Length == 0)
                        {
                            isSeparated = false;
                        }
                        
                        _position = pos;
                    }
                    
                    if (isSeparated)
                    {
                        _kind = kind;
                        _position += text.Length;
                        return;
                    }
                    
                    // If it isn't separated, then it's probably and identifier
                    break;
                }
            }
        }

        if (char.IsDigit(Current))
        {
            while (char.IsDigit(Current))
            {
                _position++;
            }
            
            if (Current == '.')
            {
                _position++;
                while (char.IsDigit(Current))
                {
                    _position++;
                }
            }
            
            var length = _position - _start;
            var text = _text.ToString(_start, length);
            if (double.TryParse(text, out var doubleValue))
            {
                _value = doubleValue;
                _kind = SyntaxKind.NumberToken;
            }
            else
            {
                var span = new TextSpan(_start, length);
                var location = new TextLocation(_text, span);
                Diagnostics.ReportInvalidNumber(location, text);
                _kind = SyntaxKind.BadToken;
            }
            
            return;
        }
        
        if (char.IsLetter(Current) ||
            Current == '_')
        {
            while (char.IsLetterOrDigit(Current) ||
                   Current == '_')
            {
                _position++;
            }
            
            _kind = SyntaxKind.IdentifierToken;
            return;
        }
        
        if (Current == '"')
        {
            _position++;
            var sb = new StringBuilder();
            var done = false;
            while (!done)
            {
                switch (Current)
                {
                    case '\0':
                    case '\r':
                    case '\n':
                        var span = new TextSpan(_start, 1);
                        var location = new TextLocation(_text, span);
                        Diagnostics.ReportUnterminatedString(location);
                        done = true;
                        break;
                    case '"':
                        if (Lookahead == '"')
                        {
                            sb.Append(Current);
                            _position += 2;
                        }
                        else
                        {
                            _position++;
                            done = true;
                        }
                        break;
                    default:
                        sb.Append(Current);
                        _position++;
                        break;
                }
            }

            _kind = SyntaxKind.StringToken;
            _value = sb.ToString();
            return;
        }
        
        _kind = SyntaxKind.BadToken;
        var badSpan = new TextSpan(_start, 1);
        var badLocation = new TextLocation(_text, badSpan);
        Diagnostics.ReportUnexpectedCharacter(badLocation, Current);
        _position++;
    }
}