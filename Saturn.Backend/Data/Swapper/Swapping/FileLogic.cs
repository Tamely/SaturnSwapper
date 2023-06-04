using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUE4Parse.UE4.AssetRegistry;
using Newtonsoft.Json;
using Saturn.Backend.Data.Compression;
using Saturn.Backend.Data.Fortnite;
using Saturn.Backend.Data.FortniteCentral;
using Saturn.Backend.Data.SaturnAPI.Models;
using Saturn.Backend.Data.Swapper.Core.Models;
using Saturn.Backend.Data.Swapper.Swapping.Models;
using Saturn.Backend.Data.Variables;
using SaturnData = CUE4Parse.SaturnData;

namespace Saturn.Backend.Data.Swapper.Swapping;

public class FileLogic
{
    private static string[] Scripts = new string[]
    {
        "CustomCharacterBodyPartData",
        "FaceCustomCharacterHatData",
        "CustomCharacterHatData",
        "CustomCharacterHeadData"
    };
    
    public static byte[] RemoveClassNames(byte[] data)
    {
        List<byte> asset = new(data);
        foreach (var script in Scripts)
        {
            byte[] search = Encoding.UTF8.GetBytes(script);

            int offset = Utilities.IndexOfSequence(asset.ToArray(), search);
            if (offset > 0)
            {
                asset.RemoveRange(offset, search.Length);
                asset.InsertRange(offset, Enumerable.Repeat((byte)0x00, search.Length));
            }
        }

        return asset.ToArray();
    }
    
    private static List<byte[]> ChunkData(byte[] data)
    {
        List<byte[]> result = new();
        var remainingLength = data.Length;
        for (var i = 0; remainingLength > 0; i++)
        {
            var chunkData = new byte[remainingLength >= 65536 ? 65536 : remainingLength];
            Array.Copy(data, i * 65536, chunkData, 0, remainingLength >= 65536 ? 65536 : remainingLength);
            result.Add(chunkData);
            remainingLength -= remainingLength >= 65536 ? 65536 : remainingLength;
        }

        return result;
    }

    public static async Task ConvertLobby(string searchId, string replaceId)
    {
        ItemModel item = new();
        
        byte[] searchArray = new byte[8];
        byte[] replaceArray = new byte[8];
        await Task.Run(async () =>
        {
            if (Constants.Provider.TryCreateReader("FortniteGame/AssetRegistry.bin", out var archive))
            {
                var registry = new FAssetRegistryState(archive);
                var (searchPathIdx, searchAssetIdx, replacePathIdx, replaceAssetIdx) = registry.Swap(searchId, replaceId);

                searchArray = new byte[8];
                Buffer.BlockCopy(BitConverter.GetBytes(searchPathIdx), 0, searchArray, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(searchAssetIdx), 0, searchArray, 4, 4);
                    
                replaceArray = new byte[8];
                Buffer.BlockCopy(BitConverter.GetBytes(replacePathIdx), 0, replaceArray, 0, 4);
                Buffer.BlockCopy(BitConverter.GetBytes(replaceAssetIdx), 0, replaceArray, 4, 4);

                SaturnData.AssetRegistrySearch = searchArray;

                Constants.Provider.TryCreateReader("FortniteGame/AssetRegistry.bin", out _);
            }
            else
            {
                throw new Exception("Unable to load AssetRegistry.bin");
            }
        });

        int offset = Utilities.IndexOfSequence(SaturnData.AssetRegistrySwap!.Value.DecompressedData, searchArray);
        CompressionBase oodle = new Oodle();
        
        Logger.Log($"Writing data with a length of {replaceArray.Length} to offset {offset} in decompressed data with a length of {SaturnData.AssetRegistrySwap.Value.DecompressedData.Length}");
        var writtenData = Utilities.WriteBytes(replaceArray, SaturnData.AssetRegistrySwap.Value.DecompressedData, offset);

        item.Name = "Lobby - " + Path.GetFileNameWithoutExtension(searchId) + " to " + Path.GetFileNameWithoutExtension(replaceId);
        item.Swaps = new[]
        {
            new Swap
            {
                File = DataCollection.GetGamePath() + "\\pakchunk0-WindowsClient.pak",
                Offset = SaturnData.AssetRegistrySwap.Value.CompressionBlock.CompressedStart,
                Data = oodle.Compress(writtenData)
            }
        };
        
        await File.WriteAllTextAsync(Constants.DataPath + item.Name + ".json", JsonConvert.SerializeObject(item));
    }

