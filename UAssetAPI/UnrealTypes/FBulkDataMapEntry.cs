using System.IO;

namespace UAssetAPI.UnrealTypes;

public struct FBulkDataMapEntry
{
    public const uint Size = 32;

    public ulong SerialOffset;
    public ulong DuplicateSerialOffset;
    public ulong SerialSize;
    public uint Flags;
    public uint Pad;

    public FBulkDataMapEntry(BinaryReader Ar)
    {
        SerialOffset = Ar.ReadUInt64();
        DuplicateSerialOffset = Ar.ReadUInt64();
        SerialSize = Ar.ReadUInt64();
        Flags = Ar.ReadUInt32();
        Flags = Ar.ReadUInt32();
    }
}