namespace Radon.Runtime.Memory;

internal sealed class FreeBlock
{
    public nuint Pointer { get; }
    public int Size { get; }
        
    public FreeBlock(nuint pointer, int size)
    {
        Pointer = pointer;
        Size = size;
    }
}
