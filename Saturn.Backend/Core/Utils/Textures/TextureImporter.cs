using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CUE4Parse;
using CUE4Parse.FileProvider;
using CUE4Parse.Utils;
using Saturn.Backend.Core.Enums;

namespace Saturn.Backend.Core.Utils.Textures
{
    internal sealed class TextureImporter : AbstractImporter
    {
        public TextureImporter(DefaultFileProvider provider) : base(provider)
        {
        }

        public async Task<(bool Success, string Error, long Offset)> SwapTexture(string asset, string toAsset, byte[]? overrideSwapData = null)
        {
            long address = 0;
            try
            {
                if (!Provider.TrySavePackage(toAsset.Split('.')[0], out var pkg))
                {
                    Logger.Log($"Failed to export asset \"{toAsset}\"!", LogLevel.Warning);
                    return (false, $"Failed to export asset \"{toAsset}\"!", -1);
                }

                var toUbulkBytes = overrideSwapData ?? pkg.FirstOrDefault(x => x.Key.Contains("ubulk")).Value;
                byte[]? originalUbulkBytes = null;

                if (overrideSwapData == null)
                {
                    // Save ubulk data
                    SaturnData.isExporting = true;
                    if (!Provider.TrySavePackage(asset.Split('.')[0], out pkg))
                    {
                        Logger.Log($"Failed to export asset \"{asset}\"!", LogLevel.Warning);
                        return (false, $"Failed to export asset \"{asset}\"!", -1);
                    }

                    originalUbulkBytes = overrideSwapData ?? pkg.FirstOrDefault(x => x.Key.Contains("ubulk")).Value;
                    SaturnData.isExporting = false;

                    int dataSize = 16384; // 128x128 data size

                    var startPos = originalUbulkBytes.Length - dataSize;

                    var startPos2 = toUbulkBytes.Length - dataSize;
                    
                    Directory.CreateDirectory(Config.CompressedDataPath);
                    await File.WriteAllBytesAsync(System.IO.Path.Combine(Config.CompressedDataPath, asset.SubstringAfterLast("/")), originalUbulkBytes);

                    Buffer.BlockCopy(toUbulkBytes, startPos2, originalUbulkBytes, startPos, dataSize);
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
                    var compressedChunk = new Saturn.Backend.Core.Utils.Compression.Oodle(Config.BasePath).Compress(chunk);
                    address = SaturnData.Offsets[0] + written; // Store the address we are writing to
                    var ptr = BitConverter.GetBytes((uint)address);
                    var shortComp = BitConverter.GetBytes((ushort)compressedChunk.Length); // Get the length of the compressed chunk
                    var shortDecomp = BitConverter.GetBytes((ushort)chunk.Length); // Get the decompressed length
                    var forceCompressedBytes = BitConverter.GetBytes((byte)1);

                    Stream.BaseStream.Position = address;
                    Stream.Write(compressedChunk, 0, compressedChunk.Length); // Write the compressed chunk

                    written += compressedChunk.Length + 10; // Skip ten so the data doesn't conflict when decompressing

                    TocStream.BaseStream.Position = SaturnData.Reader.TocResource.CompressionBlocks[SaturnData.FirstBlockIndex + i].Position;
                    await TocStream.BaseStream.WriteAsync(ptr, 0, ptr.Length); // Write address of the custom chunk

                    TocStream.BaseStream.Position += 1;
                    await TocStream.BaseStream.WriteAsync(shortComp, 0, 2); // Write compressed bytes' length

                    TocStream.BaseStream.Position += 1;
                    await TocStream.BaseStream.WriteAsync(shortDecomp, 0, shortDecomp.Length); // Write decompressed bytes' length

                    TocStream.BaseStream.Position += 1;
                    await TocStream.BaseStream.WriteAsync(forceCompressedBytes, 0, forceCompressedBytes.Length); // Force chunk to be compressed (Frick off AllyJax)
                }

                SaturnData.Offsets.Clear();

                // Close and finally write the data
                Close(FileT.Cas);
                Close(FileT.Toc);
            }
            catch (Exception e)
            {
                return new(false, e.ToString(), -1);
            }

            return new(true, null, address);
        }
    }
}
