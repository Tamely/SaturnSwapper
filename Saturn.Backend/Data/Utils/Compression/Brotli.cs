using System;

namespace Saturn.Backend.Data.Utils.Compression;

public class Brotli
{
    public static byte[] Compress(byte[] data)
    {
        int compressedLength = System.IO.Compression.BrotliEncoder.GetMaxCompressedLength(data.Length);
        byte[] compressedData = new byte[compressedLength];

        System.IO.Compression.BrotliEncoder.TryCompress(data, compressedData, out compressedLength);

        byte[] outData = Array.Empty<byte>();
        Buffer.BlockCopy(compressedData, 0, outData, 0, compressedLength);
        return outData;
    }

    public static void Decompress(byte[] compressedData, out byte[] decompressedData)
    {
        decompressedData = new byte[] { };
        System.IO.Compression.BrotliDecoder.TryDecompress(compressedData, decompressedData, out _);
    }
}