using System;
using System.Collections.Immutable;
using System.Globalization;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Conversions;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Syntax.Nodes.Clauses;
using Radon.CodeAnalysis.Syntax.Nodes.Expressions;

namespace Radon.CodeAnalysis.Binding.Analyzers;

internal sealed class ExpressionBinder : Binder
{
    private readonly bool _isStaticMethod;
    private readonly TypeSymbol? _currentType;
    
    private bool _isBindingInvocation;
    private ImmutableArray<BoundExpression> _argumentTypes;
    
    internal ExpressionBinder(Binder binder) 
        : base(binder)
    {
        _isBindingInvocation = false;
        _argumentTypes = ImmutableArray<BoundExpression>.Empty;
        if (binder is StatementBinder statementBinder)
        {
            _isStaticMethod = statementBinder.IsStaticMethod;
            _currentType = statementBinder.MethodSymbol?.ParentType;
        }
    }

    public override BoundNode Bind(SyntaxNode node, params object[] args)
    {
        var expressionContext = new SemanticContext(this, node, Diagnostics);
        if (args.Length > 0)
        {
            return new BoundErrorExpression(node, expressionContext);
        }

        if (node is not ExpressionSyntax syntax)
        {
            return new BoundErrorExpression(node, expressionContext);
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
            ImportExpressionSyntax importExpressionSyntax => BindImportExpression(importExpressionSyntax, expressionContext),
            InvocationExpressionSyntax invocationExpressionSyntax => BindInvocationExpression(invocationExpressionSyntax, expressionContext),
            LiteralExpressionSyntax literalExpressionSyntax => BindLiteralExpression(literalExpressionSyntax, expressionContext),
            MemberAccessExpressionSyntax memberAccessExpressionSyntax => BindMemberAccessExpression(memberAccessExpressionSyntax, expressionContext),
            NameExpressionSyntax nameExpressionSyntax => BindNameExpression(nameExpressionSyntax, expressionContext),
            NewExpressionSyntax newExpressionSyntax => BindNewExpression(newExpressionSyntax, expressionContext),
            ThisExpressionSyntax thisExpressionSyntax => BindThisExpression(thisExpressionSyntax, expressionContext),
            DefaultExpressionSyntax defaultExpressionSyntax => BindDefaultExpression(defaultExpressionSyntax, expressionContext),
            UnaryExpressionSyntax unaryExpressionSyntax => BindUnaryExpression(unaryExpressionSyntax, expressionContext),
            _ => new BoundErrorExpression(syntax, expressionContext)
        };
    }

