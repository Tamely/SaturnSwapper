using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Expressions;
using Radon.CodeAnalysis.Binding.Semantics.Members;
using Radon.CodeAnalysis.Binding.Semantics.Operators;
using Radon.CodeAnalysis.Binding.Semantics.Statements;
using Radon.CodeAnalysis.Binding.Semantics.Types;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax;
using Radon.CodeAnalysis.Syntax.Nodes;
using Radon.CodeAnalysis.Syntax.Nodes.Clauses;
using Radon.CodeAnalysis.Syntax.Nodes.Members;
using Radon.CodeAnalysis.Syntax.Nodes.TypeDeclarations;

namespace Radon.CodeAnalysis.Binding.Binders;

internal sealed class NamedTypeBinder : TypeBinder
{
    private readonly Dictionary<MemberDeclarationSyntax, MemberSymbol> _members;
    private readonly Dictionary<TemplateMethodSymbol, TemplateMethodDeclarationSyntax> _templateMethods;
    private readonly List<BoundMember> _boundMembers;
    private readonly TypeDeclarationSyntax _syntax;
    private readonly AssemblySymbol _assembly;
    private readonly bool _resolveTemplate;
    private TypeSymbol _type;
    private BoundConstructor? _defaultConstructor;
    private int _previousValue;
    private int _fieldOffset;
    
    public SymbolKind CurrentMember { get; private set; }

    public NamedTypeBinder(AssemblyBinder binder, TypeDeclarationSyntax syntax)
        : base(binder)
    {
        _members = new Dictionary<MemberDeclarationSyntax, MemberSymbol>();
        _templateMethods = new Dictionary<TemplateMethodSymbol, TemplateMethodDeclarationSyntax>();
        _boundMembers = new List<BoundMember>();
        _syntax = syntax;
        _assembly = binder.Assembly;
        _type = TypeSymbol.Error;
        _previousValue = 0;
        _resolveTemplate = false;
    }
    
    public NamedTypeBinder(Binder binder, TypeSymbol type, TypeDeclarationSyntax syntax)
        : base(binder)
    {
        _members = new Dictionary<MemberDeclarationSyntax, MemberSymbol>();
        _templateMethods = new Dictionary<TemplateMethodSymbol, TemplateMethodDeclarationSyntax>();
        _boundMembers = new List<BoundMember>();
        _syntax = syntax;
        _assembly = type.ParentAssembly ?? throw new ArgumentException("Type must have a parent assembly.", nameof(type));
        _type = type;
        BoundBinaryOperator.CreateTypeOperators(_type);
        _previousValue = 0;
        _resolveTemplate = true;
        ResolveMembers();
    }
    
    public TypeDeclarationSyntax Syntax => _syntax;
    
    public TypeSymbol CreateType()
    {
        switch (_syntax)
        {
            case StructDeclarationSyntax structSyntax:
            {
                CurrentMember = SymbolKind.Struct;
                var name = structSyntax.Identifier.Text;
                var modifiers = ResolveModifiers(structSyntax.Modifiers);
                var type = new StructSymbol(name, ImmutableArray<MemberSymbol>.Empty, _assembly, modifiers, this);
                _type = type;
                break;
            }
            case EnumDeclarationSyntax enumSyntax:
            {
                CurrentMember = SymbolKind.Enum;
                var name = enumSyntax.Identifier.Text;
                var modifiers = ResolveModifiers(enumSyntax.Modifiers);
                var type = new EnumSymbol(name, ImmutableArray<MemberSymbol>.Empty, _assembly, modifiers, this);
                _type = type;
                break;
            }
            case TemplateDeclarationSyntax templateSyntax:
            {
                CurrentMember = SymbolKind.Template;
                var name = templateSyntax.Identifier.Text;
                var modifiers = ResolveModifiers(templateSyntax.Modifiers);
                var typeParameters = ResolveTypeParameters(templateSyntax.TypeParameterList);
                foreach (var (typeParameter, syntax) in typeParameters)
                {
                    var context = new SemanticContext(this, syntax, Diagnostics);
                    Register(context, typeParameter);
                }

                var orderedTypeParameters = new TypeParameterSymbol[typeParameters.Count];
                foreach (var (typeParameter, _) in typeParameters)
                {
                    orderedTypeParameters[typeParameter.Ordinal] = typeParameter;
                }
                
                var type = new TemplateSymbol(name, ImmutableArray<MemberSymbol>.Empty, _assembly,
                    modifiers, orderedTypeParameters.ToImmutableArray(), this);
                _type = type;
                break;
            }
            default:
            {
                return TypeSymbol.Error;
            }
        }
        
        BoundBinaryOperator.CreateTypeOperators(_type);
        return _type;
    }

