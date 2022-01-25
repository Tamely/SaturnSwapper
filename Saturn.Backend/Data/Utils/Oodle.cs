using Saturn.Backend.Data.Enums;
using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace Saturn.Backend.Data.Utils
{
    public class Oodle
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool SetDllDirectory(string lpPathName);

        public static byte[] Decompress(byte[] compressedBuffer, int decompressedSize)
        {
            var array = new byte[decompressedSize];
            var num = OodleStream.OodleLZ_Decompress(compressedBuffer, compressedBuffer.Length, array, decompressedSize, 0U, 0U, 0UL, 0U, 0U, 0U, 0U, 0U, 0U, 3U);
            if (num == decompressedSize)
            {
                return array;
            }

            if (num < decompressedSize) return array.Take(num).ToArray();
            Logger.Log("Decompressed size is bigger than expected!", LogLevel.Error);
            throw new Exception("There was an error while decompressing!");
        }

        public static byte[] Compress(byte[] decompressed)
        {
            // Needed so it works when launched from start menu
            SetDllDirectory(Config.BasePath);

            // Needs to be outside so it always has a value
            uint len;

            try
            {
                // Get compressed size
                len = (uint)OodleStream.OodleLZ_Compress(
                    CompressionFormat.Kraken, decompressed, decompressed.Length,
                    new byte[(int)(uint)decompressed.Length + 274U * (((uint)decompressed.Length + 262143U) / 262144U)],
                    CompressionLevel.Optimal5, 0U, 0U, 0U, 0);
            }
            catch (AccessViolationException)
            {
                // Just in case there is protected memory
                len = 64U;
            }

            return OodleStream.Compress(decompressed, decompressed.Length, CompressionFormat.Kraken,
                CompressionLevel.Optimal5, len);
        }
    }

    public class OodleStream
    {
        [DllImport("oo2core_5_win64.dll")]
        public static extern int OodleLZ_Compress(
            CompressionFormat format,
            byte[]? decompressedBuffer, long decompressedSize,
            byte[] compressedBuffer,
            CompressionLevel compressionLevel,
            uint a, uint b, uint c, uint threadModule);

        [DllImport("oo2core_5_win64.dll")]
        public static extern int OodleLZ_Decompress(byte[] src, long srcSize, byte[] dst, long dstSize, uint fuzz,
            uint crc, ulong verbosity, uint context, uint unused, uint callback, uint callbackCtx, uint scratch,
            uint scratchSize, uint threadModule);

        public static byte[] Compress(byte[]? buffer, int size, CompressionFormat format, CompressionLevel level,
            uint a)
        {
            // Initializes array with compressed array size
            var array = new byte[(uint)size + 274U * (((uint)size + 262143U) / 262144U)];

            var len = OodleLZ_Compress(format, buffer, size, array, level, 0U, 0U, 0U, 0U);

            // Initializes the array we will be returning
            var compressed = new byte[a + (uint)len - (int)a];

            // Combines the two arrays
            Buffer.BlockCopy(array, 0, compressed, 0, len);

            return compressed;
        }
    }
}