    public BoundExpression BindConversion(BoundExpression expression, TypeSymbol type, bool allowExplicit = false)
    {
        var conversion = Conversion.Classify(expression.Type, type);
        if (!conversion.Exists)
        {
            if (expression.Type != TypeSymbol.Error &&
                type != TypeSymbol.Error)
            {
                Diagnostics.ReportCannotConvert(expression.Syntax.Location, expression.Type, type);
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
                var number = Convert.ToDouble(value);
                var literalType = GetLiteralType(number);
                var literalConversion = Conversion.Classify(literalType, type);
                if (literalConversion is { Exists: true, IsImplicit: true })
                {
                    return new BoundLiteralExpression(literalSyntax, type, value);
                }
                
                Diagnostics.ReportCannotConvertSourceType(literalSyntax.Location, literalType, type);
                return new BoundErrorExpression(literalSyntax, new SemanticContext(this, literalSyntax, Diagnostics));
            }
        }
        
        if (!allowExplicit && conversion.IsExplicit)
        {
            Diagnostics.ReportCannotConvertImplicitly(expression.Syntax.Location, expression.Type, type);
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
    
        if (IsInRange(byte.MinValue, byte.MaxValue, number))
        {
            return TypeSymbol.Byte;
        }
    
        if (IsInRange(sbyte.MinValue, sbyte.MaxValue, number))
        {
            return TypeSymbol.SByte;
        }
    
        if (IsInRange(short.MinValue, short.MaxValue, number))
        {
            return TypeSymbol.Short;
        }
    
        if (IsInRange(ushort.MinValue, ushort.MaxValue, number))
        {
            return TypeSymbol.UShort;
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
        var convertedRight = BindConversion(boundRight, boundLeft.Type);
        return new BoundAssignmentExpression(syntax, boundLeft, convertedRight);
    }
    
    private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax, SemanticContext context)
    {
        var boundLeft = BindExpression(syntax.Left);
        var boundRight = BindExpression(syntax.Right);
        var convertedRight = BindConversion(boundRight, boundLeft.Type);
        var boundOperator = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, convertedRight.Type);
        if (boundOperator == null)
        {
            Diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Location, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
            return new BoundErrorExpression(syntax, context);
        }
        
        return new BoundBinaryExpression(syntax, boundLeft, boundOperator, boundRight);
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
        static MethodSymbol? StripMethodSymbol(BoundExpression expression)
        {
            if (expression is BoundMemberAccessExpression memberAccessExpression)
            {
                return memberAccessExpression.Member as MethodSymbol;
            }
            
            if (expression is BoundNameExpression nameExpression)
            {
                return (nameExpression.Symbol as MethodSymbol)!;
            }
            
            return null;
        }
        
        var argumentTypes = ImmutableArray.CreateBuilder<BoundExpression>();
        foreach (var argument in syntax.ArgumentList.Arguments)
        {
            var boundArgument = BindExpression(argument);
            argumentTypes.Add(boundArgument);
        }
        
        _isBindingInvocation = true;
        _argumentTypes = argumentTypes.ToImmutable();
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

        // A type map is used to map type parameters to type arguments
        var typeMap = TypeMap.Empty;
        if (methodSymbol.TypeParameters.Length > 0)
        {
            if (syntax.TypeArgumentList is null)
            {
                Diagnostics.ReportTypeArgumentsRequired(syntax.Expression.Location, methodSymbol.Name, 
                    methodSymbol.TypeParameters.Length);
                return new BoundErrorExpression(syntax, context);
            }
            
            if (syntax.TypeArgumentList.Arguments.Count != methodSymbol.TypeParameters.Length)
            {
                Diagnostics.ReportIncorrectNumberOfTypeArguments(syntax.TypeArgumentList.Location, methodSymbol.Name, 
                    methodSymbol.TypeParameters.Length, syntax.TypeArgumentList.Arguments.Count);
                return new BoundErrorExpression(syntax, context);
            }
            
            for (var i = 0; i < syntax.TypeArgumentList.Arguments.Count; i++)
            {
                var typeArgument = BindTypeSyntax(syntax.TypeArgumentList.Arguments[i]);
                if (typeArgument == TypeSymbol.Error)
                {
                    return new BoundErrorExpression(syntax, context);
                }
                
                typeMap.AddBound(methodSymbol.TypeParameters[i], typeArgument); 
            }
        }
        
        methodSymbol = methodSymbol.WithTypeParameters(typeMap);
        var boundArguments = ImmutableDictionary.CreateBuilder<ParameterSymbol, BoundExpression>();
        if (syntax.ArgumentList.Arguments.Count != methodSymbol.Parameters.Length)
        {
            Diagnostics.ReportIncorrectNumberOfArguments(syntax.ArgumentList.Location, methodSymbol.Name, 
                methodSymbol.Parameters.Length, syntax.ArgumentList.Arguments.Count);
            return new BoundErrorExpression(syntax, context);
        }
        
        for (var i = 0; i < syntax.ArgumentList.Arguments.Count; i++)
        {
            var argument = syntax.ArgumentList.Arguments[i];
            var parameter = methodSymbol.Parameters[i];
            var boundArgument = BindExpression(argument);
            var parameterType = parameter.Type;
            if (parameter.Type is TypeParameterSymbol typeParam)
            {
                parameterType = Conversion.ResolveGenericType(typeParam);
            }

            var convertedArgument = BindConversion(boundArgument, parameterType);
            if (convertedArgument is BoundErrorExpression)
            {
                return new BoundErrorExpression(syntax, context);
            }
            
            boundArguments.Add(parameter, convertedArgument);
        }

        var returnType = methodSymbol.Type;
        _argumentTypes = _argumentTypes.Clear();
        return new BoundInvocationExpression(syntax, methodSymbol, boundExpression, typeMap, 
            boundArguments.ToImmutable(), returnType);
    }
    
