using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct ConstantPool
{
    public readonly byte[] Values;
    
    public ConstantPool(byte[] values)
    {
        Values = values;
    }
}