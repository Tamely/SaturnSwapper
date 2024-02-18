using System;
using System.Runtime.InteropServices;
using Saturn.Backend.Data.Variables;

namespace Saturn.Backend.Data.Compression
{
    public enum OodleFormat : uint
    {
        LZHLW = 1,
        LZNIB = 2,
        None = 3,
        LZB16 = 4,
        LZBLW = 5,
        LZA = 6,
        LZNA = 7,
        Kraken = 8,
        Mermaid = 9,
        BitKnit = 10,
        Selkie = 11,
        Hydra = 12,
        Leviathan = 13
    }

    public enum OodleCompressionLevel : uint
    {
        None = 0,
        SuperFast = 1,
        VeryFast = 2,
        Fast = 3,
        Normal = 4,
        Optimal1 = 5,
        Optimal2 = 6,
        Optimal3 = 7,
        Optimal4 = 8,
        Optimal5 = 9
    }

    public class Oodle : CompressionBase
    {
        /// <summary>
        /// Compresses a byte[] using Oodle.
        /// </summary>
        /// <param name="data">byte[]: The decompressed data you want to compress</param>
        /// <returns>byte[]: The compressed data</returns>
        public override byte[] Compress(byte[] data)
        {
            var maxSize = GetCompressedBounds((uint)data.Length);
            var compressedData = new byte[maxSize];
            
            var compressedSize = Compress(data, (uint)data.Length, ref compressedData, maxSize, OodleFormat.Kraken,
                OodleCompressionLevel.Optimal5);
            
            byte[] result = new byte[compressedSize];
            Buffer.BlockCopy(compressedData, 0, result, 0, (int)compressedSize);

            return result;
        }

        /// <summary>
        /// Decompresses a byte[] using Oodle.
        /// </summary>
        /// <param name="data">byte[]: The compressed data</param>
        /// <param name="decompressedSize">int: The expected size of the decompressed data</param>
        /// <returns>byte[]: The decompressed data</returns>
        /// <exception cref="Exception">Gets thrown when "decompressedSize" doesn't match with what Oodle returns</exception>
        public override byte[] Decompress(byte[] data, int decompressedSize)
        {
            byte[] decompressedData = new byte[decompressedSize];
            var verificationSize = Decompress(data, (uint)data.Length,
                ref decompressedData, (uint)decompressedSize);

            if (verificationSize != decompressedSize)
                throw new Exception("Decompression failed. Verification size does not match given size.");

            return decompressedData;
        }

        private static uint Compress(byte[] buffer, uint bufferSize, ref byte[] OutputBuffer, uint OutputBufferSize,
            OodleFormat format, OodleCompressionLevel level)
        {
            if (buffer.Length > 0 && bufferSize > 0 && OutputBuffer.Length > 0 && OutputBufferSize > 0)
            {
                IntPtr pDll = NativeMethods.LoadLibrary(Constants.OodlePath);
                if (pDll == IntPtr.Zero)
                    throw new Exception("Natively loading the Oodle library failed!");

                IntPtr pAddressOfFunctionToCall = NativeMethods.GetProcAddress(pDll, "OodleLZ_Compress");
                if (pAddressOfFunctionToCall == IntPtr.Zero)
                    throw new Exception("Natively loading the OodleLZ_Compress function failed!");

                OodleLZ_Compress compress = (OodleLZ_Compress)Marshal.GetDelegateForFunctionPointer(
                                            pAddressOfFunctionToCall,
                                            typeof(OodleLZ_Compress));

                return (uint)compress(format, buffer, bufferSize, OutputBuffer, level, 0, 0, 0);
            }

            return 0;
        }

        private static uint Decompress(byte[] buffer, uint bufferSize, ref byte[] outputBuffer, uint outputBufferSize)
        {
            if (buffer.Length > 0 && bufferSize > 0 && outputBuffer.Length > 0 && outputBufferSize > 0)
            {
                IntPtr pDll = NativeMethods.LoadLibrary(Constants.OodlePath);
                if (pDll == IntPtr.Zero)
                    throw new Exception("Natively loading the Oodle library failed!");

                IntPtr pAddressOfFunctionToCall = NativeMethods.GetProcAddress(pDll, "OodleLZ_Decompress");
                if (pAddressOfFunctionToCall == IntPtr.Zero)
                    throw new Exception("Natively loading the OodleLZ_Decompress function failed!");

                OodleLZ_Decompress decompress = (OodleLZ_Decompress)Marshal.GetDelegateForFunctionPointer(
                                                pAddressOfFunctionToCall,
                                                typeof(OodleLZ_Decompress));

                return (uint)decompress(buffer, bufferSize, outputBuffer, outputBufferSize, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            }

            return 0;
        }

        /// <summary>
        /// Does a math operation to determine the max size the compressed data can be.
        /// </summary>
        /// <param name="BufferSize">uint: The length of the decompressed buffer.</param>
        /// <returns>uint: The max size the compressed buffer can be.</returns>
        internal static uint GetCompressedBounds(uint BufferSize)
            => BufferSize + 274 * ((BufferSize + 0x3FFFF) / 0x400000);

        /// <summary>
        /// This should never be called!!! If you are going to compress something, use the Compress method in the Oodle class and don't call it from the library directly!
        /// </summary>
        /// <param name="Format">OodleFormat: The compression format used.</param>
        /// <param name="Buffer">byte[]: The decompressed data.</param>
        /// <param name="BufferSize">long: The size of the decompressed data.</param>
        /// <param name="OutputBuffer">ref byte[]: Where the compressed data will output to.</param>
        /// <param name="Level">OodleCompressionLevel: The compression level used.</param>
        /// <param name="a">uint: unused</param>
        /// <param name="b">uint: unused</param>
        /// <param name="c">uint: unused</param>
        /// <returns>int: The length of the compressed data.</returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int OodleLZ_Compress(OodleFormat Format, byte[] Buffer, long BufferSize, byte[] OutputBuffer, OodleCompressionLevel Level, uint a, uint b, uint c);

        /// <summary>
        /// This should never be called!!! If you are going to decompress something, use the Decompress method in the Oodle class and don't call it from the library directly!
        /// </summary>
        /// <param name="Buffer">byte[]: The compressed data.</param>
        /// <param name="BufferSize">long: The size of the compressed data.</param>
        /// <param name="OutputBuffer">ref byte[]: Where the decompressed data will output to.</param>
        /// <param name="OutputBufferSize">long: The size of the decompressed data.</param>
        /// <param name="a">uint: unused</param>
        /// <param name="b">uint: unused</param>
        /// <param name="c">uint: unused</param>
        /// <param name="d">uint: unused</param>
        /// <param name="e">uint: unused</param>
        /// <param name="f">uint: unused</param>
        /// <param name="g">uint: unused</param>
        /// <param name="h">uint: unused</param>
        /// <param name="i">uint: unused</param>
        /// <param name="ThreadModule">int: not really used, pass nullptr/void* in cpp or just 0 in C#</param>
        /// <returns>int: The length of the decompressed data.</returns>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int OodleLZ_Decompress(byte[] Buffer, long BufferSize, byte[] OutputBuffer, long OutputBufferSize, uint a, uint b, uint c, uint d, uint e, uint f, uint g, uint h, uint i, int ThreadModule);
    }
}
