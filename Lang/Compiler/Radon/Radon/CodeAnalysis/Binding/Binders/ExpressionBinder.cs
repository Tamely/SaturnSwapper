using System;
using System.Collections.Immutable;
using System.Globalization;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Conversions;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Binding.Semantics.Operators;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Syntax.Nodes.Expressions;

namespace Radon.CodeAnalysis.Binding.Binders;

internal sealed class ExpressionBinder : Binder
{
    private readonly bool _isStaticMethod;
    private readonly TypeSymbol? _currentType;
    
    private bool _isBindingInvocation;
    private ImmutableArray<BoundExpression> _arguments;
    private ImmutableArray<TypeSymbol> _typeArguments;

    internal ExpressionBinder(Binder binder) 
        : base(binder)
    {
        _isBindingInvocation = false;
        _arguments = ImmutableArray<BoundExpression>.Empty;
        _typeArguments = ImmutableArray<TypeSymbol>.Empty;
        if (binder is StatementBinder statementBinder)
        {
            _isStaticMethod = statementBinder.IsStaticMethod;
            _currentType = statementBinder.MethodSymbol?.ParentType;
        }
    }

    public override BoundNode Bind(SyntaxNode node, params object[] args)
    {
        if (args.Length > 0)
        {
            throw new ArgumentException($"No arguments should be passed to {nameof(ExpressionBinder)}.{nameof(Bind)} method.");
        }

        if (node is not ExpressionSyntax syntax)
        {
            throw new ArgumentException("The node must be an expression syntax node.");
        }
        
        return BindExpression(syntax);
    }

    private BoundExpression BindExpression(ExpressionSyntax syntax)
    {
        var expressionContext = new SemanticContext(this, syntax, Diagnostics);
        return syntax switch
        {
            AssignmentExpressionSyntax assignmentExpressionSyntax => BindAssignmentExpression(assignmentExpressionSyntax),
            BinaryExpressionSyntax binaryExpressionSyntax => BindBinaryExpression(binaryExpressionSyntax, expressionContext),
            CastExpressionSyntax castExpressionSyntax => BindCastExpression(castExpressionSyntax),
            ImportExpressionSyntax importExpressionSyntax => BindImportExpression(importExpressionSyntax, expressionContext),
            InvocationExpressionSyntax invocationExpressionSyntax => BindInvocationExpression(invocationExpressionSyntax, expressionContext),
            LiteralExpressionSyntax literalExpressionSyntax => BindLiteralExpression(literalExpressionSyntax, expressionContext),
            MemberAccessExpressionSyntax memberAccessExpressionSyntax => BindMemberAccessExpression(memberAccessExpressionSyntax, expressionContext),
            NameExpressionSyntax nameExpressionSyntax => BindNameExpression(nameExpressionSyntax, expressionContext),
            NewExpressionSyntax newExpressionSyntax => BindNewExpression(newExpressionSyntax, expressionContext),
            NewArrayExpressionSyntax newArrayExpressionSyntax => BindNewArrayExpression(newArrayExpressionSyntax, expressionContext),
            ParenthesizedExpressionSyntax parenthesizedExpressionSyntax => BindParenthesizedExpression(parenthesizedExpressionSyntax),
            ElementAccessExpressionSyntax elementAccessExpressionSyntax => BindElementAccessExpression(elementAccessExpressionSyntax, expressionContext),
            ThisExpressionSyntax thisExpressionSyntax => BindThisExpression(thisExpressionSyntax, expressionContext),
            DefaultExpressionSyntax defaultExpressionSyntax => BindDefaultExpression(defaultExpressionSyntax),
            UnaryExpressionSyntax unaryExpressionSyntax => BindUnaryExpression(unaryExpressionSyntax, expressionContext),
            _ => throw new ArgumentOutOfRangeException(nameof(syntax),
                $"The syntax node {syntax.Kind} is not supported by the {nameof(ExpressionBinder)} class.")
        };
    }

