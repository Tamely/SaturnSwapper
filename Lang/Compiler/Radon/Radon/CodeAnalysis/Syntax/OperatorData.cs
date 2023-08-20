namespace Radon.CodeAnalysis.Syntax;

public sealed class OperatorData : IAttributeValue
{
    public int Precedence { get; }
    public bool IsBinaryOperator { get; }
    public bool IsPrefixUnaryOperator { get; }
    public bool IsPostfixUnaryOperator { get; }
    public bool IsAssignmentOperator { get; }
    
    // The reason we have is binary, unary, and assignment operators is because an operator can be both binary and unary
    // So it's better to specify which one it is
    internal OperatorData(OperatorPrecedence precedence, bool isBinaryOperator, bool isPrefixUnaryOperator, bool isPostfixUnaryOperator, bool isAssignmentOperator)
    {
        Precedence = (int)precedence;
        IsBinaryOperator = isBinaryOperator;
        IsPrefixUnaryOperator = isPrefixUnaryOperator;
        IsPostfixUnaryOperator = isPostfixUnaryOperator;
        IsAssignmentOperator = isAssignmentOperator;
    }
}