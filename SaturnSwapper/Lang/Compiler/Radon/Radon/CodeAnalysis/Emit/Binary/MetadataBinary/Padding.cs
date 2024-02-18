using System.Runtime.InteropServices;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct Padding
{
#pragma warning disable CS0414
    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly long _padding;
#pragma warning restore CS0414

    public Padding()
    {
        _padding = 0L;
    }
}