using System;

namespace Radon.Runtime.Memory.Native;

[Flags]
public enum FreeType : uint
{
    // MemDecommit: Decommits the specified region of committed pages. After the operation, the pages are in the reserved state.
    MEM_DECOMMIT = 0x4000,
    
    // MemRelease: Releases the specified region of pages. After this operation, the pages are in the free state.
    MEM_RELEASE = 0x8000
}
