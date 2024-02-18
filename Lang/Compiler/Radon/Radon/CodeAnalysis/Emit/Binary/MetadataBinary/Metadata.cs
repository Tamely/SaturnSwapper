using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct Metadata
{
    public readonly StringTable Strings;
    private readonly Padding __padding0;
    public readonly ConstantTable Constants;
    private readonly Padding __padding1;
    public readonly SignTable Signs;
    private readonly Padding __padding2;
    public readonly TypeDefinitionTable Types;
    private readonly Padding __padding3;
    public readonly FieldTable Fields;
    private readonly Padding __padding4;
    public readonly EnumMemberTable EnumMembers;
    private readonly Padding __padding5;
    public readonly MethodTable Methods;
    private readonly Padding __padding6;
    public readonly ParameterTable Parameters;
    private readonly Padding __padding7;
    public readonly LocalTable Locals;
    private readonly Padding __padding8;
    public readonly TypeReferenceTable TypeReferences;
    private readonly Padding __padding9;
    public readonly MemberReferenceTable MemberReferences;
    private readonly Padding __padding10;
    public readonly ConstantPool ConstantsPool;

    public Metadata(StringTable strings, ConstantTable constants, SignTable signs, TypeDefinitionTable types, 
                    FieldTable fields, EnumMemberTable enumMembers, MethodTable methods, ParameterTable parameters, 
                    LocalTable locals, TypeReferenceTable typeReferences, MemberReferenceTable memberReferences, 
                    ConstantPool constantsPool)
    {
        Strings = strings;
        __padding0 = new Padding();
        Constants = constants;
        __padding1 = new Padding();
        Signs = signs;
        __padding2 = new Padding();
        Types = types;
        __padding3 = new Padding();
        Fields = fields;
        __padding4 = new Padding();
        EnumMembers = enumMembers;
        __padding5 = new Padding();
        Methods = methods;
        __padding6 = new Padding();
        Parameters = parameters;
        __padding7 = new Padding();
        Locals = locals;
        __padding8 = new Padding();
        TypeReferences = typeReferences;
        __padding9 = new Padding();
        MemberReferences = memberReferences;
        __padding10 = new Padding();
        ConstantsPool = constantsPool;
    }
}