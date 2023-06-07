using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct TypeDefinitionTable
{
    public readonly TypeDefinition[] Types;
    
    public TypeDefinitionTable(TypeDefinition[] types)
    {
        Types = types;
    }
}