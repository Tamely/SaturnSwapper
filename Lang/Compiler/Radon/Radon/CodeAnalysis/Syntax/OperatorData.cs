namespace Radon.CodeAnalysis.Syntax;

public sealed class OperatorData : IAttributeValue
{
    public int Precedence { get; }
    public bool IsBinaryOperator { get; }
    public bool IsUnaryOperator { get; }
    public bool IsAssignmentOperator { get; }
    
    // The reason we have is binary, unary, and assignment operators is because an operator can be both binary and unary
    // So it's better to specify which one it is
    internal OperatorData(OperatorPrecedence precedence, bool isBinaryOperator, bool isUnaryOperator, bool isAssignmentOperator)
    {
        Precedence = (int)precedence;
        IsBinaryOperator = isBinaryOperator;
        IsUnaryOperator = isUnaryOperator;
        IsAssignmentOperator = isAssignmentOperator;
    }
}