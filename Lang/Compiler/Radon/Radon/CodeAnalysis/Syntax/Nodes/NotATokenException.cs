using System;

namespace Radon.CodeAnalysis.Syntax.Nodes;

public sealed class NotATokenException : Exception
{
    public SyntaxKind Kind { get; }

    public NotATokenException(SyntaxKind kind)
        : base($"SyntaxKind '{kind}' is not a token.")
    {
        Kind = kind;
    }
}