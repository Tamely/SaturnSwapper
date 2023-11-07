using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CUE4Parse;
using CUE4Parse.UE4.AssetRegistry;
using Newtonsoft.Json;
using Saturn.Backend.Data.Compression;
using Saturn.Backend.Data.Fortnite;
using Saturn.Backend.Data.FortniteCentral;
using Saturn.Backend.Data.SaturnAPI.Models;
using Saturn.Backend.Data.Swapper.Assets;
using Saturn.Backend.Data.Swapper.Core.Models;
using Saturn.Backend.Data.Swapper.Swapping.Models;
using Saturn.Backend.Data.Variables;
using UAssetAPI;
using UAssetAPI.IO;
using UAssetAPI.UnrealTypes;
using UAssetAPI.Unversioned;
using Oodle = Saturn.Backend.Data.Compression.Oodle;
using SaturnData = CUE4Parse.SaturnData;

namespace Saturn.Backend.Data.Swapper.Swapping;

public class FileLogic
{
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

    public static bool isLocked = false;

    public static async Task ConvertGlobal(string search, string replace)
    {
        if (!Constants.CanSpecialSwap || !Constants.ShouldGlobalSwap || !Constants.isPlus) return;
        
        if (isLocked) return;
        isLocked = true;

        ItemModel item = new();
        
        byte[] searchArray = new byte[8];
        byte[] replaceArray = new byte[8];
        bool returnFunc = false;
        
        await Task.Run(async () =>
        {
            if (Constants.Provider.TryCreateReader("FortniteGame/AssetRegistry.bin", out var archive))
            {
                var registry = new FAssetRegistryState(archive);
                var (searchPathIdx, searchAssetIdx, replacePathIdx, replaceAssetIdx) = registry.Swap(search, replace);

                if (searchPathIdx is -1 || searchAssetIdx is -1 || replacePathIdx is -1 || replaceAssetIdx is -1)
                {
                    Logger.Log("Couldn't find an ID! Unable to lobby swap.");
                    returnFunc = true;
                    return;
                }

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

        if (returnFunc)
        {
            isLocked = false;
            return;
        }
        
        byte[] decompressedChunk = Constants.GlobalSwaps.ContainsKey(SaturnData.AssetRegistrySwap!.Value.CompressionBlock.CompressedStart) 
            ? Constants.GlobalSwaps[SaturnData.AssetRegistrySwap.Value.CompressionBlock.CompressedStart] 
            : SaturnData.AssetRegistrySwap.Value.DecompressedData;
        
        int offset = Utilities.IndexOfSequence(decompressedChunk, searchArray);

        Logger.Log($"Writing data with a length of {replaceArray.Length} to offset {offset} in decompressed data with a length of {decompressedChunk.Length}");
        var writtenData = Utilities.WriteBytes(replaceArray, decompressedChunk, offset);

        foreach (var lobbySwap in Constants.CurrentLobbySwaps.Where(lobbySwap => lobbySwap.Swaps[0].Offset == SaturnData.AssetRegistrySwap.Value.CompressionBlock.CompressedStart))
        {
            Utilities.WriteBytes(replaceArray, lobbySwap.Swaps[0].Data, offset);
            isLocked = false;
            return;
        }

        item.Name = "Lobby - " + Path.GetFileNameWithoutExtension(search) + " to " + Path.GetFileNameWithoutExtension(replace);
        item.Swaps = new[]
        {
            new Swap
            {
                File = DataCollection.GetGamePath() + "\\pakchunk0-WindowsClient.pak",
                Offset = SaturnData.AssetRegistrySwap.Value.CompressionBlock.CompressedStart,
                Data = writtenData
            }
        };

        Constants.GlobalSwaps[SaturnData.AssetRegistrySwap.Value.CompressionBlock.CompressedStart] = writtenData;
        
        Constants.CurrentLobbySwaps.Add(item);
        isLocked = false;
    }
    public static async Task ConvertLobby(AssetSelectorItem search, AssetSelectorItem replace)
    {
        //if (!Constants.ShouldLobbySwap || !Constants.isPlus) return;
        
        if (isLocked) return;
        isLocked = true;

        if (string.IsNullOrWhiteSpace(search.HID) || string.IsNullOrWhiteSpace(replace.HID))
        {
            isLocked = false;
            return;
        }

        SaturnData.Clear();

        byte[] searchHID = await Constants.Provider.SaveAssetAsync(search.HID.Split('.')[0] + ".uasset");

        var searchData = SaturnData.ToNonStatic();
        
        byte[] replaceHID = await Constants.Provider.SaveAssetAsync(replace.HID.Split('.')[0] + ".uasset");
        
        ZenAsset searchAsset = new ZenAsset(new AssetBinaryReader(searchHID), EngineVersion.VER_LATEST, Usmap.CachedMappings);
        ZenAsset replaceAsset = new ZenAsset(new AssetBinaryReader(replaceHID), EngineVersion.VER_LATEST, Usmap.CachedMappings);

        var swappedAsset = searchAsset.Swap(replaceAsset);

        List<SwapData> swapData = new()
        {
            new SwapData()
            {
                SaturnData = searchData,
                Data = swappedAsset.WriteData().ToArray()
            }
        };
        
        isLocked = false;

        await Convert(swapData, true);
    }

    public static async Task Revert(string id)
    {
        if (File.Exists(Constants.DataPath + id + ".json"))
            File.Delete(Constants.DataPath + id + ".json");
        
        new DirectoryInfo(Constants.DataPath).EnumerateFiles($"Lobby - * to {id}.json").ToList().ForEach(f => f.Delete());
        Constants.CurrentLobbySwaps.RemoveAll(x => x.Name.Contains("Lobby - ") && x.Name.Contains($" to {id}"));
    }

    public static async Task Convert(AssetSelectorItem search, AssetSelectorItem replace)
    {
        if (isLocked) return;
        isLocked = true;
        
        List<SwapData> swapData = new();
        AssetExportData selectedOption = await replace.OptionHandler.GenerateOptionDataWithFix(search);

        foreach (var characterPart in selectedOption.ExportParts)
        {
            replace.Description = $"Swapping asset: {Path.GetFileNameWithoutExtension(characterPart.Path)}";

            var pkg = await Constants.Provider.SaveAssetAsync(characterPart.Path.Split('.')[0]);
            AssetBinaryReader oldReader = new AssetBinaryReader(pkg);
            ZenAsset oldAsset = new ZenAsset(oldReader, EngineVersion.VER_LATEST, Usmap.CachedMappings);

            var data = SaturnData.ToNonStatic();
            SaturnData.Clear();

            var item = Constants.AssetCache[replace.ID];
            var swapPart = item.ExportParts.FirstOrDefault(part => part.Part == characterPart.Part);

            pkg = await Constants.Provider.SaveAssetAsync(swapPart == null ? Constants.EmptyParts[characterPart.Part].Path : swapPart.Path.Split('.')[0]);
            AssetBinaryReader newReader = new AssetBinaryReader(pkg);
            ZenAsset newAsset = new ZenAsset(newReader, EngineVersion.VER_LATEST, Usmap.CachedMappings);

            var asset = oldAsset.Swap(newAsset);

            swapData.Add(new SwapData()
            {
                SaturnData = data,
                Data = asset.WriteData().ToArray()
            });
            
            SaturnData.Clear();
        }

        isLocked = false;

        await Convert(swapData);
    }
    
    public static async Task Convert(List<SwapData> swapData, bool isLobbySwap = false)
    {
        if (isLocked) return;
        isLocked = true;
        
        CompressionBase compression = new Oodle();

        if (string.IsNullOrWhiteSpace(Constants.SelectedItem.DisplayName))
        {
            Constants.SelectedItem = new AssetSelectorItem()
            {
                DisplayName = "AssetImporter",
                ID = "AssetImporter"
            };
        }
        
        if (string.IsNullOrWhiteSpace(Constants.SelectedOption.DisplayName))
        {
            Constants.SelectedOption = new AssetSelectorItem()
            {
                DisplayName = "AssetImporter",
                ID = "AssetImporter"
            };
        }

        ItemModel item = new ItemModel();
        if (Constants.SelectedOption.DisplayName is "Plugin" or "AssetImporter")
            item.Name = Constants.SelectedItem.DisplayName;
        else
            item.Name = Constants.SelectedOption.DisplayName + " to " + Constants.SelectedItem.DisplayName + (isLobbySwap ? " - HID" : string.Empty);
        
        item.Swaps = new Swap[swapData.Sum(x => ChunkData(x.Data).Count * 2 + 1)];
        int swapIndex = 0;
            
        foreach (var swap in swapData)
        {
            if (string.IsNullOrWhiteSpace(swap.SaturnData.Path)) continue;
            
            var chunkedData = ChunkData(swap.Data);

            long totalDataCount = chunkedData.Sum(x => compression.Compress(x).Length);
            var (file, offset) = OffsetsInFile.Allocate(swap.SaturnData.Path, totalDataCount);

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

                swap.SaturnData.Reader.TocResource.CompressionBlocks[swap.SaturnData.FirstBlockIndex + index].Offset = offset + written + (long)(partitionIndex * swap.SaturnData.Reader.TocResource.Header.PartitionSize);
                swap.SaturnData.Reader.TocResource.CompressionBlocks[swap.SaturnData.FirstBlockIndex + index].CompressedSize = (uint)compressedChunk.Length;
                swap.SaturnData.Reader.TocResource.CompressionBlocks[swap.SaturnData.FirstBlockIndex + index].UncompressedSize = (uint)chunk.Length;
                swap.SaturnData.Reader.TocResource.CompressionBlocks[swap.SaturnData.FirstBlockIndex + index].CompressionMethodIndex = 1;

                item.Swaps[swapIndex++] = new Swap()
                {
                    File = swap.SaturnData.Path,
                    Offset = swap.SaturnData.Reader.TocResource.CompressionBlocks[swap.SaturnData.FirstBlockIndex + index].Position,
                    Data = swap.SaturnData.Reader.TocResource.CompressionBlocks[swap.SaturnData.FirstBlockIndex + index].Serialize()
                };

                written += compressedChunk.Length;
            }

            
            swap.SaturnData.Reader.TocResource.ChunkOffsetLengths[swap.SaturnData.TocIndex].SetLength((ulong)swap.Data.Length);
            item.Swaps[swapIndex++] = new Swap()
            {
                File = swap.SaturnData.Path,
                Offset = swap.SaturnData.Reader.TocResource.ChunkOffsetLengths[swap.SaturnData.TocIndex].Position,
                Data = swap.SaturnData.Reader.TocResource.ChunkOffsetLengths[swap.SaturnData.TocIndex].Serialize()
            };
        }
        
        while (File.Exists(Constants.DataPath + Constants.SelectedItem.ID + (isLobbySwap ? " - HID" : string.Empty) + ".json"))
        {
            Constants.SelectedItem.ID += "1";
        }
        
        await File.WriteAllTextAsync(Constants.DataPath + Constants.SelectedItem.ID + (isLobbySwap ? " - HID" : string.Empty) + ".json", JsonConvert.SerializeObject(item));
        isLocked = false;
    }
}