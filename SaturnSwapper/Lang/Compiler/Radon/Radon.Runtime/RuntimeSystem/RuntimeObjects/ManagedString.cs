using System;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

internal sealed class ManagedString : ManagedObject
{
    public unsafe nuint CharArrayReference => (nuint)(*(ulong*)Address);
    public ManagedString(RuntimeType type, nuint address) 
        : base(type, type.Size, address)
    {
    }

    public override unsafe RuntimeObject CopyTo(nuint address)
    {
        var obj = new ManagedString(Type, address);
        *(ulong*)address = *(ulong*)Address;
        return obj;
    }

    public override unsafe string ToString()
    {
        // The char array reference points to a char array allocated on the heap
        var array = (ManagedArray)ManagedRuntime.HeapManager.GetObject(CharArrayReference);
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