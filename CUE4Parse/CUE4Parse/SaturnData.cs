namespace CUE4Parse;

public class SaturnData
{
    public static bool isCompressed { get; set; } = false;
    public static byte[] CompressedData { get; set; }
    public static long Offset { get; set; }
    public static string Path { get; set; }
    public static int Parition { get; set; } = 0;
    public static string UAssetPath { get; set; }

    // ZLib nonsense
    public static ZLibBlock Block { get; set; }
    public static string SearchString { get; set; }
}

public struct ZLibBlock
{
    public long Offset;
    public int CompressedLength;
    public int Length;
    public byte[] Data;

    public ZLibBlock(long offset, int compressedSize, int size, byte[] data)
    {
        Offset = offset;
        CompressedLength = compressedSize;
        Length = size;
        Data = data;
    }
}
