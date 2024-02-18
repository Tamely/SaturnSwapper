using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace UAssetAPI.Unversioned
{
    public class Oodle
    {
        public const string OODLE_DLL_NAME = @"oo2core_9_win64.dll";

        [DllImport(OODLE_DLL_NAME)]
        private static extern int OodleLZ_Decompress(byte[] buffer, long bufferSize, byte[] outputBuffer, long outputBufferSize, uint a, uint b, ulong c, uint d, uint e, uint f, uint g, uint h, uint i, uint threadModule);

        public static byte[] Decompress(byte[] buffer, int size, int uncompressedSize)
        {
            byte[] decompressedBuffer = new byte[uncompressedSize];
            int decompressedCount = OodleLZ_Decompress(buffer, size, decompressedBuffer, uncompressedSize, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3);

            if (decompressedCount == uncompressedSize)
            {
                return decompressedBuffer;
            }
            else if (decompressedCount < uncompressedSize)
            {
                return decompressedBuffer.Take(decompressedCount).ToArray();
            }

            return new byte[0];
        }
    }
}
