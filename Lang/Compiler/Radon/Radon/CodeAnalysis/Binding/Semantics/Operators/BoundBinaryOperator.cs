using System.Collections.Generic;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Binding.Semantics.Operators;

internal sealed class BoundBinaryOperator
{
    private static readonly List<BoundBinaryOperator> Operators;
    static BoundBinaryOperator()
    {
        Operators = new List<BoundBinaryOperator>
        {
            // String
            new(SyntaxKind.PlusToken, BoundBinaryOperatorKind.Concatenation, TypeSymbol.String),
            
            
            
            // Boolean
            // Logical
            new(SyntaxKind.AmpersandAmpersandToken, BoundBinaryOperatorKind.LogicalAnd, TypeSymbol.Bool),
            new(SyntaxKind.PipePipeToken, BoundBinaryOperatorKind.LogicalOr, TypeSymbol.Bool),
            new(SyntaxKind.BangToken, BoundBinaryOperatorKind.LogicalNot, TypeSymbol.Bool),
            
            // Bitwise
            new(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Bool),
            new(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Bool),
            
            // Equality
            new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equality, TypeSymbol.Bool, TypeSymbol.String),
            new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.Inequality, TypeSymbol.Bool, TypeSymbol.String),
            
            
            
            // Char
            // Bitwise
            new(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Char),
            new(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Char),
            
            // Equality
            new(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equality, TypeSymbol.Bool, TypeSymbol.Char),
            new(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.Inequality, TypeSymbol.Bool, TypeSymbol.Char),
            
            // Comparison
            new(SyntaxKind.LessToken, BoundBinaryOperatorKind.LessThan, TypeSymbol.Bool, TypeSymbol.Char),
            new(SyntaxKind.LessEqualsToken, BoundBinaryOperatorKind.LessThanOrEqual, TypeSymbol.Bool, TypeSymbol.Char),
            new(SyntaxKind.GreaterToken, BoundBinaryOperatorKind.GreaterThan, TypeSymbol.Bool, TypeSymbol.Char),
            new(SyntaxKind.GreaterEqualsToken, BoundBinaryOperatorKind.GreaterThanOrEqual, TypeSymbol.Bool, TypeSymbol.Char)
        };

        var numericTypes = TypeSymbol.GetNumericTypes();
        var ops = new Dictionary<SyntaxKind, BoundBinaryOperatorKind>
        {
            {SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr},
            {SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd},
            
            {SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equality},
            {SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.Inequality},
            
            {SyntaxKind.LessToken, BoundBinaryOperatorKind.LessThan},
            {SyntaxKind.LessEqualsToken, BoundBinaryOperatorKind.LessThanOrEqual},
            {SyntaxKind.GreaterToken, BoundBinaryOperatorKind.GreaterThan},
            {SyntaxKind.GreaterEqualsToken, BoundBinaryOperatorKind.GreaterThanOrEqual},
            
            {SyntaxKind.LessLessToken, BoundBinaryOperatorKind.LeftShift},
            {SyntaxKind.GreaterGreaterToken, BoundBinaryOperatorKind.RightShift},
            
            {SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition},
            {SyntaxKind.MinusToken, BoundBinaryOperatorKind.Subtraction},
            
            {SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication},
            {SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division},
            {SyntaxKind.PercentToken, BoundBinaryOperatorKind.Modulo}
        };

        foreach (var numericType in numericTypes)
        {
            foreach (var (syntaxKind, kind) in ops)
            {
                Operators.Add(new BoundBinaryOperator(syntaxKind, kind, numericType));
            }
        }
    }
    
    public SyntaxKind SyntaxKind { get; }
    public BoundBinaryOperatorKind Kind { get; }
    public TypeSymbol Left { get; }
    public TypeSymbol Right { get; }
    public TypeSymbol Type { get; }
    
    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol type)
        : this(syntaxKind, kind, type, type, type)
    {
    }

    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol result, TypeSymbol type)
        : this(syntaxKind, kind, type, type, result)
    {
    }

    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol left, TypeSymbol right,
        TypeSymbol result)
    {
        SyntaxKind = syntaxKind;
        Kind = kind;
        Left = left;
        Right = right;
        Type = result;
    }

    public static BoundBinaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol left, TypeSymbol right)
    {
        foreach (var op in Operators)
        {
            if (op.SyntaxKind == syntaxKind &&
                op.Left == left &&
                op.Right == right)
            {
                return op;
            }
        }
        
        return null;
    }
}
