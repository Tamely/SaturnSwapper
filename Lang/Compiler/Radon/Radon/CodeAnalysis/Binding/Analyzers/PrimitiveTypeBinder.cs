using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Members;
using Radon.CodeAnalysis.Binding.Semantics.Statements;
using Radon.CodeAnalysis.Binding.Semantics.Types;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Analyzers;

internal sealed class PrimitiveTypeBinder : TypeBinder
{
    private readonly TypeSymbol _type;
    private readonly List<BoundMember> _boundMembers;
    private readonly bool _resolveTemplate;
    
    public ImmutableArray<BoundMember> BoundMembers => _boundMembers.ToImmutableArray();

    internal PrimitiveTypeBinder(Binder binder, TypeSymbol type) 
        : base(binder)
    {
        _type = type;
        _type.TypeBinder = this;
        _boundMembers = new List<BoundMember>();
        _resolveTemplate = false;
    }

    internal PrimitiveTypeBinder(Binder binder, TemplateSymbol template, TypeSymbol type)
        : base(binder)
    {
        _type = type;
        _type.TypeBinder = this;
        _boundMembers = new List<BoundMember>();
        _resolveTemplate = true;
        var resolvedMembers = ResolveMembers(template.Members);
        foreach (var member in resolvedMembers)
        {
            _type.AddMember(member);
        }
    }

    public override BoundNode Bind(SyntaxNode? node, params object[] args)
    {
        node ??= SyntaxNode.Empty;
        return _type switch
        {
            StructSymbol structSymbol => BindStruct(structSymbol, node),
            EnumSymbol enumSymbol => BindEnum(enumSymbol, node),
            TemplateSymbol templateSymbol => BindTemplate(templateSymbol, node),
            PrimitiveTemplateSymbol primitiveTemplateSymbol => BindTemplate(primitiveTemplateSymbol.Template, node),
            _ => throw new Exception($"Unexpected type: {_type.GetType()}")
        };
    }

    public override MethodSymbol BuildTemplateMethod(TemplateMethodSymbol templateMethod, ImmutableArray<TypeSymbol> typeArguments, SyntaxNode callSite)
    {
        var sb = new StringBuilder();
        sb.Append(templateMethod.Name);
        sb.Append('`');
        foreach (var typeArgument in typeArguments)
        {
            sb.Append(typeArgument.Name);
            sb.Append(',');
        }
        
        sb.Remove(sb.Length - 1, 1); // remove last comma
        var name = sb.ToString();
        if (_type.TryGetMember(name, out var member))
        {
            return (MethodSymbol)member!;
        }
        
        var templateMethodBinder = new TemplateMethodBinder(this);
        var boundTemplateMethod = (BoundMethod)templateMethodBinder.Bind(SyntaxNode.Empty, templateMethod, typeArguments, callSite, name);
        _boundMembers.Add(boundTemplateMethod);
        _type.AddMember(boundTemplateMethod.Symbol);
        return boundTemplateMethod.Symbol;
    }

    private ImmutableArray<MemberSymbol> ResolveMembers(ImmutableArray<MemberSymbol> memberSymbols)
    {
        var resolvedMembers = ImmutableArray.CreateBuilder<MemberSymbol>();
        foreach (var memberSymbol in memberSymbols)
        {
            var resolvedMember = memberSymbol switch
            {
                MethodSymbol methodSymbol => ResolveMethod(methodSymbol),
                ConstructorSymbol constructorSymbol => ResolveConstructor(constructorSymbol),
                FieldSymbol fieldSymbol => ResolveField(fieldSymbol),
                TemplateMethodSymbol templateMethodSymbol => ResolveTemplateMethod(templateMethodSymbol),
                _ => throw new Exception($"Unexpected member: {memberSymbol.GetType()}")
            };
            
            resolvedMembers.Add(resolvedMember);
        }
        
        return resolvedMembers.ToImmutable();
    }
    
    private MemberSymbol ResolveMethod(MethodSymbol methodSymbol)
    {
        var resolvedReturnType = ResolveType(methodSymbol.Type);
        var resolvedParameters = ResolveParameters(methodSymbol.Parameters);
        return new MethodSymbol(methodSymbol.ParentType, methodSymbol.Name, resolvedReturnType, resolvedParameters,
            methodSymbol.Modifiers);
    }
    
    private MemberSymbol ResolveConstructor(ConstructorSymbol constructorSymbol)
    {
        var resolvedParameters = ResolveParameters(constructorSymbol.Parameters);
        return new ConstructorSymbol(constructorSymbol.ParentType, resolvedParameters, constructorSymbol.Modifiers);
    }
    
