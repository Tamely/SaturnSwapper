using System.IO;

namespace UAssetAPI.UnrealTypes;

public struct FExportMapEntry
{
    public const int Size = 72;

    public ulong CookedSerialOffset;
    public ulong CookedSerialSize;
    public FMappedName ObjectName;
    public FPackageObjectIndex OuterIndex;
    public FPackageObjectIndex ClassIndex;
    public FPackageObjectIndex SuperIndex;
    public FPackageObjectIndex TemplateIndex;
    public ulong PublicExportHash;
    public EObjectFlags ObjectFlags;
    public byte FilterFlags; // EExportFilterFlags: client/server flags

    public FExportMapEntry(UnrealBinaryReader Ar)
    {
        var start = Ar.BaseStream.Position;
        
        CookedSerialOffset = Ar.ReadUInt64();
        CookedSerialSize = Ar.ReadUInt64();
        ObjectName = new FMappedName(Ar);
        OuterIndex = FPackageObjectIndex.Read(Ar);
        ClassIndex = FPackageObjectIndex.Read(Ar);
        SuperIndex = FPackageObjectIndex.Read(Ar);
        TemplateIndex = FPackageObjectIndex.Read(Ar);
        PublicExportHash = Ar.ReadUInt64();

        ObjectFlags = (EObjectFlags)Ar.ReadUInt32();
        FilterFlags = Ar.ReadByte();
        
        Ar.BaseStream.Position = start + Size;
    }
}