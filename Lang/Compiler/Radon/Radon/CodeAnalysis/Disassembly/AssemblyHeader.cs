using System;
using System.Runtime.InteropServices;
using Radon.CodeAnalysis.Emit.Binary;

namespace Radon.CodeAnalysis.Disassembly;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal readonly struct AssemblyHeader
{
    public readonly ulong MagicNumber;
    public readonly Guid Guid;
    public readonly AssemblyFlags Flags;
    public readonly long EncryptionKey;
    public readonly double Version;
}