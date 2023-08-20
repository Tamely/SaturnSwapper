using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Text;

namespace Radon.CodeAnalysis.Syntax.Nodes;

public sealed class SyntaxToken : SyntaxNode
{
    public override string SyntaxName => Kind.Name;
    public override SyntaxKind Kind { get; }
    public override int Position { get; }
    public override string Text { get; }
    public object? Value { get; }
    public ImmutableArray<SyntaxTrivia> LeadingTrivia { get; }
    public ImmutableArray<SyntaxTrivia> TrailingTrivia { get; }
    public bool IsMissing => string.IsNullOrEmpty(Text);
    public override TextSpan Span => new(Position, Text.Length);

    internal SyntaxToken(SyntaxTree syntaxTree, SyntaxKind kind, int position, string text, object? value, 
                         ImmutableArray<SyntaxTrivia> leadingTrivia, ImmutableArray<SyntaxTrivia> trailingTrivia)
        : base(syntaxTree)
    {
        Kind = kind;
        Position = position;
        Text = text;
        Value = value;
        LeadingTrivia = leadingTrivia;
        TrailingTrivia = trailingTrivia;
    }

    public override IEnumerable<SyntaxNode> GetChildren()
    {
        return Enumerable.Empty<SyntaxNode>();
    }
}