    public BoundExpression BindConversion(BoundExpression expression, TypeSymbol type, ImmutableArray<TypeSymbol> typeArguments, bool allowExplicit = false)
    {
        var expressionType = expression.Type;
        if (expressionType is BoundTypeParameterSymbol typeParamExpr)
        {
            expressionType = typeParamExpr.BoundType;
        }
        
        if (type is BoundTypeParameterSymbol typeParam)
        {
            type = typeParam.BoundType;
        }

        if (expressionType == type)
        {
            return expression;
        }
        
        if (type is TypeParameterSymbol t &&
            typeArguments.Length > 0)
        {
            var context = new SemanticContext(this, expression.Syntax, Diagnostics);
            type = typeArguments[t.Ordinal];
            if (!TryResolveSymbol(context, ref type))
            {
                if (expression.Type != TypeSymbol.Error &&
                    type != TypeSymbol.Error)
                {
                    Diagnostics.ReportCannotConvert(expression.Syntax.Location, expressionType, type);
                }
                
                return new BoundErrorExpression(expression.Syntax, context);
            }
        }
        
        var conversion = Conversion.Classify(expressionType, type);
        if (!conversion.Exists)
        {
            if (expression.Type != TypeSymbol.Error &&
                type != TypeSymbol.Error)
            {
                Diagnostics.ReportCannotConvert(expression.Syntax.Location, expressionType, type);
            }
            
            return new BoundErrorExpression(expression.Syntax, new SemanticContext(this, expression.Syntax, Diagnostics));
        }
        
        if (expression is BoundLiteralExpression boundLiteralExpression)
        {
            // We will try to implicitly convert the literal to the type
            var value = boundLiteralExpression.Value;
            var literalSyntax = (LiteralExpressionSyntax)boundLiteralExpression.Syntax;
            if (literalSyntax.LiteralToken.Kind == SyntaxKind.NumberToken)
            {
                var literalType = boundLiteralExpression.Type;
                var literalConversion = Conversion.Classify(this, boundLiteralExpression, type);
                if (literalConversion is { Exists: true, IsImplicit: true })
                {
                    var literalValue = Convert.ChangeType(value, Conversion.GetSystemType(type));
                    return new BoundLiteralExpression(literalSyntax, type, literalValue);
                }
                
                if (literalType != TypeSymbol.Error &&
                    type != TypeSymbol.Error)
                {
                    Diagnostics.ReportCannotConvertSourceType(literalSyntax.Location, literalType, type);
                }
                
                return new BoundErrorExpression(literalSyntax, new SemanticContext(this, literalSyntax, Diagnostics));
            }
        }
        
        if (!allowExplicit && conversion.IsExplicit)
        {
            if (expression.Type != TypeSymbol.Error &&
                type != TypeSymbol.Error)
            {
                Diagnostics.ReportCannotConvertImplicitly(expression.Syntax.Location, expressionType, type);
            }
            
            return new BoundErrorExpression(expression.Syntax, new SemanticContext(this, expression.Syntax, Diagnostics));
        }
        
        if (conversion.IsIdentity)
        {
            return expression;
        }
        
        return new BoundConversionExpression(expression.Syntax, type, expression);
    }

    private TypeSymbol GetLiteralType(double number)
    {
        var isFloatingPoint = false;
        var text = number.ToString(CultureInfo.InvariantCulture);
        if (text.Contains('.'))
        {
            isFloatingPoint = true;
        }

        if (isFloatingPoint)
        {
            if (IsInRange(float.MinValue, float.MaxValue, number))
            {
                return TypeSymbol.Float;
            }
            
            return TypeSymbol.Double;
        }
    
        if (IsInRange(int.MinValue, int.MaxValue, number))
        {
            return TypeSymbol.Int;
        }
    
        if (IsInRange(uint.MinValue, uint.MaxValue, number))
        {
            return TypeSymbol.UInt;
        }
    
        if (IsInRange(long.MinValue, long.MaxValue, number))
        {
            return TypeSymbol.Long;
        }
    
        if (IsInRange(ulong.MinValue, ulong.MaxValue, number))
        {
            return TypeSymbol.ULong;
        }
    
        return TypeSymbol.Error;
    }

    private bool IsInRange(double min, double max, double value)
    {
        if (value < min || value > max)
        {
            return false;
        }
    
        return true;
    }

    private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
    {
        // We don't have compound assignment yet
        var boundLeft = BindExpression(syntax.Left);
        var boundRight = BindExpression(syntax.Right);
        var convertedRight = BindConversion(boundRight, boundLeft.Type, ImmutableArray<TypeSymbol>.Empty);
        return new BoundAssignmentExpression(syntax, boundLeft, convertedRight, syntax.OperatorToken.Kind);
    }
    
