using System;
using System.Collections.Immutable;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Symbols;

public sealed class FieldSymbol : MemberSymbol
{
    public override TypeSymbol ParentType { get; }
    public override string Name { get; }
    public override SymbolKind Kind => SymbolKind.Field;
    public override TypeSymbol Type { get; }
    public override ImmutableArray<SyntaxKind> Modifiers { get; }
    public int Offset { get; }

    internal FieldSymbol(TypeSymbol parentType, string name, TypeSymbol type, ImmutableArray<SyntaxKind> modifiers, int offset)
    {
        ParentType = parentType;
        Name = name;
        Type = type;
        Modifiers = modifiers;
        Offset = offset;
    }
    
    public override MemberSymbol WithType(TypeSymbol type)
    {
        return new FieldSymbol(ParentType, Name, type, Modifiers, Offset);
    }

    public override MemberSymbol WithParentType(TypeSymbol parentType)
    {
        return new FieldSymbol(parentType, Name, Type, Modifiers, Offset);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Name);
        hash.Add(ParentType);
        hash.Add(Type);
        return hash.ToHashCode();
    }

    public override string ToString() => Name;
}