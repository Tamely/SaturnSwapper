using System.Collections.Generic;
using System.Linq;
using Radon.CodeAnalysis.Binding.Semantics.Conversions;
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

            // Bitwise
            new(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Bool),
            new(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Bool),
            new(SyntaxKind.HatToken, BoundBinaryOperatorKind.BitwiseXor, TypeSymbol.Bool),

            // Char
            // Bitwise
            new(SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr, TypeSymbol.Char),
            new(SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd, TypeSymbol.Char),
            new(SyntaxKind.HatToken, BoundBinaryOperatorKind.BitwiseXor, TypeSymbol.Char),

            // Comparison
            new(SyntaxKind.LessThanToken, BoundBinaryOperatorKind.LessThan, TypeSymbol.Char, TypeSymbol.Bool),
            new(SyntaxKind.LessEqualsToken, BoundBinaryOperatorKind.LessThanOrEqual, TypeSymbol.Char, TypeSymbol.Bool),
            new(SyntaxKind.GreaterThanToken, BoundBinaryOperatorKind.GreaterThan, TypeSymbol.Char, TypeSymbol.Bool),
            new(SyntaxKind.GreaterEqualsToken, BoundBinaryOperatorKind.GreaterThanOrEqual, TypeSymbol.Char, TypeSymbol.Bool)
        };

        var numericTypes = TypeSymbol.GetNumericTypes();
        var numericOps = new Dictionary<SyntaxKind, BoundBinaryOperatorKind>
        {
            {SyntaxKind.PlusEqualsToken, BoundBinaryOperatorKind.PlusEquals},
            {SyntaxKind.MinusEqualsToken, BoundBinaryOperatorKind.MinusEquals},
            {SyntaxKind.StarEqualsToken, BoundBinaryOperatorKind.StarEquals},
            {SyntaxKind.SlashEqualsToken, BoundBinaryOperatorKind.SlashEquals},
            {SyntaxKind.PercentEqualsToken, BoundBinaryOperatorKind.PercentEquals},
            {SyntaxKind.PipeEqualsToken, BoundBinaryOperatorKind.PipeEquals},
            {SyntaxKind.AmpersandEqualsToken, BoundBinaryOperatorKind.AmpersandEquals},
            
            {SyntaxKind.PipeToken, BoundBinaryOperatorKind.BitwiseOr},
            {SyntaxKind.AmpersandToken, BoundBinaryOperatorKind.BitwiseAnd},
            {SyntaxKind.HatToken, BoundBinaryOperatorKind.BitwiseXor},
            
            {SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equality},
            {SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.Inequality},
            
            {SyntaxKind.LessThanToken, BoundBinaryOperatorKind.LessThan},
            {SyntaxKind.LessEqualsToken, BoundBinaryOperatorKind.LessThanOrEqual},
            {SyntaxKind.GreaterThanToken, BoundBinaryOperatorKind.GreaterThan},
            {SyntaxKind.GreaterEqualsToken, BoundBinaryOperatorKind.GreaterThanOrEqual},
            
            {SyntaxKind.LessLessToken, BoundBinaryOperatorKind.LeftShift},
            {SyntaxKind.GreaterGreaterToken, BoundBinaryOperatorKind.RightShift},
            
            {SyntaxKind.PlusToken, BoundBinaryOperatorKind.Addition},
            {SyntaxKind.MinusToken, BoundBinaryOperatorKind.Subtraction},
            
            {SyntaxKind.StarToken, BoundBinaryOperatorKind.Multiplication},
            {SyntaxKind.SlashToken, BoundBinaryOperatorKind.Division},
            {SyntaxKind.PercentToken, BoundBinaryOperatorKind.Modulus}
        };

        var primitiveTypes = TypeSymbol.GetPrimitiveTypes();
        foreach (var primitiveType in primitiveTypes)
        {
            if (numericTypes.Contains(primitiveType))
            {
                for (var i = 0; i < numericOps.Count; i++)
                {
                    var (syntaxKind, kind) = numericOps.ElementAt(i);
                    // if i is between 8 and 15, then the type is bool
                    if (kind is BoundBinaryOperatorKind.LeftShift or BoundBinaryOperatorKind.RightShift)
                    {
                        var shiftOp = new BoundBinaryOperator(syntaxKind, kind, primitiveType, TypeSymbol.Int, primitiveType);
                        AddOperator(shiftOp);
                        continue;
                    }
                    
                    var type = primitiveType;
                    if (i is >= 10 and <= 15)
                    {
                        type = TypeSymbol.Bool;
                    }
                    
                    var op = new BoundBinaryOperator(syntaxKind, kind, primitiveType, primitiveType, type);
                    AddOperator(op);
                }
            }
            
            var equalsOp = new BoundBinaryOperator(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equality,
                primitiveType, TypeSymbol.Bool);
            var notEqualsOp = new BoundBinaryOperator(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.Inequality,
                primitiveType, TypeSymbol.Bool);
            AddOperator(equalsOp);
            AddOperator(notEqualsOp);
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

    private BoundBinaryOperator(SyntaxKind syntaxKind, BoundBinaryOperatorKind kind, TypeSymbol type, TypeSymbol result)
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

    private static void AddOperator(BoundBinaryOperator op)
    {
        if (Operators.Any(o => o.SyntaxKind == op.SyntaxKind && o.Left == op.Left && o.Right == op.Right))
        {
            return;
        }
        
        Operators.Add(op);
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

    public static void CreateTypeOperators(TypeSymbol type)
    {
        var equalsOp = new BoundBinaryOperator(SyntaxKind.EqualsEqualsToken, BoundBinaryOperatorKind.Equality,
            type, TypeSymbol.Bool);
        var notEqualsOp = new BoundBinaryOperator(SyntaxKind.BangEqualsToken, BoundBinaryOperatorKind.Inequality,
            type, TypeSymbol.Bool);
        AddOperator(equalsOp);
        AddOperator(notEqualsOp);
    }

    public override string ToString()
    {
        return $"{Kind}: {Left} {SyntaxKind.Text} {Right} => {Type}";
    }
}