    private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax, SemanticContext context)
    {
        var boundLeft = BindExpression(syntax.Left);
        var boundRight = BindExpression(syntax.Right);
        var convertedRight = BindConversion(boundRight, boundLeft.Type, ImmutableArray<TypeSymbol>.Empty);
        var boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, convertedRight.Type);
        if (boundOperator == null)
        {
            Diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text, boundLeft.Type, convertedRight.Type);
            return new BoundErrorExpression(syntax, context);
        }
        
        var opConversion = BindConversion(convertedRight, boundOperator.Right, ImmutableArray<TypeSymbol>.Empty);
        return new BoundBinaryExpression(syntax, boundLeft, boundOperator, opConversion);
    }

    private BoundExpression BindCastExpression(CastExpressionSyntax syntax)
    {
        // Conversions are not typical in Radon. The way they work is the actual bytes of the struct get molded into the new type.
        var type = BindTypeSyntax(syntax.Type);
        var boundExpression = BindExpression(syntax.Expression);
        return new BoundConversionExpression(syntax, type, boundExpression);
    }
    
    private BoundExpression BindImportExpression(ImportExpressionSyntax syntax,SemanticContext context)
    {
        var boundPath = BindExpression(syntax.Path);
        if (boundPath.Type != TypeSymbol.String)
        {
            Diagnostics.ReportInvalidImportPathType(syntax.Path.Location, boundPath.Type);
            return new BoundErrorExpression(syntax, context);
        }
        
        return new BoundImportExpression(syntax, boundPath);
    }
    
    private BoundExpression BindInvocationExpression(InvocationExpressionSyntax syntax, SemanticContext context)
    {
        static SyntaxToken GetMethodIdentifier(InvocationExpressionSyntax syntax)
        {
            return syntax.Expression switch
            {
                NameExpressionSyntax nameExpression => nameExpression.IdentifierToken,
                MemberAccessExpressionSyntax memberAccessExpression => memberAccessExpression.Name,
                _ => throw new InvalidOperationException("Invalid invocation expression")
            };
        }
        
        static AbstractMethodSymbol? StripMethodSymbol(BoundExpression expression)
        {
            return expression switch
            {
                BoundMemberAccessExpression memberAccessExpression =>
                    memberAccessExpression.Member as AbstractMethodSymbol,
                BoundNameExpression nameExpression => (nameExpression.Symbol as AbstractMethodSymbol)!,
                _ => null
            };
        }
        
        var iArguments = ImmutableArray.CreateBuilder<BoundExpression>();
        Diagnostics.Block();
        foreach (var argument in syntax.ArgumentList.Arguments)
        {
            var boundArgument = BindExpression(argument);
            iArguments.Add(boundArgument);
        }
        
        Diagnostics.Unblock();
        var typeArgs = ImmutableArray.CreateBuilder<TypeSymbol>();
        if (syntax.TypeArgumentList is { } typeArgList)
        {
            foreach (var typeArg in typeArgList.Arguments)
            {
                var boundTypeArg = BindTypeSyntax(typeArg);
                typeArgs.Add(boundTypeArg);
            }
        }

        _arguments = iArguments.ToImmutable();
        _typeArguments = typeArgs.ToImmutable();
        _isBindingInvocation = true;
        var boundExpression = BindExpression(syntax.Expression);
        _isBindingInvocation = false;
        if (boundExpression is BoundErrorExpression)
        {
            return new BoundErrorExpression(syntax, context);
        }
        
        var methodSymbol = StripMethodSymbol(boundExpression);
        if (methodSymbol is null)
        {
            Diagnostics.ReportCannotInvoke(syntax.Expression.Location, boundExpression.Kind);
            return new BoundErrorExpression(syntax, context);
        }
        
        if (methodSymbol.Parameters.Length != syntax.ArgumentList.Arguments.Count)
        {
            Diagnostics.ReportIncorrectNumberOfArguments(GetMethodIdentifier(syntax).Location, methodSymbol.Name,
                methodSymbol.Parameters.Length, syntax.ArgumentList.Arguments.Count);
            return new BoundErrorExpression(syntax, context);
        }

        var arguments = ImmutableArray.CreateBuilder<BoundExpression>();
        for (var i = 0; i < syntax.ArgumentList.Arguments.Count; i++)
        {
            var argument = syntax.ArgumentList.Arguments[i];
            var boundArgument = BindExpression(argument);
            var converted = BindConversion(boundArgument, methodSymbol.Parameters[i].Type, _typeArguments);
            arguments.Add(converted);
        }
        
        return new BoundInvocationExpression(syntax, methodSymbol, boundExpression, arguments.ToImmutable(), methodSymbol.Type);
    }
    
    private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax, SemanticContext context)
    {
        if (syntax.LiteralToken.Kind == SyntaxKind.EncryptedKeyword)
        {
            return new BoundLiteralExpression(syntax, TypeSymbol.String, "Encrypted");
        }
        
        var value = syntax.LiteralToken.Value;
        if (value is null)
        {
            Diagnostics.ReportNullLiteral(syntax.LiteralToken.Location);
            return new BoundErrorExpression(syntax, context);
        }
        
        if (value is bool boolValue)
        {
            return new BoundLiteralExpression(syntax, TypeSymbol.Bool, boolValue);
        }

        if (value is double doubleValue)
        {
            return doubleValue switch
            {
                <= int.MaxValue and >= int.MinValue => new BoundLiteralExpression(syntax, TypeSymbol.Int,
                    (int)doubleValue),
                <= uint.MaxValue and >= uint.MinValue => new BoundLiteralExpression(syntax, TypeSymbol.UInt,
                    (uint)doubleValue),
                <= long.MaxValue and >= long.MinValue => new BoundLiteralExpression(syntax, TypeSymbol.Long,
                    (long)doubleValue),
                <= ulong.MaxValue and >= ulong.MinValue => new BoundLiteralExpression(syntax, TypeSymbol.ULong,
                    (ulong)doubleValue),
                <= float.MaxValue and >= float.MinValue => new BoundLiteralExpression(syntax, TypeSymbol.Float,
                    (float)doubleValue),
                _ => new BoundLiteralExpression(syntax, TypeSymbol.Double, doubleValue)
            };
        }

        if (value is string stringValue)
        {
            return new BoundLiteralExpression(syntax, TypeSymbol.String, stringValue);
        }
        
        Diagnostics.ReportInvalidLiteralExpression(syntax.LiteralToken.Location, syntax.LiteralToken.Text);
        return new BoundErrorExpression(syntax, context);
    }
    
    private BoundExpression BindMemberAccessExpression(MemberAccessExpressionSyntax syntax, SemanticContext context)
    {
        var isBindingInvocation = _isBindingInvocation;
        _isBindingInvocation = false; // We have to set this to false because we only want the immediate expression to
                                      // be bound with the invocation flag
        var boundExpression = BindExpression(syntax.Expression);
        _isBindingInvocation = isBindingInvocation;
        if (boundExpression.Type == TypeSymbol.Error)
        {
            return new BoundErrorExpression(syntax, context);
        }

        var memberName = syntax.Name.Text;
        if (_isBindingInvocation)
        {
            var methodContext = new SemanticContext(syntax.Name.Location, this, syntax.Name, Diagnostics);
            if (!TryResolveMethod<AbstractMethodSymbol>(methodContext, boundExpression.Type, memberName, _typeArguments, 
                    _arguments, syntax.Expression, out var methodSymbol) ||
                methodSymbol is null)
            {
                return new BoundErrorExpression(syntax, context);
            }

            if (!methodSymbol.Modifiers.Contains(SyntaxKind.PublicKeyword) &&
                _currentType != methodSymbol.ParentType)
            {
                Diagnostics.ReportCannotAccessNonPublicMember(syntax.Name.Location, memberName);
            }
            
            return new BoundMemberAccessExpression(syntax, boundExpression, methodSymbol);
        }

        var memberSymbol = boundExpression.Type.GetMember(memberName);
        if (memberSymbol is null)
        {
            Diagnostics.ReportUndefinedMember(syntax.Name.Location, memberName, boundExpression.Type);
            return new BoundErrorExpression(syntax, context);
        }
        
        if (!memberSymbol.Modifiers.Contains(SyntaxKind.PublicKeyword) &&
            _currentType != memberSymbol.ParentType)
        {
            Diagnostics.ReportCannotAccessNonPublicMember(syntax.Name.Location, memberName);
        }
        
        return new BoundMemberAccessExpression(syntax, boundExpression, memberSymbol);
    }
    
    private BoundExpression BindNameExpression(NameExpressionSyntax syntax, SemanticContext context)
    {
        var name = syntax.IdentifierToken.Text;
        if (syntax.IdentifierToken.IsMissing)
        {
            // This is a parser error
            return new BoundErrorExpression(syntax, context);
        }

        var symbolContext = new SemanticContext(syntax.Location, this, syntax, Diagnostics);
        if (_isBindingInvocation)
        {
            // If there is no type to go off of, then we assume that the name is referring to a method that is declared
            // within the scope of the type
            // If the current type is null, then we will set it to null, and will try to find the method within the
            // current scope
            var type = _currentType ?? TypeSymbol.Error;
            if (!TryResolveMethod<AbstractMethodSymbol>(symbolContext, type, name, _typeArguments, _arguments, syntax, out var methodSymbol) ||
                methodSymbol is null)
            {
                return new BoundErrorExpression(syntax, context);
            }
            
            return new BoundNameExpression(syntax, methodSymbol);
        }

        if (!TryResolve<Symbol>(symbolContext, name, out var symbol))
        {
            return new BoundErrorExpression(syntax, context);
        }
        
        return new BoundNameExpression(syntax, symbol!);
    }
    
    private BoundExpression BindNewExpression(NewExpressionSyntax syntax, SemanticContext context)
    {
        var type = BindTypeSyntax(syntax.Type);
        var arguments = ImmutableArray.CreateBuilder<BoundExpression>();
        foreach (var argument in syntax.ArgumentList.Arguments)
        {
            var boundArgument = BindExpression(argument);
            arguments.Add(boundArgument);
        }

        if (type is not StructSymbol)
        {
            Diagnostics.ReportCannotInstantiateNonStruct(syntax.Type.Location, type.Name);
            return new BoundErrorExpression(syntax, context);
        }

        var constructorContext = new SemanticContext(syntax.Location, this, syntax, Diagnostics);
        if (!TryResolveMethod<ConstructorSymbol>(constructorContext, type, ".ctor", _typeArguments, 
                arguments.ToImmutable(), syntax.NewKeyword, out var constructor) ||
            constructor is null)
        {
            return new BoundErrorExpression(syntax, context);
        }
        
        // Now we bind conversion expressions for each argument
        var convertedArguments = ImmutableArray.CreateBuilder<BoundExpression>();
        for (var i = 0; i < arguments.Count; i++)
        {
            var argument = arguments[i];
            var parameter = constructor.Parameters[i];
            var convertedArgument = BindConversion(argument, parameter.Type, ImmutableArray<TypeSymbol>.Empty);
            convertedArguments.Add(convertedArgument);
        }
        
        return new BoundNewExpression(syntax, type, constructor, convertedArguments.ToImmutable());
    }

    private BoundExpression BindNewArrayExpression(NewArrayExpressionSyntax syntax, SemanticContext context)
    {
        var type = BindTypeSyntax(syntax.Type);
        if (type is not ArrayTypeSymbol array)
        {
            Diagnostics.ReportCannotInstantiateNonArray(syntax.Type.Location, type.Name);
            return new BoundErrorExpression(syntax, context);
        }

        if (syntax.Type.SizeExpression is not { } sizeExpr)
        {
            Diagnostics.ReportArrayMustHaveSize(syntax.Type.Location);
            return new BoundErrorExpression(syntax, context);
        }
        
        var size = BindExpression(sizeExpr);
        return new BoundNewArrayExpression(syntax, array, size);
    }
    
    private BoundExpression BindParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
    {
        var boundExpression = BindExpression(syntax.Expression);
        return boundExpression;
    }

    private BoundExpression BindElementAccessExpression(ElementAccessExpressionSyntax syntax, SemanticContext context)
    {
        var expression = BindExpression(syntax.Expression);
        if (expression.Type == TypeSymbol.Error)
        {
            return new BoundErrorExpression(syntax, context);
        }

        if (expression.Type is not ArrayTypeSymbol array)
        {
            Diagnostics.ReportCannotIndexNonArray(syntax.Expression.Location, expression.Type.Name);
            return new BoundErrorExpression(syntax, context);
        }
        
        var indexExpression = BindExpression(syntax.IndexExpression);
        var conversion = Conversion.Classify(indexExpression.Type, TypeSymbol.Int);
        if (!conversion.Exists ||
            conversion.IsExplicit)
        {
            Diagnostics.ReportIndexMustBeInteger(indexExpression.Syntax.Location);
            return new BoundErrorExpression(syntax, context);
        }
        
        return new BoundElementAccessExpression(syntax, array.ElementType, expression, indexExpression);
    }
    
    private BoundExpression BindThisExpression(ThisExpressionSyntax syntax, SemanticContext context)
    {
        if (_currentType is null)
        {
            Diagnostics.ReportThisExpressionOutsideOfMethod(syntax.Location);
            return new BoundErrorExpression(syntax, context);
        }

        if (_isStaticMethod)
        {
            Diagnostics.ReportThisExpressionInStaticMethod(syntax.Location);
        }
        
        return new BoundThisExpression(syntax, _currentType);
    }
    
    private BoundExpression BindDefaultExpression(DefaultExpressionSyntax syntax)
    {
        var type = BindTypeSyntax(syntax.Type);
        return new BoundDefaultExpression(syntax, type);
    }
    
    private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax, SemanticContext context)
    {
        var boundOperand = BindExpression(syntax.Operand);
        var boundOperator = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);
        if (boundOperator == null)
        {
            Diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text, boundOperand.Type);
            return new BoundErrorExpression(syntax, context);
        }
        
        return new BoundUnaryExpression(syntax, boundOperator, boundOperand);
    }
}