using System.IO;

namespace UAssetAPI.UnrealTypes;

public struct FMappedName
{
    private const int IndexBits = 30;
    private const uint IndexMask = (1u << IndexBits) - 1u;
    private const uint TypeMask = ~IndexMask;
    private const int TypeShift = IndexBits;
        
    private uint _nameIndex;
    public uint ExtraIndex;
        
    public uint NameIndex => _nameIndex & IndexMask;
    public EType Type => (EType) ((_nameIndex & TypeMask) >> TypeShift);
    public bool IsGlobal => ((_nameIndex & TypeMask) >> TypeShift) != 0;

    public enum EType
    {
        Package,
        Container,
        Global
    }

    public FMappedName(BinaryReader reader)
    {
        _nameIndex = reader.ReadUInt32();
        ExtraIndex = reader.ReadUInt32();
    }
}