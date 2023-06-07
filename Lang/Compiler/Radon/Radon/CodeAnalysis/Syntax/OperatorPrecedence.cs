namespace Radon.CodeAnalysis.Syntax;

internal enum OperatorPrecedence
{
    None = 0,
    Assignment,
    LogicalOr,
    LogicalAnd,
    LogicalNot,
    BitwiseOr,
    BitwiseXor,
    BitwiseAnd,
    Equality,
    Relational,
    Shift,
    Additive,
    Multiplicative,
    Dot
}