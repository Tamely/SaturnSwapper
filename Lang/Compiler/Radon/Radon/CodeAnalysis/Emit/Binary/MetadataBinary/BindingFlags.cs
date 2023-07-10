using System;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[Flags]
internal enum BindingFlags : byte
{
    None = 0, // No flags.
    Static = 1, // Static member.
    Instance = 2, // Instance member.
    RuntimeInternal = 4, // Internal member.
    Public = 8, // Public member.
    Private = 16, // Private member.
}