namespace UAssetAPI.UnrealTypes;

public struct FDependencyBundleEntry
{
    public int LocalImportOrExportIndex;

    public FDependencyBundleEntry(UnrealBinaryReader Ar)
    {
        LocalImportOrExportIndex = Ar.ReadInt32();
    }
}