    public override BoundNode Bind(SyntaxNode? node, params object[] args)
    {
        node ??= _syntax;
        var typeContext = new SemanticContext(this, node, Diagnostics);
        if (node is StructDeclarationSyntax structSyntax)
        {
            return BindStruct(structSyntax);
        }
        if (node is EnumDeclarationSyntax enumSyntax)
        {
            return BindEnum(enumSyntax);
        }
        if (node is TemplateDeclarationSyntax templateSyntax)
        {
            return BindTemplate(templateSyntax);
        }
        
        return new BoundErrorType(node, typeContext);
    }

    private BoundStruct BindStruct(StructDeclarationSyntax node)
    {
        return new BoundStruct(node, (StructSymbol)_type, _boundMembers.ToImmutableArray());
    }
    
    private BoundEnum BindEnum(EnumDeclarationSyntax node)
    {
        _previousValue = 0;
        return new BoundEnum(node, (EnumSymbol)_type, _boundMembers.Cast<BoundEnumMember>().ToImmutableArray());
    }

    private BoundType BindTemplate(TemplateDeclarationSyntax node)
    {
        if (_resolveTemplate)
        {
            return new BoundStruct(node, (StructSymbol)_type, _boundMembers.ToImmutableArray());
        }
        
        return new BoundTemplate(node, (TemplateSymbol)_type, _boundMembers.ToImmutableArray());
    }

    private ImmutableDictionary<TypeParameterSymbol, TypeParameterSyntax> ResolveTypeParameters(TypeParameterListSyntax typeParameters)
    {
        var parameters = new Dictionary<TypeParameterSymbol, TypeParameterSyntax>();
        for (var i = 0; i < typeParameters.Parameters.Count; i++)
        {
            var parameter = typeParameters.Parameters[i];
            var name = parameter.Identifier.Text;
            var typeParameter = new TypeParameterSymbol(name, i);
            parameters.Add(typeParameter, parameter);
        }

        return parameters.ToImmutableDictionary();
    }

    public void BindMembers()
    {
        if (_syntax is StructDeclarationSyntax structDeclaration)
        {
            foreach (var member in structDeclaration.Body.Members)
            {
                var context = new SemanticContext(this, member, Diagnostics);
                var m = BindMember(context, member);
                if (m is not BoundTemplateMethod)
                {
                    _boundMembers.Add(m);
                }
            }
        }
        else if (_syntax is EnumDeclarationSyntax enumDeclaration)
        {
            foreach (var member in enumDeclaration.Body.Members)
            {
                var m = BindEnumMember(member);
                _boundMembers.Add(m);
            }
        }
        else if (_syntax is TemplateDeclarationSyntax templateDeclaration)
        {
            foreach (var member in templateDeclaration.Body.Members)
            {
                var context = new SemanticContext(this, member, Diagnostics);
                var m = BindMember(context, member);
                if (m is not BoundTemplateMethod)
                {
                    _boundMembers.Add(m);
                }
            }
        }
    }

