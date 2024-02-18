using System;

namespace Radon.CodeAnalysis.Emit.Binary.MetadataBinary;

[Flags]
public enum BindingFlags
{
    None = 0x00,
    
    // Binding flags for all members
    Instance = 0x01,
    Static = 0x02,
    Public = 0x04,
    NonPublic = 0x08,
    RuntimeInternal = 0x10,
    Entry = 0x20,
    Ref = 0x40,
}