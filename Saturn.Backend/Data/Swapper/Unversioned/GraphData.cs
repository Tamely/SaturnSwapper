using System.Runtime.InteropServices;
using CUE4Parse.UE4.IO.Objects;

namespace Saturn.Backend.Data.Swapper.Unversioned;

[StructLayout(LayoutKind.Sequential)]
public struct FDependencyBundleHeader
{
    public int FirstEntryIndex;
    public uint[,] EntryCount = new uint[(int)EExportCommandType.ExportCommandType_Count, (int)EExportCommandType.ExportCommandType_Count];

    public FDependencyBundleHeader(AssetBufferReader Ar)
    {
        FirstEntryIndex = Ar.Read<int>();
        for (int i = 0; i < (int)EExportCommandType.ExportCommandType_Count; i++)
        {
            for (int j = 0; j < (int)EExportCommandType.ExportCommandType_Count; j++)
            {
                EntryCount[i, j] = Ar.Read<uint>();
            }
        }
    }
}
    
[StructLayout(LayoutKind.Sequential)]
public struct FDependencyBundleEntry
{
    public int LocalImportOrExportIndex;

    public FDependencyBundleEntry(AssetBufferReader Ar)
    {
        LocalImportOrExportIndex = Ar.Read<int>();
    }
}