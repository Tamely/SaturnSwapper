using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Binding.Semantics.Members;
using Radon.CodeAnalysis.Binding.Semantics.Types;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Syntax.Nodes.Clauses;
using Radon.CodeAnalysis.Syntax.Nodes.Members;
using Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations;

namespace Radon.CodeAnalysis.Binding.Analyzers;

internal sealed class TypeBinder : Binder
{
    private readonly Dictionary<MemberDeclarationSyntax, MemberSymbol> _members;
    private readonly TypeDeclarationSyntax _syntax;
    private readonly AssemblySymbol _assembly;
    private TypeSymbol _type;
    private int _previousValue;

    public TypeBinder(AssemblyBinder binder, TypeDeclarationSyntax syntax)
        : base(binder)
    {
        _members = new Dictionary<MemberDeclarationSyntax, MemberSymbol>();
        _syntax = syntax;
        _assembly = binder.Assembly;
        _type = TypeSymbol.Error;
    }
    
    public TypeDeclarationSyntax Syntax => _syntax;
    
    public TypeSymbol CreateType()
    {
        switch (_syntax)
        {
            case StructDeclarationSyntax structSyntax:
            {
                var name = structSyntax.Identifier.Text;
                var type = new StructSymbol(name, ImmutableArray<MemberSymbol>.Empty, _assembly,
                    ImmutableArray<SyntaxKind>.Empty);
                _type = type;
                return type;
            }
            case EnumDeclarationSyntax enumSyntax:
            {
                var name = enumSyntax.Identifier.Text;
                var type = new EnumSymbol(name, ImmutableArray<MemberSymbol>.Empty, _assembly,
                    ImmutableArray<SyntaxKind>.Empty);
                _type = type;
                return type;
            }
            default:
            {
                return TypeSymbol.Error;
            }
        }
    }

    public override BoundNode Bind(SyntaxNode? node, params object[] args)
    {
        node ??= _syntax;
        var typeContext = new SemanticContext(this, node, Diagnostics);
        if (node is StructDeclarationSyntax structSyntax)
        {
            return BindStruct(typeContext, structSyntax);
        }
        if (_syntax is EnumDeclarationSyntax enumSyntax)
        {
            return BindEnum(enumSyntax);
        }
        
        return new BoundErrorType(node, typeContext);
    }

    private BoundStruct BindStruct(SemanticContext context, StructDeclarationSyntax node)
    {
        var members = new List<BoundMember>();
        foreach (var member in node.Body.Members)
        {
            var boundMember = BindMember(context, member);
            members.Add(boundMember);
        }
        
        return new BoundStruct(node, (StructSymbol)_type, members.ToImmutableArray());
    }
    
    private BoundEnum BindEnum(EnumDeclarationSyntax node)
    {
        var members = new List<BoundEnumMember>();
        foreach (var member in node.Body.Members)
        {
            var boundMember = BindEnumMember(member);
            members.Add(boundMember);
        }
        
        _previousValue = 0;
        return new BoundEnum(node, (EnumSymbol)_type, members.ToImmutableArray());
    }

    public TypeSymbol ResolveMembers()
    {
        if (_syntax is StructDeclarationSyntax structDeclaration)
        {
            var members = structDeclaration.Body.Members;
            foreach (var member in members)
            {
                var memberSymbol = ResolveMember(member, structDeclaration);
                _members.Add(member, memberSymbol);
                var memberRegContext = new SemanticContext(member.Location, this, member, Diagnostics);
                Register(memberRegContext, memberSymbol);
                _type.AddMember(memberSymbol);
            }
        }
        else if (_syntax is EnumDeclarationSyntax enumDeclaration)
        {
            var members = enumDeclaration.Body.Members;
            foreach (var member in members)
            {
                var memberSymbol = ResolveEnumMember(member);
                _members.Add(member, memberSymbol);
                var memberRegContext = new SemanticContext(member.Location, this, member, Diagnostics);
                Register(memberRegContext, memberSymbol);
                _type.AddMember(memberSymbol);
            }
        }

        return _type;
    }

    private MemberSymbol ResolveMember(MemberDeclarationSyntax member, StructDeclarationSyntax structDeclaration)
    {
        if (member is ConstructorDeclarationSyntax constructor)
        {
            return ResolveConstructor(constructor, structDeclaration);
        }
        
        if (member is FieldDeclarationSyntax field)
        {
            return ResolveField(field);
        }

        if (member is MethodDeclarationSyntax method)
        {
            return ResolveMethod(method);
        }

        return MemberSymbol.Error;
    }

    private ConstructorSymbol ResolveConstructor(ConstructorDeclarationSyntax constructor, StructDeclarationSyntax structDeclaration)
    {
        var modifiers = constructor.Modifiers;
        var name = constructor.Type.Identifier.Text;
        var context = new SemanticContext(this, constructor.Type, Diagnostics);
        if (!TryResolve<TypeSymbol>(context, name, out var type))
        {
            type = TypeSymbol.Error;
        }
        
        if (type != _type)
        {
            Diagnostics.ReportConstructorNameMismatch(constructor.Location, name, structDeclaration.Identifier.Text);
            return ConstructorSymbol.Error;
        }
        
        var parameters = ResolveParameters(constructor.ParameterList.Parameters);
        return new ConstructorSymbol(_type, parameters, modifiers.Select(x => x.Kind).ToImmutableArray());
    }
    