    private MemberSymbol ResolveField(FieldSymbol fieldSymbol)
    {
        var resolvedType = ResolveType(fieldSymbol.Type);
        return new FieldSymbol(fieldSymbol.ParentType, fieldSymbol.Name, resolvedType, fieldSymbol.Modifiers);
    }
    
    private MemberSymbol ResolveTemplateMethod(TemplateMethodSymbol templateMethodSymbol)
    {
        var resolvedReturnType = ResolveType(templateMethodSymbol.Type);
        var resolvedParameters = ResolveParameters(templateMethodSymbol.Parameters);
        return new TemplateMethodSymbol(templateMethodSymbol.ParentType, templateMethodSymbol.Name, resolvedReturnType, 
            resolvedParameters, templateMethodSymbol.Modifiers, templateMethodSymbol.TypeParameters);
    }
    
    private TypeSymbol ResolveType(TypeSymbol typeSymbol)
    {
        var semanticContext = new SemanticContext(this, SyntaxNode.Empty, Diagnostics);
        TryResolveSymbol(semanticContext, ref typeSymbol);
        return typeSymbol;
    }
    
    private ImmutableArray<ParameterSymbol> ResolveParameters(ImmutableArray<ParameterSymbol> parameterSymbols)
    {
        var resolvedParameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
        foreach (var parameterSymbol in parameterSymbols)
        {
            var resolvedType = ResolveType(parameterSymbol.Type);
            var resolvedParameter = new ParameterSymbol(parameterSymbol.Name, resolvedType, parameterSymbol.Ordinal);
            resolvedParameters.Add(resolvedParameter);
        }
        
        return resolvedParameters.ToImmutable();
    }

    private BoundStruct BindStruct(StructSymbol structSymbol, SyntaxNode node)
    {
        return new BoundStruct(node, structSymbol, _boundMembers.ToImmutableArray());
    }
    
    private BoundEnum BindEnum(EnumSymbol enumSymbol, SyntaxNode node)
    {
        return new BoundEnum(node, enumSymbol, _boundMembers.Cast<BoundEnumMember>().ToImmutableArray());
    }
    
    private BoundNode BindTemplate(TemplateSymbol templateSymbol, SyntaxNode node)
    {
        if (_resolveTemplate)
        {
            return new BoundStruct(node, (StructSymbol)_type, _boundMembers.ToImmutableArray());
        }
        
        return new BoundTemplate(node, templateSymbol, _boundMembers.ToImmutableArray());
    }

    public void BindMembers()
    {
        if (_type is StructSymbol structSymbol)
        {
            foreach (var member in structSymbol.Members)
            {
                var boundMember = BindMember(member, SyntaxNode.Empty);
                if (boundMember is not BoundTemplateMethod)
                {
                    _boundMembers.Add(boundMember);
                }
            }
        }
        
        if (_type is EnumSymbol enumSymbol)
        {
            foreach (var member in enumSymbol.Members)
            {
                var boundMember = BindEnumMember((EnumMemberSymbol)member, SyntaxNode.Empty);
                _boundMembers.Add(boundMember);
            }
        }
        
        if (_type is TemplateSymbol templateSymbol)
        {
            foreach (var member in templateSymbol.Members)
            {
                var boundMember = BindMember(member, SyntaxNode.Empty);
                if (boundMember is not BoundTemplateMethod)
                {
                    _boundMembers.Add(boundMember);
                }
            }
        }
    }

    private BoundMember BindMember(MemberSymbol member, SyntaxNode syntax)
    {
        return member switch
        {
            FieldSymbol field => new BoundField(syntax, field, null),
            MethodSymbol method => new BoundMethod(syntax, method, ImmutableArray<BoundStatement>.Empty,
                ImmutableArray<LocalVariableSymbol>.Empty),
            ConstructorSymbol constructor => new BoundConstructor(syntax, constructor,
                ImmutableArray<BoundStatement>.Empty, ImmutableArray<LocalVariableSymbol>.Empty),
            TemplateMethodSymbol templateMethod => new BoundTemplateMethod(syntax, templateMethod,
                ImmutableArray<BoundStatement>.Empty, ImmutableArray<LocalVariableSymbol>.Empty),
            _ => throw new Exception($"Unexpected member: {member.GetType()}")
        };
    }
    
    private BoundEnumMember BindEnumMember(EnumMemberSymbol member, SyntaxNode syntax)
    {
        return new BoundEnumMember(syntax, member);
    }
}