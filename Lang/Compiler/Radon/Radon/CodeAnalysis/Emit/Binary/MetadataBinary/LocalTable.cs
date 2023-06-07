using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct LocalTable
{
    public readonly Local[] Locals;
    
    public LocalTable(Local[] locals)
    {
        Locals = locals;
    }
}