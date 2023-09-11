using Radon.Common;

namespace Radon.Runtime.Memory;

internal static class MemoryUtils
{
    public static nuint HeapSize { get; set; }
    public static nuint StackSize { get; set; }
    
    public static unsafe void Copy(nuint source, nuint destination, int size)
    {
        var src = (byte*)source;
        var dest = (byte*)destination;
        for (var i = 0; i < size; i++)
        {
            *dest++ = *src++;
        }
    }

    public static unsafe T GetValue<T>(nuint pointer)
        where T : unmanaged
    {
        return *(T*)pointer;
    }
}