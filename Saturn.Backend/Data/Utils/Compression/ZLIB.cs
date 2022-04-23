using Ionic.Zlib;

namespace Saturn.Backend.Data.Utils.Compression;

public class ZLIB
{
    public static byte[] Decompress(byte[] input) => ZlibStream.UncompressBuffer(input);
    public static byte[] Compress(byte[] input) => ZlibStream.CompressBuffer(input);
}