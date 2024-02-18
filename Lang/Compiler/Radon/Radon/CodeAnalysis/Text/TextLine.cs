namespace Radon.CodeAnalysis.Text;

public sealed class TextLine
{
    public TextLine(SourceText text, int start, int length, int lineIndex, int lengthIncludingLineBreak)
    {
        Text = text;
        Start = start;
        Length = length;
        LineIndex = lineIndex;
        LengthIncludingLineBreak = lengthIncludingLineBreak;
    }

    public SourceText Text { get; }
    public int Start { get; }
    public int Length { get; }
    public int End => Start + Length;
    public int LineIndex { get; }
    public int LengthIncludingLineBreak { get; }
    public TextSpan Span => new(Start, Length);
    public TextSpan SpanIncludingLineBreak => new(Start, LengthIncludingLineBreak);
    public override string ToString()
    {
        return Text.ToString(Span);
    }
}