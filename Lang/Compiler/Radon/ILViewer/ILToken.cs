namespace ILViewer;

// ReSharper disable once InconsistentNaming
internal sealed class ILToken
{
    public ILTokenKind Kind { get; }
    public string Text { get; }
    
    public ILToken(ILTokenKind kind, string text)
    {
        Kind = kind;
        Text = text;
    }
}