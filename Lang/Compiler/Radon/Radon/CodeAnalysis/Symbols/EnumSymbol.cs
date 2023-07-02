using System.Collections.Immutable;
using Radon.CodeAnalysis.Binding.Analyzers;
using Radon.CodeAnalysis.Syntax;

namespace Radon.CodeAnalysis.Symbols;

public sealed class EnumSymbol : TypeSymbol
{
    public override string Name { get; }
    public override int Size { get; internal set; }
    internal override TypeBinder? TypeBinder { get; set; }
    public override SymbolKind Kind => SymbolKind.Enum;
    public override ImmutableArray<MemberSymbol> Members { get; private protected set; }
    public override AssemblySymbol? ParentAssembly { get; }
    public override ImmutableArray<SyntaxKind> Modifiers { get; }
    public TypeSymbol UnderlyingType => Int;
    
    internal EnumSymbol(string name, ImmutableArray<MemberSymbol> members, AssemblySymbol? parentAssembly, 
        ImmutableArray<SyntaxKind> modifiers, TypeBinder? typeBinder = null)
    {
        Name = name;
        Size = UnderlyingType.Size;
        Members = members;
        ParentAssembly = parentAssembly;
        Modifiers = modifiers;
        TypeBinder = typeBinder;
    }


    public void AddEnumMember(string name, int value)
    {
        var member = new EnumMemberSymbol(this, name, UnderlyingType, value);
        AddMember(member);
    }
    
    public override string ToString() => Name;
}