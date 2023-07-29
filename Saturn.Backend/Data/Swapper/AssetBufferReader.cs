using System;
using System.Runtime.CompilerServices;
using System.Text;
using CUE4Parse.UE4.Objects.UObject;
using GenericReader;

namespace Saturn.Backend.Data.Swapper;

public class AssetBufferReader : GenericBufferReader
{
    public AssetBufferReader(byte[] buffer) : base(buffer) {}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte[] ReadBytes(uint length)
    {
        return ReadBytes((int)length);
    }

    public FNameEntrySerialized ReadNameEntry(FSerializedNameHeader header)
    {
        var length = header.Length;
        var s = header.IsUtf16 ? new string(ReadArray<char>((int)length)) : Encoding.UTF8.GetString(ReadBytes(length));
        return new FNameEntrySerialized(s);
    }
}