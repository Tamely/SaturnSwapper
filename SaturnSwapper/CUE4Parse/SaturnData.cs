using CUE4Parse.UE4.IO;
using CUE4Parse.UE4.Pak.Objects;

namespace CUE4Parse;

public struct AssetRegistrySwap
{
    public byte[] DecompressedData;
    public FPakCompressedBlock CompressionBlock;
}

public class SaturnData
{
    public static uint CompressedSize = 0;
    public static uint TocIndex { get; set; } = 0;
    public static uint PartitionIndex { get; set; } = 0;
    public static ulong PartitionOffset { get; set; } = 0;
    public static IoStoreReader Reader { get; set; }
    public static string Path { get; set; }
    public static int FirstBlockIndex { get; set; }

    public static byte[]? AssetRegistrySearch { get; set; } = null;
    public static AssetRegistrySwap? AssetRegistrySwap { get; set; } = null;

    public static void Clear()
    {
        TocIndex = 0;
        CompressedSize = 0;
        PartitionIndex = 0;
        PartitionOffset = 0;
        Reader = null;
        Path = null;
        FirstBlockIndex = 0;
        AssetRegistrySearch = null;
        AssetRegistrySwap = null;
    }
    
    public static NonStaticSaturnData ToNonStatic()
    {
        return new()
        {
            TocIndex = TocIndex,
            CompressedSize = CompressedSize,
            PartitionIndex = PartitionIndex,
            PartitionOffset = PartitionOffset,
            Reader = Reader,
            Path = Path,
            FirstBlockIndex = FirstBlockIndex
        };
    }
}

public class NonStaticSaturnData
{
    public uint TocIndex { get; set; } = 0;
    public uint CompressedSize = 0;
    public uint PartitionIndex { get; set; } = 0;
    public ulong PartitionOffset { get; set; } = 0;
    public IoStoreReader Reader { get; set; }
    public string Path { get; set; }
    public int FirstBlockIndex { get; set; }
}