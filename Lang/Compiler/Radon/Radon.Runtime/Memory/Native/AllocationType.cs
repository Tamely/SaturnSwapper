using System;

namespace Radon.Runtime.Memory.Native;

[Flags]
internal enum AllocationType : uint
{
    // Commit: Reserves and commits a region of pages in the virtual address space of the calling process.
    COMMIT = 0x1000,
    
    // Reserve: Reserves a range of the process's virtual address space without allocating any actual physical storage in memory or in the paging file on disk.
    RESERVE = 0x2000,
    
    // Reset: Indicates that data in the memory range specified by lpAddress and dwSize is no longer of interest. The pages should not be read from or written to the paging file.
    RESET = 0x80000,
    
    // LargePages: Allocates memory using large page support.
    LARGE_PAGES = 0x20000000,
    
    // Physical: Reserves an address range that can be used to map Address Windowing Extensions (AWE) pages.
    PHYSICAL = 0x400000,
    
    // TopDown: Allocates memory at the highest possible address.
    TOP_DOWN = 0x100000,
    
    // WriteWatch: Causes the system to track pages that are written to in the allocated region. If you specify this value, you must also specify MEM_RESERVE.
    WRITE_WATCH = 0x200000
}