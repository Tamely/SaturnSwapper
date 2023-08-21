namespace Radon.CodeAnalysis.Syntax;

internal enum OperatorPrecedence
{
    None = 0,
    LogicalOr,
    LogicalAnd,
    LogicalNot,
    BitwiseOr,
    BitwiseAnd,
    BitwiseXor,
    Equality,
    Relational,
    Shift,
    Additive,
    Multiplicative,
    Dot
}