namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class ManagedString : ManagedObject
{
    public nuint CharArrayReference { get; }
    public ManagedString(RuntimeType type, nuint address) 
        : base(type, type.Size, address)
    {
        CharArrayReference = address;
    }
    
    public override unsafe string ToString()
    {
        // The char array reference points to a char array allocated on the heap
        var arrayRef = (ulong*)CharArrayReference;
        var array = (ManagedArray)ManagedRuntime.HeapManager.GetObject((nuint)arrayRef);
        // The length is an int
        var length = array.Length;
        var chars = new char[length];
        var arrayStart = array.ArrayStart;
        for (var i = 0; i < length; i++)
        {
            var c = *(byte*)(arrayStart + (nuint)i);
            chars[i] = (char)c;
        }
        
        return new string(chars);
    }
}