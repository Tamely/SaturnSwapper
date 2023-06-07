using System;

namespace Radon.CodeAnalysis.Emit.Binary;

[Flags]
internal enum AssemblyFlags : byte
{
    None = 0,
    Encryption = 1
}