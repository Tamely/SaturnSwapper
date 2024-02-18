namespace Radon.CodeAnalysis.Syntax;

public sealed class SyntaxTrivia
{
    public SyntaxTree SyntaxTree { get; }
    public SyntaxKind Kind { get; }
    public int Position { get; }
    public string Text { get; }

    internal SyntaxTrivia(SyntaxTree syntaxTree, SyntaxKind kind, int position, string text)
    {
        SyntaxTree = syntaxTree;
        Kind = kind;
        Position = position;
        Text = text;
    }
}