using System;

namespace CUE4Parse
{
    public static class SaturnData
    {
        public static string Path { get; set; } = "";
        public static long Offset { get; set; }
        public static byte[] CompressedData { get; set; } = Array.Empty<byte>();
        public static string UAssetPath { get; set; } = "";
        public static bool isCompressed { get; set; } = true;
    }
}