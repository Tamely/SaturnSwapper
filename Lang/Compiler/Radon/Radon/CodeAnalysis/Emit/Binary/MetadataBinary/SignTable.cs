using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct SignTable
{
    public readonly Sign[] Signs;
    
    public SignTable(Sign[] signs)
    {
        Signs = signs;
    }
}