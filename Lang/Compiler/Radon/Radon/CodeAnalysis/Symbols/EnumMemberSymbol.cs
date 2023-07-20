using System;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Symbols;

public sealed class EnumMemberSymbol : MemberSymbol
{
    public override TypeSymbol ParentType { get; }
    public override string Name { get; }
    public override SymbolKind Kind => SymbolKind.EnumMember;
    public override TypeSymbol Type => ParentType;
    public override ImmutableArray<SyntaxKind> Modifiers { get; }
    public int Value { get; private set; }
    public TypeSymbol UnderlyingType { get; }
    
    internal EnumMemberSymbol(TypeSymbol parentType, string name, TypeSymbol type, int value)
    {
        ParentType = parentType;
        Name = name;
        UnderlyingType = type;
        Modifiers = ImmutableArray.Create(SyntaxKind.PublicKeyword);
        Value = value;
        if (!Modifiers.Contains(SyntaxKind.StaticKeyword))
        {
            Modifiers = Modifiers.Add(SyntaxKind.StaticKeyword);
        }
    }
    
    public override MemberSymbol WithType(TypeSymbol type)
    {
        return new EnumMemberSymbol(ParentType, Name, type, Value);
    }

    public override MemberSymbol WithParentType(TypeSymbol parentType)
    {
        return new EnumMemberSymbol(parentType, Name, Type, Value);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(ParentType);
        hash.Add(Type);
        return hash.ToHashCode();
    }

    internal void ReplaceValue(int value)
    {
        Value = value;
    }
    
    public override string ToString() => $"{ParentType.Name}.{Name}";
}