    private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax, SemanticContext context)
    {
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
                <= sbyte.MaxValue and >= sbyte.MinValue => new BoundLiteralExpression(syntax, TypeSymbol.SByte,
                    (sbyte)doubleValue),
                <= byte.MaxValue and >= byte.MinValue => new BoundLiteralExpression(syntax, TypeSymbol.Byte,
                    (byte)doubleValue),
                <= short.MaxValue and >= short.MinValue => new BoundLiteralExpression(syntax, TypeSymbol.Short,
                    (short)doubleValue),
                <= ushort.MaxValue and >= ushort.MinValue => new BoundLiteralExpression(syntax, TypeSymbol.UShort,
                    (ushort)doubleValue),
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
            if (!TryResolveMethod<MethodSymbol>(methodContext, boundExpression.Type, memberName, _argumentTypes, out var methodSymbol))
            {
                return new BoundErrorExpression(syntax, context);
            }

            return new BoundMemberAccessExpression(syntax, boundExpression, methodSymbol);
        }

        var memberSymbol = boundExpression.Type.GetMember(memberName);
        if (memberSymbol is null)
        {
            Diagnostics.ReportUndefinedMember(syntax.Name.Location, memberName, boundExpression.Type);
            return new BoundErrorExpression(syntax, context);
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
            if (!TryResolveMethod<MethodSymbol>(symbolContext, type, name, _argumentTypes, out var methodSymbol))
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
        var argumentTypes = ImmutableArray.CreateBuilder<BoundExpression>();
        foreach (var argument in syntax.ArgumentList.Arguments)
        {
            var boundArgument = BindExpression(argument);
            argumentTypes.Add(boundArgument);
        }
        
        var typeContext = new SemanticContext(syntax.Location, this, syntax, Diagnostics);
        if (!TryResolve<TypeSymbol>(typeContext, syntax.Type.Identifier.Text, out var typeSymbol) ||
            typeSymbol is null)
        {
            return new BoundErrorExpression(syntax, context);
        }

        if (typeSymbol is not StructSymbol s)
        {
            Diagnostics.ReportCannotInstantiateNonStruct(syntax.Type.Location, typeSymbol.Name);
            return new BoundErrorExpression(syntax, context);
        }
        
        var constructorContext = new SemanticContext(syntax.Location, this, syntax, Diagnostics);
        if (!TryResolveMethod<ConstructorSymbol>(constructorContext, typeSymbol, ".ctor", argumentTypes.ToImmutable(), out var constructor))
        {
            return new BoundErrorExpression(syntax, context);
        }

        var typeMap = TypeMap.Empty;
        var typeArgumentList = syntax.Type.TypeArgumentList;
        if (s.TypeParameters.Length > 0)
        {
            if (typeArgumentList is null)
            {
                Diagnostics.ReportTypeArgumentsRequired(syntax.Type.Location, constructor.Name, 
                    constructor.TypeParameters.Length);
                return new BoundErrorExpression(syntax, context);
            }
            
            if (typeArgumentList.Arguments.Count != s.TypeParameters.Length)
            {
                Diagnostics.ReportIncorrectNumberOfTypeArguments(typeArgumentList.Location, constructor.Name, 
                    constructor.TypeParameters.Length, typeArgumentList.Arguments.Count);
                return new BoundErrorExpression(syntax, context);
            }
            
            for (var i = 0; i < typeArgumentList.Arguments.Count; i++)
            {
                var typeArgument = BindTypeSyntax(typeArgumentList.Arguments[i]);
                if (typeArgument == TypeSymbol.Error)
                {
                    return new BoundErrorExpression(syntax, context);
                }
                
                typeMap.AddBound(s.TypeParameters[i], typeArgument);
            }
        }

        s = s.WithTypeParameters(typeMap);
        constructor = constructor.WithTypeParameters(typeMap);
        var boundArguments = ImmutableDictionary.CreateBuilder<ParameterSymbol, BoundExpression>();
        if (syntax.ArgumentList.Arguments.Count != constructor.Parameters.Length)
        {
            Diagnostics.ReportIncorrectNumberOfArguments(syntax.ArgumentList.Location, constructor.Name, 
                constructor.Parameters.Length, syntax.ArgumentList.Arguments.Count);
            return new BoundErrorExpression(syntax, context);
        }
        
        for (var i = 0; i < syntax.ArgumentList.Arguments.Count; i++)
        {
            var argument = syntax.ArgumentList.Arguments[i];
            var parameter = constructor.Parameters[i];
            var boundArgument = BindExpression(argument);
            var parameterType = parameter.Type;
            if (parameter.Type is TypeParameterSymbol typeParam)
            {
                parameterType = Conversion.ResolveGenericType(typeParam);
            }

            var convertedArgument = BindConversion(boundArgument, parameterType);
            if (convertedArgument is BoundErrorExpression)
            {
                return new BoundErrorExpression(syntax, context);
            }
            
            boundArguments.Add(parameter, convertedArgument);
        }

        return new BoundNewExpression(syntax, s, typeMap, constructor, boundArguments.ToImmutable());
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
    
    private BoundExpression BindDefaultExpression(DefaultExpressionSyntax syntax, SemanticContext context)
    {
        var typeContext = new SemanticContext(syntax.Location, this, syntax, Diagnostics);
        if (!TryResolve<TypeSymbol>(typeContext, syntax.Type.Identifier.Text, out var typeSymbol))
        {
            return new BoundErrorExpression(syntax, context);
        }
        
        return new BoundDefaultExpression(syntax, typeSymbol!);
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
    
    private TypeSymbol BindTypeSyntax(TypeSyntax syntax)
    {
        var typeContext = new SemanticContext(syntax.Location, this, syntax, Diagnostics);
        if (!TryResolve<TypeSymbol>(typeContext, syntax.Identifier.Text, out var typeSymbol))
        {
            return TypeSymbol.Error;
        }
        
        return typeSymbol!;
    }
}