    private FieldSymbol ResolveField(FieldDeclarationSyntax field)
    {
        var modifiers = field.Modifiers;
        var context = new SemanticContext(this, field.Type, Diagnostics);
        if (!TryResolve<TypeSymbol>(context, field.Type.Identifier.Text, out var type))
        {
            type = TypeSymbol.Error;
        }

        CheckForCycle(type!, field);

        var name = field.VariableDeclarator.Identifier.Text;
        return new FieldSymbol(_type, name, type!, modifiers.Select(x => x.Kind).ToImmutableArray());
    }
    
    private EnumMemberSymbol ResolveEnumMember(EnumMemberDeclarationSyntax member)
    {
        var name = member.Identifier.Text;
        return new EnumMemberSymbol(_type, name, TypeSymbol.Int, 0); // 0 is a placeholder until the enum member is bound
    }

    private void CheckForCycle(TypeSymbol type, FieldDeclarationSyntax syntax)
    {
        if (type == _type)
        {
            Diagnostics.ReportCycleInStructLayout(syntax.Location, _type.Name);
            return;
        }
        
        if (type is StructSymbol structSymbol)
        {
            foreach (var member in structSymbol.Members)
            {
                if (member is FieldSymbol field)
                {
                    CheckForCycle(field.Type, syntax);
                }
            }
        }
    }
    
    private MethodSymbol ResolveMethod(MethodDeclarationSyntax method)
    {
        var modifiers = method.Modifiers;
        var context = new SemanticContext(this, method.ReturnType, Diagnostics);
        if (!TryResolve<TypeSymbol>(context, method.ReturnType.Identifier.Text, out var type))
        {
            type = TypeSymbol.Error;
        }

        var name = method.Identifier.Text;
        var parameters = ResolveParameters(method.ParameterList.Parameters);
        return new MethodSymbol(_type, name, type!, ImmutableArray<TypeParameterSymbol>.Empty, 
            parameters, modifiers.Select(x => x.Kind).ToImmutableArray());
    }
    
    private ImmutableArray<ParameterSymbol> ResolveParameters(ImmutableSyntaxList<ParameterSyntax> parameters)
    {
        var parameterSymbols = new List<ParameterSymbol>();
        for (var i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];
            var parameterSymbol = ResolveParameter(parameter, i);
            parameterSymbols.Add(parameterSymbol);
        }

        return parameterSymbols.ToImmutableArray();
    }

    private ParameterSymbol ResolveParameter(ParameterSyntax parameter, int ordinal)
    {
        var name = parameter.Identifier.Text;
        var context = new SemanticContext(this, parameter.Type, Diagnostics);
        if (!TryResolve<TypeSymbol>(context, parameter.Type.Identifier.Text, out var type))
        {
            type = TypeSymbol.Error;
        }
        
        return new ParameterSymbol(name, type!, ordinal);
    }
    
    private BoundMember BindMember(SemanticContext context, MemberDeclarationSyntax member)
    {
        if (member is ConstructorDeclarationSyntax constructor)
        {
            return BindConstructor(constructor);
        }
        
        if (member is FieldDeclarationSyntax field)
        {
            return BindField(field);
        }

        if (member is MethodDeclarationSyntax method)
        {
            return BindMethod(method);
        }

        return new BoundErrorMember(member, context);
    }
    
    private BoundConstructor BindConstructor(ConstructorDeclarationSyntax syntax)
    {
        var methodBinder = new MethodBinder(this);
        var constructorSymbol = _members[syntax];
        var method = methodBinder.Bind(syntax, constructorSymbol);
        Diagnostics.AddRange(methodBinder.Diagnostics);
        return (BoundConstructor)method;
    }
    
    private BoundField BindField(FieldDeclarationSyntax syntax)
    {
        var expressionBinder = new ExpressionBinder(this);
        var fieldSymbol = (FieldSymbol)_members[syntax];
        var initializer = syntax.VariableDeclarator.Initializer;
        if (initializer is null)
        {
            return new BoundField(syntax, fieldSymbol, null);
        }
        
        var expression = (BoundExpression)expressionBinder.Bind(initializer);
        Diagnostics.AddRange(expressionBinder.Diagnostics);
        return new BoundField(syntax, fieldSymbol, expression);
    }
    
    private BoundMethod BindMethod(MethodDeclarationSyntax syntax)
    {
        var methodBinder = new MethodBinder(this);
        var methodSymbol = _members[syntax];
        var method = methodBinder.Bind(syntax, methodSymbol);
        Diagnostics.AddRange(methodBinder.Diagnostics);
        return (BoundMethod)method;
    }

    private BoundEnumMember BindEnumMember(EnumMemberDeclarationSyntax syntax)
    {
        var expressionBinder = new ExpressionBinder(this);
        var enumMemberSymbol = (EnumMemberSymbol)_members[syntax];
        var initializer = syntax.Initializer;
        if (initializer is null)
        {
            enumMemberSymbol.ReplaceValue(_previousValue++);
        }
        else
        {
            var expression = (BoundExpression)expressionBinder.Bind(initializer);
            Diagnostics.AddRange(expressionBinder.Diagnostics);
            if (expression.ConstantValue is null)
            {
                Diagnostics.ReportEnumMemberMustHaveConstantValue(syntax.Location, enumMemberSymbol.Name);
                return new BoundEnumMember(syntax, enumMemberSymbol);
            }
            
            enumMemberSymbol.ReplaceValue(expression.ConstantValue?.Value as int? ?? _previousValue++);
        }
        
        return new BoundEnumMember(syntax, enumMemberSymbol);
    }
}