    public void ResolveMembers()
    {
        if (_syntax is StructDeclarationSyntax structDeclaration)
        {
            var members = structDeclaration.Body.Members;
            foreach (var member in members)
            {
                var memberSymbol = ResolveMember(member, structDeclaration);
                if (memberSymbol == MemberSymbol.Error)
                {
                    continue;
                }

                if (!memberSymbol.Modifiers.Contains(SyntaxKind.PublicKeyword) &&
                    !memberSymbol.Modifiers.Contains(SyntaxKind.PrivateKeyword))
                {
                }
                
                _members.Add(member, memberSymbol);
                var memberRegContext = new SemanticContext(member.Location, this, member, Diagnostics);
                Register(memberRegContext, memberSymbol);
                _type.AddMember(memberSymbol);
            }
            
            CurrentMember = SymbolKind.Error;
        }
        else if (_syntax is EnumDeclarationSyntax enumDeclaration)
        {
            var members = enumDeclaration.Body.Members;
            foreach (var member in members)
            {
                var memberSymbol = ResolveEnumMember(member);
                if (memberSymbol == MemberSymbol.Error)
                {
                    continue;
                }
                
                _members.Add(member, memberSymbol);
                var memberRegContext = new SemanticContext(member.Location, this, member, Diagnostics);
                Register(memberRegContext, memberSymbol);
                _type.AddMember(memberSymbol);
            }
            
            CurrentMember = SymbolKind.Error;
        }
        else if (_syntax is TemplateDeclarationSyntax templateDeclaration)
        {
            var members = templateDeclaration.Body.Members;
            foreach (var member in members)
            {
                var memberSymbol = ResolveMember(member, templateDeclaration);
                if (memberSymbol == MemberSymbol.Error)
                {
                    continue;
                }
                
                _members.Add(member, memberSymbol);
                var memberRegContext = new SemanticContext(member.Location, this, member, Diagnostics);
                Register(memberRegContext, memberSymbol);
                _type.AddMember(memberSymbol);
            }

            CurrentMember = SymbolKind.Error;
        }

        // Add default constructor if none is defined
        if (!_type.Members.Any(m => m is ConstructorSymbol { Parameters.Length: 0 }) &&
            _type is not EnumSymbol)
        {
            var constructor = new ConstructorSymbol(_type, ImmutableArray<ParameterSymbol>.Empty, ImmutableArray<SyntaxKind>.Empty);
            _type.AddMember(constructor);
            _defaultConstructor =
                new BoundConstructor(SyntaxNode.Empty, constructor, ImmutableArray<BoundStatement>.Empty, ImmutableArray<LocalVariableSymbol>.Empty);
            _boundMembers.Add(_defaultConstructor);
        }
    }

    private MemberSymbol ResolveMember(MemberDeclarationSyntax member, StructDeclarationSyntax structDeclaration)
    {
        if (member is ConstructorDeclarationSyntax constructor)
        {
            CurrentMember = SymbolKind.Constructor;
            return ResolveConstructor(constructor, structDeclaration);
        }
        
        if (member is FieldDeclarationSyntax field)
        {
            CurrentMember = SymbolKind.Field;
            return ResolveField(field);
        }

        if (member is MethodDeclarationSyntax method)
        {
            CurrentMember = SymbolKind.Method;
            return ResolveMethod(method);
        }

        if (member is TemplateMethodDeclarationSyntax templateMethod)
        {
            CurrentMember = SymbolKind.TemplateMethod;
            var sym = ResolveTemplateMethod(templateMethod);
            _templateMethods.Add(sym, templateMethod);
            return sym;
        }

        CurrentMember = SymbolKind.Error;
        return MemberSymbol.Error;
    }

    private MemberSymbol ResolveMember(MemberDeclarationSyntax member, TemplateDeclarationSyntax templateDeclaration)
    {
        if (member is ConstructorDeclarationSyntax constructor)
        {
            CurrentMember = SymbolKind.Constructor;
            return ResolveConstructor(constructor, templateDeclaration);
        }
        
        if (member is FieldDeclarationSyntax field)
        {
            CurrentMember = SymbolKind.Field;
            return ResolveField(field);
        }

        if (member is MethodDeclarationSyntax method)
        {
            CurrentMember = SymbolKind.Method;
            return ResolveMethod(method);
        }

        if (member is TemplateMethodDeclarationSyntax templateMethod)
        {
            CurrentMember = SymbolKind.TemplateMethod;
            var sym = ResolveTemplateMethod(templateMethod);
            _templateMethods.Add(sym, templateMethod);
        }

        return MemberSymbol.Error;
    }

