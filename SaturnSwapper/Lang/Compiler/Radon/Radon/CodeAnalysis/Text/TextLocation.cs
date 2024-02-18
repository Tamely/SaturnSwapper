namespace Radon.CodeAnalysis.Text;

public readonly record struct TextLocation(SourceText Text, TextSpan Span)
{
    public static readonly TextLocation Empty = new(SourceText.Empty, TextSpan.Empty);

    public string FileName => Text.FileName;
    public int StartLine => Text.GetLineIndex(Span.Start);
    public int StartCharacter => Span.Start - Text.Lines[StartLine].Start;
    public int EndLine => Text.GetLineIndex(Span.End);
    public int EndCharacter => Span.End - Text.Lines[EndLine].Start;

    public bool Contains(TextLocation location)
    {
        return location.Span.Start >= Span.Start && location.Span.End <= Span.End;
    }

    public override string ToString()
    {
        return $"{Span} : '{Text.ToString(Span)}'";
    }
}