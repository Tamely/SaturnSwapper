using System;
using System.Collections.Generic;
using System.IO;
using CUE4Parse;
using Radon.CodeAnalysis.Emit;
using Radon.Runtime.Memory;
using UAssetAPI.IO;

namespace Radon.Runtime.RuntimeSystem.RuntimeObjects;

public sealed class ManagedArchive : RuntimeObject
{
    public static List<SwapData> Swaps = new();
    public class SwapData
    {
        public NonStaticSaturnData SaturnData { get; set; }
        public byte[] Data { get; set; }
    }
    
    public override RuntimeType Type { get; }
    public override int Size { get; } // The size in bytes of the array on the heap. This includes the 4 bytes for the length.
    public override nuint Address { get; } // The address of the array on the heap.
    public ZenAsset Archive { get; set; } // The archive
    public NonStaticSaturnData Data { private get; set; } // The file info

    public ManagedArchive(ZenAsset? archive, NonStaticSaturnData? data, nuint pointer)
    {
        Archive = archive ?? new ZenAsset();
        Data = data ?? new NonStaticSaturnData();
        Address = pointer;
        Type = ManagedRuntime.System.GetType("archive");
        Size = Type.Size;
    }

    public override RuntimeObject ComputeOperation(OpCode operation, RuntimeObject? other, StackFrame stackFrame)
    {
        if (other is not ManagedArchive otherArchive)
        {
            throw new InvalidOperationException("Cannot perform an operation on a archive and a non-archive.");
        }

        switch (operation)
        {
            case OpCode.Ceq:
            {
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, Address == otherArchive.Address);
            }
            case OpCode.Cne:
            {
                return stackFrame.AllocatePrimitive(ManagedRuntime.Boolean, Address != otherArchive.Address);
            }
        }

        throw new InvalidOperationException($"Cannot perform operation {operation} on a archive.");
    }

    public override RuntimeObject CopyTo(nuint address)
    {
        return new ManagedArchive(Archive, Data, address);
    }

    public void Save()
    {
        Swaps.Add(new SwapData()
        {
            SaturnData = Data,
            Data = Archive.WriteData().GetBuffer()
        });
    }

    public override string ToString()
    {
        return Archive.Name.Value.Value;
    }
}