    private ConstructorSymbol ResolveConstructor(ConstructorDeclarationSyntax constructor, StructDeclarationSyntax structDeclaration)
    {
        var modifiers = ResolveModifiers(constructor.Modifiers);
        var type = BindTypeSyntax(constructor.Type);
        if (type != _type)
        {
            Diagnostics.ReportConstructorNameMismatch(constructor.Location, type.Name, structDeclaration.Identifier.Text);
            return ConstructorSymbol.Error;
        }
        
        var parameters = ResolveParameters(constructor.ParameterList.Parameters);
        return new ConstructorSymbol(_type, parameters, modifiers);
    }

    private ConstructorSymbol ResolveConstructor(ConstructorDeclarationSyntax constructor, TemplateDeclarationSyntax templateDeclaration)
    {
        var modifiers = ResolveModifiers(constructor.Modifiers);
        var type = BindTypeSyntax(constructor.Type);
        if (_resolveTemplate)
        {
            type = _type;
        }
        
        if (type != _type)
        {
            Diagnostics.ReportConstructorNameMismatch(constructor.Location, type.Name, templateDeclaration.Identifier.Text);
            return ConstructorSymbol.Error;
        }
        
        var parameters = ResolveParameters(constructor.ParameterList.Parameters);
        return new ConstructorSymbol(_type, parameters, modifiers);
    }
    
    private FieldSymbol ResolveField(FieldDeclarationSyntax field)
    {
        var modifiers = ResolveModifiers(field.Modifiers);
        var type = BindTypeSyntax(field.Type);
        CheckForCycle(type, field);
        var name = field.VariableDeclarator.Identifier.Text;
        var offset = _fieldOffset;
        _fieldOffset += type.Size;
        return new FieldSymbol(_type, name, type, modifiers, offset);
    }
    
    private EnumMemberSymbol ResolveEnumMember(EnumMemberDeclarationSyntax member)
    {
        var name = member.Identifier.Text;
        return new EnumMemberSymbol(_type, name, TypeSymbol.Int, 0); // 0 is a placeholder until the enum member is bound
    }

    private ImmutableArray<SyntaxKind> ResolveModifiers(ImmutableSyntaxList<SyntaxToken> modifierTokens)
    {
        var modifiers = ImmutableArray.CreateBuilder<SyntaxKind>();
        var duplicateModifiers = new List<SyntaxToken>();
        SyntaxToken? privateToken = null;
        SyntaxToken? publicToken = null;
        SyntaxToken? entryToken = null;
        foreach (var modifierToken in modifierTokens)
        {
            var modifier = modifierToken.Kind;
            if (modifier == SyntaxKind.PrivateKeyword)
            {
                privateToken = modifierToken;
            }
            else if (modifier == SyntaxKind.PublicKeyword)
            {
                publicToken = modifierToken;
            }
            else if (modifier == SyntaxKind.EntryKeyword)
            {
                entryToken = modifierToken;
            }

            if (modifiers.Contains(modifier))
            {
                duplicateModifiers.Add(modifierToken);
            }
            else
            {
                modifiers.Add(modifier);
            }
        }
        
        if (privateToken is null && publicToken is null)
        {
            modifiers.Add(SyntaxKind.PrivateKeyword);
        }
        
        if (publicToken is not null && privateToken is not null)
        {
            Diagnostics.ReportCannotHaveBothPublicAndPrivateModifier(publicToken.Location);
            Diagnostics.ReportCannotHaveBothPublicAndPrivateModifier(privateToken.Location);
        }
        
        if (CurrentMember != SymbolKind.Method && CurrentMember != SymbolKind.Struct && entryToken is not null)
        {
            Diagnostics.ReportEntryModifierOnlyAllowedOnMethodsAndStructs(entryToken.Location);
        }

        if (entryToken is not null && CurrentMember == SymbolKind.Method &&
            !_type.Modifiers.Contains(SyntaxKind.EntryKeyword))
        {
            Diagnostics.ReportEntryModifierMustBeAppliedToParentStruct(entryToken.Location);
        }
        
        foreach (var duplicateModifier in duplicateModifiers)
        {
            Diagnostics.ReportDuplicateModifier(duplicateModifier.Location, duplicateModifier.Kind);
        }
        
        return modifiers.ToImmutable();
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
        var modifiers = ResolveModifiers(method.Modifiers);
        var type = BindTypeSyntax(method.ReturnType);
        var name = method.Identifier.Text;
        var parameters = ResolveParameters(method.ParameterList.Parameters);
        return new MethodSymbol(_type, name, type, parameters, modifiers);
    }

