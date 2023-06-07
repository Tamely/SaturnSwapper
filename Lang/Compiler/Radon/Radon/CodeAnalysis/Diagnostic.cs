using Radon.CodeAnalysis.Text;

namespace Radon.CodeAnalysis;

public sealed class Diagnostic
{
    public bool IsError { get; }
    public bool IsWarning => !IsError;
    public TextLocation Location { get; }
    public string Message { get; }
    public ErrorCode Code { get; }

    private Diagnostic(bool isError, TextLocation location, string message, ErrorCode code)
    {
        IsError = isError;
        Location = location;
        Message = message;
        Code = code;
    }
    
    public static Diagnostic Error(TextLocation location, string message, ErrorCode code)
    {
        return new Diagnostic(true, location, message, code);
    }
    
    public static Diagnostic Warning(TextLocation location, string message, ErrorCode code)
    {
        return new Diagnostic(false, location, message, code);
    }
    
    public override string ToString()
    {
        return Message;
    }
}