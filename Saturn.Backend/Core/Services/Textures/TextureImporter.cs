using CUE4Parse;
using CUE4Parse.FileProvider;
using CUE4Parse.Utils;
using Saturn.Backend.Core.Services;
using Saturn.Backend.Core.Services.Textures;
using System;
using Saturn.Backend.Core.Utils.Compression;
using System.Threading.Tasks;
using CUE4Parse.UE4.Assets.Exports.Texture;
using System.IO;
using Saturn.Backend.Core.Utils;

namespace Saturn.Backend.Core.Services.Textures
{
    internal sealed class TextureImporter : AbstractImporter
    {
        public TextureImporter(DefaultFileProvider provider) : base(provider)
        {
        }

        public async Task<(bool Success, string Error)> SwapTexture(string asset, string toAsset, byte[]? overrideSwapData = null)
        {
            try
            {
                var toUbulkBytes = overrideSwapData ?? Provider.SaveAsset(toAsset);
                byte[]? originalUbulkBytes = null;

                if (overrideSwapData == null)
                {
                    // Save ubulk data
                    SaturnData.isExporting = true;
                    originalUbulkBytes = Provider.SaveAsset(asset);
                    SaturnData.isExporting = false;

                    int dataSize = 16384; // 128x128 data size

                    var startPos = originalUbulkBytes.Length - dataSize;

                    var startPos2 = toUbulkBytes.Length - dataSize;

                    Buffer.BlockCopy(toUbulkBytes, startPos2, originalUbulkBytes, startPos, dataSize);

                    await File.WriteAllBytesAsync(System.IO.Path.Combine(Config.CompressedDataPath, asset.SubstringAfterLast("/")), originalUbulkBytes);
                }

                // Initialize ucas stream
                var path = Path;
                Open(path.Replace(".utoc", ".ucas"));
                Open(path.SubstringBeforeLast('_').Replace(".utoc", string.Empty) + ".utoc");

                // Chunk and write data
                var chunked = ChunkData(overrideSwapData ?? originalUbulkBytes);
                var written = 0L;

                for (int i = 0; i < chunked.Count; i++)
                {
                    byte[] chunk = chunked[i];
                    var compressedChunk = new Saturn.Backend.Core.Utils.Compression.Oodle().Compress(chunk);
                    long address = SaturnData.Offsets[0] + written; // Store the address we are writing to
                    var ptr = BitConverter.GetBytes((uint)address);
                    var shortComp = BitConverter.GetBytes((ushort)compressedChunk.Length); // Get the length of the compressed chunk
                    var shortDecomp = BitConverter.GetBytes((ushort)chunk.Length); // Get the decompressed length
                    var forceCompressedBytes = BitConverter.GetBytes((byte)1);

                    Stream.BaseStream.Position = address;
                    Stream.Write(compressedChunk, 0, compressedChunk.Length); // Write the compressed chunk

                    written += compressedChunk.Length + 10; // Skip ten so the data doesn't conflict when decompressing

                    TocStream.BaseStream.Position = SaturnData.Reader.TocResource.CompressionBlocks[SaturnData.FirstBlockIndex + i].Position;
                    TocStream.BaseStream.Write(ptr, 0, ptr.Length); // Write address of the custom chunk

                    TocStream.BaseStream.Position += 1;
                    TocStream.BaseStream.Write(shortComp, 0, 2); // Write compressed bytes' length

                    TocStream.BaseStream.Position += 1;
                    TocStream.BaseStream.Write(shortDecomp, 0, shortDecomp.Length); // Write decompressed bytes' length

                    TocStream.BaseStream.Position += 1;
                    TocStream.BaseStream.Write(forceCompressedBytes, 0, forceCompressedBytes.Length); // Force chunk to be compressed (Frick off AllyJax)
                }

                SaturnData.Offsets.Clear();

                // Close and finally write the data
                Close(FileT.Cas);
                Close(FileT.Toc);
            }
            catch (Exception e)
            {
                return new(false, e.ToString());
            }

            return new(true, null);
        }
    }
}
