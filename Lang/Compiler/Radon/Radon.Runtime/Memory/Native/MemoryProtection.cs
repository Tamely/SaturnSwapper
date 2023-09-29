using System;

namespace Radon.Runtime.Memory.Native;

[Flags]
internal enum MemoryProtection : uint
{
    // Execute: Allows execution of the committed region of pages.
    EXECUTE = 0x10,
    
    // ExecuteRead: Allows execution of the committed region of pages and allows pages to be read.
    EXECUTE_READ = 0x20,
    
    // ExecuteReadWrite: Allows execution of the committed region of pages, allows pages to be read, and allows pages to be written.
    EXECUTE_READWRITE = 0x40,
    
    // ExecuteWriteCopy: Allows execution of the committed region of pages and allows pages to be written.
    EXECUTE_WRITECOPY = 0x80,
    
    // NoAccess: Disables all access to the committed region of pages. An attempt to read from, write to, or execute the committed region results in an access violation.
    NOACCESS = 0x01,
    
    // ReadOnly: Enables read-only access to the committed region of pages. An attempt to write to the committed region results in an access violation.
    READONLY = 0x02,
    
    // ReadWrite: Enables read-only or read/write access to the committed region of pages. If Data Execution Prevention is enabled, attempting to execute code in the committed region results in an access violation.
    READWRITE = 0x04,
    
    // WriteCopy: Enables read-only or copy-on-write access to a mapped view of a file mapping object. An attempt to write to a committed copy-on-write page results in a private copy of the page being made for the process. The private page is marked as PAGE_EXECUTE_READWRITE, and the change is written to the new page.
    WRITECOPY = 0x08,
    
    // GuardModifierflag: Pages in the region become guard pages. Any attempt to access a guard page causes the system to raise a STATUS_GUARD_PAGE_VIOLATION exception and turn off the guard page status. Guard pages thus act as a one-time access alarm. For more information, see Creating Guard Pages.
    GUARD_Modifierflag = 0x100,
    
    // NoCacheModifierflag: Sets all pages to be non-cachable. Applications should not use this attribute except when explicitly required for a device. Using the interlocked functions with memory that is mapped with SEC_NOCACHE can result in an EXCEPTION_ILLEGAL_INSTRUCTION exception.
    NOCACHE_Modifierflag = 0x200,
    
    // WriteCombineModifierflag: Sets all pages to be write-combined.
    WRITECOMBINE_Modifierflag = 0x400
}