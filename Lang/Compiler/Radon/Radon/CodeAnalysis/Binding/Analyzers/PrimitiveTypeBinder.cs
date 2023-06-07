using System;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Semantics;
using Radon.CodeAnalysis.Binding.Semantics.Members;
using Radon.CodeAnalysis.Binding.Semantics.Statements;
using Radon.CodeAnalysis.Binding.Semantics.Types;
using Radon.CodeAnalysis.Symbols;
using Radon.CodeAnalysis.Syntax.Nodes;

namespace Radon.CodeAnalysis.Binding.Analyzers;

internal sealed class PrimitiveTypeBinder : Binder
{
    private readonly TypeSymbol _type;
    internal PrimitiveTypeBinder(Binder binder, TypeSymbol type) 
        : base(binder)
    {
        _type = type;
    }

    public override BoundNode Bind(SyntaxNode node, params object[] args)
    {
        return _type switch
        {
            StructSymbol structSymbol => BindStruct(structSymbol, node),
            EnumSymbol enumSymbol => BindEnum(enumSymbol, node),
            _ => throw new Exception($"Unexpected type: {_type.GetType()}")
        };
    }
    
    private BoundStruct BindStruct(StructSymbol structSymbol, SyntaxNode node)
    {
        var members = ImmutableArray.CreateBuilder<BoundMember>();
        foreach (var member in structSymbol.Members)
        {
            var boundMember = BindMember(member, node);
            members.Add(boundMember);
        }

        return new BoundStruct(node, structSymbol, members.ToImmutable());
    }
    
    private BoundEnum BindEnum(EnumSymbol enumSymbol, SyntaxNode node)
    {
        var members = ImmutableArray.CreateBuilder<BoundEnumMember>();
        foreach (var member in enumSymbol.Members)
        {
            var boundMember = BindEnumMember((EnumMemberSymbol)member, node);
            members.Add(boundMember);
        }

        return new BoundEnum(node, enumSymbol, members.ToImmutable());
    }

    private BoundMember BindMember(MemberSymbol member, SyntaxNode syntax)
    {
        if (member is FieldSymbol field)
        {
            return new BoundField(syntax, field, null);
        }
        
        if (member is MethodSymbol method)
        {
            return new BoundMethod(syntax, method, ImmutableArray<BoundStatement>.Empty, ImmutableArray<LocalVariableSymbol>.Empty);
        }
        
        if (member is ConstructorSymbol constructor)
        {
            return new BoundConstructor(syntax, constructor, ImmutableArray<BoundStatement>.Empty);
        }
        
        throw new Exception($"Unexpected member: {member.GetType()}");
    }
    
    private BoundEnumMember BindEnumMember(EnumMemberSymbol member, SyntaxNode syntax)
    {
        return new BoundEnumMember(syntax, member);
    }
}