namespace Radon.CodeAnalysis.Text;

public readonly struct TextSpan
{
    public static readonly TextSpan Empty = new(0, 0);

    public TextSpan(int start, int length)
    {
        Start = start;
        Length = length;
    }
    
    public int Start { get; }
    public int Length { get; }
    public int End => Start + Length;
    
    public static TextSpan FromBounds(int start, int end)
    {
        var lenght = end - start;
        return new TextSpan(start, lenght);
    }
    
    public bool OverlapsWith(TextSpan span)
    {
        return span.Start < End && Start < span.End;
    }

    public override string ToString()
    {
        return $"{Start}..{End}";
    }
}