    private TemplateMethodSymbol ResolveTemplateMethod(TemplateMethodDeclarationSyntax method)
    {
        Scope = Scope?.CreateChild();
        for (var i = 0; i < method.TypeParameterList.Parameters.Count; i++)
        {
            var typeParameter = method.TypeParameterList.Parameters[i];
            var typeParamName = typeParameter.Identifier.Text;
            var typeParamSymbol = new TypeParameterSymbol(typeParamName, i);
            var context = new SemanticContext(typeParameter.Location, this, typeParameter, Diagnostics);
            Register(context, typeParamSymbol);
        }

        var modifiers = ResolveModifiers(method.Modifiers);
        var type = BindTypeSyntax(method.ReturnType);
        var name = method.Identifier.Text;
        var typeParameters = ResolveTypeParameters(method.TypeParameterList);
        var parameters = ResolveParameters(method.ParameterList.Parameters);
        Scope = Scope?.Parent;
        return new TemplateMethodSymbol(_type, name, type, parameters, modifiers, typeParameters.Keys.ToImmutableArray());
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
        var type = BindTypeSyntax(parameter.Type);
        return new ParameterSymbol(name, type, ordinal);
    }
    
    private BoundMember BindMember(SemanticContext context, SyntaxNode member)
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

        if (member is TemplateMethodDeclarationSyntax templateMethod)
        {
            return BindTemplateMethod(templateMethod);
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

    private BoundTemplateMethod BindTemplateMethod(TemplateMethodDeclarationSyntax syntax)
    {
        var methodBinder = new MethodBinder(this);
        var methodSymbol = _members[syntax];
        var method = methodBinder.Bind(syntax, methodSymbol);
        Diagnostics.AddRange(methodBinder.Diagnostics);
        return (BoundTemplateMethod)method;
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

            var value = Convert.ToInt32(expression.ConstantValue?.Value ?? _previousValue++);
            enumMemberSymbol.ReplaceValue(value);
        }
        
        return new BoundEnumMember(syntax, enumMemberSymbol);
    }

    public override MethodSymbol BuildTemplateMethod(TemplateMethodSymbol templateMethod, ImmutableArray<TypeSymbol> typeArguments,
        SyntaxNode callSite)
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
        var syntax = _templateMethods[templateMethod];
        var boundTemplateMethod = templateMethodBinder.Bind(syntax, templateMethod, typeArguments, 
            callSite, name);
        Diagnostics.AddRange(templateMethodBinder.Diagnostics);
        if (boundTemplateMethod is not BoundMethod method)
        {
            Diagnostics.ReportCouldNotBindTemplateMethod(syntax.Location, name);
            return MethodSymbol.Error;
        }
        
        _boundMembers.Add(method);
        _type.AddMember(method.Symbol);
        return method.Symbol;
    }

    public override string ToString()
    {
        return _type.ToString();
    }
}