namespace Radon.CodeAnalysis.Binding.Semantics.Operators;

internal enum BoundBinaryOperatorKind
{
    LogicalOr,
    LogicalAnd,
    LogicalNot,
    
    BitwiseOr,
    BitwiseAnd,
    
    Equality,
    Inequality,
    
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,
    
    LeftShift,
    RightShift,
    
    Addition,
    Concatenation,
    Subtraction,

    Multiplication,
    Division,
    Modulo,
    
}