using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct Constant
{
    public readonly ConstantType Type;
    public readonly int ValueOffset; // Represents the index of the value in the value pool.
    
    public Constant(ConstantType type, int valueOffset)
    {
        Type = type;
        ValueOffset = valueOffset;
    }
}