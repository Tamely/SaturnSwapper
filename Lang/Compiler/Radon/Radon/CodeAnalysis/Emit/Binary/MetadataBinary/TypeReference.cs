using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct TypeReference
{
    public readonly BindingFlags Flags;
    public readonly int TypeDefinition;
    public readonly int ConstructorReference;

    public TypeReference(BindingFlags flags, int typeDefinition, int constructorReference)
    {
        Flags = flags;
        TypeDefinition = typeDefinition;
        ConstructorReference = constructorReference;
    }
}