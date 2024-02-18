using Radon.CodeAnalysis.Text;

namespace Radon.CodeAnalysis;

public sealed class Diagnostic
{
    public bool IsError { get; }
    public bool IsWarning => !IsError;
    public TextLocation Location { get; }
    public string Message { get; }
    public ErrorCode Code { get; }
    public string? SourceMethod { get; }

    private Diagnostic(bool isError, TextLocation location, string message, ErrorCode code, string? sourceMethod)
    {
        IsError = isError;
        Location = location;
        Message = message;
        Code = code;
        SourceMethod = sourceMethod;
    }
    
    public static Diagnostic Error(TextLocation location, string message, ErrorCode code, string? sourceMethod)
    {
        return new Diagnostic(true, location, message, code, sourceMethod);
    }
    
    public static Diagnostic Warning(TextLocation location, string message, ErrorCode code, string? sourceMethod)
    {
        return new Diagnostic(false, location, message, code, sourceMethod);
    }

    public override string ToString()
    {
        return Message;
    }
}