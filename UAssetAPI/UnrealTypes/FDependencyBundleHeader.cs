using UAssetAPI.IO;

namespace UAssetAPI.UnrealTypes;

public struct FDependencyBundleHeader
{
    public int FirstEntryIndex;
    public uint[,] EntryCount = new uint[(int)EExportCommandType.ExportCommandType_Count, (int)EExportCommandType.ExportCommandType_Count];

    public FDependencyBundleHeader(UnrealBinaryReader Ar)
    {
        FirstEntryIndex = Ar.ReadInt32();
        for (int i = 0; i < (int)EExportCommandType.ExportCommandType_Count; i++)
        {
            for (int j = 0; j < (int)EExportCommandType.ExportCommandType_Count; j++)
            {
                EntryCount[i, j] = Ar.ReadUInt32();
            }
        }
    }
}