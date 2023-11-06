using System.Runtime.InteropServices;

namespace Radon.Runtime.Memory.Native;

internal static partial class PInvoke
{
    // VirtualAlloc
    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial nint VirtualAlloc(nint lpAddress, nuint dwSize, AllocationType flAllocationType, MemoryProtection flProtect);
    
    // VirtualFree
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool VirtualFree(nint lpAddress, nuint dwSize, FreeType dwFreeType);
    
    // GetLastError
    [LibraryImport("kernel32.dll", SetLastError = true)]
    public static partial uint GetLastError();
}