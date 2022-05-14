using System;
using static System.IO.Compression.BrotliDecoder;
using static System.IO.Compression.BrotliEncoder;

namespace Saturn.Backend.Data.Utils.Compression;

public class Brotli
{
    public static byte[] Compress(byte[] data)
    {
        int compressedLength = GetMaxCompressedLength(data.Length);
        byte[] compressedData = new byte[compressedLength];

        TryCompress(data, compressedData, out compressedLength);

        byte[] outData = Array.Empty<byte>();
        Buffer.BlockCopy(compressedData, 0, outData, 0, compressedLength);
        return outData;
    }

    public static void Decompress(byte[] compressedData, out byte[] decompressedData)
    {
        decompressedData = new byte[] { };
        TryDecompress(compressedData, decompressedData, out _);
    }
}