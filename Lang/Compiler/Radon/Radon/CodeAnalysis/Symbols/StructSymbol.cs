using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Radon.CodeAnalysis.Binding;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Symbols;

public sealed class StructSymbol : TypeSymbol
{
    public override string Name { get; }
    public override int Size { get; }
    public override SymbolKind Kind => SymbolKind.Struct;
    public ImmutableArray<TypeParameterSymbol> TypeParameters { get; }
    public override ImmutableArray<MemberSymbol> Members { get; private protected set; }
    public override AssemblySymbol? ParentAssembly { get; }
    public override ImmutableArray<SyntaxKind> Modifiers { get; }

    internal StructSymbol(string name, ImmutableArray<TypeParameterSymbol> typeParameters, ImmutableArray<MemberSymbol> members, 
        AssemblySymbol? parentAssembly, ImmutableArray<SyntaxKind> modifiers)
    {
        Name = name;
        TypeParameters = typeParameters;
        Members = ImmutableArray<MemberSymbol>.Empty;
        ParentAssembly = parentAssembly;
        Modifiers = modifiers;
        foreach (var member in members)
        {
            AddMember(member);
        }
        
        var fields = Members.OfType<FieldSymbol>().ToArray();
        var size = 0;
        foreach (var field in fields)
        {
            field.Offset = size;
            size += field.Type.Size;
        }
        
        Size = size;
    }
    
    internal StructSymbol(string name, ImmutableArray<MemberSymbol> members, AssemblySymbol? parentAssembly, 
        ImmutableArray<SyntaxKind> modifiers)
        : this(name, ImmutableArray<TypeParameterSymbol>.Empty, members, parentAssembly, modifiers)
    {
    }
    
    internal StructSymbol(string name, int size, ImmutableArray<MemberSymbol> members, AssemblySymbol? parentAssembly, 
        ImmutableArray<SyntaxKind> modifiers)
        : this(name, ImmutableArray<TypeParameterSymbol>.Empty, members, parentAssembly, modifiers)
    {
        Size = size;
    }

    internal StructSymbol WithTypeParameters(TypeMap map)
    {
        var newTypeParameters = ImmutableArray.CreateBuilder<TypeParameterSymbol>(TypeParameters.Length);
        var oldNewPairs = new Dictionary<TypeParameterSymbol, TypeParameterSymbol>();
        // Create new type parameters with the type map applied to them.
        // Make sure to take into account that the return type or parameter types may be type parameters themselves.
        foreach (var oldTypeParameter in TypeParameters)
        {
            var newTypeParameter = new TypeParameterSymbol(oldTypeParameter.Name, oldTypeParameter.Ordinal, map);
            newTypeParameters.Add(newTypeParameter);
            oldNewPairs.Add(oldTypeParameter, newTypeParameter);
        }
        
        // return new StructSymbol(Name, newTypeParameters.ToImmutable(), Members, ParentAssembly, Modifiers);

        var newMembers = ImmutableArray.CreateBuilder<MemberSymbol>(Members.Length);
        foreach (var member in Members)
        {
            var newType = member.Type;
            if (newType is TypeParameterSymbol typeParameter)
            {
                newType = oldNewPairs[typeParameter];
            }

            var newMember = member.WithType(newType);
            switch (newMember)
            {
                case MethodSymbol method:
                    newMember = method.WithTypeParameters(map);
                    break;
                case ConstructorSymbol constructor:
                    newMember = constructor.WithTypeParameters(map);
                    break;
            }
            
            newMembers.Add(newMember);
        }
        
        return new StructSymbol(Name, newTypeParameters.ToImmutable(), newMembers.ToImmutable(), ParentAssembly, Modifiers);
    }
    
    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode());
    }
    
    public override string ToString()
    {
        return Name;
    }
}