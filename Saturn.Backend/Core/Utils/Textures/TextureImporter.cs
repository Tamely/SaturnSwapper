using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CUE4Parse;
using CUE4Parse.FileProvider;
using CUE4Parse.Utils;

namespace Saturn.Backend.Core.Utils.Textures
{
    internal sealed class TextureImporter : AbstractImporter
    {
        public TextureImporter(DefaultFileProvider provider) : base(provider)
        {
        }
        
        // Get bytes from binary reader, offset, length
        private byte[] GetBytes(BinaryReader reader, long offset, int length)
        {
            reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            return reader.ReadBytes(length);
        }

        public async Task<(bool Success, string Error, Dictionary<(long, long, bool), byte[]> Offsets)> SwapUbulk(string asset, string toAsset)
        {
            Directory.CreateDirectory(Config.CompressedDataPath);
            Dictionary<(long, long, bool), byte[]> Positions = new();
            try
            {
                 SaturnData.isExporting = true;
                
                if (!Provider.TrySaveAsset(toAsset.Replace(".uasset", ".ubulk"), out var replaceUbulk))
                    return (false, "Failed to save replace ubulk", new());
                
                if (!Provider.TrySaveAsset(asset.Replace(".uasset", ".ubulk"), out _))
                    return (false, "Failed to save original ubulk", new());

                SaturnData.isExporting = false;
                
                Open(Path.Replace(".utoc", ".ucas"));
                Open(Path.SubstringBeforeLast('_').Replace(".utoc", string.Empty) + ".utoc");

                var chunkedData = ChunkData(replaceUbulk);

                long written = 0;
                byte[] data;

                Compression.Oodle Oodle = new Compression.Oodle(Config.BasePath);

                for (int i = 0; i < chunkedData.Count; i++)
                {
                    byte[] chunk = chunkedData[i];
                    var compressedChunk = Oodle.Compress(chunk);
                    var address = SaturnData.Offsets[0] + written; // Store the address we are writing to
                    var ptr = BitConverter.GetBytes((uint)address);
                    var shortComp = BitConverter.GetBytes((ushort)compressedChunk.Length); // Get the length of the compressed chunk
                    var shortDecomp = BitConverter.GetBytes((ushort)chunk.Length); // Get the decompressed length
                    var forceCompressedBytes = BitConverter.GetBytes((byte)1);

                    Stream.BaseStream.Position = address;
                    data = GetBytes(Reader, Stream.BaseStream.Position, compressedChunk.Length);
                    Positions.Add((Positions.Count + 1, Stream.BaseStream.Position, true), data);
                    Stream.Write(compressedChunk, 0, compressedChunk.Length); // Write the compressed chunk
                    await File.WriteAllBytesAsync(Config.CompressedDataPath + "\\" + Positions.Count + "_" + asset.SubstringAfterLast('/') + "_Chunk.uasset", data);

                    written += compressedChunk.Length + 10; // Skip ten so the data doesn't conflict when decompressing

                    TocStream.BaseStream.Position = SaturnData.Reader.TocResource.CompressionBlocks[SaturnData.FirstBlockIndex + i].Position;
                    data = GetBytes(TocReader, TocStream.BaseStream.Position, ptr.Length);
                    Positions.Add((Positions.Count + 1, TocStream.BaseStream.Position, false), data);
                    await TocStream.BaseStream.WriteAsync(ptr, 0, ptr.Length); // Write address of the custom chunk
                    await File.WriteAllBytesAsync(Config.CompressedDataPath + "\\" + Positions.Count + "_" + asset.SubstringAfterLast('/') + "_Chunk.uasset", data);

                    TocStream.BaseStream.Position += 1;
                    data = GetBytes(TocReader, TocStream.BaseStream.Position, shortComp.Length);
                    Positions.Add((Positions.Count + 1, TocStream.BaseStream.Position, false), data);
                    await TocStream.BaseStream.WriteAsync(shortComp, 0, 2); // Write compressed bytes' length
                    await File.WriteAllBytesAsync(Config.CompressedDataPath + "\\" + Positions.Count + "_" + asset.SubstringAfterLast('/') + "_Chunk.uasset", data);

                    TocStream.BaseStream.Position += 1;
                    data = GetBytes(TocReader, TocStream.BaseStream.Position, shortDecomp.Length);
                    Positions.Add((Positions.Count + 1, TocStream.BaseStream.Position, false), data);
                    await TocStream.BaseStream.WriteAsync(shortDecomp, 0, shortDecomp.Length); // Write decompressed bytes' length
                    await File.WriteAllBytesAsync(Config.CompressedDataPath + "\\" + Positions.Count + "_" + asset.SubstringAfterLast('/') + "_Chunk.uasset", data);

                    TocStream.BaseStream.Position += 1;
                    data = GetBytes(TocReader, TocStream.BaseStream.Position, forceCompressedBytes.Length);
                    Positions.Add((Positions.Count + 1, TocStream.BaseStream.Position, false), data);
                    await TocStream.BaseStream.WriteAsync(forceCompressedBytes, 0, forceCompressedBytes.Length); // Force chunk to be compressed (Frick off AllyJax)
                    await File.WriteAllBytesAsync(Config.CompressedDataPath + "\\" + Positions.Count + "_" + asset.SubstringAfterLast('/') + "_Chunk.uasset", data);
                }

                TocStream.BaseStream.Position = SaturnData.Reader.TocResource.ChunkOffsetLengths[0].Position + 6;
                data = GetBytes(TocReader, TocStream.BaseStream.Position,
                    BitConverter.GetBytes(replaceUbulk.Length).Reverse().ToArray().Length);
                Positions.Add((Positions.Count + 1, TocStream.BaseStream.Position, false), data);
                await TocStream.BaseStream.WriteAsync(BitConverter.GetBytes(replaceUbulk.Length).Reverse().ToArray(), 0, 4);
                await File.WriteAllBytesAsync(Config.CompressedDataPath + "\\" + Positions.Count + "_" + asset.SubstringAfterLast('/') + "_Chunk.uasset", data);
                
                SaturnData.Offsets.Clear();

                // Close and finally write the data
                Close(FileT.Cas);
                Close(FileT.Toc);
            }
            catch (Exception e)
            {
                return new(false, e.ToString(), new());
            }

            return (true, null, Positions);
        }

        public async Task<(bool Success, string Error)> SwapTexture(string asset, string toAsset, byte[]? overrideSwapData = null)
        {
            Directory.CreateDirectory(Config.CompressedDataPath);
            try
            {
                var toUbulkBytes = overrideSwapData ?? await Provider.SaveAssetAsync(toAsset);
                byte[]? originalUbulkBytes = null;

                if (overrideSwapData == null)
                {
                    // Save ubulk data
                    SaturnData.isExporting = true;
                    originalUbulkBytes = await Provider.SaveAssetAsync(asset);
                    SaturnData.isExporting = false;

                    int dataSize = 16384; // 128x128 data size

                    var startPos = originalUbulkBytes.Length - dataSize;

                    var startPos2 = toUbulkBytes.Length - dataSize;
                    
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
                
                Compression.Oodle Oodle = new Compression.Oodle(Config.BasePath);

                for (int i = 0; i < chunked.Count; i++)
                {
                    byte[] chunk = chunked[i];
                    var compressedChunk = Oodle.Compress(chunk);
                    var address = SaturnData.Offsets[0] + written; // Store the address we are writing to
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
                return new(false, e.ToString());
            }

            return new(true, null);
        }
    }
}
