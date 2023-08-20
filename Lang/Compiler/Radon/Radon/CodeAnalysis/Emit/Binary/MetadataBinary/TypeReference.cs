using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct TypeReference
{
    public readonly int TypeDefinition;
    public readonly int ConstructorReference;

    public TypeReference(int typeDefinition, int constructorReference)
    {
        TypeDefinition = typeDefinition;
        ConstructorReference = constructorReference;
    }
}