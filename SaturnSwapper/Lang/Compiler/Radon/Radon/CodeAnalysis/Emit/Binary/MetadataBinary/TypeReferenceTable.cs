using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct TypeReferenceTable
{
    public readonly TypeReference[] TypeReferences;
    
    public TypeReferenceTable(TypeReference[] typeReferences)
    {
        TypeReferences = typeReferences;
    }
}