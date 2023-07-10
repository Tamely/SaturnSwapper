using System;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Binding.Semantics.Operators;

namespace Radon.CodeAnalysis.Binding.Semantics;

internal sealed class BoundConstant
{
    public object Value { get; }
    public BoundConstant(object value)
    {
        Value = value;
    }
}

internal sealed class ConstantFolding
{
    public static BoundConstant? Fold(BoundExpression left, BoundBinaryOperator op, BoundExpression right)
    {
        var leftConstant = left.ConstantValue;
        var rightConstant = right.ConstantValue;
        if (leftConstant == null || rightConstant == null)
        {
            return null;
        }
        
        try
        {
            dynamic l = leftConstant.Value;
            dynamic r = rightConstant.Value;
            switch (op.Kind)
            {
                case BoundBinaryOperatorKind.Addition:
                case BoundBinaryOperatorKind.Concatenation:
                {
                    return new BoundConstant(l + r);
                }
                case BoundBinaryOperatorKind.Subtraction:
                {
                    return new BoundConstant(l - r);
                }
                case BoundBinaryOperatorKind.Multiplication:
                {
                    return new BoundConstant(l * r);
                }
                case BoundBinaryOperatorKind.Division:
                {
                    return new BoundConstant(l / r);
                }
            }
        }
        catch (Exception)
        {
            return null;
        }
        
        return null;
    }
    
    public static BoundConstant? Fold(BoundUnaryOperator op, BoundExpression expression)
    {
        var leftConstant = expression.ConstantValue;
        if (leftConstant == null)
        {
            return null;
        }

        try
        {
            dynamic l = leftConstant.Value;
            switch (op.Kind)
            {
                case BoundUnaryOperatorKind.LogicalNot:
                {
                    return new BoundConstant(!l);
                }
                case BoundUnaryOperatorKind.Identity:
                {
                    return new BoundConstant(+l);
                }
                case BoundUnaryOperatorKind.Negation:
                {
                    return new BoundConstant(-l);
                }
                case BoundUnaryOperatorKind.Increment:
                {
                    return new BoundConstant(l + 1);
                }
                case BoundUnaryOperatorKind.Decrement:
                {
                    return new BoundConstant(l - 1);
                }
            }
        }
        catch (Exception)
        {
            return null;
        }

        return null;
    }
}
