using System;

namespace Radon.CodeAnalysis.Syntax;

public sealed class SyntaxKindAttribute
{
    public string Name { get; }
    public object? Value { get; }
    internal SyntaxKindAttribute(string name, object? value = null)
    {
        Name = name;
        Value = value;
    }
    
    public static implicit operator SyntaxKindAttribute(string name) => new(name);

    public T? GetValue<T>()
        where T : IAttributeValue
    {
        try
        {
            return (T)Value!;
        }
        catch
        {
            return default;
        }
    }

    internal static SyntaxKindAttribute CreateOperator(OperatorPrecedence precedence, OperatorKind kind)
    {
        if (kind == OperatorKind.None)
        {
            return new SyntaxKindAttribute(SKAttributes.Operator.Name, new OperatorData(precedence, 
                false, false, false, false));
        }
        
        var isBinaryOperator = kind.HasFlag(OperatorKind.Binary);
        var isUnaryOperator = kind.HasFlag(OperatorKind.Unary);
        var isPostfixUnaryOperator = kind.HasFlag(OperatorKind.PostfixUnary);
        var isAssignmentOperator = kind.HasFlag(OperatorKind.Assignment);
        return new(SKAttributes.Operator.Name, new OperatorData(precedence, isBinaryOperator,
            isUnaryOperator, isPostfixUnaryOperator, isAssignmentOperator));
    }
}

[Flags]
internal enum OperatorKind
{
    None = 0x00,
    Binary = 0x10,
    Unary = 0x20,
    PostfixUnary = 0x40,
    Assignment = 0x80
}
