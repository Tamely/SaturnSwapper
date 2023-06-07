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

    internal static SyntaxKindAttribute CreateOperator(OperatorPrecedence precedence, bool isBinaryOperator, bool isUnaryOperator, bool isAssignmentOperator)
    {
        return new(SKAttributes.Operator.Name, new OperatorData(precedence, isBinaryOperator, isUnaryOperator, isAssignmentOperator));
    }
}