    public static async Task Revert(string id)
    {
        if (File.Exists(Constants.DataPath + id + ".json"))
            File.Delete(Constants.DataPath + id + ".json");
        
        new DirectoryInfo(Constants.DataPath).EnumerateFiles($"Lobby - * to {id}.json").ToList().ForEach(f => f.Delete());
    }
    
    public static async Task Convert(List<SwapData> swapData)
    {        
        CompressionBase compression = new Oodle();
        
        Constants.SelectedItem ??= new SaturnItemModel()
        {
            Name = "AssetImporter",
            ID = "AssetImporter"
        };
        
        Constants.SelectedOption ??= new SaturnItemModel()
        {
            Name = "AssetImporter",
            ID = "AssetImporter"
        };

        ItemModel item = new ItemModel();
        item.Name = Constants.SelectedOption.Name + " to " + Constants.SelectedItem.Name;
        item.Swaps = new Swap[swapData.Sum(x => ChunkData(x.Data).Count * 6 + 1)];
        int swapIndex = 0;
            
        foreach (var swap in swapData)
        {
            if (string.IsNullOrWhiteSpace(swap.SaturnData.Path)) continue;
            
            var chunkedData = ChunkData(swap.Data);

            long totalDataCount = chunkedData.Sum(x => compression.Compress(x).Length);
            var (file, offset) = OffsetsInFile.Allocate(swap.SaturnData.Path, totalDataCount);
            Logger.Log("Allocated space at offset: " + offset + " in file: " + file);
            
            byte partitionIndex = 0;
            if (file.Contains("Client_s"))
            {
                string temp = file.Split("Client_s")[1];
                partitionIndex = (byte)int.Parse(Path.GetFileNameWithoutExtension(temp));
            }

            long written = 0;

            for (var index = 0; index < chunkedData.Count; index++)
            {
                var chunk = chunkedData[index];
                var compressedChunk = compression.Compress(chunk);

                item.Swaps[swapIndex++] = new Swap()
                {
                    File = file,
                    Data = compressedChunk
                };
                
                item.Swaps[swapIndex++] = new Swap()
                {
                    File = swap.SaturnData.Path,
                    Offset = swap.SaturnData.Reader.TocResource.CompressionBlocks[swap.SaturnData.FirstBlockIndex + index].Position,
                    Data = BitConverter.GetBytes((uint)offset + (uint)written)
                };
                
                item.Swaps[swapIndex++] = new Swap()
                {
                    File = swap.SaturnData.Path,
                    Offset = swap.SaturnData.Reader.TocResource.CompressionBlocks[swap.SaturnData.FirstBlockIndex + index].Position + 4,
                    Data = new byte[] { partitionIndex }
                };
                
                item.Swaps[swapIndex++] = new Swap()
                {
                    File = swap.SaturnData.Path,
                    Offset = swap.SaturnData.Reader.TocResource.CompressionBlocks[swap.SaturnData.FirstBlockIndex + index].Position + 5,
                    Data = BitConverter.GetBytes((ushort)compressedChunk.Length)
                };
                
                item.Swaps[swapIndex++] = new Swap()
                {
                    File = swap.SaturnData.Path,
                    Offset = swap.SaturnData.Reader.TocResource.CompressionBlocks[swap.SaturnData.FirstBlockIndex + index].Position + 8,
                    Data = BitConverter.GetBytes((ushort)chunk.Length)
                };
                
                item.Swaps[swapIndex++] = new Swap()
                {
                    File = swap.SaturnData.Path,
                    Offset = swap.SaturnData.Reader.TocResource.CompressionBlocks[swap.SaturnData.FirstBlockIndex + index].Position + 11,
                    Data = new byte[] { 0x01 }
                };

                written += chunkedData.Count;
            }
            
            
            item.Swaps[swapIndex++] = new Swap()
            {
                File = swap.SaturnData.Path,
                Offset = swap.SaturnData.Reader.TocResource.ChunkOffsetLengths[swap.SaturnData.TocIndex].Position + 6,
                Data = BitConverter.GetBytes(swap.Data.Length).Reverse().ToArray()
            };
        }
        
        while (File.Exists(Constants.DataPath + Constants.SelectedItem.ID + ".json"))
        {
            Constants.SelectedItem.ID += "1";
        }
        
        await File.WriteAllTextAsync(Constants.DataPath + Constants.SelectedItem.ID + ".json", JsonConvert.SerializeObject(item));
    }
}