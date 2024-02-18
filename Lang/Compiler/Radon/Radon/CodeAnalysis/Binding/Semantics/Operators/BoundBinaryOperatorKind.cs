namespace Radon.CodeAnalysis.Binding.Semantics.Operators;

internal enum BoundBinaryOperatorKind
{
    PlusEquals,
    MinusEquals,
    StarEquals,
    SlashEquals,
    PercentEquals,
    PipeEquals,
    AmpersandEquals,
    
    LogicalOr,
    LogicalAnd,

    BitwiseOr,
    BitwiseAnd,
    BitwiseXor,
    
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
    Modulus,
}