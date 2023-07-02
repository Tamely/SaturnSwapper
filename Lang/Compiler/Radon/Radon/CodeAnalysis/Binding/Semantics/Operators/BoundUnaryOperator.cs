using System.Collections.Generic;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Binding.Semantics.Operators;

internal sealed class BoundUnaryOperator
{
    private static readonly List<BoundUnaryOperator> Operators;
    
    static BoundUnaryOperator()
    {
        Operators = new List<BoundUnaryOperator>
        {
            new(SyntaxKind.BangToken, BoundUnaryOperatorKind.LogicalNot, TypeSymbol.Bool),
        };
        
        var numericTypes = TypeSymbol.GetNumericTypes();
        // All of the operators that can be done on numeric types.
        var ops = new Dictionary<SyntaxKind, BoundUnaryOperatorKind>
        {
            {SyntaxKind.PlusToken, BoundUnaryOperatorKind.Identity},
            {SyntaxKind.MinusToken, BoundUnaryOperatorKind.Negation},
            {SyntaxKind.PlusPlusToken, BoundUnaryOperatorKind.Increment},
            {SyntaxKind.MinusMinusToken, BoundUnaryOperatorKind.Decrement}
        };
        
        foreach (var type in numericTypes)
        {
            foreach (var (syntaxKind, kind) in ops)
            {
                Operators.Add(new(syntaxKind, kind, type));
            }
        }
    }
    
    public SyntaxKind SyntaxKind { get; }
    public BoundUnaryOperatorKind Kind { get; }
    public TypeSymbol Type { get; }
    
    private BoundUnaryOperator(SyntaxKind syntaxKind, BoundUnaryOperatorKind kind, TypeSymbol type)
    {
        SyntaxKind = syntaxKind;
        Kind = kind;
        Type = type;
    }
    
    public static BoundUnaryOperator? Bind(SyntaxKind syntaxKind, TypeSymbol type)
    {
        foreach (var op in Operators)
        {
            if (op.SyntaxKind == syntaxKind &&
                op.Type == type)
            {
                return op;
            }
        }
        
        return null;
    }
}