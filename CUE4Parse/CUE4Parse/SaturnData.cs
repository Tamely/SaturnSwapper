namespace CUE4Parse;

public class SaturnData
{
    public static bool isCompressed { get; set; } = false;
    public static byte[] CompressedData { get; set; }
    public static long Offset { get; set; }
    public static string Path { get; set; }
    public static string UAssetPath { get; set; }
}