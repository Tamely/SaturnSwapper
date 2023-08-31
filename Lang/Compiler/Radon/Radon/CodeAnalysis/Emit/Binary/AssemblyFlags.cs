using System;

namespace Radon.CodeAnalysis.Emit.Binary;

[Flags]
public enum AssemblyFlags : byte
{
    None = 0,
    Encryption = 1
}