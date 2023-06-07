using System;
using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct Flags
{
    public static readonly Flags Empty = new(0, Array.Empty<BindingFlags>());
    
    public readonly int Length;
    public readonly BindingFlags[] BindingFlags;
    
    public Flags(int length, BindingFlags[] bindingFlags)
    {
        Length = length;
        BindingFlags = bindingFlags;
    }
}